namespace Altinn.AccessManagement.Core.Resolvers;

/// <summary>
/// Resolves attributes for <see cref="Urn"/> 
/// </summary>
public class UrnResolver(AltinnResolver altinn) : AttributeResolver(Urn.String(), altinn)
{
}