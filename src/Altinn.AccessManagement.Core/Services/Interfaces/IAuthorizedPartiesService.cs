using Altinn.AccessManagement.Core.Models;

namespace Altinn.AccessManagement.Core.Services.Interfaces;

/// <summary>
/// Service for operations regarding retrieval of authorized parties (aka reporteelist)
/// </summary>
public interface IAuthorizedPartiesService
{
    /// <summary>
    /// Gets the full unfiltered list of all authorized parties a given user or organization have some access for in Altinn
    /// </summary>
    /// <param name="subjectAttribute">Attribute identifying the user or organization retrieve the authorized party list for</param>
    /// <param name="includeAltinn2AuthorizedParties">Whether Authorized Parties from Altinn 2 should be included in the result set</param>
    /// <param name="includeAuthorizedResourcesThroughRoles">Whether Authorized Resources per party should be enriched with resources the user has access to through AuthorizedRoles</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>The unfiltered party list</returns>
    Task<List<AuthorizedParty>> GetAuthorizedParties(BaseAttribute subjectAttribute, bool includeAltinn2AuthorizedParties, bool includeAuthorizedResourcesThroughRoles, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the full unfiltered list of authorized parties the given user can represent in Altinn
    /// </summary>
    /// <param name="subjectUserId">The user id of the user to retrieve the authorized party list for</param>
    /// <param name="includeAltinn2AuthorizedParties">Whether Authorized Parties from Altinn 2 should be included in the result set</param>
    /// <param name="includeAuthorizedResourcesThroughRoles">Whether Authorized Resources per party should be enriched with resources the user has access to through AuthorizedRoles</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>The unfiltered party list</returns>
    Task<List<AuthorizedParty>> GetAuthorizedPartiesForUser(int subjectUserId, bool includeAltinn2AuthorizedParties, bool includeAuthorizedResourcesThroughRoles, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the full unfiltered list of authorized parties the given user or organization have some access for in Altinn
    /// </summary>
    /// <param name="subjectPartyId">The party id of the user or organization to retrieve the authorized party list for</param>
    /// <param name="includeAltinn2AuthorizedParties">Whether Authorized Parties from Altinn 2 should be included in the result set</param>
    /// <param name="includeAuthorizedResourcesThroughRoles">Whether Authorized Resources per party should be enriched with resources the user has access to through AuthorizedRoles</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>The unfiltered party list</returns>
    Task<List<AuthorizedParty>> GetAuthorizedPartiesForParty(int subjectPartyId, bool includeAltinn2AuthorizedParties, bool includeAuthorizedResourcesThroughRoles, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the full unfiltered list of authorized parties the given person can represent in Altinn
    /// </summary>
    /// <param name="subjectNationalId">The national identity number of the person to retrieve the authorized party list for</param>
    /// <param name="includeAltinn2AuthorizedParties">Whether Authorized Parties from Altinn 2 should be included in the result set</param>
    /// <param name="includeAuthorizedResourcesThroughRoles">Whether Authorized Resources per party should be enriched with resources the user has access to through AuthorizedRoles</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>The unfiltered party list</returns>
    Task<List<AuthorizedParty>> GetAuthorizedPartiesForPerson(string subjectNationalId, bool includeAltinn2AuthorizedParties, bool includeAuthorizedResourcesThroughRoles, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the full unfiltered list of authorized parties the given person can represent in Altinn
    /// </summary>
    /// <param name="subjectPersonUuid">The uuid of the person to retrieve the authorized party list for</param>
    /// <param name="includeAltinn2AuthorizedParties">Whether Authorized Parties from Altinn 2 should be included in the result set</param>
    /// <param name="includeAuthorizedResourcesThroughRoles">Whether Authorized Resources per party should be enriched with resources the user has access to through AuthorizedRoles</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>The unfiltered party list</returns>
    Task<List<AuthorizedParty>> GetAuthorizedPartiesForPersonUuid(string subjectPersonUuid, bool includeAltinn2AuthorizedParties, bool includeAuthorizedResourcesThroughRoles, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the full unfiltered list of authorized parties the given organization can represent in Altinn
    /// </summary>
    /// <param name="subjectOrganizationNumber">The organization number of the organization to retrieve the authorized party list for</param>
    /// <param name="includeAltinn2AuthorizedParties">Whether Authorized Parties from Altinn 2 should be included in the result set</param>
    /// <param name="includeAuthorizedResourcesThroughRoles">Whether Authorized Resources per party should be enriched with resources the user has access to through AuthorizedRoles</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>The unfiltered party list</returns>
    Task<List<AuthorizedParty>> GetAuthorizedPartiesForOrganization(string subjectOrganizationNumber, bool includeAltinn2AuthorizedParties, bool includeAuthorizedResourcesThroughRoles, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the full unfiltered list of authorized parties the given organization can represent in Altinn
    /// </summary>
    /// <param name="subjectOrganizationUuid">The organization uuid of the organization to retrieve the authorized party list for</param>
    /// <param name="includeAltinn2AuthorizedParties">Whether Authorized Parties from Altinn 2 should be included in the result set</param>
    /// <param name="includeAuthorizedResourcesThroughRoles">Whether Authorized Resources per party should be enriched with resources the user has access to through AuthorizedRoles</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>The unfiltered party list</returns>
    Task<List<AuthorizedParty>> GetAuthorizedPartiesForOrganizationUuid(string subjectOrganizationUuid, bool includeAltinn2AuthorizedParties, bool includeAuthorizedResourcesThroughRoles, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the full unfiltered list of authorized parties the given enterprise user can represent in Altinn
    /// </summary>
    /// <param name="subjectEnterpriseUsername">The username of the enterprise user to retrieve the authorized party list for</param>
    /// <param name="includeAltinn2AuthorizedParties">Whether Authorized Parties from Altinn 2 should be included in the result set</param>
    /// <param name="includeAuthorizedResourcesThroughRoles">Whether Authorized Resources per party should be enriched with resources the user has access to through AuthorizedRoles</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>The unfiltered party list</returns>
    Task<List<AuthorizedParty>> GetAuthorizedPartiesForEnterpriseUser(string subjectEnterpriseUsername, bool includeAltinn2AuthorizedParties, bool includeAuthorizedResourcesThroughRoles, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the full unfiltered list of authorized parties the given enterprise user can represent in Altinn
    /// </summary>
    /// <param name="subjectEnterpriseUserUuid">The uuid of the enterprise user to retrieve the authorized party list for</param>
    /// <param name="includeAltinn2AuthorizedParties">Whether Authorized Parties from Altinn 2 should be included in the result set</param>
    /// <param name="includeAuthorizedResourcesThroughRoles">Whether Authorized Resources per party should be enriched with resources the user has access to through AuthorizedRoles</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>The unfiltered party list</returns>
    Task<List<AuthorizedParty>> GetAuthorizedPartiesForEnterpriseUserUuid(string subjectEnterpriseUserUuid, bool includeAltinn2AuthorizedParties, bool includeAuthorizedResourcesThroughRoles, CancellationToken cancellationToken);
}
