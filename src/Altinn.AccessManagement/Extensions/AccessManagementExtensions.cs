using Altinn.AccessManagement.Configuration;
using Altinn.AccessManagement.Core.Asserters;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Resolvers.Extensions;
using Altinn.AccessManagement.Core.Services;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Health;
using Altinn.AccessManagement.Integration.Clients;
using Altinn.AccessManagement.Integration.Services;
using Altinn.AccessManagement.Integration.Services.Interfaces;
using Altinn.AccessManagement.Persistence;
using Altinn.AccessManagement.Persistence.Extensions;
using Altinn.AccessManagement.Services;
using Altinn.Common.AccessToken;
using Altinn.Common.AccessToken.Services;
using Altinn.Common.AccessTokenClient.Services;
using Altinn.Common.PEP.Clients;
using Altinn.Common.PEP.Implementation;
using Altinn.Common.PEP.Interfaces;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Azure.Security.KeyVault.Secrets;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Swashbuckle.AspNetCore.Filters;

namespace Altinn.AccessManagement.Extensions;

/// <summary>
/// Extensions for Access Mgmt
/// </summary>
public static class AccessManagementExtensions
{
    /// <summary>
    /// Configure HttpClients
    /// </summary>
    /// <param name="builder">IHostApplicationBuilder</param>
    /// <returns>Extended IServiceCollection</returns>
    public static IHostApplicationBuilder ConfigureSettings(this IHostApplicationBuilder builder)
    {
        builder.Services.Configure<AccessMgmtSettings>(builder.Configuration.GetRequiredSection("AccessMgmt:General"));
        builder.Services.Configure<ConfigTelemetry>(builder.Configuration.GetRequiredSection("AccessMgmt:Telemetry"));

        // builder.Services.Configure<PostgreSQLSettings>(builder.Configuration.GetRequiredSection("PostgreSQLSettings"));
        // builder.Services.Configure<PlatformSettings>(builder.Configuration.GetRequiredSection("Platform"));
        // builder.Services.Configure<CacheConfig>(builder.Configuration.GetRequiredSection("CacheConfig"));
        // builder.Services.Configure<AzureStorageConfiguration>(builder.Configuration.GetRequiredSection("AzureStorageConfiguration"));
        // builder.Services.Configure<AccessTokenSettings>(builder.Configuration.GetRequiredSection("AccessTokenSettings"));
        // builder.Services.Configure<UserProfileLookupSettings>(builder.Configuration.GetRequiredSection("UserProfileLookupSettings"));
        // builder.Services.Configure<OidcProviderSettings>(builder.Configuration.GetRequiredSection("OidcProviderSettings"));

        /*
         PostgreSQLSettings
            _postgreSQLSettings.ConnectionString
            _postgreSQLSettings.AuthorizationDbPwd

         */

        /*
         PlatformSettings
            _platformSettings.JwtCookieName
         */

        /*
         CacheConfig
            _cacheConfig.AltinnRoleCacheTimeout
            _cacheConfig.PartyCacheTimeout
            _cacheConfig.PolicyCacheTimeout

         */

        /*
            AzureStorageConfiguration
                _storageConfig.DelegationEventQueueAccountName
                _storageConfig.DelegationEventQueueAccountKey
                _storageConfig.DelegationEventQueueEndpoint

                _storageConfig.MetadataAccountName
                _storageConfig.MetadataAccountKey
                _storageConfig.MetadataBlobEndpoint
                _storageConfig.MetadataContainer

                _storageConfig.DelegationsAccountName
                _storageConfig.DelegationsAccountKey
                _storageConfig.DelegationsBlobEndpoint
                _storageConfig.DelegationsContainer

                _storageConfig.ResourceRegistryAccountName
                _storageConfig.ResourceRegistryAccountKey
                _storageConfig.ResourceRegistryBlobEndpoint
                _storageConfig.ResourceRegistryContainer
         */

        /*
            AccessTokenSettings
                _accessTokenSettings.CacheCertLifetimeInSeconds
                _accessTokenSettings.DisableAccessTokenVerification
                _accessTokenSettings.AccessTokenHeaderId
                _accessTokenSettings.AccessTokenHttpContextId
                _accessTokenSettings.AccessTokenSigningKeysFolder
                _accessTokenSettings.AccessTokenSigningCertificateFileName
                _accessTokenSettings.ValidFromAdjustmentSeconds
                _accessTokenSettings.TokenLifetimeInSeconds
         */

        /*
            UserProfileLookupSettings
                _userProfileLookupSettings.MaximumFailedAttempts
                _userProfileLookupSettings.FailedAttemptsCacheLifetimeSeconds
         */

        /*
            OidcProviderSettings
                _oidcProviderSettings["altinn"].Issuer
         */

        /*
            KeyVaultSettings
                keyVaultSettings.Value.SecretUri

         */

        return builder;
    }

