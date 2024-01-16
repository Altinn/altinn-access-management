namespace Altinn.AccessManagement.Resovers;

/// <summary>
/// AltinnResolver
/// </summary>
public class AltinnResolver : BaseResolver, IAttributeResolver
{
    /// <summary>
    /// AltinnResolver
    /// </summary>
    /// <param name="person">person</param>
    public AltinnResolver(AltinnPersonResolver person) : base("altinn", person)
    {
    }
}