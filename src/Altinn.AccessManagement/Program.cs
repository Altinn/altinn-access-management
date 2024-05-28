using System.Reflection;
using Altinn.AccessManagement.Configuration;
using Altinn.AccessManagement.Core.Asserters;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Resolvers.Extensions;
using Altinn.AccessManagement.Core.Services;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Filters;
using Altinn.AccessManagement.Health;
using Altinn.AccessManagement.Integration.Clients;
using Altinn.AccessManagement.Integration.Configuration;
using Altinn.AccessManagement.Integration.Services;
using Altinn.AccessManagement.Integration.Services.Interfaces;
using Altinn.AccessManagement.Persistence;
using Altinn.AccessManagement.Persistence.Configuration;
using Altinn.AccessManagement.Persistence.Extensions;
using Altinn.AccessManagement.Services;
using Altinn.Common.AccessToken;
using Altinn.Common.AccessToken.Services;
using Altinn.Common.AccessTokenClient.Services;
using Altinn.Common.Authentication.Configuration;
using Altinn.Common.Authentication.Models;
using Altinn.Common.PEP.Authorization;
using Altinn.Common.PEP.Clients;
using Altinn.Common.PEP.Implementation;
using Altinn.Common.PEP.Interfaces;
using AltinnCore.Authentication.JwtCookie;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.Exporter;
using Azure.Security.KeyVault.Secrets;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.FeatureManagement;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Swashbuckle.AspNetCore.Filters;
using Yuniql.AspNetCore;
using Yuniql.PostgreSql;
using KeyVaultSettings = AltinnCore.Authentication.Constants.KeyVaultSettings;

ILogger logger;

var builder = WebApplication.CreateBuilder(args);

string applicationInsightsKeySecretName = "ApplicationInsights--InstrumentationKey";
string applicationInsightsConnectionString = string.Empty;

ConfigureSetupLogging();

await SetConfigurationProviders(builder.Configuration);

ConfigureLogging(builder.Logging);

ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

Configure();
ConfigurePostgreSql();
app.Run();

void ConfigureSetupLogging()
{
    // Setup logging for the web host creation
    var logFactory = LoggerFactory.Create(builder =>
    {
        builder
            .AddFilter("Microsoft", LogLevel.Warning)
            .AddFilter("System", LogLevel.Warning)
            .AddFilter("Altinn.AccessManagement.Program", LogLevel.Debug)
            .AddConsole();
    });

    logger = logFactory.CreateLogger<Program>();

    NpgsqlLoggingConfiguration.InitializeLogging(logFactory);
}

void ConfigureLogging(ILoggingBuilder logging)
{
    // Clear log providers
    logging.ClearProviders();

    // Setup up application insight if ApplicationInsightsConnectionString is available
    if (!string.IsNullOrEmpty(applicationInsightsConnectionString))
    {
        // Add application insights https://docs.microsoft.com/en-us/azure/azure-monitor/app/ilogger
        logging.AddApplicationInsights(
             configureTelemetryConfiguration: (config) => config.ConnectionString = applicationInsightsConnectionString,
             configureApplicationInsightsLoggerOptions: (options) => { });

        // Optional: Apply filters to control what logs are sent to Application Insights.
        // The following configures LogLevel Information or above to be sent to
        // Application Insights for all categories.
        logging.AddFilter<Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Warning);

        // Adding the filter below to ensure logs of all severity from Program.cs
        // is sent to ApplicationInsights.
        logging.AddFilter<Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider>(typeof(Program).FullName, LogLevel.Trace);
    }
    else
    {
        // If not application insight is available log to console
        logging.AddFilter("Microsoft", LogLevel.Warning);
        logging.AddFilter("System", LogLevel.Warning);
        logging.AddConsole();
    }
}

async Task SetConfigurationProviders(ConfigurationManager config)
{
    string basePath = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;

    logger.LogInformation("Program // Loading Configuration from basePath={basePath}", basePath);

    config.SetBasePath(basePath);
    string configJsonFile1 = $"{basePath}/altinn-appsettings/altinn-dbsettings-secret.json";

    logger.LogInformation("Loading configuration file: '{configJsonFile1}'", configJsonFile1);
    config.AddJsonFile(configJsonFile1, optional: true, reloadOnChange: true);

    config.AddEnvironmentVariables();
    config.AddCommandLine(args);

    await ConnectToKeyVaultAndSetApplicationInsights(config);
}

