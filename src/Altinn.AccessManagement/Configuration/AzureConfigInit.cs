using Altinn.AccessManagement.Core.Asserters; // Move to Altinn.AccessManagement.Extensions
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Resolvers.Extensions; // Move to Altinn.AccessManagement.Extensions
using Altinn.AccessManagement.Core.Services;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Extensions;
using Altinn.AccessManagement.Health;
using Altinn.AccessManagement.Integration.Clients;
using Altinn.AccessManagement.Integration.Services;
using Altinn.AccessManagement.Integration.Services.Interfaces;
using Altinn.AccessManagement.Persistence;
using Altinn.AccessManagement.Persistence.Configuration;
using Altinn.AccessManagement.Persistence.Extensions; // Move to Altinn.AccessManagement.Extensions
using Altinn.Common.AccessToken;
using Altinn.Common.AccessToken.Configuration;
using Altinn.Common.AccessToken.Services;
using Altinn.Common.AccessTokenClient.Services;
using Altinn.Common.Authentication.Configuration;
using Altinn.Common.PEP.Implementation;
using Altinn.Common.PEP.Interfaces;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Microsoft.OpenApi.Models;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Swashbuckle.AspNetCore.Filters;

namespace Altinn.AccessManagement.Configuration
{
    /// <summary>
    /// Temporary static class to replace Program.cs
    /// </summary>
    public static class AzureConfigInit
    {

        /*
         NOTES:
        var featMgm = app.Services.GetRequiredService<IFeatureManager>();
        bool feature = await featMgm.IsEnabledAsync("");
         
         */

        public static void Init(IHostApplicationBuilder builder, ConfigEnvironment environment)
        {
            // builder.Configuration.AddCommandLine(args);
            builder.Configuration.AddEnvironmentVariables();

            builder.ConfigureAzureAppConfig(environment);
            ConfigureSettings(builder.Services, builder.Configuration);

            builder.Services.AddFeatureManagement(); // Needed?

            ConfigureTelemetry(builder.Services, builder.Configuration);
            ConfigureBaseServices(builder.Services);

            builder.Services.ConfigureHttpClients();
        }

        /// <summary>
        /// Configure Telemetry
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="config">IConfiguration</param>
        public static void ConfigureTelemetry(IServiceCollection services, IConfiguration config)
        {
            var telemetryConfig = config.GetValue<ConfigTelemetry>("Telemetry");

            services.AddOpenTelemetry()
               .ConfigureResource(resource =>
               {
                   resource.AddService("access-management", "Altinn.AccessManagement");

                   // resource.AddDetector(services => services.GetRequiredService<AltinnServiceResourceDetector>());
               })
               .WithTracing(tracing =>
               {
                   if (telemetryConfig.UseAlwaysOnSampler)
                   {
                       tracing.SetSampler(new AlwaysOnSampler());
                   }
                   
                   tracing.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddNpgsql();

                   if (telemetryConfig.WriteToConsole)
                   {
                       tracing.AddConsoleExporter();
                   }
               })
               .WithMetrics(metrics =>
               {
                   metrics.AddAspNetCoreInstrumentation().AddRuntimeInstrumentation();
                   if (telemetryConfig.WriteToConsole)
                   {
                       metrics.AddConsoleExporter();
                   }
               });
            
            if (!string.IsNullOrEmpty(telemetryConfig.AppInsightsConnectionString))
            {
                services.AddOpenTelemetry().UseAzureMonitor(config => config.ConnectionString = telemetryConfig.AppInsightsConnectionString);
            }
        }

        /// <summary>
        /// Configure BaseServices
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        public static void ConfigureBaseServices(IServiceCollection services)
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
        }

        /// <summary>
        /// Maps settings
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="configuration">IConfiguration</param>
        public static void ConfigureSettings(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<PostgreSQLSettings>(configuration.GetRequiredSection("PostgreSQLSettings"));
            services.Configure<PlatformSettings>(configuration.GetRequiredSection("Platform"));
            services.Configure<CacheConfig>(configuration.GetRequiredSection("CacheConfig"));

            services.Configure<AzureStorageConfiguration>(configuration.GetRequiredSection("AzureStorageConfiguration"));
            services.Configure<AccessTokenSettings>(configuration.GetRequiredSection("AccessTokenSettings"));
            services.Configure<UserProfileLookupSettings>(configuration.GetRequiredSection("UserProfileLookupSettings"));
            services.Configure<OidcProviderSettings>(configuration.GetRequiredSection("OidcProviderSettings"));


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

        }

        /// <summary>
        /// Configures services
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="azureConfig">AccessMgmtAppConfig</param>
        public static void ConfigureServices(IServiceCollection services, AccessMgmtAppConfig azureConfig)
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
        }
    }
}
