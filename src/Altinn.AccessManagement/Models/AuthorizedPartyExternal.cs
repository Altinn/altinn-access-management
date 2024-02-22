using System.Text.Json.Serialization;
using Altinn.AccessManagement.Enums;

namespace Altinn.AccessManagement.Core.Models;

/// <summary>
/// Model representing an authorized party, meaning a party for which a user has been authorized for one or more rights (either directly or through role(s), rightspackage
/// Used in new implementation of what has previously been named ReporteeList in Altinn 2.
/// </summary>
public class AuthorizedPartyExternal
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizedPartyExternal"/> class.
    /// </summary>
    public AuthorizedPartyExternal()
    {
    }

    /// <summary>
    /// Gets or sets the ID of the party
    /// </summary>
    public int PartyId { get; set; }

    /// <summary>
    /// Gets or sets the UUID of the party
    /// </summary>
    public Guid? PartyUuid { get; set; }

    /// <summary>
    /// Gets or sets the type of party
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PartyTypeExternal PartyType { get; set; }

    /// <summary>
    /// Gets the parties org number
    /// </summary>
    public string OrgNumber { get; set; }

    /// <summary>
    /// Gets the parties ssn of the party is a person
    /// </summary>
    public string SSN { get; set; }

    /// <summary>
    /// Gets or sets the UnitType if the party is an organization
    /// </summary>
    public string UnitType { get; set; }

    /// <summary>
    /// Gets or sets the name of the party
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets whether this party is marked as deleted in Enhetsregisteret
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the party is only for showing the hierarchy (a parent unit with no access)
    /// </summary>
    public bool OnlyHierarchyElementWithNoAccess { get; set; }

    /// <summary>
    /// Gets or sets the value of ChildParties
    /// </summary>
    public List<AuthorizedPartyExternal> ChildParties { get; set; } = [];

    /// <summary>
    /// Gets or sets a collection of all resource identifier the authorized actor has been authorized with some right for, on behalf of this party
    /// </summary>
    public List<string> AuthorizedResources { get; set; } = [];

    /// <summary>
    /// Gets or sets a collection of all rolecodes for roles from either Enhetsregisteret or Altinn 2 which the authorized actor has been authorized for, on behalf of this party
    /// </summary>
    public List<string> AuthorizedRoles { get; set; } = [];
}