async Task ConnectToKeyVaultAndSetApplicationInsights(ConfigurationManager config)
{
    logger.LogInformation("Program // Connect to key vault and set up application insights");

    KeyVaultSettings keyVaultSettings = new();
    config.GetSection("kvSetting").Bind(keyVaultSettings);

    if (!string.IsNullOrEmpty(keyVaultSettings.ClientId) &&
        !string.IsNullOrEmpty(keyVaultSettings.TenantId) &&
        !string.IsNullOrEmpty(keyVaultSettings.ClientSecret) &&
        !string.IsNullOrEmpty(keyVaultSettings.SecretUri))
    {
        Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", keyVaultSettings.ClientId);
        Environment.SetEnvironmentVariable("AZURE_CLIENT_SECRET", keyVaultSettings.ClientSecret);
        Environment.SetEnvironmentVariable("AZURE_TENANT_ID", keyVaultSettings.TenantId);

        try
        {
            SecretClient client = new SecretClient(new Uri(keyVaultSettings.SecretUri), new EnvironmentCredential());
            KeyVaultSecret secret = await client.GetSecretAsync(applicationInsightsKeySecretName);
            applicationInsightsConnectionString = string.Format("InstrumentationKey={0}", secret.Value);
        }
        catch (Exception vaultException)
        {
            logger.LogError(vaultException, $"Unable to read application insights key.");
        }

        try
        {
            //// TODO: microsoft.extensions.configuration.azurekeyvault is depricated
            config.AddAzureKeyVault(keyVaultSettings.SecretUri, keyVaultSettings.ClientId, keyVaultSettings.ClientSecret);
        }
        catch (Exception vaultException)
        {
            logger.LogError(vaultException, $"Unable to add key vault secrets to config.");
        }
    }
}

