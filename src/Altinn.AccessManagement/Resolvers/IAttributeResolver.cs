using Altinn.AccessManagement.Core.Models;

namespace Altinn.AccessManagement.Resovers;

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
    /// <returns></returns>
    Task<IEnumerable<AttributeMatch>> Resolve(IEnumerable<AttributeMatch> attributes, IEnumerable<string> wants);

    /// <summary>
    /// ResourceName
    /// </summary>
    public string ResourceName { get; }
}