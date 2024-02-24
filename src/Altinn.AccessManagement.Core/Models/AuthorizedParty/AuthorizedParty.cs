using Altinn.Platform.Register.Enums;
using Altinn.Platform.Register.Models;

namespace Altinn.AccessManagement.Core.Models;

/// <summary>
/// Model representing an authorized party, meaning a party for which a user has been authorized for one or more rights (either directly or through role(s), rightspackage
/// Used in new implementation of what has previously been named ReporteeList in Altinn 2.
/// </summary>
public class AuthorizedParty
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizedParty"/> class.
    /// </summary>
    public AuthorizedParty()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizedParty"/> class based on a <see cref="Party"/> class.
    /// </summary>
    /// <param name="party">Party model from registry</param>
    /// <param name="includeChildParties">Whether model should also build list of child parties if any exists</param>
    public AuthorizedParty(Party party, bool includeChildParties = true)
    {
        PartyId = party.PartyId;
        PartyUuid = party.PartyUuid;
        PartyTypeName = party.PartyTypeName;
        OrgNumber = party.OrgNumber;
        SSN = party.SSN;
        UnitType = party.UnitType;
        Name = party.Name;
        IsDeleted = party.IsDeleted;
        OnlyHierarchyElementWithNoAccess = party.OnlyHierarchyElementWithNoAccess;
        ChildParties = includeChildParties ? party.ChildParties?.Select(child => new AuthorizedParty(child)).ToList() : null;
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
    public PartyType PartyTypeName { get; set; }

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
    /// Gets or sets a collection of all resource identifier the authorized actor has been authorized with some right for, on behalf of this party
    /// </summary>
    public List<string> AuthorizedResources { get; set; } = [];

    /// <summary>
    /// Gets or sets a collection of all rolecodes for roles from either Enhetsregisteret or Altinn 2 which the authorized actor has been authorized for, on behalf of this party
    /// </summary>
    public List<string> AuthorizedRoles { get; set; } = [];

    /// <summary>
    /// Gets or sets the value of ChildParties
    /// </summary>
    public List<AuthorizedParty> ChildParties { get; set; } = [];

    /// <summary>
    /// Enriches this authorized party and any child/subunits with the list of authorized resources
    /// </summary>
    /// <param name="resourceId">The list of resource IDs to add to the authorized party (and any subunits) list of authorized resources</param>
    public void EnrichWithResourceAccess(string resourceId)
    {
        OnlyHierarchyElementWithNoAccess = false;
        AuthorizedResources.Add(resourceId);

        if (ChildParties != null)
        {
            foreach (var subunit in ChildParties)
            {
                subunit.EnrichWithResourceAccess(resourceId);
            }
        }
    }
}