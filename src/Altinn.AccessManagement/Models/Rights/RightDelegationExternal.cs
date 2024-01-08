namespace Altinn.AccessManagement.Models;

/// <summary>
/// Describes the delegation result for a given single right.
/// </summary>
public record RightDelegationExternal
{
    /// <summary>
    /// Specifies who have delegated permissions 
    /// </summary>
    public List<AttributeMatchExternal> From { get; set; } = [];

    /// <summary>
    /// Receiver of the permissions
    /// </summary>
    public List<AttributeMatchExternal> To { get; set; } = [];

    /// <summary>
    /// Specifies the permissions
    /// </summary>
    public List<AttributeMatchExternal> Resource { get; set; } = [];
}