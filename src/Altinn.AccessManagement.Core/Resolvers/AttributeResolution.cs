namespace Altinn.AccessManagement.Core.Resolvers;

/// <summary>
/// A data container that encapsulates the needed parameters in order to run a resolver.
/// </summary>
public class AttributeResolution(IEnumerable<string> needs, IEnumerable<string> resolves, LeafResolver resolver)
{
    /// <summary>
    /// The required attributes to be present in order for the attribute to run.
    /// </summary>
    public IEnumerable<string> Needs { get; } = needs;

    /// <summary>
    /// The attributes the resolver are able to fetch if provided it needs.
    /// </summary>
    public IEnumerable<string> Resolves { get; } = resolves;

    /// <summary>
    /// A function reference which when called upon does the work.
    /// </summary>
    public LeafResolver Resolver { get; } = resolver;
}