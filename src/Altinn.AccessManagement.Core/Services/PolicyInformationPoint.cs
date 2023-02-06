using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Authorization.ABAC;
using Altinn.Authorization.ABAC.Constants;
using Altinn.Authorization.ABAC.Xacml;
using Authorization.Platform.Authorization.Models;
using Microsoft.Extensions.Logging;

namespace Altinn.AccessManagement.Core.Services
{
    /// <summary>
    /// The Policy Information Point responsible for storing and modifying delegation policies
    /// </summary>
    public class PolicyInformationPoint : IPolicyInformationPoint
    {
        private readonly ILogger _logger;
        private readonly IPolicyRetrievalPoint _prp;
        private readonly IDelegationMetadataRepository _delegationRepository;
        private readonly IContextRetrievalService _contextRetrievalService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyInformationPoint"/> class.
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="policyRetrievalPoint">The policy retrieval point</param>
        /// <param name="delegationRepository">The delegation change repository</param>
        /// <param name="contextRetrievalService">Service for retrieving context information</param>
        public PolicyInformationPoint(ILogger<IPolicyInformationPoint> logger, IPolicyRetrievalPoint policyRetrievalPoint, IDelegationMetadataRepository delegationRepository, IContextRetrievalService contextRetrievalService)
        {
            _logger = logger;
            _prp = policyRetrievalPoint;
            _delegationRepository = delegationRepository;
            _contextRetrievalService = contextRetrievalService;
        }

        /// <inheritdoc/>
        public async Task<List<Rule>> GetRulesAsync(List<string> resourceIds, List<int> offeredByPartyIds, List<int> coveredByPartyIds, List<int> coveredByUserIds)
        {
            List<Rule> rules = new List<Rule>();
            List<DelegationChange> delegationChanges = await _delegationRepository.GetAllCurrentAppDelegationChanges(offeredByPartyIds, resourceIds, coveredByPartyIds, coveredByUserIds);
            foreach (DelegationChange delegationChange in delegationChanges)
            {
                if (delegationChange.DelegationChangeType != DelegationChangeType.RevokeLast)
                {
                    XacmlPolicy policy = await _prp.GetPolicyVersionAsync(delegationChange.BlobStoragePolicyPath, delegationChange.BlobStorageVersionId);
                    rules.AddRange(GetRulesFromPolicyAndDelegationChange(policy.Rules, delegationChange));
                }
            }

            return rules;
        }

