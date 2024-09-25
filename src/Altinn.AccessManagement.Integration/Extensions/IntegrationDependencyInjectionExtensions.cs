using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Integration.Clients;
using Altinn.AccessManagement.Integration.Services;
using Altinn.AccessManagement.Integration.Services.Interfaces;
using Altinn.AccessManagement.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.AccessManagement.Integration.Extensions;

/// <summary>
/// Extension methods for adding access management services to the dependency injection container.
/// </summary>
public static class IntegrationDependencyInjectionExtensions
{
    /// <summary>
    /// Registers access management integration services with the dependency injection container.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/>.</param>
    /// <returns><paramref name="builder"/> for further chaining.</returns>
    public static WebApplicationBuilder AddAccessManagementIntegration(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IKeyVaultService, KeyVaultService>();
        builder.Services.AddSingleton<IPlatformAuthorizationTokenProvider, PlatformAuthorizationTokenProvider>();
        builder.Services.AddHttpClient<IDelegationRequestsWrapper, DelegationRequestProxy>();
        builder.Services.AddSingleton<IEventMapperService, EventMapperService>();

        builder.Services.AddHttpClient<IPartiesClient, PartiesClient>();
        builder.Services.AddHttpClient<IProfileClient, ProfileClient>();
        builder.Services.AddHttpClient<IAltinnRolesClient, AltinnRolesClient>();
        builder.Services.AddHttpClient<IAltinn2RightsClient, Altinn2RightsClient>();
        builder.Services.AddSingleton<IAuthenticationClient, AuthenticationClient>();
        builder.Services.AddSingleton<IResourceRegistryClient, ResourceRegistryClient>();

        return builder;
    }
}