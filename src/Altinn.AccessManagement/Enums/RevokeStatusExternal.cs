using System.Text.Json.Serialization;

namespace Altinn.AccessManagement.Enums;

/// <summary>
/// Enum for different right revoke status responses
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RevokeStatusExternal
{
    /// <summary>
    /// Right was not revoked
    /// </summary>
    NotRevoked = 0,

    /// <summary>
    /// Right was revoked
    /// </summary>
    Revoked = 1
}
