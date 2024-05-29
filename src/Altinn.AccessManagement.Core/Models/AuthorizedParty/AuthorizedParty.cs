using Altinn.AccessManagement.Core.Enums;
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
    /// <param name="includeSubunits">Whether model should also build list of subunits if any exists</param>
    public AuthorizedParty(Party party, bool includeSubunits = true)
    {
        PartyId = party.PartyId;
        PartyUuid = party.PartyUuid.Value;
        Name = party.Name;
        Type = (AuthorizedPartyType)party.PartyTypeName;

        if (Type == AuthorizedPartyType.Organization)
        {
            OrganizationNumber = party.OrgNumber;
            UnitType = party.UnitType;
            IsDeleted = party.IsDeleted;
            OnlyHierarchyElementWithNoAccess = party.OnlyHierarchyElementWithNoAccess;
            Subunits = includeSubunits ? party.ChildParties?.Select(subunit => new AuthorizedParty(subunit)).ToList() ?? [] : [];
        }
        else if (Type == AuthorizedPartyType.Person)
        {
            PersonId = party.SSN;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizedParty"/> class based on a <see cref="SblAuthorizedParty"/> class.
    /// </summary>
    /// <param name="sblAuthorizedParty">Authorized Party model from Altinn 2 SBL Bridge</param>
    /// <param name="includeSubunits">Whether model should also build list of subunits if any exists</param>
    public AuthorizedParty(SblAuthorizedParty sblAuthorizedParty, bool includeSubunits = true)
    {
        PartyId = sblAuthorizedParty.PartyId;
        PartyUuid = sblAuthorizedParty.PartyUuid.Value;
        Name = sblAuthorizedParty.Name;
        Type = (AuthorizedPartyType)sblAuthorizedParty.PartyTypeName;
        AuthorizedRoles = sblAuthorizedParty.AuthorizedRoles;

        if (Type == AuthorizedPartyType.Organization)
        {
            OrganizationNumber = sblAuthorizedParty.OrgNumber;
            UnitType = sblAuthorizedParty.UnitType;
            IsDeleted = sblAuthorizedParty.IsDeleted;
            OnlyHierarchyElementWithNoAccess = sblAuthorizedParty.OnlyHierarchyElementWithNoAccess;
            Subunits = includeSubunits ? sblAuthorizedParty.ChildParties?.Select(subunit => new AuthorizedParty(subunit)).ToList() ?? [] : [];
        }
        else if (Type == AuthorizedPartyType.Person)
        {
            PersonId = sblAuthorizedParty.SSN;
        }
    }

    /// <summary>
    /// Gets or sets the universally unique identifier of the party
    /// </summary>
    public Guid PartyUuid { get; set; }

    /// <summary>
    /// Gets or sets the name of the party
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets the organization number if the party is an organization
    /// </summary>
    public string OrganizationNumber { get; set; }

    /// <summary>
    /// Gets the national identity number if the party is a person
    /// </summary>
    public string PersonId { get; set; }

    /// <summary>
    /// Gets or sets the party id
    /// </summary>
    public int PartyId { get; set; }

    /// <summary>
    /// Gets or sets the type of party
    /// </summary>
    public AuthorizedPartyType Type { get; set; }

    /// <summary>
    /// Gets or sets the unit type if the party is an organization
    /// </summary>
    public string UnitType { get; set; }

    /// <summary>
    /// Gets or sets whether this party is marked as deleted in the Central Coordinating Register for Legal Entities
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the party is only included as a hierarchy element without any access. Meaning a main unit where the authorized subject only have access to one or more of the subunits.
    /// </summary>
    public bool OnlyHierarchyElementWithNoAccess { get; set; }

    /// <summary>
    /// Gets or sets a collection of all resource identifier the authorized subject has some access to on behalf of this party
    /// </summary>
    public List<string> AuthorizedResources { get; set; } = [];

    /// <summary>
    /// Gets or sets a collection of all rolecodes for roles from either Enhetsregisteret or Altinn 2 which the authorized subject has been authorized for on behalf of this party
    /// </summary>
    public List<string> AuthorizedRoles { get; set; } = [];

    /// <summary>
    /// Gets or sets a set of subunits of this party, which the authorized subject also has some access to.
    /// </summary>
    public List<AuthorizedParty> Subunits { get; set; } = [];

    /// <summary>
    /// Enriches this authorized party and any subunits with the list of authorized resources
    /// </summary>
    /// <param name="resourceId">The list of resource IDs to add to the authorized party (and any subunits) list of authorized resources</param>
    public void EnrichWithResourceAccess(string resourceId)
    {
        resourceId = MapAppIdToResourceId(resourceId);
        OnlyHierarchyElementWithNoAccess = false;
        AuthorizedResources.Add(resourceId);

        if (Subunits != null)
        {
            foreach (var subunit in Subunits)
            {
                subunit.EnrichWithResourceAccess(resourceId);
            }
        }
    }

    private static string MapAppIdToResourceId(string altinnAppId)
    {
        string[] orgAppSplit = altinnAppId.Split('/');
        if (orgAppSplit.Length == 2)
        {
            return $"app_{orgAppSplit[0]}_{orgAppSplit[1]}";
        }

        return altinnAppId;
    }
}