        /// <inheritdoc/>
        public async Task<List<Right>> GetRights(RightsQuery rightsQuery, bool returnAllPolicyRights = false, bool getDelegableRights = false)
        {
            Dictionary<string, Right> result = new Dictionary<string, Right>();
            XacmlPolicy policy = null;

            // TODO: Caching??

            // Verify resource
            if (!DelegationHelper.TryGetResourceFromAttributeMatch(rightsQuery.Resource, out ResourceAttributeMatchType resourceMatchType, out string resourceId, out string org, out string app)
                || resourceMatchType == ResourceAttributeMatchType.None)
            {
                throw new ValidationException($"RightsQuery must specify a valid Resource. Valid resource can either be a single resource from the Altinn resource registry ({AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceRegistryAttribute}) or an Altinn app (identified by both {AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute} and {AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute})");
            }

            if (resourceMatchType == ResourceAttributeMatchType.ResourceRegistry)
            {
                // ToDo: does resource existance matter?
                ServiceResource registryResource = await _contextRetrievalService.GetResource(resourceId);
                if (registryResource == null || !registryResource.IsComplete.HasValue || !registryResource.IsComplete.Value || DateTime.Now < registryResource.ValidFrom || DateTime.Now > registryResource.ValidTo)
                {
                    throw new ValidationException($"The specified resource registry id: {resourceId} does not exist or is not active");
                }

                policy = await _prp.GetPolicyAsync(resourceId);
            }
            else if (resourceMatchType == ResourceAttributeMatchType.AltinnAppId)
            {
                policy = await _prp.GetPolicyAsync(org, app);
            }

            if (policy == null)
            {
                throw new ValidationException($"No valid policy found for the specified resource");
            }

            // Verify From/OfferedBy
            if (!DelegationHelper.TryGetPartyIdFromAttributeMatch(rightsQuery.From, out int offeredByPartyId))
            {
                throw new ValidationException($"Rights query currently only support lookup of rights FROM partyid ({AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute})");
            }

            // Verify To/CoveredBy
            if (!DelegationHelper.TryGetUserIdFromAttributeMatch(rightsQuery.To, out int coveredByUserId))
            {
                throw new ValidationException($"Rights query currently only support lookup of rights TO a userid: ({AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute})");
            }

            // Policy Rights
            List<Role> userRoles;
            if (getDelegableRights)
            {
                userRoles = await _contextRetrievalService.GetRolesForDelegation(coveredByUserId, offeredByPartyId);
            }
            else
            {
                userRoles = await _contextRetrievalService.GetDecisionPointRolesForUser(coveredByUserId, offeredByPartyId);
            }

            int minimumAuthenticationLevel = PolicyHelper.GetMinimumAuthenticationLevelFromXacmlPolicy(policy);
            if (userRoles.Any() || returnAllPolicyRights || getDelegableRights)
            {
                List<AttributeMatch> userRoleAttributeMatches = RightsHelper.GetRoleAttributeMatches(userRoles);
                RightSourceType policyType = resourceMatchType == ResourceAttributeMatchType.ResourceRegistry ? RightSourceType.ResourceRegistryPolicy : RightSourceType.AppPolicy;
                EnrichRightsDictionaryWithRightsFromPolicy(result, policy, policyType, userRoleAttributeMatches, minimumAuthenticationLevel: minimumAuthenticationLevel, returnAllPolicyRights: returnAllPolicyRights, getDelegableRights: getDelegableRights);
            }

            // Delegation Policy Rights
            List<DelegationChange> delegations = await FindAllDelegations(coveredByUserId, offeredByPartyId, resourceId, resourceMatchType);
            
            foreach (DelegationChange delegation in delegations)
            {
                XacmlPolicy delegationPolicy = await _prp.GetPolicyVersionAsync(delegation.BlobStoragePolicyPath, delegation.BlobStorageVersionId);
                List<AttributeMatch> subjects = RightsHelper.GetDelegationSubjectAttributeMatches(delegation);
                EnrichRightsDictionaryWithRightsFromPolicy(result, delegationPolicy, RightSourceType.DelegationPolicy, subjects, minimumAuthenticationLevel: minimumAuthenticationLevel, delegationOfferedByPartyId: delegation.OfferedByPartyId, getDelegableRights: getDelegableRights);
            }

            if (returnAllPolicyRights)
            {
                return result.Values.ToList();
            }

            if (getDelegableRights)
            {
                return result.Values.Where(r => r.CanDelegate.HasValue && r.CanDelegate.Value).ToList();
            }

            return result.Values.Where(r => r.HasPermit.HasValue && r.HasPermit.Value).ToList();
        }