    /// <summary>
    /// Configure Telemetry
    /// </summary>
    /// <param name="builder">IHostApplicationBuilder</param>
    public static IHostApplicationBuilder ConfigureTelemetry(this IHostApplicationBuilder builder)
    {
        var telemetryConfig = builder.Configuration.GetSection("AccessMgmt:Telemetry").Get<ConfigTelemetry>();

        builder.Services.AddSingleton<ITelemetryInitializer, CustomTelemetryInitializer>();

        var ignoreUrls = new List<string>() { "/swagger/index.html", "/swagger/v1/swagger.json", "/_vs/browserLink", "/_framework/aspnetcore-browser-refresh.js" };

        builder.Services.AddOpenTelemetry()
           .ConfigureResource(resource =>
           {
               resource.AddService("access-management", "Altinn.AccessManagement");
           })
           .WithTracing(tracing =>
           {
               if (telemetryConfig.UseAlwaysOnSampler)
               {
                   tracing.SetSampler(new AlwaysOnSampler());
               }

               tracing.AddAspNetCoreInstrumentation(opt =>
               {
                   opt.Filter = context => { return !ignoreUrls.Contains(context.Request.Path); };
               })
               .AddHttpClientInstrumentation()
               .AddNpgsql();

               if (telemetryConfig.WriteToConsole)
               {
                   tracing.AddConsoleExporter();
               }
           })
           .WithMetrics(metrics =>
           {
               metrics.AddAspNetCoreInstrumentation().AddRuntimeInstrumentation();
               /*
               if (telemetryConfig.WriteToConsole)
               {
                   metrics.AddConsoleExporter();
               }
               */
           });

        if (!string.IsNullOrEmpty(telemetryConfig.AppInsightsConnectionString))
        {
            builder.Services.AddOpenTelemetry().UseAzureMonitor(config => config.ConnectionString = telemetryConfig.AppInsightsConnectionString);
        }

        return builder;
    }

    /// <summary>
    /// Configure HttpClients
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <returns>Extended IServiceCollection</returns>
    public static IServiceCollection ConfigureHttpClients(this IServiceCollection services)
    {
        // SblBridge
        services.AddHttpClient<IDelegationRequestsWrapper, DelegationRequestProxy>();
        services.AddTransient<IDelegationRequests, DelegationRequestService>();

        // SblBridge, Platform
        services.AddHttpClient<IPartiesClient, PartiesClient>();

        // Platform
        services.AddHttpClient<IProfileClient, ProfileClient>();

        // SblBridge
        services.AddHttpClient<IAltinnRolesClient, AltinnRolesClient>();

        // SblBridge
        services.AddHttpClient<IAltinn2RightsClient, Altinn2RightsClient>();

        // Platform
        services.AddHttpClient<AuthorizationApiClient>();

        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        return services;
    }

