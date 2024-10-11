using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
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
        /// Builds a RightsQuery request model for lookup of a users rights for a given service resource on behalf of the given reportee party
        /// </summary>
        public static RightsQuery GetRightsQuery(int userId, int fromPartyId, ServiceResource resource)
        {
            return new RightsQuery()
            {
                Type = RightsQueryType.User,
                To = new List<AttributeMatch> { new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, Value = userId.ToString() } },
                From = new List<AttributeMatch> { new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, Value = fromPartyId.ToString() } },
                Resource = resource
            };
        }

        /// <summary>
        /// Check if it exist any roles giving access to the resource if there is no such access rules this must be a rule defined for the service owner as there is not any way the end user could gain access
        /// </summary>
        /// <param name="right">the right to analyze</param>
        /// <returns>the decision</returns>
        public static bool CheckIfRuleIsAnEndUserRule(Right right)
        {
            List<RightSource> roleAccessSources = right.RightSources.Where(rs => rs.RightSourceType != RightSourceType.DelegationPolicy).ToList();
            if (roleAccessSources.Any())
            {
                List<AttributeMatch> roles = GetAttributeMatches(roleAccessSources.SelectMany(roleAccessSource => roleAccessSource.PolicySubjects)).FindAll(policySubject => policySubject.Id.Equals(AltinnXacmlConstants.MatchAttributeIdentifiers.RoleAttribute, StringComparison.OrdinalIgnoreCase));
                return roles.Count != 0;
            }

            return false;
        }

        /// <summary>
        /// Analyzes a Right model for a reason for the rights delegation access status
        /// </summary>
        public static List<Detail> AnalyzeDelegationAccessReason(Right right)
        {
            List<Detail> reasons = new();

            // Analyze why able to delegate
            if (right.CanDelegate.HasValue && right.CanDelegate.Value)
            {
                // Analyze for role access
                List<RightSource> roleAccessSources = right.RightSources.Where(rs => rs.RightSourceType != Enums.RightSourceType.DelegationPolicy && rs.CanDelegate.HasValue && rs.CanDelegate.Value).ToList();
                if (roleAccessSources.Count != 0)
                {
                    List<AttributeMatch> roles = GetAttributeMatches(roleAccessSources.SelectMany(roleAccessSource => roleAccessSource.PolicySubjects)).FindAll(policySubject => policySubject.Id.Equals(AltinnXacmlConstants.MatchAttributeIdentifiers.RoleAttribute, StringComparison.OrdinalIgnoreCase));
                    string requiredRoles = string.Join(", ", roles);

                    if (roles.Count != 0)
                    {
                        reasons.Add(new Detail
                        {
                            Code = DetailCode.RoleAccess,
                            Description = $"Delegator have access through having one of the following role(s) for the reportee party: {requiredRoles}. Note: if the user is a Main Administrator (HADM) the user might not have direct access to the role other than for delegation purposes.",
                            Parameters = new Dictionary<string, List<AttributeMatch>>()
                            {
                                {
                                    "RoleRequirementsMatches", roles
                                }
                            }
                        });
                    }
                }

                // Analyze for delegation policy access
                List<RightSource> delegationPolicySources = right.RightSources.Where(rs => rs.RightSourceType == Enums.RightSourceType.DelegationPolicy && rs.CanDelegate.HasValue && rs.CanDelegate.Value).ToList();
                if (delegationPolicySources.Count != 0)
                {
                    string delegationRecipients = string.Join(", ", delegationPolicySources.SelectMany(delegationPolicySource => delegationPolicySource.PolicySubjects.SelectMany(policySubjects => policySubjects)));

                    reasons.Add(new Detail
                    {
                        Code = DetailCode.DelegationAccess,
                        Description = $"The user have access through delegation(s) of the right to the following recipient(s): {delegationRecipients}",
                        Parameters = new Dictionary<string, List<AttributeMatch>>() { { "DelegationRecipients", GetAttributeMatches(delegationPolicySources.SelectMany(delegationAccessSource => delegationAccessSource.PolicySubjects)) } }
                    });
                }
            }

            // Analyze why not allowed to delegate
            if (right.CanDelegate.HasValue && !right.CanDelegate.Value)
            {
                // Analyze for role access failure
                List<RightSource> roleAccessSources = right.RightSources.Where(rs => rs.RightSourceType != Enums.RightSourceType.DelegationPolicy).ToList();
                if (roleAccessSources.Count != 0)
                {
                    List<AttributeMatch> roles = GetAttributeMatches(roleAccessSources.SelectMany(roleAccessSource => roleAccessSource.PolicySubjects)).FindAll(policySubject => policySubject.Id.Equals(AltinnXacmlConstants.MatchAttributeIdentifiers.RoleAttribute, StringComparison.OrdinalIgnoreCase));
                    string requiredRoles = string.Join(", ", roles);

                    if (roles.Count != 0)
                    {
                        reasons.Add(new Detail
                        {
                            Code = DetailCode.MissingRoleAccess,
                            Description = $"Delegator does not have any required role(s) for the reportee party: ({requiredRoles}), which would give access to delegate the right.",
                            Parameters = new Dictionary<string, List<AttributeMatch>>() { { "RequiredRoles", roles } }
                        });
                    }
                }

                // Analyze for delegation policy failure
                List<RightSource> delegationPolicySources = right.RightSources.Where(rs => rs.RightSourceType == Enums.RightSourceType.DelegationPolicy).ToList();
                if (delegationPolicySources.Count == 0)
                {
                    reasons.Add(new Detail
                    {
                        Code = DetailCode.MissingDelegationAccess,
                        Description = $"The user does not have access through delegation(s) of the right"
                    });
                }
            }

            if (reasons.Count == 0)
            {
                reasons.Add(new Detail
                {
                    Code = DetailCode.Unknown,
                    Description = $"Unknown"
                });
            }

            return reasons;
        }

        /// <summary>
        /// Converts a list of policy attribute matches into a list of attribute matches
        /// </summary>
        /// <param name="policySubjects">a list of policy attribute matches</param>
        /// <returns>a list of attribute matches</returns>
        private static List<AttributeMatch> GetAttributeMatches(IEnumerable<List<PolicyAttributeMatch>> policySubjects)
        {
            List<AttributeMatch> attributeMatches = new List<AttributeMatch>();
            foreach (List<PolicyAttributeMatch> attributeMatch in policySubjects)
            {
                attributeMatches.AddRange(attributeMatch);
            }

            return attributeMatches;
        }
    }
}