        private async Task<List<DelegationChange>> FindAllDelegations(int subjectUserId, int reporteePartyId, string resourceId, ResourceAttributeMatchType resourceMatchType)
        {
            if (resourceMatchType == ResourceAttributeMatchType.None)
            {
                throw new NotSupportedException("Must specify the resource match type");
            }

            List<DelegationChange> delegations = new List<DelegationChange>();
            List<int> offeredByPartyIds = reporteePartyId.SingleToList();
            List<string> resourceIds = resourceId.SingleToList();

            // 1. Direct user delegations
            List<DelegationChange> userDelegations = resourceMatchType == ResourceAttributeMatchType.AltinnAppId ?
                await _delegationRepository.GetAllCurrentAppDelegationChanges(offeredByPartyIds, resourceIds, coveredByUserIds: subjectUserId.SingleToList()) :
                await _delegationRepository.GetAllCurrentResourceRegistryDelegationChanges(offeredByPartyIds, resourceIds, coveredByUserId: subjectUserId);
            delegations.AddRange(userDelegations);

            // 2. Direct user delegations from main unit
            List<MainUnit> mainunits = await _contextRetrievalService.GetMainUnits(reporteePartyId);
            List<int> mainunitPartyIds = mainunits.Where(m => m.PartyId.HasValue).Select(m => m.PartyId.Value).ToList();

            if (mainunitPartyIds.Any())
            {
                offeredByPartyIds.AddRange(mainunitPartyIds);
                List<DelegationChange> directMainUnitDelegations = resourceMatchType == ResourceAttributeMatchType.AltinnAppId ?
                    await _delegationRepository.GetAllCurrentAppDelegationChanges(mainunitPartyIds, resourceIds, coveredByUserIds: subjectUserId.SingleToList()) :
                    await _delegationRepository.GetAllCurrentResourceRegistryDelegationChanges(mainunitPartyIds, resourceIds, coveredByUserId: subjectUserId);

                if (directMainUnitDelegations.Any())
                {
                    delegations.AddRange(directMainUnitDelegations);
                }
            }

            // 3. Direct party delegations to keyrole units
            List<int> keyrolePartyIds = await _contextRetrievalService.GetKeyRolePartyIds(subjectUserId);
            if (keyrolePartyIds.Any())
            {
                List<DelegationChange> keyRoleDelegations = resourceMatchType == ResourceAttributeMatchType.AltinnAppId ?
                    await _delegationRepository.GetAllCurrentAppDelegationChanges(offeredByPartyIds, resourceIds, coveredByPartyIds: keyrolePartyIds) :
                    await _delegationRepository.GetAllCurrentResourceRegistryDelegationChanges(offeredByPartyIds, resourceIds, coveredByPartyIds: keyrolePartyIds);

                if (keyRoleDelegations.Any())
                {
                    delegations.AddRange(keyRoleDelegations);
                }
            }

            return delegations;
        }

        private static List<Rule> GetRulesFromPolicyAndDelegationChange(ICollection<XacmlRule> xacmlRules, DelegationChange delegationChange)
        {
            List<Rule> rules = new List<Rule>();
            foreach (XacmlRule xacmlRule in xacmlRules)
            {
                if (xacmlRule.Effect.Equals(XacmlEffectType.Permit) && xacmlRule.Target != null)
                {
                    Rule rule = new Rule
                    {
                        RuleId = xacmlRule.RuleId,
                        OfferedByPartyId = delegationChange.OfferedByPartyId,
                        DelegatedByUserId = delegationChange.PerformedByUserId,
                        CoveredBy = new List<AttributeMatch>(),
                        Resource = new List<AttributeMatch>()
                    };
                    AddAttributeMatchesToRule(xacmlRule.Target, rule);
                    rules.Add(rule);
                }
            }

            return rules;
        }

        private static void AddAttributeMatchesToRule(XacmlTarget xacmlTarget, Rule rule)
        {
            foreach (XacmlAnyOf anyOf in xacmlTarget.AnyOf)
            {
                foreach (XacmlAllOf allOf in anyOf.AllOf)
                {
                    foreach (XacmlMatch xacmlMatch in allOf.Matches)
                    {
                        AddAttributeMatchToRule(xacmlMatch, rule);
                    }
                }
            }
        }

        private static void AddAttributeMatchToRule(XacmlMatch xacmlMatch, Rule rule)
        {
            if (xacmlMatch.AttributeDesignator.Category.Equals(XacmlConstants.MatchAttributeCategory.Action))
            {
                rule.Action = new AttributeMatch
                {
                    Id = xacmlMatch.AttributeDesignator.AttributeId.OriginalString,
                    Value = xacmlMatch.AttributeValue.Value
                };
            }

            if (xacmlMatch.AttributeDesignator.Category.Equals(XacmlConstants.MatchAttributeCategory.Subject))
            {
                rule.CoveredBy.Add(new AttributeMatch
                {
                    Id = xacmlMatch.AttributeDesignator.AttributeId.OriginalString,
                    Value = xacmlMatch.AttributeValue.Value
                });
            }

            if (xacmlMatch.AttributeDesignator.Category.Equals(XacmlConstants.MatchAttributeCategory.Resource))
            {
                rule.Resource.Add(new AttributeMatch
                {
                    Id = xacmlMatch.AttributeDesignator.AttributeId.OriginalString,
                    Value = xacmlMatch.AttributeValue.Value
                });
            }
        }

