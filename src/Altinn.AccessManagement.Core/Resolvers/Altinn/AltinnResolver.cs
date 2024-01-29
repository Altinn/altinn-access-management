using Altinn.AccessManagement.Resolvers;

namespace Altinn.AccessManagement.Core.Resolvers;

/// <summary>
/// summary
/// </summary>
/// <param name="person">person</param>
/// <param name="organization">organization</param>
/// <param name="resource">resource</param>
/// <param name="enterpriseUserResolver">enterpriseuser</param>
public class AltinnResolver(AltinnPersonResolver person, AltinnOrganizationResolver organization, AltinnResourceResolver resource, AltinnEnterpriseUserResolver enterpriseUserResolver)
    : AttributeResolver(Urn.Altinn.String(), person, organization, resource, enterpriseUserResolver), IAttributeResolver
{
}