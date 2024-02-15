using System.Text.Json.Serialization;

namespace Altinn.AccessManagement.Models;

/// <summary>
/// Describes the delegation result for a given single right.
/// </summary>
public class RightDelegationExternal
{
    /// <summary>
    /// Specifies who have delegated permissions 
    /// </summary>
    [JsonPropertyName("from")]
    public List<AttributeMatchExternal> From { get; set; } = [];

    /// <summary>
    /// Receiver of the permissions
    /// </summary>
    [JsonPropertyName("to")]
    public List<AttributeMatchExternal> To { get; set; } = [];

    /// <summary>
    /// Specifies the permissions
    /// </summary>
    [JsonPropertyName("resource")]
    public List<AttributeMatchExternal> Resource { get; set; } = [];
}