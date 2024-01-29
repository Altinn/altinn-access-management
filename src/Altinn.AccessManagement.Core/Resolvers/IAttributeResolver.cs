using Altinn.AccessManagement.Core.Models;

namespace Altinn.AccessManagement.Core.Resolvers;

/// <summary>
/// Resolves attributes
/// </summary>
public interface IAttributeResolver
{
    /// <summary>
    /// Allows the caller to fetch new requested attributes that are specified by the wants paramaters.
    /// Given attributes should contains values that allows the user to resolve/fetch wanted attributes.
    /// </summary>
    /// <param name="attributes">Current attributes</param>
    /// <param name="wants">Attributes that are wanted by the callee</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New list of attributes containing given attributes and wanted attributes</returns>
    Task<IEnumerable<AttributeMatch>> Resolve(IEnumerable<AttributeMatch> attributes, IEnumerable<string> wants, CancellationToken cancellationToken);
}