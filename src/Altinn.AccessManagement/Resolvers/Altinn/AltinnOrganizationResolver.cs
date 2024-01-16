using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Services.Interfaces;

namespace Altinn.AccessManagement.Resovers;

/// <summary>
/// test
/// </summary>
public class AltinnOrganizationResolver : BaseResolver, IAttributeResolver
{
    private readonly IContextRetrievalService _contextRetrievalService;

    /// <summary>
    /// kake
    /// </summary>
    /// <param name="contextRetrievalService">service init</param>
    public AltinnOrganizationResolver(IContextRetrievalService contextRetrievalService) : base("person")
    {
        LeafResolvers.Add("partyid", ResolveInputPartyId());
        LeafResolvers.Add("organizationnumber", ResolveOrganizationNumber());
        _contextRetrievalService = contextRetrievalService;
    }

    /// <summary>
    /// aa
    /// </summary>
    /// <returns></returns>
    public LeafResolver ResolveInputPartyId() => async (attributes, wants) => 
    {
        var result = new List<AttributeMatch>();
        var attribute = attributes.First(attribute => attribute.Id == "partyid");
        var party = await _contextRetrievalService.GetPartyAsync(int.Parse(attribute.Value));

        AddAttribute(result, "name", party.Organization.Name, wants);

        return result;
    };

    /// <summary>
    /// Resolve SSN
    /// </summary>
    /// <returns></returns>
    public LeafResolver ResolveOrganizationNumber() => async (attributes, wants) =>
    {
        var result = new List<AttributeMatch>();
        var party = await _contextRetrievalService.GetPartyForOrganization(attributes.First(p => p.Id == "identifier-no").Value);

        AddAttribute(result, "partyid", party.PartyId.ToString(), wants);
        return result;
    };
}