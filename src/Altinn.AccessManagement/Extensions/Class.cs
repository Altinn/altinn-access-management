using Altinn.AccessManagement.Configuration;
using Altinn.AccessManagement.Core.Asserters;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Services;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Integration.Clients;
using Altinn.AccessManagement.Services;
using Altinn.Common.PEP.Clients;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Altinn.AccessManagement.Extensions;

public static class AccessManagementExtensions
{
    /// <summary>
    /// Configure HttpClients
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <returns>Extended IServiceCollection</returns>
    public static IServiceCollection ConfigureHttpClients(this IServiceCollection services)
    {
        if (!services.Any(t => t.ServiceType == typeof(AccessMgmtAppConfig)))
        {
            throw new Exception("Missing AccessMgmtAppConfig");
        }

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
    /// <param name="environment">ConfigEnvironment</param>
    public static IHostApplicationBuilder ConfigureAzureAppConfig(this IHostApplicationBuilder builder, ConfigEnvironment environment)
    {
        var appConfigUri = builder.Configuration.GetValue<Uri>("appConfigUri") ?? new Uri("https://auth-dev-shared-config.azconfig.io");
        var keyVaultUri = builder.Configuration.GetValue<Uri>("keyVaultUri") ?? new Uri("https://auth-dev-shared-kv.vault.azure.net");

        // ConfigurationBuilder configurationBuilder = new ConfigurationBuilder(); //Sjekk...
        Console.WriteLine(appConfigUri);
        Console.WriteLine(keyVaultUri);

        builder.Services.AddAzureAppConfiguration();

        var defaultCred = new DefaultAzureCredential();
        builder.Configuration.AddAzureAppConfiguration(c =>
        {
            c.Connect(appConfigUri, defaultCred)
                .Select(KeyFilter.Any, LabelFilter.Null)
                .Select(KeyFilter.Any, environment.ToString());
            c.UseFeatureFlags()
                .Select(KeyFilter.Any, LabelFilter.Null)
                .Select(KeyFilter.Any, environment.ToString());
            c.ConfigureRefresh(c => c.Register("Sentinel", true)); // Can add label
            c.ConfigureKeyVault(kv =>
            {
                kv.Register(new SecretClient(keyVaultUri, defaultCred));
            });
        });

        return builder;
    }
}
