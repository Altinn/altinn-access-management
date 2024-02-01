using Altinn.AccessManagement.Core.Resolvers.Extensions;
using Altinn.AccessManagement.Core.Services.Interfaces;

namespace Altinn.AccessManagement.Core.Resolvers;

/// <summary>
/// Resolves attributes for <see cref="Urn.Altinn.Resource"/> 
/// </summary>
public class AltinnResourceResolver : AttributeResolver
{
    private readonly IContextRetrievalService _contextRetrievalService;

    /// <summary>
    /// ctor
    /// </summary>
    public AltinnResourceResolver(IContextRetrievalService contextRetrievalService) : base(Urn.Altinn.Resource.String())
    {
        AddLeaf([Urn.Altinn.Resource.AppOwner, Urn.Altinn.Resource.AppId], [Urn.Altinn.Resource.Delegable, Urn.Altinn.Resource.Type, Urn.Altinn.Resource.ResourceRegistryId], ResolveAppOwnerAndAppId());
        AddLeaf([Urn.Altinn.Resource.ResourceRegistryId], [Urn.Altinn.Resource.Delegable, Urn.Altinn.Resource.Type], ResolveResourceRegistryId());
        _contextRetrievalService = contextRetrievalService;
    }

    /// <summary>
    /// Resolves a resource if given <see cref="Urn.Altinn.Resource.AppOwner"/> and <see cref="Urn.Altinn.Resource.AppId"/>
    /// </summary>
    public LeafResolver ResolveAppOwnerAndAppId() => async (attributes, cancellationToken) =>
    {
        var resource = await _contextRetrievalService.GetResourceFromResourceList(null, attributes.GetRequiredString(Urn.Altinn.Resource.AppOwner), attributes.GetRequiredString(Urn.Altinn.Resource.AppId), null, null);
        if (resource != null)
        {
            return
            [
                new(Urn.Altinn.Resource.Delegable, resource.Delegable),
                new(Urn.Altinn.Resource.Type, resource.ResourceType),
                new(Urn.Altinn.Resource.ResourceRegistryId, resource.Identifier),
            ];
        }

        return [];
    };

    /// <summary>
    /// /// Resolves a resource if given <see cref="Urn.Altinn.Resource.ResourceRegistryId"/>
    /// </summary>
    public LeafResolver ResolveResourceRegistryId() => async (attributes, cancellationToken) =>
    {
        var resource = await _contextRetrievalService.GetResourceFromResourceList(attributes.GetRequiredString(Urn.Altinn.Resource.ResourceRegistryId), null, null, null, null);
        if (resource != null)
        {
            return
            [
                new(Urn.Altinn.Resource.Delegable, resource.Delegable),
                new(Urn.Altinn.Resource.Type, resource.ResourceType),
            ];
        }

        return [];
    };
}