void ConfigureServices(IServiceCollection services, IConfiguration config)
{
    builder.Services.AddAccessManagementPersistence();
    logger.LogInformation("Startup // ConfigureServices");
    services.ConfigureAsserters();
    services.ConfigureResolvers();
    services.AddAutoMapper(typeof(Program));
    services.AddControllersWithViews();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddFeatureManagement();

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
    services.AddMvc();

    PlatformSettings platformSettings = config.GetSection("Platform").Get<PlatformSettings>();
    OidcProviderSettings oidcProviders = config.GetSection("OidcProviders").Get<OidcProviderSettings>();
    services.Configure<GeneralSettings>(config.GetSection("GeneralSettings"));
    services.Configure<PlatformSettings>(config.GetSection("Platform"));
    services.Configure<Altinn.Common.PEP.Configuration.PlatformSettings>(config.GetSection("Platform"));
    services.Configure<CacheConfig>(config.GetSection("CacheConfig"));
    services.Configure<PostgreSQLSettings>(config.GetSection("PostgreSQLSettings"));
    services.Configure<AzureStorageConfiguration>(config.GetSection("AzureStorageConfiguration"));
    services.Configure<SblBridgeSettings>(config.GetSection("SblBridgeSettings"));
    services.Configure<Altinn.Common.AccessToken.Configuration.KeyVaultSettings>(config.GetSection("kvSetting"));
    services.Configure<KeyVaultSettings>(config.GetSection("kvSetting"));
    services.Configure<OidcProviderSettings>(config.GetSection("OidcProviders"));
    services.Configure<UserProfileLookupSettings>(config.GetSection("UserProfileLookupSettings"));

    services.AddHttpClient<IDelegationRequestsWrapper, DelegationRequestProxy>();
    services.AddHttpClient<IPartiesClient, PartiesClient>();
    services.AddHttpClient<IProfileClient, ProfileClient>();
    services.AddHttpClient<IAltinnRolesClient, AltinnRolesClient>();
    services.AddHttpClient<IAltinn2RightsClient, Altinn2RightsClient>();
    services.AddHttpClient<AuthorizationApiClient>();

    services.AddTransient<IDelegationRequests, DelegationRequestService>();

    services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

    services.AddSingleton(config);
    services.AddSingleton<IPolicyRetrievalPoint, PolicyRetrievalPoint>();
    services.AddSingleton<IPolicyInformationPoint, PolicyInformationPoint>();
    services.AddSingleton<IPolicyAdministrationPoint, PolicyAdministrationPoint>();
    services.AddSingleton<IResourceAdministrationPoint, ResourceAdministrationPoint>();
    services.AddSingleton<IPolicyRepository, PolicyRepository>();
    services.AddSingleton<IResourceRegistryClient, ResourceRegistryClient>();
    services.AddSingleton<IDelegationMetadataRepository, DelegationMetadataRepository>();
    services.AddSingleton<IResourceMetadataRepository, ResourceMetadataRepository>();
    services.AddSingleton<IDelegationChangeEventQueue, DelegationChangeEventQueue>();
    services.AddSingleton<IEventMapperService, EventMapperService>();
    services.AddSingleton<IResourceAdministrationPoint, ResourceAdministrationPoint>();
    services.AddSingleton<IContextRetrievalService, ContextRetrievalService>();
    services.AddSingleton<IMaskinportenSchemaService, MaskinportenSchemaService>();
    services.AddSingleton<IAuthorizationHandler, AccessTokenHandler>();
    services.AddTransient<IPublicSigningKeyProvider, PublicSigningKeyProvider>();
    services.AddSingleton<IAccessTokenGenerator, AccessTokenGenerator>();
    services.AddTransient<ISigningCredentialsResolver, SigningCredentialsResolver>();
    services.AddSingleton<IAuthenticationClient, AuthenticationClient>();
    services.AddSingleton<IPDP, PDPAppSI>();
    services.AddSingleton<ISingleRightsService, SingleRightsService>();
    services.AddSingleton<IUserProfileLookupService, UserProfileLookupService>();
    services.AddSingleton<IKeyVaultService, KeyVaultService>();
    services.AddSingleton<IPlatformAuthorizationTokenProvider, PlatformAuthorizationTokenProvider>();
    services.AddSingleton<IAuthorizedPartiesService, AuthorizedPartiesService>();
    services.AddSingleton<IAltinn2RightsService, Altinn2RightsService>();
    services.AddAccessManagementPersistence();

    if (oidcProviders.TryGetValue("altinn", out OidcProvider altinnOidcProvder))
    {
        services.AddAuthentication(JwtCookieDefaults.AuthenticationScheme)
        .AddJwtCookie(JwtCookieDefaults.AuthenticationScheme, options =>
        {
            options.JwtCookieName = platformSettings.JwtCookieName;
            options.MetadataAddress = altinnOidcProvder.Issuer;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            if (builder.Environment.IsDevelopment())
            {
                options.RequireHttpsMetadata = false;
            }
        });
    }
    else
    {
        logger.LogError("Unable to setup authentication. Missing altinn OidcProvider config.");
    }

    services.AddAuthorization(options =>
    {
        options.AddPolicy("PlatformAccess", policy => policy.Requirements.Add(new AccessTokenRequirement()));
        options.AddPolicy(AuthzConstants.ALTINNII_AUTHORIZATION, policy => policy.Requirements.Add(new ClaimAccessRequirement("urn:altinn:app", "sbl.authorization")));
        options.AddPolicy(AuthzConstants.INTERNAL_AUTHORIZATION, policy => policy.Requirements.Add(new ClaimAccessRequirement("urn:altinn:app", "internal.authorization")));
        options.AddPolicy(AuthzConstants.POLICY_MASKINPORTEN_DELEGATION_READ, policy => policy.Requirements.Add(new ResourceAccessRequirement("read", "altinn_maskinporten_scope_delegation")));
        options.AddPolicy(AuthzConstants.POLICY_MASKINPORTEN_DELEGATION_WRITE, policy => policy.Requirements.Add(new ResourceAccessRequirement("write", "altinn_maskinporten_scope_delegation")));
        options.AddPolicy(AuthzConstants.POLICY_MASKINPORTEN_DELEGATIONS_PROXY, policy => policy.Requirements.Add(new ScopeAccessRequirement(new string[] { "altinn:maskinporten/delegations", "altinn:maskinporten/delegations.admin" })));
        options.AddPolicy(AuthzConstants.POLICY_ACCESS_MANAGEMENT_READ, policy => policy.Requirements.Add(new ResourceAccessRequirement("read", "altinn_access_management")));
        options.AddPolicy(AuthzConstants.POLICY_ACCESS_MANAGEMENT_WRITE, policy => policy.Requirements.Add(new ResourceAccessRequirement("write", "altinn_access_management")));
        options.AddPolicy(AuthzConstants.POLICY_RESOURCEOWNER_AUTHORIZEDPARTIES, policy =>
            policy.Requirements.Add(new ScopeAccessRequirement(new string[] { AuthzConstants.SCOPE_AUTHORIZEDPARTIES_RESOURCEOWNER, AuthzConstants.SCOPE_AUTHORIZEDPARTIES_ADMIN })));
    });

    services.AddTransient<IAuthorizationHandler, ClaimAccessHandler>();
    services.AddTransient<IAuthorizationHandler, ResourceAccessHandler>();
    services.AddTransient<IAuthorizationHandler, ScopeAccessHandler>();

    services.Configure<KestrelServerOptions>(options =>
    {
        options.AllowSynchronousIO = true;
    });

    if (!string.IsNullOrEmpty(applicationInsightsConnectionString))
    {
        services.AddSingleton(typeof(ITelemetryChannel), new ServerTelemetryChannel() { StorageFolder = "/tmp/logtelemetry" });
        services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
        {
            ConnectionString = applicationInsightsConnectionString
        });

        services.AddApplicationInsightsTelemetryProcessor<HealthTelemetryFilter>();
        services.AddApplicationInsightsTelemetryProcessor<IdentityTelemetryFilter>();
        services.AddSingleton<ITelemetryInitializer, CustomTelemetryInitializer>();

        if (config.GetSection("FeatureManagement").GetValue<bool>("OpenTelementry"))
        {
            var telemetry = new List<TracerProviderBuilder>()
            {
                {
                    Sdk.CreateTracerProviderBuilder()
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(Altinn.AccessManagement.Persistence.Configuration.TelemetryConfig._activitySource.Name))
                    .AddNpgsql()
                    .AddSource(Altinn.AccessManagement.Persistence.Configuration.TelemetryConfig._activitySource.Name)
                }
            };

            foreach (var t in telemetry)
            {
                t.SetSampler(new AlwaysOnSampler());
                if (builder.Environment.IsDevelopment())
                {
                    t.AddConsoleExporter();
                }

                t.AddAzureMonitorTraceExporter(opt => { opt.ConnectionString = applicationInsightsConnectionString; });
                t.Build();
            }
        }

        logger.LogInformation("Startup // ApplicationInsightsConnectionString = {applicationInsightsConnectionString}", applicationInsightsConnectionString);
    }
}

