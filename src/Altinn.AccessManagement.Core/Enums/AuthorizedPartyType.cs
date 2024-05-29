using System.Text.Json.Serialization;

namespace Altinn.AccessManagement.Core.Enums;

/// <summary>
/// Enum for different types of Authorized Party
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AuthorizedPartyType
{
    /// <summary>
    /// Unknown or unspecified
    /// </summary>
    None = 0,

    /// <summary>
    /// Party Type is a Person
    /// </summary>
    Person = 1,

    /// <summary>
    /// Party Type is an Organization
    /// </summary>
    Organization = 2,

    /// <summary>
    /// Party Type is a Self Identified user
    /// </summary>
    SelfIdentified = 3
}
