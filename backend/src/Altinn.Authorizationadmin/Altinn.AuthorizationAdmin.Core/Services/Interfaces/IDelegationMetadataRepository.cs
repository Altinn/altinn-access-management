using Altinn.AuthorizationAdmin.Core.Models;
using Altinn.AuthorizationAdmin.Core.Models.ResourceRegistry;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Altinn.AuthorizationAdmin.Core.Repositories.Interface
{
    /// <summary>
    /// Interface for PostgreSQL operations on delegations.
    /// </summary>
    public interface IDelegationMetadataRepository
    {
        /// <summary>
        /// Writes the delegation change metadata to the delegation database
        /// </summary>
        /// <param name="delegationChange">The DelegationChange model describing the delegation, to insert in the database</param>
        /// <returns>The complete DelegationChange record stored in the database</returns>
        Task<DelegationChange> InsertDelegation(DelegationChange delegationChange);

        /// <summary>
        /// Gets the latest delegation change matching the filter values
        /// </summary>
        /// <param name="altinnAppId">The AltinnApp identifier iin the format org/appname</param>
        /// <param name="resourceRegistryId">The Id of the resource in resourceRegistry.</param>
        /// <param name="offeredByPartyId">The party id of the entity offering the delegated the policy</param>
        /// <param name="coveredByPartyId">The party id of the entity having received the delegated policy, if the entity is an organization</param>
        /// <param name="coveredByUserId">The user id of the entity having received the delegated policy, if the entity is a user</param>
        Task<DelegationChange> GetCurrentDelegationChange(string? altinnAppId, string? resourceRegistryId, int offeredByPartyId, int? coveredByPartyId, int? coveredByUserId);

        /// <summary>
        /// Gets all the delegation change records matching the filter values for a complete changelog
        /// </summary>
        /// <param name="altinnAppId">The AltinnApp identifier iin the format org/appname</param>
        /// <param name="offeredByPartyId">The party id of the entity offering the delegated the policy</param>
        /// <param name="coveredByPartyId">The party id of the entity having received the delegated policy, if the entity is an organization</param>
        /// <param name="coveredByUserId">The user id of the entity having received the delegated policy, if the entity is a user</param>
        Task<List<DelegationChange>> GetAllDelegationChanges(string altinnAppId, int offeredByPartyId, int? coveredByPartyId, int? coveredByUserId);

        /// <summary>
        /// Gets all the current delegation change records matching the filter values
        /// </summary>
        /// <param name="offeredByPartyIds">The list of party id of the entity offering the delegated the policy</param>
        /// <param name="altinnAppIds">The list of AltinnApp identifier iin the format org/appname</param>
        /// <param name="coveredByPartyIds">The list of party id of the entity having received the delegated policy, if the entity is an organization</param>
        /// <param name="coveredByUserIds">The list of user id of the entity having received the delegated policy, if the entity is a user</param>
        Task<List<DelegationChange>> GetAllCurrentDelegationChanges(List<int> offeredByPartyIds, List<string> altinnAppIds = null, List<int> coveredByPartyIds = null, List<int> coveredByUserIds = null);

        /// <summary>
        /// Gets the delegated resources for a given reportee
        /// </summary>
        /// <param name="offeredByPartyId">The party id of the entity offering the delegation</param>
        /// <param name="resourceType">the type of the resource that was delegated</param>
        Task<List<DelegationChange>> GetAllOfferedDelegations(int offeredByPartyId, ResourceType resourceType);
        /// <summary>
        /// Gets the received resource delgations for a given reportee
        /// </summary>
        /// <param name="coveredByPartyId">The party id of the entity that received the delegation</param>
        Task<List<DelegationChange>> GetReceivedDelegationsAsync(int coveredByPartyId);
    }
}
