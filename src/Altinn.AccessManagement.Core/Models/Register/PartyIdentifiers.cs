#nullable enable

using System;

namespace Altinn.AccessManagement.Core.Models.Register;

/// <summary>
/// A set of identifiers for a party.
/// </summary>
public record PartyIdentifiers
{
    /// <summary>
    /// The party id.
    /// </summary>
    public required int PartyId { get; init; }

    /// <summary>
    /// The party uuid.
    /// </summary>
    public required Guid PartyUuid { get; init; }

    /// <summary>
    /// The organization number of the party (if applicable).
    /// </summary>
    public required string? OrgNumber { get; init; }

    /// <summary>
    /// The social security number of the party (if applicable and included).
    /// </summary>
    public string? SSN { get; init; }
}