        private static void EnrichRightsDictionaryWithRightsFromPolicy(Dictionary<string, Right> rights, XacmlPolicy policy, RightSourceType policySourceType, List<AttributeMatch> userSubjects, int minimumAuthenticationLevel = 0, int delegationOfferedByPartyId = 0, bool returnAllPolicyRights = false, bool getDelegableRights = false)
        {
            PolicyDecisionPoint pdp = new PolicyDecisionPoint();

            foreach (XacmlRule rule in policy.Rules)
            {
                XacmlPolicy singleRulePolicy = new XacmlPolicy(new Uri($"{policy.PolicyId}_{rule.RuleId}"), policy.RuleCombiningAlgId, policy.Target);
                singleRulePolicy.Rules.Add(rule);

                List<List<PolicyAttributeMatch>> ruleSubjects = PolicyHelper.GetRulePolicyAttributeMatchesForCategory(rule, XacmlConstants.MatchAttributeCategory.Subject);
                ICollection<Right> ruleRights = PolicyHelper.GetRightsFromXacmlRules(rule.SingleToList());
                foreach (Right ruleRight in ruleRights)
                {
                    ICollection<XacmlContextAttributes> contextAttributes = PolicyHelper.GetContextAttributes(userSubjects, ruleRight.Resource, ruleRight.Action.SingleToList());
                    XacmlContextRequest authRequest = new XacmlContextRequest(false, false, contextAttributes);

                    XacmlContextResponse response = pdp.Authorize(authRequest, singleRulePolicy);
                    XacmlContextResult decisionResult = response.Results.First();

                    // If getting rights for delegation, the right source is a delegation policy and the right does no longer exist in the app/resource policy: it should NOT be added as a delegable right
                    if (getDelegableRights && policySourceType == RightSourceType.DelegationPolicy && !rights.ContainsKey(ruleRight.RightKey))
                    {
                        continue;
                    }

                    if (!rights.ContainsKey(ruleRight.RightKey))
                    {
                        rights.Add(ruleRight.RightKey, ruleRight);
                    }

                    // If getting rights for delegation, the xacml decision is to be used for indicating if the user can delegate the right. Otherwise the decision indicate whether the user actually have the right.
                    if (getDelegableRights)
                    {
                        rights[ruleRight.RightKey].CanDelegate = (rights[ruleRight.RightKey].CanDelegate.HasValue && rights[ruleRight.RightKey].CanDelegate.Value) || decisionResult.Decision.Equals(XacmlContextDecision.Permit);
                    }
                    else
                    {
                        rights[ruleRight.RightKey].HasPermit = (rights[ruleRight.RightKey].HasPermit.HasValue && rights[ruleRight.RightKey].HasPermit.Value) || decisionResult.Decision.Equals(XacmlContextDecision.Permit);
                    }

                    if (decisionResult.Decision.Equals(XacmlContextDecision.Permit) || returnAllPolicyRights)
                    {
                        rights[ruleRight.RightKey].RightSources.Add(
                            new RightSource
                            {
                                PolicyId = policy.PolicyId.OriginalString,
                                PolicyVersion = policy.Version,
                                RuleId = rule.RuleId,
                                RightSourceType = policySourceType,
                                HasPermit = getDelegableRights ? null : decisionResult.Decision.Equals(XacmlContextDecision.Permit),
                                CanDelegate = getDelegableRights ? decisionResult.Decision.Equals(XacmlContextDecision.Permit) : null,
                                MinimumAuthenticationLevel = minimumAuthenticationLevel,
                                OfferedByPartyId = delegationOfferedByPartyId,
                                UserSubjects = userSubjects,
                                PolicySubjects = ruleSubjects
                            });
                    }
                }
            }
        }
    }
}
