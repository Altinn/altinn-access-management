namespace Altinn.AccessManagement.Models;

public record RightDelegationOfferedExternal
{
    /// <summary>
    /// Specifies who have delegated permissions 
    /// </summary>
    public List<AttributeMatchExternal> From { get; set; }

    /// <summary>
    /// Receiver of the permissions
    /// </summary>
    public List<AttributeMatchExternal> To { get; set; }

    /// <summary>
    /// Specifies the permissions
    /// </summary>
    public List<AttributeMatchExternal> Resources { get; set; }
}