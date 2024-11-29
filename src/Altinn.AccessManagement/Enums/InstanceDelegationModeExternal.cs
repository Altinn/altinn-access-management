using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using NpgsqlTypes;

namespace Altinn.AccessManagement.Enums;

/// <summary>
/// Enum defining the delegation mode for instance delegation Normal or PArallelSigning
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InstanceDelegationModeExternal
{
    /// <summary>
    /// Identifies a unit could also be identified with a Organization number
    /// </summary>
    [EnumMember(Value = "normal")]
    [PgName("normal")]
    Normal,
}