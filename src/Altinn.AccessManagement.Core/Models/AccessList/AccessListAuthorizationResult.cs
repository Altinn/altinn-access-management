using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Altinn.AccessManagement.Core.Models.AccessList;

/// <summary>
/// Enum defining the different results of an access list authorization
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AccessListAuthorizationResult
{
    /// <summary>
    /// If PartyType is not Organization then access lists are not applicable
    /// </summary>
    [EnumMember(Value = "NotApplicable")]
    NotApplicable,

    /// <summary>
    /// Result is not yet determined
    /// </summary>
    [EnumMember(Value = "NotDetermined")]
    NotDetermined,

    /// <summary>
    /// Subject is not authorized to access the resource through any access lists
    /// </summary>
    [EnumMember(Value = "NotAuthorized")]
    NotAuthorized,

    /// <summary>
    /// Subject is authorized to access the resource through one or more access lists
    /// </summary>
    [EnumMember(Value = "Authorized")]
    Authorized
}