    /// <summary>
    /// Configure Azure AccessMgmtAppConfig
    /// </summary>
    /// <param name="builder">IHostApplicationBuilder</param>
    public static IHostApplicationBuilder ConfigureAzureAppConfig(this IHostApplicationBuilder builder)
    {
        var envCode = builder.Configuration.GetValue<string>("EnvironmentCode");
        if (string.IsNullOrWhiteSpace(envCode))
        {
            throw new Exception("Missing EnvironmentCode");
        }

        var appConfigConnectionString = builder.Configuration.GetValue<string>("AppConfigConnectionString");

        var appConfigUrl = builder.Configuration.GetValue<string>("AppConfigUrl");
        var keyVaultUrl = builder.Configuration.GetValue<string>("KeyVaultUrl");

        if (string.IsNullOrWhiteSpace(appConfigConnectionString) && string.IsNullOrWhiteSpace(appConfigUrl))
        {
            throw new Exception("Missing AppConfigConnectionString or AppConfigUrl");
        }

        builder.Services.AddAzureAppConfiguration();

        if (appConfigConnectionString != null)
        {
            builder.Configuration.AddAzureAppConfiguration(c =>
            {
                c.Connect(appConfigConnectionString)
                    .Select(KeyFilter.Any, LabelFilter.Null)
                    .Select(KeyFilter.Any, envCode);
                c.UseFeatureFlags()
                    .Select(KeyFilter.Any, LabelFilter.Null)
                    .Select(KeyFilter.Any, envCode);
                c.ConfigureRefresh(c => c.Register("Sentinel", true));
            });
        }
        else
        {
            if (string.IsNullOrWhiteSpace(keyVaultUrl))
            {
                throw new Exception("Missing KeyVaultUrl");
            }

            var defaultCred = new EnvironmentCredential();
            builder.Configuration.AddAzureAppConfiguration(c =>
            {
                c.Connect(new Uri(appConfigUrl), defaultCred)
                    .Select(KeyFilter.Any, LabelFilter.Null)
                    .Select(KeyFilter.Any, envCode);
                c.UseFeatureFlags()
                    .Select(KeyFilter.Any, LabelFilter.Null)
                    .Select(KeyFilter.Any, envCode);
                c.ConfigureRefresh(c => c.Register("Sentinel", true));
                c.ConfigureKeyVault(kv =>
                {
                    kv.Register(new SecretClient(new Uri(keyVaultUrl), defaultCred));
                });
            });
        }

        return builder;
    }

