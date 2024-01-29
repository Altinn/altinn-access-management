namespace Altinn.AccessManagement.Core.Resolvers;

/// <summary>
/// this is a resolver
/// </summary>
public class UrnResolver(AltinnResolver altinn) : AttributeResolver(Urn.ToString(), altinn)
{
}