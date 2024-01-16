using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Services.Interfaces;

namespace Altinn.AccessManagement.Resovers;

/// <summary>
/// test
/// </summary>
public class AltinnPersonResolver : BaseResolver, IAttributeResolver
{
    private readonly IContextRetrievalService _contextRetrievalService;

    /// <summary>
    /// kake
    /// </summary>
    /// <param name="contextRetrievalService">service init</param>
    public AltinnPersonResolver(IContextRetrievalService contextRetrievalService) : base("person")
    {
        LeafResolvers.Add("partyid", ResolveInputPartyId());
        LeafResolvers.Add("identifier-no", ResolveInputSSN());
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

        AddAttribute(result, "lastname", party.Person.LastName, wants);
        AddAttribute(result, "firstname", party.Person.FirstName, wants);
        AddAttribute(result, "identifier-no", party.Person.SSN, wants);

        return result;
    };

    /// <summary>
    /// REsolve ssn
    /// </summary>
    /// <returns></returns>
    public LeafResolver ResolveInputSSN() => async (attributes, wants) =>
    {
        var result = new List<AttributeMatch>();
        var party = await _contextRetrievalService.GetPartyForPerson(attributes.First(p => p.Id == "identifier-no").Value);
        return result;
    };
}