using Altinn.AccessManagement.Configuration;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Filters;
using Altinn.AccessManagement.Health;
using Altinn.AccessManagement.Integration.Clients;
using Altinn.AccessManagement.Integration.Configuration;
using Altinn.AccessManagement.Integration.Services;
using Altinn.AccessManagement.Persistence;
using Altinn.AccessManagement.Persistence.Configuration;
using Altinn.AccessManagement.Services;
using Altinn.Common.PEP.Authorization;
using AltinnCore.Authentication.Constants;
using AltinnCore.Authentication.JwtCookie;

using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Npgsql.Logging;
using Yuniql.AspNetCore;
using Yuniql.PostgreSql;

ILogger logger;

var builder = WebApplication.CreateBuilder(args);

NpgsqlLogManager.Provider = new ConsoleLoggingProvider(NpgsqlLogLevel.Trace, true, true);

string frontendProdFolder = AppEnvironment.GetVariable("FRONTEND_PROD_FOLDER", "wwwroot/AccessManagement/");
builder.Configuration.AddJsonFile(frontendProdFolder + "manifest.json", optional: true, reloadOnChange: true);

string applicationInsightsKeySecretName = "ApplicationInsights--InstrumentationKey";
string applicationInsightsConnectionString = string.Empty;

ConfigureSetupLogging();

await SetConfigurationProviders(builder.Configuration);

ConfigureLogging(builder.Logging);

ConfigureServices(builder.Services, builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://devenv.altinn.no");
        });
});

var app = builder.Build();
ConfigurePostgreSql();

Configure();

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

    logger.LogInformation($"Program // Loading Configuration from basePath={basePath}");

    config.SetBasePath(basePath);
    string configJsonFile1 = $"{basePath}/altinn-appsettings/altinn-dbsettings-secret.json";
    string configJsonFile2 = $"{Directory.GetCurrentDirectory()}/appsettings.json";

    if (basePath == "/")
    {
        configJsonFile2 = "/app/appsettings.json";
    }

    logger.LogInformation($"Loading configuration file: '{configJsonFile1}'");
    config.AddJsonFile(configJsonFile1, optional: true, reloadOnChange: true);

    logger.LogInformation($"Loading configuration file2: '{configJsonFile2}'");
    config.AddJsonFile(configJsonFile2, optional: false, reloadOnChange: true);

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
    logger.LogInformation("Startup // ConfigureServices");
    services.AddControllersWithViews();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();

    services.AddHealthChecks().AddCheck<HealthCheck>("authorization_admin_health_check");
    services.AddSwaggerGen();
    services.AddMvc();

    PlatformSettings platformSettings = config.GetSection("PlatformSettings").Get<PlatformSettings>();
    services.Configure<GeneralSettings>(config.GetSection("GeneralSettings"));
    services.Configure<PlatformSettings>(config.GetSection("PlatformSettings"));
    services.Configure<CacheConfig>(config.GetSection("CacheConfig"));
    services.Configure<PostgreSQLSettings>(config.GetSection("PostgreSQLSettings"));
    services.Configure<AzureStorageConfiguration>(config.GetSection("AzureStorageConfiguration"));
    services.Configure<ResourceRegistrySettings>(config.GetSection("ResourceRegistrySettings"));
    services.Configure<SblBridgeSettings>(config.GetSection("SblBridgeSettings"));

    services.AddHttpClient<IDelegationRequestsWrapper, DelegationRequestProxy>();
    services.AddHttpClient<IPartiesClient, PartiesClient>();
    services.AddHttpClient<IAltinnRolesClient, AltinnRolesClient>();

    services.AddTransient<IDelegationRequests, DelegationRequestService>();

    services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

    services.AddSingleton(config);
    services.AddSingleton<IPolicyRetrievalPoint, PolicyRetrievalPoint>();
    services.AddSingleton<IPolicyInformationPoint, PolicyInformationPoint>();
    services.AddSingleton<IPolicyAdministrationPoint, PolicyAdministrationPoint>();
    services.AddSingleton<IPolicyRepository, PolicyRepository>();
    services.AddSingleton<IResourceRegistryClient, ResourceRegistryClient>();
    services.AddSingleton<IDelegationMetadataRepository, DelegationMetadataRepository>();
    services.AddSingleton<IDelegationChangeEventQueue, DelegationChangeEventQueue>();
    services.AddSingleton<IEventMapperService, EventMapperService>();
    services.AddSingleton<IDelegationsService, DelegationsService>();
    services.AddSingleton<IContextRetrievalService, ContextRetrievalService>();

    services.AddAuthentication(JwtCookieDefaults.AuthenticationScheme)
        .AddJwtCookie(JwtCookieDefaults.AuthenticationScheme, options =>
        {
            options.JwtCookieName = platformSettings.JwtCookieName;
            options.MetadataAddress = platformSettings.OpenIdWellKnownEndpoint;
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

    services.AddAuthorization(options =>
    {
        options.AddPolicy(AuthzConstants.POLICY_STUDIO_DESIGNER, policy => policy.Requirements.Add(new ClaimAccessRequirement("urn:altinn:app", "studio.designer")));
        options.AddPolicy(AuthzConstants.ALTINNII_AUTHORIZATION, policy => policy.Requirements.Add(new ClaimAccessRequirement("urn:altinn:app", "sbl.authorization")));
    });

    services.AddTransient<IAuthorizationHandler, ClaimAccessHandler>();

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
        app.UseExceptionHandler("/access-management/api/v1/error");
    }

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
        endpoints.MapHealthChecks("/health");
    });

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors();
    app.UseStaticFiles();
    app.MapControllers();
}

void ConfigurePostgreSql()
{
    if (builder.Configuration.GetValue<bool>("PostgreSQLSettings:EnableDBConnection"))
    {
        ConsoleTraceService traceService = new ConsoleTraceService { IsDebugEnabled = true };

        string connectionString = string.Format(
            builder.Configuration.GetValue<string>("PostgreSQLSettings:AdminConnectionString"),
            builder.Configuration.GetValue<string>("PostgreSQLSettings:authorizationDbAdminPwd"));

        app.UseYuniql(
            new PostgreSqlDataService(traceService),
            new PostgreSqlBulkImportService(traceService),
            traceService,
            new Yuniql.AspNetCore.Configuration
            {
                Workspace = Path.Combine(Environment.CurrentDirectory, builder.Configuration.GetValue<string>("PostgreSQLSettings:WorkspacePath")),
                ConnectionString = connectionString,
                IsAutoCreateDatabase = false,
                IsDebug = true,
            });
    }
}
