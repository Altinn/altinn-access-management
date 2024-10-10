namespace Altinn.AccessManagement.Core.Resolvers;

/// <summary>
/// Resolves attributes for <see cref="BaseUrn"/> 
/// </summary>
public class UrnResolver(AltinnResolver altinn) : AttributeResolver(BaseUrn.String(), altinn)
{
}