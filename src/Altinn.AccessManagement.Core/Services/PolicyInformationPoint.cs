using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.Profile;
using Altinn.AccessManagement.Core.Models.Register;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Models.Rights;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Enums;
using Altinn.Authorization.ABAC;
using Altinn.Authorization.ABAC.Constants;
using Altinn.Authorization.ABAC.Xacml;
using Altinn.Platform.Profile.Models;
using Altinn.Platform.Register.Enums;
using Altinn.Platform.Register.Models;
using Altinn.Urn;
using Altinn.Urn.Json;
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
        private readonly IProfileClient _profile;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyInformationPoint"/> class.
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="policyRetrievalPoint">The policy retrieval point</param>
        /// <param name="delegationRepository">The delegation change repository</param>
        /// <param name="contextRetrievalService">Service for retrieving context information</param>
        /// <param name="profile">Service for retrieving user profile information</param>
        public PolicyInformationPoint(ILogger<IPolicyInformationPoint> logger, IPolicyRetrievalPoint policyRetrievalPoint, IDelegationMetadataRepository delegationRepository, IContextRetrievalService contextRetrievalService, IProfileClient profile)
        {
            _logger = logger;
            _prp = policyRetrievalPoint;
            _delegationRepository = delegationRepository;
            _contextRetrievalService = contextRetrievalService;
            _profile = profile;
        }

        /// <inheritdoc/>
        public async Task<List<Rule>> GetRulesAsync(List<string> resourceIds, List<int> offeredByPartyIds, List<int> coveredByPartyIds, List<int> coveredByUserIds, CancellationToken cancellationToken = default)
        {
            List<Rule> rules = new List<Rule>();
            List<DelegationChange> delegationChanges = await _delegationRepository.GetAllCurrentAppDelegationChanges(offeredByPartyIds, resourceIds, coveredByPartyIds, coveredByUserIds, cancellationToken);
            foreach (DelegationChange delegationChange in delegationChanges)
            {
                if (delegationChange.DelegationChangeType != DelegationChangeType.RevokeLast)
                {
                    XacmlPolicy policy = await _prp.GetPolicyVersionAsync(delegationChange.BlobStoragePolicyPath, delegationChange.BlobStorageVersionId, cancellationToken);
                    rules.AddRange(GetRulesFromPolicyAndDelegationChange(policy.Rules, delegationChange));
                }
            }

            return rules;
        }

        /// <inheritdoc/>
        public async Task<List<Right>> GetRights(RightsQuery rightsQuery, bool returnAllPolicyRights = false, bool getDelegableRights = false, CancellationToken cancellationToken = default)
        {
            Dictionary<string, Right> result = new Dictionary<string, Right>();
            XacmlPolicy policy = null;

            if (rightsQuery.Type != RightsQueryType.User)
            {
                return result.Values.ToList();
            }

            // TODO: Caching??

            // Verify resource
            if (!DelegationHelper.TryGetResourceFromAttributeMatch(rightsQuery.Resource.AuthorizationReference, out ResourceAttributeMatchType resourceMatchType, out string resourceId, out string org, out string app, out string serviceCode, out string serviceEditionCode)
                || resourceMatchType == ResourceAttributeMatchType.None)
            {
                throw new ValidationException($"RightsQuery must specify a valid Resource. Valid resource can either be a single resource from the Altinn resource registry ({AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceRegistryAttribute}) or an Altinn app (identified by both {AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute} and {AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute})");
            }

            if (resourceMatchType == ResourceAttributeMatchType.ResourceRegistry)
            {
                policy = await _prp.GetPolicyAsync(resourceId, cancellationToken);
            }
            else if (resourceMatchType == ResourceAttributeMatchType.AltinnAppId)
            {
                policy = await _prp.GetPolicyAsync(org, app, cancellationToken);
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
                userRoles = await _contextRetrievalService.GetRolesForDelegation(coveredByUserId, offeredByPartyId, cancellationToken);
            }
            else
            {
                userRoles = await _contextRetrievalService.GetDecisionPointRolesForUser(coveredByUserId, offeredByPartyId, cancellationToken);
            }

            int minimumAuthenticationLevel = PolicyHelper.GetMinimumAuthenticationLevelFromXacmlPolicy(policy);
            if (userRoles.Any() || returnAllPolicyRights || getDelegableRights)
            {
                List<AttributeMatch> userRoleAttributeMatches = RightsHelper.GetRoleAttributeMatches(userRoles);
                RightSourceType policyType = resourceMatchType == ResourceAttributeMatchType.ResourceRegistry ? RightSourceType.ResourceRegistryPolicy : RightSourceType.AppPolicy;
                EnrichRightsDictionaryWithRightsFromPolicy(result, policy, policyType, userRoleAttributeMatches, minimumAuthenticationLevel: minimumAuthenticationLevel, returnAllPolicyRights: returnAllPolicyRights, getDelegableRights: getDelegableRights);
            }

            // Delegation Policy Rights
            List<DelegationChange> delegations = await FindAllDelegations(coveredByUserId, 0, Guid.Empty, UuidType.NotSpecified, offeredByPartyId, resourceId, resourceMatchType, cancellationToken: cancellationToken);

            foreach (DelegationChange delegation in delegations)
            {
                XacmlPolicy delegationPolicy = await _prp.GetPolicyVersionAsync(delegation.BlobStoragePolicyPath, delegation.BlobStorageVersionId, cancellationToken);
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

        /// <inheritdoc />
        public async Task<List<Right>> GetDelegableRightsByApp(RightsQuery rightsQuery, CancellationToken cancellationToken = default)
        {
            Dictionary<string, Right> result = new Dictionary<string, Right>();
            if (rightsQuery.Type != RightsQueryType.AltinnApp)
            {
                return result.Values.ToList();
            }

            XacmlPolicy policy = await GetPolicy(rightsQuery.Resource.AuthorizationReference, cancellationToken);

            int minimumAuthenticationLevel = PolicyHelper.GetMinimumAuthenticationLevelFromXacmlPolicy(policy);
            RightSourceType policyType = rightsQuery.Resource.ResourceType == ResourceType.AltinnApp ? RightSourceType.AppPolicy : RightSourceType.ResourceRegistryPolicy;
            EnrichRightsDictionaryWithRightsFromPolicy(result, policy, policyType, rightsQuery.To, minimumAuthenticationLevel: minimumAuthenticationLevel, returnAllPolicyRights: false, getDelegableRights: true);

            return result.Values.Where(r => r.CanDelegate.HasValue && r.CanDelegate.Value).ToList();
        }

        private async Task<XacmlPolicy> GetPolicy(List<AttributeMatch> resource, CancellationToken cancellationToken)
        {
            XacmlPolicy policy = null;

            // Verify resource
            if (!DelegationHelper.TryGetResourceFromAttributeMatch(resource, out ResourceAttributeMatchType resourceMatchType, out string resourceId, out string org, out string app, out string _, out string _)
                || resourceMatchType == ResourceAttributeMatchType.None)
            {
                throw new ValidationException($"RightsQuery must specify a valid Resource. Valid resource can either be a single resource from the Altinn resource registry ({AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceRegistryAttribute}) or an Altinn app (identified by both {AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute} and {AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute})");
            }

            if (resourceMatchType == ResourceAttributeMatchType.ResourceRegistry)
            {
                policy = await _prp.GetPolicyAsync(resourceId, cancellationToken);
            }
            else if (resourceMatchType == ResourceAttributeMatchType.AltinnAppId)
            {
                policy = await _prp.GetPolicyAsync(org, app, cancellationToken);
            }

            if (policy == null)
            {
                throw new ValidationException($"No valid policy found for the specified resource");
            }

            return policy;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DelegationChange>> GetReceivedDelegationFromRepository(int partyId, CancellationToken cancellationToken = default)
        {
            var party = await _contextRetrievalService.GetPartyAsync(partyId, cancellationToken);

            if (party?.PartyTypeName == PartyType.Person)
            {
                var user = await _profile.GetUser(new() { Ssn = party.SSN }, cancellationToken);

                var keyRoles = await _contextRetrievalService.GetKeyRolePartyIds(user.UserId, cancellationToken);
                return await _delegationRepository.GetAllDelegationChangesForAuthorizedParties(user.UserId.SingleToList(), keyRoles, cancellationToken);
            }

            if (party?.PartyTypeName == PartyType.Organisation)
            {
                return await _delegationRepository.GetAllDelegationChangesForAuthorizedParties(null, party.PartyId.SingleToList(), cancellationToken);
            }

            throw new ArgumentException($"failed to handle party with id '{partyId}'");
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DelegationChange>> GetOfferedDelegationsFromRepository(int partyId, CancellationToken cancellationToken = default)
        {
            var party = await _contextRetrievalService.GetPartyAsync(partyId, cancellationToken);

            if (party.PartyTypeName == PartyType.Person)
            {
                return await _delegationRepository.GetOfferedDelegations(partyId.SingleToList(), cancellationToken);
            }

            if (party.PartyTypeName == PartyType.Organisation)
            {
                var mainUnits = await _contextRetrievalService.GetMainUnits(party.PartyId.SingleToList(), cancellationToken);
                var parties = party.PartyId.SingleToList();
                if (mainUnits?.FirstOrDefault() is var mainUnit && mainUnit?.PartyId != null)
                {
                    parties.Add((int)mainUnit.PartyId);
                }

                return await _delegationRepository.GetOfferedDelegations(parties, cancellationToken);
            }

            throw new ArgumentException($"failed to handle party with id '{partyId}'");
        }

        /// <inheritdoc/>
        public async Task<DelegationChangeList> GetAllDelegations(DelegationChangeInput request, bool includeInstanceDelegations = false, CancellationToken cancellationToken = default)
        {
            DelegationChangeList result = new DelegationChangeList();
            bool validSubjectUser = DelegationHelper.TryGetUserIdFromAttributeMatch(request.Subject.SingleToList(), out int subjectUserId);
            bool validSubjectParty = DelegationHelper.TryGetPartyIdFromAttributeMatch(request.Subject.SingleToList(), out int subjectPartyId);
            bool validSubjectUuid = DelegationHelper.TryGetUuidFromAttributeMatch(request.Subject.SingleToList(), out Guid subjectUuid, out UuidType subjectUuidType);
            bool validParty = DelegationHelper.TryGetPartyIdFromAttributeMatch(request.Party.SingleToList(), out int partyId);
            bool validResourceMatchType = DelegationHelper.TryGetResourceFromAttributeMatch(request.Resource, out ResourceAttributeMatchType resourceMatchType, out string resourceId, out string _, out string _, out string _, out string _);

            if (!validSubjectUser && !validSubjectParty && (!validSubjectUuid || subjectUuidType != UuidType.SystemUser))
            {
                result.Errors.Add("request.Subject", $"Missing valid subject on request. Valid subject attribute types: either {AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute}, {AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute} or {AltinnXacmlConstants.MatchAttributeIdentifiers.SystemUserUuid}");
                return result;
            }

            if (!validParty)
            {
                result.Errors.Add("request.Party", $"Missing valid party on request. Valid party attribute type: {AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute}");
                return result;
            }

            if (!validResourceMatchType)
            {
                result.Errors.Add("request.Resource", $"Missing valid resource on request. Valid resource attribute types: either a single {AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceRegistryAttribute} or combination of both {AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute} and {AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute}");
                return result;
            }

            result.DelegationChanges = await FindAllDelegations(subjectUserId, subjectPartyId, subjectUuid, subjectUuidType, partyId, resourceId, resourceMatchType, includeInstanceDelegations, cancellationToken);
            return result;
        }

        /// <inheritdoc/>
        public async Task<List<AppsInstanceDelegationResponse>> GetInstanceDelegations(AppsInstanceGetRequest request, CancellationToken cancellationToken)
        {
            List<AppsInstanceDelegationResponse> result = new List<AppsInstanceDelegationResponse>();

            List<InstanceDelegationChange> delegations = await _delegationRepository.GetAllLatestInstanceDelegationChanges(request.InstanceDelegationSource, request.ResourceId, request.InstanceId, cancellationToken);

            foreach (InstanceDelegationChange delegation in delegations)
            {
                AppsInstanceDelegationResponse appsInstanceDelegationResponse = new AppsInstanceDelegationResponse
                {
                    From = GetPartyUrnFromUuidTypeAndUuid(delegation.FromUuid, delegation.FromUuidType),
                    To = GetPartyUrnFromUuidTypeAndUuid(delegation.ToUuid, delegation.ToUuidType),
                    InstanceDelegationMode = delegation.InstanceDelegationMode,
                    ResourceId = delegation.ResourceId,
                    InstanceId = delegation.InstanceId
                };

                XacmlPolicy policy = await _prp.GetPolicyVersionAsync(delegation.BlobStoragePolicyPath, delegation.BlobStorageVersionId, cancellationToken);
                appsInstanceDelegationResponse.Rights = GetRightsFromPolicy(policy);
                result.Add(appsInstanceDelegationResponse);
            }

            return result;
        }

        private static List<InstanceRightDelegationResult> GetRightsFromPolicy(XacmlPolicy policy)
        {
            List<InstanceRightDelegationResult> result = new List<InstanceRightDelegationResult>();

            foreach (XacmlRule xacmlRule in policy.Rules)
            {
                result.Add(GetInstanceRightDelegationResultFromPolicyRule(xacmlRule));
            }
            
            return result;
        }

        private static InstanceRightDelegationResult GetInstanceRightDelegationResultFromPolicyRule(XacmlRule xacmlRule)
        {
            InstanceRightDelegationResult rule = new InstanceRightDelegationResult { Resource = [], Status = DelegationStatus.Delegated };

            foreach (XacmlAnyOf anyOf in xacmlRule.Target.AnyOf)
            {
                foreach (XacmlAllOf allOf in anyOf.AllOf)
                {
                    foreach (XacmlMatch xacmlMatch in allOf.Matches)
                    {
                        if (xacmlMatch.AttributeDesignator.Category.Equals(XacmlConstants.MatchAttributeCategory.Action))
                        {
                            rule.Action = ActionUrn.Parse($"{xacmlMatch.AttributeDesignator.AttributeId.OriginalString}:{xacmlMatch.AttributeValue.Value}");
                        }

                        if (xacmlMatch.AttributeDesignator.Category.Equals(XacmlConstants.MatchAttributeCategory.Resource))
                        {
                            UrnJsonTypeValue resourcePart = KeyValueUrn.Create($"{xacmlMatch.AttributeDesignator.AttributeId.OriginalString}:{xacmlMatch.AttributeValue.Value}", xacmlMatch.AttributeDesignator.AttributeId.OriginalString.Length + 1);
                            rule.Resource.Add(resourcePart);
                        }
                    }
                }
            }

            return rule;
        }

        private static PartyUrn GetPartyUrnFromUuidTypeAndUuid(Guid uuid, UuidType type)
        {
            string urnString = null;

            switch (type)
            {
                case UuidType.Person:
                case UuidType.Organization:
                    urnString = $"urn:altinn:party:uuid:{uuid.ToString()}";
                    break;
            }

            bool validParty = PartyUrn.TryParse(urnString, out PartyUrn result);

            return validParty ? result : null;
        }

        private async Task<List<DelegationChange>> FindAllDelegations(int subjectUserId, int subjectPartyId, Guid subjectUuid, UuidType subjectUuidType, int reporteePartyId, string resourceId, ResourceAttributeMatchType resourceMatchType, bool includeInstanceDelegations = false, CancellationToken cancellationToken = default)
        {
            if (resourceMatchType == ResourceAttributeMatchType.None)
            {
                throw new NotSupportedException("Must specify the resource match type");
            }

            if ((subjectUserId == 0 ^ subjectPartyId == 0 ^ subjectUuidType == UuidType.NotSpecified) || (subjectUserId != 0 && subjectPartyId != 0 && subjectUuidType != UuidType.NotSpecified))
            {
                throw new NotSupportedException("Must specify the single subjectUserId, subjectPartyId or subjectUuid");
            }

            List<DelegationChange> delegations = new List<DelegationChange>();
            List<int> offeredByPartyIds = reporteePartyId.SingleToList();
            List<string> resourceIds = resourceId.SingleToList();

            // Check if request should include instance delegations, which will require lookup of reportee party uuid
            Guid? fromParty = null;
            List<Guid> toParties = null;
            if (includeInstanceDelegations)
            {
                Party reporteeParty = await _contextRetrievalService.GetPartyAsync(reporteePartyId, cancellationToken);
                fromParty = reporteeParty?.PartyUuid;
                toParties = new List<Guid>();
            }

            // Check if mainunit exists
            MainUnit mainunit = await _contextRetrievalService.GetMainUnit(reporteePartyId, cancellationToken);
            if (mainunit?.PartyId > 0)
            {
                offeredByPartyIds.Add(mainunit.PartyId.Value);
            }

            // 1. Direct user delegations
            if (subjectUserId > 0)
            {
                delegations = resourceMatchType == ResourceAttributeMatchType.AltinnAppId
                ? await _delegationRepository.GetAllCurrentAppDelegationChanges(offeredByPartyIds, resourceIds, coveredByUserIds: subjectUserId.SingleToList(), cancellationToken: cancellationToken)
                : await _delegationRepository.GetAllCurrentResourceRegistryDelegationChanges(offeredByPartyIds, resourceIds, coveredByUserId: subjectUserId, cancellationToken: cancellationToken);

                if (includeInstanceDelegations)
                {
                    UserProfile subjectUserProfile = await _profile.GetUser(new UserProfileLookup { UserId = subjectUserId }, cancellationToken: cancellationToken);
                    if (subjectUserProfile != null)
                    {
                        toParties.Add(subjectUserProfile.Party.PartyUuid.Value);
                    }
                }
            }
            else if (subjectUuidType == UuidType.SystemUser)
            {
                delegations = resourceMatchType == ResourceAttributeMatchType.AltinnAppId
                ? await _delegationRepository.GetAllCurrentAppDelegationChanges(resourceIds, offeredByPartyIds, subjectUuidType, subjectUuid, cancellationToken)
                : await _delegationRepository.GetAllCurrentResourceRegistryDelegationChanges(resourceIds, offeredByPartyIds, subjectUuidType, subjectUuid, cancellationToken);
            }

            // 2. Direct party delegations incl. any keyrole units
            List<int> coveredByPartyIds = subjectPartyId > 0 ? new List<int> { subjectPartyId } : new List<int>();

            if (subjectUserId > 0)
            {
                coveredByPartyIds = await _contextRetrievalService.GetKeyRolePartyIds(subjectUserId, cancellationToken);
            }

            if (coveredByPartyIds.Count > 0)
            {
                List<DelegationChange> partyDelegations = resourceMatchType == ResourceAttributeMatchType.AltinnAppId
                    ? await _delegationRepository.GetAllCurrentAppDelegationChanges(offeredByPartyIds, resourceIds, coveredByPartyIds: coveredByPartyIds, cancellationToken: cancellationToken)
                    : await _delegationRepository.GetAllCurrentResourceRegistryDelegationChanges(offeredByPartyIds, resourceIds, coveredByPartyIds: coveredByPartyIds, cancellationToken: cancellationToken);
                delegations.AddRange(partyDelegations);

                if (includeInstanceDelegations)
                {
                    List<Party> coveredByPartys = await _contextRetrievalService.GetPartiesAsync(coveredByPartyIds, cancellationToken: cancellationToken);
                    if (coveredByPartys.Count > 0)
                    {
                        toParties.AddRange(coveredByPartys.Select(p => p.PartyUuid.Value));
                    }
                }
            }

            // 3. Get all instance delegations of the resource both directly delegated to user and indirectly through keyrole units
            if (includeInstanceDelegations && fromParty.HasValue && toParties.Count > 0)
            {
                if (resourceMatchType == ResourceAttributeMatchType.AltinnAppId)
                {
                    string[] resourceOrgApp = resourceId.Split('/');
                    resourceIds = $"app_{resourceOrgApp[0]}_{resourceOrgApp[1]}".SingleToList();
                }

                delegations.AddRange(await GetInstanceDelegations(resourceIds, fromParty.Value, toParties, cancellationToken));
            }

            return delegations;
        }

        private async Task<IEnumerable<DelegationChange>> GetInstanceDelegations(List<string> resourceIds, Guid from, List<Guid> to, CancellationToken cancellationToken = default)
        {
            IEnumerable<InstanceDelegationChange> instanceDelegations = await _delegationRepository.GetActiveInstanceDelegations(resourceIds, from, to, cancellationToken);
            return from InstanceDelegationChange instanceDelegation in instanceDelegations
                    let delegationChange = new DelegationChange
                    {
                        ResourceId = instanceDelegation.ResourceId,
                        InstanceId = instanceDelegation.InstanceId,
                        FromUuidType = instanceDelegation.FromUuidType,
                        FromUuid = instanceDelegation.FromUuid,
                        ToUuidType = instanceDelegation.ToUuidType,
                        ToUuid = instanceDelegation.ToUuid,
                        PerformedByUuidType = instanceDelegation.PerformedByType,
                        PerformedByUuid = instanceDelegation.PerformedBy,
                        DelegationChangeType = instanceDelegation.DelegationChangeType,
                        BlobStoragePolicyPath = instanceDelegation.BlobStoragePolicyPath,
                        BlobStorageVersionId = instanceDelegation.BlobStorageVersionId,
                        Created = instanceDelegation.Created
                    }
                    select delegationChange;
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

        private static void EnrichRightsDictionaryWithRightsFromPolicy(Dictionary<string, Right> rights, XacmlPolicy policy, RightSourceType policySourceType, List<AttributeMatch> subjectMatches, int minimumAuthenticationLevel = 0, int delegationOfferedByPartyId = 0, bool returnAllPolicyRights = false, bool getDelegableRights = false)
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
                    ICollection<XacmlContextAttributes> contextAttributes = PolicyHelper.GetContextAttributes(subjectMatches, ruleRight.Resource, ruleRight.Action.SingleToList());
                    XacmlContextRequest authRequest = new XacmlContextRequest(false, false, contextAttributes);

                    XacmlContextResponse response = pdp.Authorize(authRequest, singleRulePolicy);
                    XacmlContextResult decisionResult = response.Results.First();

                    // If getting rights for delegation, the right source is a delegation policy and the right does no longer exist in the app/resource policy: it should NOT be added as a delegable right
                    if (getDelegableRights && policySourceType == RightSourceType.DelegationPolicy && !rights.ContainsKey(ruleRight.RightKey))
                    {
                        continue;
                    }

                    if (!rights.TryGetValue(ruleRight.RightKey, out Right right))
                    {
                        rights.Add(ruleRight.RightKey, ruleRight);
                        right = ruleRight;
                    }

                    // If getting rights for delegation, the xacml decision is to be used for indicating if the user can delegate the right. Otherwise the decision indicate whether the user actually have the right.
                    if (getDelegableRights)
                    {
                        right.CanDelegate = (right.CanDelegate.HasValue && right.CanDelegate.Value) || decisionResult.Decision.Equals(XacmlContextDecision.Permit);
                    }
                    else
                    {
                        right.HasPermit = (right.HasPermit.HasValue && right.HasPermit.Value) || decisionResult.Decision.Equals(XacmlContextDecision.Permit);
                    }

                    if (decisionResult.Decision.Equals(XacmlContextDecision.Permit) || returnAllPolicyRights)
                    {
                        right.RightSources.Add(
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
                                UserSubjects = subjectMatches,
                                PolicySubjects = ruleSubjects
                            });
                    }
                }
            }
        }
    }
}
