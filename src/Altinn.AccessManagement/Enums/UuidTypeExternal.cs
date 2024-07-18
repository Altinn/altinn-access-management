using System.Runtime.Serialization;
using NpgsqlTypes;

namespace Altinn.AccessManagement.Enums;

/// <summary>
/// Enum defining the different uuids used for defining parts in a delegation
/// </summary>
public enum UuidTypeExternal
{
    /// <summary>
    /// Placeholder when type is not specified should only happen when there is no Uuid to match it with
    /// </summary>
    [EnumMember]
    NotSpecified,

    /// <summary>
    /// Defining a person this could also be identified with "Fødselsnummer"/"Dnummer"
    /// </summary>
    [EnumMember(Value = "urn:altinn:person:uuid")]
    [PgName("urn:altinn:person:uuid")]
    Person,

    /// <summary>
    /// Identifies a unit could also be identified with a Organization number
    /// </summary>
    [EnumMember(Value = "urn:altinn:organization:uuid")]
    [PgName("urn:altinn:organization:uuid")]
    Organization,

    /// <summary>
    /// Identifies a systemuser this is a identifier for machine integration it could also be identified with a unique name
    /// </summary>
    [EnumMember(Value = "urn:altinn:systemuser:uuid")]
    [PgName("urn:altinn:systemuser:uuid")]
    SystemUser,

    /// <summary>
    /// Identifies a enterpriseuser this is marked as obsolete and is used for existing integration is also identified with an unique username
    /// </summary>
    [EnumMember(Value = "urn:altinn:enterpriseuser:uuid")]
    [PgName("urn:altinn:enterpriseuser:uuid")]
    EnterpriseUser
}