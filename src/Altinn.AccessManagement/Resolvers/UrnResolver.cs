using Altinn.AccessManagement.Core.Models;

namespace Altinn.AccessManagement.Resovers;

/// <summary>
/// this is a resolver
/// </summary>
public class UrnResolver(AltinnResolver altinn) : BaseResolver("urn", altinn), IAttributeResolver 
{
}