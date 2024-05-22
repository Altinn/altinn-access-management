using Altinn.AccessManagement.Core.Asserters; // Move to Altinn.AccessManagement.Extensions
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Resolvers.Extensions; // Move to Altinn.AccessManagement.Extensions
using Altinn.AccessManagement.Core.Services;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Extensions;
using Altinn.AccessManagement.Health;
using Altinn.AccessManagement.Integration.Clients;
using Altinn.AccessManagement.Integration.Configuration;
using Altinn.AccessManagement.Integration.Services;
using Altinn.AccessManagement.Integration.Services.Interfaces;
using Altinn.AccessManagement.Persistence;
using Altinn.AccessManagement.Persistence.Extensions; // Move to Altinn.AccessManagement.Extensions
using Altinn.Common.AccessToken;
using Altinn.Common.AccessToken.Services;
using Altinn.Common.AccessTokenClient.Services;
using Altinn.Common.PEP.Implementation;
using Altinn.Common.PEP.Interfaces;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Authorization;
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
    /// Configuration for Access Management API
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        /// Telemetry configuration
        /// </summary>
        public ConfigTelemetry Telemetry { get; set; }

        /// <summary>
        /// SblBridge configuration
        /// </summary>
        public ConfigSblBridge SblBridge { get; set; }
        
        /// <summary>
        /// Platform configuration
        /// </summary>
        public PlatformSettings PlatformSettings { get; set; } // Replace with new model
    }

    /// <summary>
    /// Telemetry configuration
    /// </summary>
    public class ConfigTelemetry
    {
        /// <summary>
        /// Instructs Telemetry to write to console
        /// </summary>
        public bool WriteToConsole { get; set; }

        /// <summary>
        /// Telemetry will use AlwaysOnSampler
        /// </summary>
        public bool UseAlwaysOnSampler { get; set; }
        
        /// <summary>
        /// Connectionstring for exporting Telemetry to Azure Monitor / AppInsights
        /// </summary>
        public string AppInsightsConnectionString { get; set; }

        /// <summary>
        /// Endpoint to send telemetry data
        /// </summary>
        public string ScrapingEndpoint { get; set; }
    }

    /// <summary>
    /// SblBridge Configuration
    /// </summary>
    public class ConfigSblBridge
    {
        /// <summary>
        /// Base Url for SblBridge API
        /// </summary>
        public string BaseApiUrl { get; set; }
    }

    /// <summary>
    /// Enum for the environments used for Azure AppConfig labels
    /// </summary>
    public enum ConfigEnvironment 
    { 
        Local, 
        AT21, 
        AT22, 
        AT23, 
        AT24 
    }

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
            var azureConfig = builder.Configuration.GetRequiredSection("AccessMgmt").Get<AppConfig>();
            builder.Services.AddSingleton<AppConfig>(azureConfig);

            builder.Services.AddFeatureManagement(); // Needed?

            ConfigureTelemetry(builder.Services, azureConfig);
            ConfigureBaseServices(builder.Services, azureConfig);

            builder.Services.ConfigureHttpClients();
        }

        /// <summary>
        /// Configure Telemetry
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="azureConfig">AppConfig</param>
        public static void ConfigureTelemetry(IServiceCollection services, AppConfig azureConfig)
        {
            services.AddOpenTelemetry()
               .ConfigureResource(resource =>
               {
                   resource.AddService("access-management", "Altinn.AccessManagement");

                   // resource.AddDetector(services => services.GetRequiredService<AltinnServiceResourceDetector>());
               })
               .WithTracing(tracing =>
               {
                   if (azureConfig.Telemetry.UseAlwaysOnSampler)
                   {
                       tracing.SetSampler(new AlwaysOnSampler());
                   }
                   
                   tracing.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddNpgsql();

                   if (azureConfig.Telemetry.WriteToConsole)
                   {
                       tracing.AddConsoleExporter();
                   }
               })
               .WithMetrics(metrics =>
               {
                   metrics.AddAspNetCoreInstrumentation().AddRuntimeInstrumentation();
                   if (azureConfig.Telemetry.WriteToConsole)
                   {
                       metrics.AddConsoleExporter();
                   }
               });
            
            if (!string.IsNullOrEmpty(azureConfig.Telemetry.AppInsightsConnectionString))
            {
                services.AddOpenTelemetry().UseAzureMonitor(config => config.ConnectionString = azureConfig.Telemetry.AppInsightsConnectionString);
            }
        }

        /// <summary>
        /// Configure BaseServices
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="azureConfig">AppConfig</param>
        public static void ConfigureBaseServices(IServiceCollection services, AppConfig azureConfig)
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
        /// Configures services
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="azureConfig">AppConfig</param>
        public static void ConfigureServices(IServiceCollection services, AppConfig azureConfig)
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

            // Requires: IOptions<PlatformSettings> settings, ILogger<IResourceRegistryClient> logger
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

            // Requires: IOptions<PlatformSettings> platformSettings, ILogger<AuthenticationClient> logger, IHttpContextAccessor httpContextAccessor, HttpClient httpClient
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
