#nullable enable
namespace Altinn.AccessManagement.Core.Models.ResourceRegistry;

/// <summary>
/// Defines resources that a given subject have access to
/// </summary>
public class SubjectResources
{
    /// <summary>
    /// The subject
    /// </summary>
    public required BaseAttribute Subject { get; set; }    

    /// <summary>
    /// List of resources that the given subject has access to
    /// </summary>
    public required IEnumerable<BaseAttribute> Resources { get; set; }
}
