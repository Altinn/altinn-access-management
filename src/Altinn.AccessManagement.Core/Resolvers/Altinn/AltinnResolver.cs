using Altinn.AccessManagement.Resolvers;

namespace Altinn.AccessManagement.Core.Resolvers;

/// <summary>
/// summary
/// </summary>
/// <param name="person">person</param>
/// <param name="organization">organization</param>
public class AltinnResolver(AltinnPersonResolver person, AltinnOrganizationResolver organization)
    : AttributeResolver(Urn.Altinn.ToString(), person, organization), IAttributeResolver
{
}