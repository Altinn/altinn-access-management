using System;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Resolvers;
using Altinn.AccessManagement.Core.Resolvers.Extensions;
using Altinn.AccessManagement.Core.Services;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Resolvers;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.Common.AccessToken.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.AccessManagement.Tests.Resolvers;

/// <summary>
/// ResolverServiceCollection
/// </summary>
public static class ResolverServiceCollection
{
    /// <summary>
    /// ConfigureServices
    /// </summary>
    /// <param name="actions">actions</param>
    public static IAttributeResolver ConfigureServices(params Action<IServiceCollection>[] actions)
    {
        var services = new ServiceCollection();
        foreach (var action in actions)
        {
            action(services);
        }

        return services.BuildServiceProvider().GetService<IAttributeResolver>();
    }

    /// <summary>
    /// DefaultServiceCollection
    /// </summary>
    public static void DefaultServiceCollection(IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddTransient<IDelegationRequests, DelegationRequestService>();
        services.AddTransient<IAttributeResolver, UrnResolver>();

        services.AddTransient<UrnResolver>();
        services.AddTransient<AltinnEnterpriseUserResolver>();
        services.AddTransient<AltinnResolver>();
        services.AddTransient<AltinnResourceResolver>();
        services.AddTransient<AltinnOrganizationResolver>();
        services.AddTransient<AltinnPersonResolver>();
        services.AddTransient<PartyAttributeResolver>();
        services.AddTransient<UserAttributeResolver>();

        services.Configure<CacheConfig>(options =>
        {
            options.PartyCacheTimeout = 5;
        });
        services.AddSingleton<IContextRetrievalService, ContextRetrievalService>();
        services.AddSingleton<IPolicyRetrievalPoint, PolicyRetrievalPointMock>();
        services.AddSingleton<IDelegationMetadataRepository, DelegationMetadataRepositoryMock>();
        services.AddSingleton<IPolicyFactory, PolicyFactoryMock>();
        services.AddSingleton<IPublicSigningKeyProvider, SigningKeyResolverMock>();
        services.AddSingleton<IPartiesClient, PartiesClientMock>();
        services.AddSingleton<IProfileClient, ProfileClientMock>();
        services.AddSingleton<IResourceRegistryClient, ResourceRegistryClientMock>();
        services.AddSingleton<IAltinnRolesClient, AltinnRolesClientMock>();
        services.AddSingleton<IAltinn2RightsClient, Altinn2RightsClientMock>();
        services.AddSingleton<IProfileClient, ProfileClientMock>();
        services.AddSingleton<IAuthenticationClient, AuthenticationMock>();
    }
}
