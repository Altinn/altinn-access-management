using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Authorization.Platform.Authorization.Models;

namespace Altinn.AccessManagement.Core.Helpers
{
    /// <summary>
    /// Helper methods for rights retrieval
    /// </summary>
    public static class RightsHelper
    {
        /// <summary>
        /// Gets the list of Roles as a list of AttributeMatch elements
        /// </summary>
        /// <param name="roles">The list of altinn role codes</param>
        /// <returns>List of attribute matches</returns>
        public static List<AttributeMatch> GetRoleAttributeMatches(List<Role> roles)
        {
            List<AttributeMatch> roleMatches = new List<AttributeMatch>();
            foreach (Role role in roles)
            {
                roleMatches.Add(new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.RoleAttribute, Value = role.Value });
            }

            return roleMatches;
        }

        /// <summary>
        /// Gets the subject list for a given delegation change, for building a XacmlContextRequest
        /// </summary>
        /// <param name="delegationChange">The delegation change to retrieve subject from</param>
        /// <returns>List of attribute matches</returns>
        public static List<AttributeMatch> GetSubjectAttributeMatches(DelegationChange delegationChange)
        {
            if (delegationChange.CoveredByUserId.HasValue)
            {
                return new List<AttributeMatch>()
                {
                    new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, Value = delegationChange.CoveredByUserId.Value.ToString() }
                };
            }

            if (delegationChange.CoveredByPartyId.HasValue)
            {
                return new List<AttributeMatch>()
                {
                    new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, Value = delegationChange.CoveredByPartyId.Value.ToString() }
                };
            }

            return new();
        }
    }
}
