namespace Altinn.AccessManagement.Core.Models;

/// <summary>
/// Conteiner for parameters used to build policy files and rules
/// </summary>
public class PolicyParameters
{
    /// <summary>
    /// ResourceId as a string
    /// </summary>
    public string ResourceId { get; set; }

    /// <summary>
    /// Instance id as a string
    /// </summary>
    public string InstanceId { get; set; }

    /// <summary>
    /// From type as urn string
    /// </summary>
    public string FromType { get; set; }

    /// <summary>
    /// from uuid as string
    /// </summary>
    public string FromId { get; set; }

    /// <summary>
    /// to type urn string
    /// </summary>
    public string ToType { get; set; }

    /// <summary>
    /// to uuid as string
    /// </summary>
    public string ToId { get; set; }

    /// <summary>
    /// performed by uuid as string
    /// </summary>
    public string PerformedById { get; set; }

    /// <summary>
    /// performed by type urn string
    /// </summary>
    public string PerformedByType { get; set; }
}
