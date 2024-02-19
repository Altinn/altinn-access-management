using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Models;

/// <summary>
/// Input model
/// </summary>
public record AuthorizedPartyInput
{
    /// <summary>
    /// Used to provide the organization number of the party, in combination with 'organization' as placeholder value for the {party} path parameter
    /// </summary>
    [FromHeader(Name = IdentifierUtil.OrganizationNumberHeader)]
    public string OrganizationNumber { get; set; }

    /// <summary>
    /// Used to provide the social security number of the party, in combination with 'person' as placeholder value for the {party} path parameter
    /// </summary>
    [FromHeader(Name = IdentifierUtil.PersonHeader)]
    public string PartySSN { get; set; }

    /// <summary>
    /// Used to specify the reportee party the authenticated user is acting on behalf of. Can either be the PartyId, or the placeholder values: 'person' or 'organization' in combination with providing the social security number or the organization number using the header values.
    /// </summary>
    [FromRoute(Name = "party")]
    public string Party { get; set; }
}