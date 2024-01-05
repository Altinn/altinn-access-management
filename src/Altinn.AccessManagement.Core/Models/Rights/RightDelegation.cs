namespace Altinn.AccessManagement.Core.Models;

public record RightDelegation
{
    /// <summary>
    /// Specifies who have delegated permissions 
    /// </summary>
    public List<AttributeMatch> From { get; set; } = [];

    /// <summary>
    /// Receiver of the permissions
    /// </summary>
    public List<AttributeMatch> To { get; set; } = [];

    /// <summary>
    /// Specifies the permissions
    /// </summary>
    public List<AttributeMatch> Resource { get; set; } = [];
}