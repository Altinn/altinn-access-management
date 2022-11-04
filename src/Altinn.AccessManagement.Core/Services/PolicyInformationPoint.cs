using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Core.Clients.Interfaces;
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
        /// <param name="contextRetrievalService">Context service for getting </param>
        public PolicyInformationPoint(ILogger<IPolicyInformationPoint> logger, IPolicyRetrievalPoint policyRetrievalPoint, IDelegationMetadataRepository delegationRepository, IContextRetrievalService contextRetrievalService)
        {
            _logger = logger;
            _prp = policyRetrievalPoint;
            _delegationRepository = delegationRepository;
            _contextRetrievalService = contextRetrievalService;
        }

        /// <inheritdoc/>
        public async Task<List<Rule>> GetRulesAsync(List<string> appIds, List<int> offeredByPartyIds, List<int> coveredByPartyIds, List<int> coveredByUserIds)
        {
            List<Rule> rules = new List<Rule>();
            List<DelegationChange> delegationChanges = await _delegationRepository.GetAllCurrentDelegationChanges(offeredByPartyIds, appIds, coveredByPartyIds, coveredByUserIds);
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
        public async Task<List<Right>> GetRights(RightsQuery rightsQuery)
        {
            Dictionary<string, Right> result = new Dictionary<string, Right>();
            XacmlPolicy policy = null;

            // Verify resource
            if (!DelegationHelper.TryGetResourceFromAttributeMatch(rightsQuery.Resource, out ResourceAttributeMatchType resourceMatchType, out string resourceRegistryId, out string org, out string app)
                || resourceMatchType == ResourceAttributeMatchType.None)
            {
                throw new ValidationException("RightsQuery must specify a valid Resource. Valid resource can either be a single resource from the Altinn resource registry or an Altinn app");
            }

            if (resourceMatchType == ResourceAttributeMatchType.ResourceRegistry)
            {
                // ToDo: does resource existance matter?
                ServiceResource registryResource = await _contextRetrievalService.GetResource(resourceRegistryId);
                if (registryResource == null || !registryResource.IsComplete.HasValue || !registryResource.IsComplete.Value || DateTime.Now < registryResource.ValidFrom || DateTime.Now > registryResource.ValidTo)
                {
                    throw new ValidationException($"The specified resource registry id: {resourceRegistryId} does not exist or is not active");
                }

                policy = await _prp.GetPolicyAsync(resourceRegistryId);
            }
            else if (resourceMatchType == ResourceAttributeMatchType.AltinnApp)
            {
                policy = await _prp.GetPolicyAsync(org, app);
            }

            if (policy == null)
            {
                throw new ValidationException($"No valid policy found for the specified resource");
            }

            // Verify coveredBy
            if (!DelegationHelper.TryGetCoveredByUserIdFromMatch(rightsQuery.Reportee, out int coveredByUserId))
            {
                throw new ValidationException($"Rights query currently only support lookup of rights for a coveredBy user id ({AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute})");
            }

            // Policy Rights
            List<Role> roles = await _contextRetrievalService.GetDecisionPointRolesForUser(coveredByUserId, rightsQuery.From);
            List<AttributeMatch> roleAttributeMatches = RightsHelper.GetRoleAttributeMatches(roles);

            RightSourceType policyType = resourceMatchType == ResourceAttributeMatchType.ResourceRegistry ? RightSourceType.ResourceRegistryPolicy : RightSourceType.AppPolicy;
            EnrichRightsDictionaryWithRightsFromPolicy(result, policy, policyType, roleAttributeMatches);

            // Delegation Policy Rights
            List<AttributeMatch> subjects = new();
            List<MainUnit> mainUnit = await _contextRetrievalService.GetMainUnits(rightsQuery.From);
            
            foreach (XacmlPolicy delegationPolicy in new List<XacmlPolicy>())
            {
                EnrichRightsDictionaryWithRightsFromPolicy(result, delegationPolicy, RightSourceType.DelegationPolicy, subjects);
            }

            return result.Values.ToList();
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

        private void EnrichRightsDictionaryWithRightsFromPolicy(Dictionary<string, Right> rights, XacmlPolicy policy, RightSourceType policySourceType, List<AttributeMatch> subjects)
        {
            PolicyDecisionPoint pdp = new PolicyDecisionPoint();

            foreach (XacmlRule rule in policy.Rules)
            {
                XacmlPolicy singleRulePolicy = new XacmlPolicy(new Uri($"{policy.PolicyId}_{rule.RuleId}"), policy.RuleCombiningAlgId, policy.Target);
                singleRulePolicy.Rules.Add(rule);

                ICollection<Right> ruleRights = PolicyHelper.GetRightsFromXacmlRules(rule.SingleToList());
                foreach (Right ruleRight in ruleRights)
                {
                    ICollection<XacmlContextAttributes> contextAttributes = PolicyHelper.GetContextAttributes(subjects, ruleRight.Resource, ruleRight.Action.SingleToList());
                    XacmlContextRequest authRequest = new XacmlContextRequest(false, false, contextAttributes);

                    XacmlContextResponse response = pdp.Authorize(authRequest, singleRulePolicy);

                    XacmlContextResult decisionResult = response.Results.FirstOrDefault();

                    if (!rights.ContainsKey(ruleRight.RightKey))
                    {
                        rights.Add(ruleRight.RightKey, ruleRight);
                    }

                    if (decisionResult.Decision.Equals(XacmlContextDecision.Permit))
                    {
                        rights[ruleRight.RightKey].HasPermit = true;
                        rights[ruleRight.RightKey].RightSources.Add(
                            new RightSource
                            {
                                PolicyId = policy.PolicyId.OriginalString,
                                PolicyVersion = policy.Version,
                                RuleId = rule.RuleId,
                                RightSourceType = policySourceType,
                                Subject = subjects // ToDo: Get subject matches from ABAC
                            });
                    }
                }
            }
        }
    }
}
