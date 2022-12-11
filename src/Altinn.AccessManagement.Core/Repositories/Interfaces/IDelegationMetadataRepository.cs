using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;

namespace Altinn.AccessManagement.Core.Repositories.Interfaces
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
        /// <param name="resourceMatchType">The resource match type specifying whether the lookup is for an Altinn App delegation or a resource from the Resource Registry</param>
        /// <param name="resourceId">The resourceId to look up. Either Altinn app id (org/app) or resource registry id</param>
        /// <param name="offeredByPartyId">The party id of the entity offering the delegated the policy</param>
        /// <param name="coveredByPartyId">The party id of the entity having received the delegated policy, if the entity is an organization</param>
        /// <param name="coveredByUserId">The user id of the entity having received the delegated policy, if the entity is a user</param>
        Task<DelegationChange> GetCurrentDelegationChange(ResourceAttributeMatchType resourceMatchType, string resourceId, int offeredByPartyId, int? coveredByPartyId, int? coveredByUserId);

        /// <summary>
        /// Gets all the delegation change records matching the filter values for a complete changelog
        /// </summary>
        /// <param name="altinnAppId">The Altinn app id (org/app)</param>
        /// <param name="offeredByPartyId">The party id of the entity offering the delegated the policy</param>
        /// <param name="coveredByPartyId">The party id of the entity having received the delegated policy, if the entity is an organization</param>
        /// <param name="coveredByUserId">The user id of the entity having received the delegated policy, if the entity is a user</param>
        Task<List<DelegationChange>> GetAllAppDelegationChanges(string altinnAppId, int offeredByPartyId, int? coveredByPartyId, int? coveredByUserId);

        /// <summary>
        /// Gets all the current delegation change records matching the filter values
        /// </summary>
        /// <param name="offeredByPartyIds">The list of party id of the entity offering the delegated the policy</param>
        /// <param name="altinnAppIds">The list of altinn app ids to look up</param>
        /// <param name="coveredByPartyIds">The list of party id of the entity having received the delegated policy, if the entity is an organization</param>
        /// <param name="coveredByUserIds">The list of user id of the entity having received the delegated policy, if the entity is a user</param>
        Task<List<DelegationChange>> GetAllCurrentAppDelegationChanges(List<int> offeredByPartyIds, List<string> altinnAppIds = null, List<int> coveredByPartyIds = null, List<int> coveredByUserIds = null);

        /// <summary>
        /// Gets the delegated resources for a given reportee
        /// </summary>
        /// <param name="offeredByPartyId">The party id of the entity offering the delegation</param>
        /// <param name="resourceType">the type of the resource that was delegated</param>
        Task<List<DelegationChange>> GetOfferedResourceRegistryDelegations(int offeredByPartyId, ResourceType resourceType);

        /// <summary>
        /// Gets the received resource delgations for a given reportee
        /// </summary>
        /// <param name="coveredByPartyId">The party id of the entity that received the delegation</param>
        /// <param name="resourceType">the type of resource</param>
        Task<List<DelegationChange>> GetReceivedResourceRegistryDelegationsForCoveredByPartys(int coveredByPartyId, ResourceType resourceType);

        /// <summary>
        /// Gets the delgations for a given supplier, consumer and resourcetype based on resourceids
        /// </summary>
        /// <param name="resourceIds">the resource ids</param>
        /// <param name="offeredByPartyId">the party id of the entity that offered the delegation</param>
        /// <param name="coveredByPartyId">The party id of the entity that received the delegation</param>
        /// <param name="resourceType">the type of resource</param>
        Task<List<DelegationChange>> GetResourceRegistryDelegationChanges(List<string> resourceIds, int offeredByPartyId, int coveredByPartyId, ResourceType resourceType);
    }
}