void Configure()
{
    logger.LogInformation("Startup // Configure");

    if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
    {
        logger.LogInformation("IsDevelopment || IsStaging");

        app.UseDeveloperExceptionPage();

        // Enable higher level of detail in exceptions related to JWT validation
        IdentityModelEventSource.ShowPII = true;
    }
    else
    {
        app.UseExceptionHandler("/accessmanagement/api/v1/error");
    }

    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");
    app.UseCors();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
}

void ConfigurePostgreSql()
{
    if (builder.Configuration.GetValue<bool>("PostgreSQLSettings:EnableDBConnection"))
    {
        ConsoleTraceService traceService = new ConsoleTraceService { IsDebugEnabled = false };

        string connectionString = string.Format(
            builder.Configuration.GetValue<string>("PostgreSQLSettings:AdminConnectionString"),
            builder.Configuration.GetValue<string>("PostgreSQLSettings:authorizationDbAdminPwd"));

        app.UseYuniql(
            new PostgreSqlDataService(traceService),
            new PostgreSqlBulkImportService(traceService),
            traceService,
            new Configuration
            {
                Workspace = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Migration"),
                ConnectionString = connectionString,
                IsAutoCreateDatabase = false,
                IsDebug = true,
            });
    }
}

/// <summary>
/// Program
/// </summary>
public partial class Program
{
}