using Altinn.AccessManagement.Core.Asserters;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Resolvers;
using Altinn.AccessManagement.Core.Services;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Resolvers;
using Altinn.Common.AccessTokenClient.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.AccessManagement.Core.Extensions;

/// <summary>
/// Extension methods for adding access management core services to the dependency injection container.
/// </summary>
public static class CoreDependencyInjectionExtensions
{
    /// <summary>
    /// Extension methods for adding access management core services to the dependency injection container.
    /// </summary>
    /// <param name="builder">web application builder</param>
    public static void AddAccessManagementCore(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<IDelegationRequests, DelegationRequestService>();
        builder.Services.AddTransient<IAssert<AttributeMatch>, Asserter<AttributeMatch>>();
        builder.Services.AddTransient<IAttributeResolver, UrnResolver>();

        builder.Services.AddTransient<UrnResolver>();
        builder.Services.AddTransient<AltinnEnterpriseUserResolver>();
        builder.Services.AddTransient<AltinnResolver>();
        builder.Services.AddTransient<AltinnResourceResolver>();
        builder.Services.AddTransient<AltinnOrganizationResolver>();
        builder.Services.AddTransient<AltinnPersonResolver>();
        builder.Services.AddTransient<PartyAttributeResolver>();
        builder.Services.AddTransient<UserAttributeResolver>();

        builder.Services.AddSingleton<IPolicyRetrievalPoint, PolicyRetrievalPoint>();
        builder.Services.AddSingleton<IPolicyInformationPoint, PolicyInformationPoint>();
        builder.Services.AddSingleton<IPolicyAdministrationPoint, PolicyAdministrationPoint>();
        builder.Services.AddSingleton<IResourceAdministrationPoint, ResourceAdministrationPoint>();

        builder.Services.AddSingleton<IResourceAdministrationPoint, ResourceAdministrationPoint>();
        builder.Services.AddSingleton<IContextRetrievalService, ContextRetrievalService>();
        builder.Services.AddSingleton<IMaskinportenSchemaService, MaskinportenSchemaService>();

        builder.Services.AddSingleton<IAccessTokenGenerator, AccessTokenGenerator>();
        builder.Services.AddTransient<ISigningCredentialsResolver, SigningCredentialsResolver>();

        builder.Services.AddSingleton<ISingleRightsService, SingleRightsService>();
        builder.Services.AddSingleton<IUserProfileLookupService, UserProfileLookupService>();
        builder.Services.AddSingleton<IAuthorizedPartiesService, AuthorizedPartiesService>();
        builder.Services.AddSingleton<IAltinn2RightsService, Altinn2RightsService>();
    }
}