    /// <summary>
    /// Configure BaseServices
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    public static IServiceCollection ConfigureBaseServices(this IServiceCollection services)
    {
        services.AddAccessManagementPersistence();
        services.ConfigureAsserters();
        services.ConfigureResolvers();
        services.AddAutoMapper(typeof(Program));
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddHealthChecks().AddCheck<HealthCheck>("authorization_admin_health_check");
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            });
            options.OperationFilter<SecurityRequirementsOperationFilter>();
        });

        return services;
    }

    /// <summary>
    /// Configures services
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        /*
        NOTES:
        IMemoryCache, where is it?
        IAttributeResolver, where is it?
        IAssert<AttributeMatch>, where is it?
        */

        // Requires: 
        services.AddSingleton<IEventMapperService, EventMapperService>();

        // Requires: NpgsqlDataSource conn
        services.AddSingleton<IDelegationMetadataRepository, DelegationMetadataRepository>();

        // Requires: IOptions<PostgreSQLSettings> postgresSettings
        services.AddSingleton<IResourceMetadataRepository, ResourceMetadataRepository>();

        // Requires: IOptions<Platform> settings
        services.AddSingleton<IResourceRegistryClient, ResourceRegistryClient>();

        // Requires: IOptions<CacheConfig> cacheConfig, IMemoryCache memoryCache, IResourceRegistryClient resourceRegistryClient, IAltinnRolesClient altinnRolesClient, IPartiesClient partiesClient
        services.AddSingleton<IContextRetrievalService, ContextRetrievalService>();

        // Requires: IEventMapperService eventMapperService, IOptions<AzureStorageConfiguration> storageConfig
        services.AddSingleton<IDelegationChangeEventQueue, DelegationChangeEventQueue>();

        // Requires: IOptions<AzureStorageConfiguration> storageConfig
        services.AddSingleton<IPolicyRepository, PolicyRepository>();

        // Requires: IPolicyRepository policyRepository, IMemoryCache memoryCache, IOptions<CacheConfig> cacheConfig
        services.AddSingleton<IPolicyRetrievalPoint, PolicyRetrievalPoint>();

        // Requires: ILogger<IPolicyInformationPoint> logger, IPolicyRetrievalPoint policyRetrievalPoint, IDelegationMetadataRepository delegationRepository, IContextRetrievalService contextRetrievalService, IProfileClient profile
        services.AddSingleton<IPolicyInformationPoint, PolicyInformationPoint>();

        // Requires: IPolicyRetrievalPoint policyRetrievalPoint, IPolicyRepository policyRepository, IDelegationMetadataRepository delegationRepository, IDelegationChangeEventQueue eventQueue, ILogger<IPolicyAdministrationPoint> logger
        services.AddSingleton<IPolicyAdministrationPoint, PolicyAdministrationPoint>();

        // Requires: IResourceMetadataRepository resourceRepository, ILogger<IResourceAdministrationPoint> logger, IContextRetrievalService contextRetrievalService
        services.AddSingleton<IResourceAdministrationPoint, ResourceAdministrationPoint>();

        // Requires: ILogger<IMaskinportenSchemaService> logger, IDelegationMetadataRepository delegationRepository, IContextRetrievalService contextRetrievalService, IResourceAdministrationPoint resourceAdministrationPoint, IPolicyInformationPoint pip, IPolicyAdministrationPoint pap
        services.AddSingleton<IMaskinportenSchemaService, MaskinportenSchemaService>();

        // Requires: IOptions<KeyVaultSettings> keyVaultSettings, IOptions<AccessTokenSettings> accessTokenSettings, IMemoryCache memoryCache
        services.AddTransient<IPublicSigningKeyProvider, PublicSigningKeyProvider>();

        // Requires: IHttpContextAccessor httpContextAccessor, ILogger<AccessTokenHandler> logger, IOptions<AccessTokenSettings> accessTokenSettings, IPublicSigningKeyProvider publicSigningKeyProvider
        services.AddSingleton<IAuthorizationHandler, AccessTokenHandler>();

        // Requires: IOptions<AccessTokenSettings> accessTokenSettings
        services.AddTransient<ISigningCredentialsResolver, SigningCredentialsResolver>();

        // Requires: ILogger<AccessTokenGenerator> logger, IOptions<AccessTokenSettings> accessTokenSettings, ISigningCredentialsResolver signingKeysResolver = null
        services.AddSingleton<IAccessTokenGenerator, AccessTokenGenerator>();

        // Requires: IOptions<Platform> platformSettings, ILogger<AuthenticationClient> logger, IHttpContextAccessor httpContextAccessor, HttpClient httpClient
        services.AddSingleton<IAuthenticationClient, AuthenticationClient>();

        // Requires: ILogger<PDPAppSI> logger, AuthorizationApiClient authorizationApiClient
        services.AddSingleton<IPDP, PDPAppSI>();

        // Requires: ILogger<UserProfileLookupService> logger, IOptions<UserProfileLookupSettings> userProfileLookupSettings, IMemoryCache memoryCache, IProfileClient profile
        services.AddSingleton<IUserProfileLookupService, UserProfileLookupService>();

        // Requires: IAttributeResolver resolver, IAssert<AttributeMatch> asserter, IContextRetrievalService contextRetrievalService, IPolicyInformationPoint pip, IPolicyAdministrationPoint pap, IAltinn2RightsClient altinn2RightsClient, IProfileClient profile, IUserProfileLookupService profileLookup
        services.AddSingleton<ISingleRightsService, SingleRightsService>();

        // Requires: 
        services.AddSingleton<IKeyVaultService, KeyVaultService>();

        // Requires: IKeyVaultService keyVaultService, IAccessTokenGenerator accessTokenGenerator, IOptions<AccessTokenSettings> accessTokenSettings, IOptions<KeyVaultSettings> keyVaultSettings, IOptions<OidcProviderSettings> oidcProviderSettings
        services.AddSingleton<IPlatformAuthorizationTokenProvider, PlatformAuthorizationTokenProvider>();

        // Requires: IContextRetrievalService contextRetrievalService, IDelegationMetadataRepository delegations, IAltinnRolesClient altinn2, IProfileClient profile
        services.AddSingleton<IAuthorizedPartiesService, AuthorizedPartiesService>();

        // Requires: IContextRetrievalService contextRetrievalService, IPolicyInformationPoint pip, IAltinn2RightsClient altinn2RightsClient, IProfileClient profileClient
        services.AddSingleton<IAltinn2RightsService, Altinn2RightsService>();

        return services;
    }
}
