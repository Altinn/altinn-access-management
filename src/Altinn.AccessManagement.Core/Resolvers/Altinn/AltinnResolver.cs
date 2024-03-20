using Altinn.AccessManagement.Resolvers;

namespace Altinn.AccessManagement.Core.Resolvers;

/// <summary>
/// Resolves attributes for <see cref="Urn.Altinn"/> 
/// </summary>
public class AltinnResolver(AltinnResourceResolver resource, PartyAttributeResolver partyAttributeResolver, UserAttributeResolver userAttributeResolver, AltinnPersonResolver personResolver, AltinnOrganizationResolver organizationResolver)
    : AttributeResolver(Urn.Altinn.String(), resource, partyAttributeResolver, userAttributeResolver, personResolver, organizationResolver)
{
}