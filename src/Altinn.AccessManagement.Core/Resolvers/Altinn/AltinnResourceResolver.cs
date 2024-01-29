using Altinn.AccessManagement.Core.Resolvers.Extensions;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Npgsql;

namespace Altinn.AccessManagement.Core.Resolvers;

/// <summary>
/// test
/// </summary>
public class AltinnResourceResolver : AttributeResolver
{
    private readonly IContextRetrievalService _contextRetrievalService;

    /// <summary>
    /// summary
    /// </summary>
    /// <param name="contextRetrievalService">service init</param>
    public AltinnResourceResolver(IContextRetrievalService contextRetrievalService) : base(Urn.Altinn.Resource.String())
    {
        AddLeaf([Urn.Altinn.Resource.AppOwner, Urn.Altinn.Resource.AppId], [Urn.Altinn.Resource.Delegable, Urn.Altinn.Resource.Type, Urn.Altinn.Resource.ResourceRegistryId], ResolveAppOwnerAndAppId());
        AddLeaf([Urn.Altinn.Resource.ResourceRegistryId], [Urn.Altinn.Resource.Delegable, Urn.Altinn.Resource.Type], ResolveResourceRegistryId());
        _contextRetrievalService = contextRetrievalService;
    }

    /// <summary>
    /// Resolve social security number and lastname
    /// </summary>
    /// <returns></returns>
    public LeafResolver ResolveAppOwnerAndAppId() => async (attributes, cancellationToken) =>
    {
        var resource = await _contextRetrievalService.GetResourceFromResourceList(null, attributes.GetString(Urn.Altinn.Resource.AppOwner), attributes.GetString(Urn.Altinn.Resource.AppId), null, null);
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
    /// summary
    /// </summary>
    /// <returns></returns>
    public LeafResolver ResolveResourceRegistryId() => async (attributes, cancellationToken) =>
    {
        var resource = await _contextRetrievalService.GetResourceFromResourceList(attributes.GetString(Urn.Altinn.Resource.AppId), null, null, null, null);
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