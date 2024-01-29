using Altinn.AccessManagement.Core.Models;

namespace Altinn.AccessManagement.Core.Resolvers;

/// <summary>
/// Resolves attributes.
/// </summary>
public interface IAttributeResolver
{
    /// <summary>
    /// Allows the caller to fetch new requested attributes specified by the wants parameter.
    /// Given attributes should contain values that allow the user to resolve/fetch wanted attributes.
    /// </summary>
    /// <param name="attributes">Current attributes.</param>
    /// <param name="wants">Attributes wanted by the callee.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>New list of attributes containing given attributes and wanted attributes.</returns>
    Task<IEnumerable<AttributeMatch>> Resolve(IEnumerable<AttributeMatch> attributes, IEnumerable<string> wants, CancellationToken cancellationToken);
}