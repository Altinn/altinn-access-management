using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using NpgsqlTypes;

namespace Altinn.AccessManagement.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InstanceDelegationModeExternal
{
    /// <summary>
    /// Defining a instance delegation to be of type parallell task person this could also be identified with "Fødselsnummer"/"Dnummer"
    /// </summary>
    [EnumMember(Value = "parallelSigning")]
    [PgName("parallelsigning")]
    ParallelSigning,

    /// <summary>
    /// Identifies a unit could also be identified with a Organization number
    /// </summary>
    [EnumMember(Value = "normal")]
    [PgName("normal")]
    Normal
}