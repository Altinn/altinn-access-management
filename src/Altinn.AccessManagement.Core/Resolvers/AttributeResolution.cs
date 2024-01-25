namespace Altinn.AccessManagement.Core.Resolvers;

/// <summary>
/// summary
/// </summary>
/// <param name="needs">a</param>
/// <param name="resolves">b</param>
/// <param name="resolver">c</param>
public class AttributeResolution(IEnumerable<string> needs, IEnumerable<string> resolves, LeafResolver resolver)
{
    /// <summary>
    /// summary
    /// </summary>
    public IEnumerable<string> Needs { get; } = needs;

    /// <summary>
    /// summary
    /// </summary>
    public IEnumerable<string> Resolves { get; } = resolves;

    /// <summary>
    /// summary
    /// </summary>
    public LeafResolver Resolver { get; } = resolver;
}