using Altinn.AccessManagement.Core.Models;

namespace Altinn.AccessManagement.Core.Resolvers;

/// <summary>
/// resolver nice
/// </summary>
public interface IAttributeResolver
{
    /// <summary>
    /// resolver
    /// </summary>
    /// <param name="attributes">attributes</param>
    /// <param name="wants">wants</param>
    /// <param name="cancellationToken">c</param>
    /// <returns></returns>
    Task<IEnumerable<AttributeMatch>> Resolve(IEnumerable<AttributeMatch> attributes, IEnumerable<string> wants, CancellationToken cancellationToken);

    /// <summary>
    /// summary
    /// </summary>
    /// <param name="needs">a</param>
    /// <param name="resolves">b</param>
    /// <param name="resolver">c</param>
    IAttributeResolver AddLeaf(IEnumerable<string> needs, IEnumerable<string> resolves, LeafResolver resolver);
}