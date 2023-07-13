using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Helpers.Extensions;
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
        public static List<AttributeMatch> GetDelegationSubjectAttributeMatches(DelegationChange delegationChange)
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

        /// <summary>
        /// Builds a RightsQuery request model for lookup of a users rights for a given resource registry service on behalf of the given reportee party
        /// </summary>
        public static RightsQuery GetRightsQueryForResourceRegistryService(int userId, string resourceRegistryId, int fromPartyId)
        {
            return new RightsQuery
            {
                To = new List<AttributeMatch> { new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, Value = userId.ToString() } },
                From = new List<AttributeMatch> { new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, Value = fromPartyId.ToString() } },
                Resource = new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceRegistryAttribute, Value = resourceRegistryId }.SingleToList()
            };
        }

        /// <summary>
        /// Analyses a Right model for a reason for the rights delegation access status
        /// </summary>
        public static List<RightReason> AnalyzeDelegationAccessReason(Right right)
        {
            List<RightReason> reasons = new();

            // Analyse why able to delegate
            if (right.CanDelegate.HasValue && right.CanDelegate.Value)
            {
                // Analyze for role access
                List<RightSource> roleAccessSources = right.RightSources.Where(rs => rs.RightSourceType != Enums.RightSourceType.DelegationPolicy && rs.CanDelegate.HasValue && rs.CanDelegate.Value).ToList();
                if (roleAccessSources.Any())
                {
                    string requiredRoles = string.Join(", ", roleAccessSources.SelectMany(roleAccessSource => roleAccessSource.PolicySubjects.SelectMany(policySubjects => policySubjects)));

                    reasons.Add(new RightReason
                    {
                        ReasonCode = "RoleAccess",
                        Reason = $"Delegator have access through having one of the following role(s) for the reportee party: {requiredRoles}. Note: if the user is a Main Administrator (HADM) the user might not have direct access to the role other than for delegation purposes.",
                        ReasonParams = new Dictionary<string, string>() { { "RoleRequirementsMatches", $"{requiredRoles}" } }
                    });
                }

                // Analyze for delegation policy access
                List<RightSource> delegationPolicySources = right.RightSources.Where(rs => rs.RightSourceType == Enums.RightSourceType.DelegationPolicy && rs.CanDelegate.HasValue && rs.CanDelegate.Value).ToList();
                if (delegationPolicySources.Any())
                {
                    string delegationRecipients = string.Join(", ", delegationPolicySources.SelectMany(delegationPolicySource => delegationPolicySource.PolicySubjects.SelectMany(policySubjects => policySubjects)));

                    reasons.Add(new RightReason
                    {
                        ReasonCode = "DelegationAccess",
                        Reason = $"The user have access through delegation(s) of the right to the following recipient(s): {delegationRecipients}",
                        ReasonParams = new Dictionary<string, string>() { { "DelegationRecipients", $"{delegationRecipients}" } }
                    });
                }
            }

            // Analyse why not allowed to delegate
            if (right.CanDelegate.HasValue && !right.CanDelegate.Value)
            {
                // Analyze for role access failure
                List<RightSource> roleAccessSources = right.RightSources.Where(rs => rs.RightSourceType != Enums.RightSourceType.DelegationPolicy).ToList();
                if (roleAccessSources.Any())
                {
                    string requiredRoles = string.Join(", ", roleAccessSources.SelectMany(roleAccessSource => roleAccessSource.PolicySubjects.SelectMany(policySubjects => policySubjects)));

                    reasons.Add(new RightReason
                    {
                        ReasonCode = "MissingRoleAccess",
                        Reason = $"Delegator does not have any required role(s) for the reportee party: ({requiredRoles}), which would give access to delegate the right.",
                        ReasonParams = new Dictionary<string, string>() { { "RequiredRoles", $"{requiredRoles}" } }
                    });
                }

                // Analyze for delegation policy failure
                List<RightSource> delegationPolicySources = right.RightSources.Where(rs => rs.RightSourceType == Enums.RightSourceType.DelegationPolicy).ToList();
                if (!delegationPolicySources.Any())
                {
                    reasons.Add(new RightReason
                    {
                        ReasonCode = "MissingDelegationAccess",
                        Reason = $"The user does not have access through delegation(s) of the right"
                    });
                }
            }

            if (reasons.Count == 0)
            {
                reasons.Add(new RightReason
                {
                    ReasonCode = "Unknown",
                    Reason = $"Unknown reason"
                });
            }

            return reasons;
        }
    }
}
