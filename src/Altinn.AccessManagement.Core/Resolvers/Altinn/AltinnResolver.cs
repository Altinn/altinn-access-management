using Altinn.AccessManagement.Resolvers;

namespace Altinn.AccessManagement.Core.Resolvers;

/// <summary>
/// Resolves attributes for <see cref="BaseUrn.Altinn"/> 
/// </summary>
public class AltinnResolver(AltinnResourceResolver resource, PartyAttributeResolver partyAttributeResolver, UserAttributeResolver userAttributeResolver, AltinnPersonResolver personResolver, AltinnOrganizationResolver organizationResolver)
    : AttributeResolver(BaseUrn.Altinn.String(), resource, partyAttributeResolver, userAttributeResolver, personResolver, organizationResolver)
{
}