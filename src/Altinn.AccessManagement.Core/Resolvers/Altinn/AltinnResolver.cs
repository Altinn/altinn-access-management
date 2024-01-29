using Altinn.AccessManagement.Resolvers;

namespace Altinn.AccessManagement.Core.Resolvers;

/// <summary>
/// Resolves attributes for <see cref="Urn.Altinn"/> 
/// </summary>
public class AltinnResolver(AltinnPersonResolver person, AltinnOrganizationResolver organization, AltinnResourceResolver resource, AltinnEnterpriseUserResolver enterpriseUserResolver)
    : AttributeResolver(Urn.Altinn.String(), person, organization, resource, enterpriseUserResolver), IAttributeResolver
{
}