﻿using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.Authorization.ProblemDetails;
using Altinn.Urn.Json;

namespace Altinn.AccessManagement.Core.Services.Interfaces
{
    /// <summary>
    /// Defines the required methods for an implementation of a policy information point.
    /// </summary>
    public interface IPolicyInformationPoint
    {
        /// <summary>
        /// Gets the rules for a list of authorization resources, given by a list of offeredbyPartyIds to a list of coveredbyIds
        /// </summary>
        /// <param name="resourceIds">The list of resource identifiers, either from the resource registry or altinn app ids</param>
        /// <param name="offeredByPartyIds">the list of offeredby party ids</param>
        /// <param name="coveredByPartyIds">the list of coveredby party ids</param>
        /// <param name="coveredByUserIds">the list of coveredby user ids</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>a list of rules that match the lists of org/apps, offeredby ids, and coveredby ids</returns>
        Task<List<Rule>> GetRulesAsync(List<string> resourceIds, List<int> offeredByPartyIds, List<int> coveredByPartyIds, List<int> coveredByUserIds, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the all rights a user have for a given reportee and resource
        /// </summary>
        /// <param name="rightsQuery">The query model</param>
        /// <param name="returnAllPolicyRights">Whether the response should return all possible rights for the resource, not just the rights the user have access to</param>
        /// <param name="getDelegableRights">Whether the query is only rights the user is allowed to delegate to others</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns>A list of rights</returns>
        Task<List<Right>> GetRights(RightsQuery rightsQuery, bool returnAllPolicyRights = false, bool getDelegableRights = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the all rights an app have right to delegate
        /// </summary>
        /// <param name="rightsQuery">The query model</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns>A list of rights</returns>
        Task<List<Right>> GetDelegableRightsByApp(RightsQuery rightsQuery, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds all delegation changes for a given user, reportee and app/resource context
        /// </summary>
        /// <param name="request">The object containing the resource/app that's checked for delegation changes</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns>A list of delegation changes that's stored in the database</returns>
        Task<DelegationChangeList> GetAllDelegations(DelegationChangeInput request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds all InstanceDelegations for a given Resource/Instance combination
        /// </summary>
        /// <param name="resourceId">The resourceId to fetch delegations for</param>
        /// <param name="instanceId">The instanceId to fetch delegations for</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns>all delegations for a given Resource/Instance combination</returns>
        Task<List<AppsInstanceDelegationResponse>> GetInstanceDelegations(string resourceId, string instanceId, CancellationToken cancellationToken);

        /// <summary>
        /// Finds all active received delegations (not including maskinporten schema) from db, both directly delegated to the party or through key roles if the party is a person
        /// </summary>
        /// <param name="partyId">Party id of a user or organization</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns></returns>
        Task<IEnumerable<DelegationChange>> GetReceivedDelegationFromRepository(int partyId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds all active offered delegations (not including maskinporten schema) from db, both directly delegated from the party or from it's main unit if the party is a subunit
        /// </summary>
        /// <param name="partyId">Party id of a user or organization</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns></returns>
        Task<IEnumerable<DelegationChange>> GetOfferedDelegationsFromRepository(int partyId, CancellationToken cancellationToken = default);
    }
}
