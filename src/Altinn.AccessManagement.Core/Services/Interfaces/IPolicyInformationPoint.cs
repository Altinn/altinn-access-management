using Altinn.AccessManagement.Core.Models;
using LanguageExt.Common;

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
        /// Gets the all rights the user have for the reportee and resource
        /// </summary>
        /// <returns>A list of rights</returns>
        Task<List<Right>> GetRights(RightsQuery rightsQuery);
    }
}
