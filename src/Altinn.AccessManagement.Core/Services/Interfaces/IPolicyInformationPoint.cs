using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models;

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
        /// <returns>a list of rules that match the lists of org/apps, offeredby ids, and coveredby ids</returns>
        Task<List<Rule>> GetRulesAsync(List<string> resourceIds, List<int> offeredByPartyIds, List<int> coveredByPartyIds, List<int> coveredByUserIds);

        /// <summary>
        /// Gets the all rights a user have for a given reportee and resource
        /// </summary>
        /// <param name="rightsQuery">The query model</param>
        /// <param name="returnAllPolicyRights">Whether the response should return all possible rights for the resource, not just the rights the user have access to</param>
        /// <param name="getDelegableRights">Whether the query is only rights the user is allowed to delegate to others</param>
        /// <returns>A list of rights</returns>
        Task<List<Right>> GetRights(RightsQuery rightsQuery, bool returnAllPolicyRights = false, bool getDelegableRights = false);

        /// <summary>
        /// Finds all delegation changes for a given user, reportee and app/resource context
        /// </summary>
        /// <param name="subjectUserId">The subjects user id</param>
        /// <param name="reporteePartyId">The reportee's partyId</param>
        /// <param name="resourceId">The Resource's id</param>
        /// <param name="resourceMatchType">The resources attribute match type</param>
        /// <returns>A list of delegation changes that's stored in the database </returns>
        Task<List<DelegationChange>> GetAllDelegations(int subjectUserId, int reporteePartyId, string resourceId, ResourceAttributeMatchType resourceMatchType);
    }
}
