using Altinn.AccessManagement.Core.Resolvers.Extensions;
using Altinn.AccessManagement.Core.Services.Interfaces;

namespace Altinn.AccessManagement.Core.Resolvers;

/// <summary>
/// Resolves attributes for <see cref="BaseUrn.Altinn.Resource"/> 
/// </summary>
public class AltinnResourceResolver : AttributeResolver
{
    private readonly IContextRetrievalService _contextRetrievalService;

    /// <summary>
    /// ctor
    /// </summary>
    public AltinnResourceResolver(IContextRetrievalService contextRetrievalService) : base(BaseUrn.Altinn.Resource.String())
    {
        AddLeaf([BaseUrn.Altinn.Resource.AppOwner, BaseUrn.Altinn.Resource.AppId], [BaseUrn.Altinn.Resource.Delegable, BaseUrn.Altinn.Resource.Type, BaseUrn.Altinn.Resource.ResourceRegistryId], ResolveAppOwnerAndAppId());
        AddLeaf([BaseUrn.Altinn.Resource.ResourceRegistryId], [BaseUrn.Altinn.Resource.Delegable, BaseUrn.Altinn.Resource.Type], ResolveResourceRegistryId());
        _contextRetrievalService = contextRetrievalService;
    }

    /// <summary>
    /// Resolves a resource if given <see cref="BaseUrn.Altinn.Resource.AppOwner"/> and <see cref="BaseUrn.Altinn.Resource.AppId"/>
    /// </summary>
    public LeafResolver ResolveAppOwnerAndAppId() => async (attributes, cancellationToken) =>
    {
        var resource = await _contextRetrievalService.GetResourceFromResourceList(null, attributes.GetRequiredString(BaseUrn.Altinn.Resource.AppOwner), attributes.GetRequiredString(BaseUrn.Altinn.Resource.AppId), null, null);
        if (resource != null)
        {
            return
            [
                new(BaseUrn.Altinn.Resource.Delegable, resource.Delegable),
                new(BaseUrn.Altinn.Resource.Type, resource.ResourceType),
                new(BaseUrn.Altinn.Resource.ResourceRegistryId, resource.Identifier),
            ];
        }

        return [];
    };

    /// <summary>
    /// /// Resolves a resource if given <see cref="BaseUrn.Altinn.Resource.ResourceRegistryId"/>
    /// </summary>
    public LeafResolver ResolveResourceRegistryId() => async (attributes, cancellationToken) =>
    {
        var resource = await _contextRetrievalService.GetResourceFromResourceList(attributes.GetRequiredString(BaseUrn.Altinn.Resource.ResourceRegistryId), null, null, null, null);
        if (resource != null)
        {
            return
            [
                new(BaseUrn.Altinn.Resource.Delegable, resource.Delegable),
                new(BaseUrn.Altinn.Resource.Type, resource.ResourceType),
            ];
        }

        return [];
    };
}