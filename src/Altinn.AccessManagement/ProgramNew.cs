using Altinn.AccessManagement.Configuration;
using Altinn.Authorization.ABAC.Utils;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.Security.Policy;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using Azure.Monitor.OpenTelemetry.AspNetCore;

namespace Altinn.AccessManagement;

public class ProgramNew
{
    void Start(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddEnvironmentVariables();

        var appConfigUri = builder.Configuration.GetValue<Uri>("appConfigUri"); // new Uri("https://auth-dev-shared-config.azconfig.io");
        var keyVaultUri = builder.Configuration.GetValue<Uri>("keyVaultUri"); // new Uri("https://auth-dev-shared-kv.vault.azure.net");

        Console.WriteLine(appConfigUri);
        Console.WriteLine(keyVaultUri);

        var defaultCred = new DefaultAzureCredential();
        builder.Configuration.AddAzureAppConfiguration(c =>
        {
            c.Connect(appConfigUri, defaultCred);
            c.UseFeatureFlags();
            c.ConfigureKeyVault(kv =>
            {
                kv.Register(new SecretClient(keyVaultUri, defaultCred));
            });
        });

        var config = builder.Configuration.GetRequiredSection("AccessMgmt").Get<AppConfig>();

        var isLocalDevelopment = builder.Environment.IsDevelopment() && config.IsDev; // builder.Configuration.GetValue<bool>("Altinn:LocalDev"); // IsDev

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddDetector(services => services.GetRequiredService<AltinnServiceResourceDetector>());
            })
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation().AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    tracing.SetSampler(new AlwaysOnSampler());
                }
                tracing.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation();
            });

        // builder.AddOpenTelemetryExporters();

        builder.Services.AddOpenTelemetry().UseAzureMonitor();



        //Configure LoggerFactory => Filter: Microsoft:Warning, System: Warning, Altinn.AccessManagement.Program: Debug
        // NpgsqlLoggingConfiguration.InitializeLogging

        //Setup ConfigProviders
        // altinn-appsettings/altinn-dbsettings-secret.json
        // AddEnvironmentVariables
        // AddCommandLine(args)
        // ConnectToKeyVaultAndSetApplicationInsights

        //ConnectToKeyVaultAndSetApplicationInsights
        // keyVaultSettings = config.GetSection("kvSetting")
        // Set applicationInsightsConnectionString
        // config.AddAzureKeyVault

        //ConfigureLogging => applicationInsightsConnectionString | AddConsole

        //-----------------

        //ConfigureTelemetry

        //ConfigureAsserters => Andreas
        /*
        services.AddTransient<IAssert<AttributeMatch>, Asserter<AttributeMatch>>();
        services.AddAuthorization();
         */

        //ConfigureResolvers => JK
        /*
        services.AddTransient<IAttributeResolver, UrnResolver>();
        services.AddTransient<UrnResolver>();
        services.AddTransient<AltinnEnterpriseUserResolver>();
        services.AddTransient<AltinnResolver>();
        services.AddTransient<AltinnResourceResolver>();
        services.AddTransient<AltinnOrganizationResolver>();
        services.AddTransient<AltinnPersonResolver>();
        services.AddTransient<PartyAttributeResolver>();
        services.AddTransient<UserAttributeResolver>();
         */

        //AddAutoMapper
        //AddControllersWithViews
        //builder.Services.AddEndpointsApiExplorer();
        //builder.Services.AddFeatureManagement();
        //services.AddHealthChecks()
        //services.AddSwaggerGen
        //services.AddMvc()

        //------------------------------

        //SETTINGS!
        /*
         
        PlatformSettings platformSettings = config.GetSection("PlatformSettings").Get<PlatformSettings>();
        OidcProviderSettings oidcProviders = config.GetSection("OidcProviders").Get<OidcProviderSettings>();
        services.Configure<GeneralSettings>(config.GetSection("GeneralSettings"));
        services.Configure<PlatformSettings>(config.GetSection("PlatformSettings"));
        services.Configure<Altinn.Common.PEP.Configuration.PlatformSettings>(config.GetSection("PlatformSettings"));
        services.Configure<CacheConfig>(config.GetSection("CacheConfig"));
        services.Configure<PostgreSQLSettings>(config.GetSection("PostgreSQLSettings"));
        services.Configure<AzureStorageConfiguration>(config.GetSection("AzureStorageConfiguration"));
        services.Configure<SblBridgeSettings>(config.GetSection("SblBridgeSettings"));
        services.Configure<Altinn.Common.AccessToken.Configuration.KeyVaultSettings>(config.GetSection("kvSetting"));
        services.Configure<KeyVaultSettings>(config.GetSection("kvSetting"));
        services.Configure<OidcProviderSettings>(config.GetSection("OidcProviders"));
        services.Configure<UserProfileLookupSettings>(config.GetSection("UserProfileLookupSettings"));
         
        Altinn.AccessManagement.Core.Constants.AuthzConstants => CONVERT TO CONFIG ?

         */

    }
}

/// <summary>
/// Resource detector for OpenTelemetry
/// </summary>
internal class AltinnServiceResourceDetector : IResourceDetector
{
    private readonly Resource _resource; // public AltinnServiceResourceDetector(AltinnServiceDescriptor serviceDescription)

    /// <summary>
    /// Default constructor for AltinnServiceResourceDetector
    /// </summary>
    /// <param name="name">Name of the Service</param>
    public AltinnServiceResourceDetector(string name)
    {
        var attributes = new List<KeyValuePair<string, object>>(1)
        {
            KeyValuePair.Create("service.name", (object)name),
        };

        // ??.environment .. add? (at21,tt02..etc)
        
        /*
        if (serviceDescription.IsLocalDev)
        {
            attributes.Add(KeyValuePair.Create("altinn.local_dev", (object)true));
        }
        */

        _resource = new Resource(attributes);
    }

    /// <summary>
    /// Detects implementation of Resource
    /// </summary>
    /// <returns></returns>
    public Resource Detect() => _resource;
}