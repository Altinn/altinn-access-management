using System.Text;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.Authentication;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Enums;
using Altinn.Authorization.ABAC.Constants;
using Altinn.Authorization.ABAC.Xacml;
using Altinn.Platform.Register.Models;
using Altinn.Urn.Json;

namespace Altinn.AccessManagement.Core.Helpers
{
    /// <summary>
    /// Delegation helper methods
    /// </summary>
    public static class DelegationHelper
    {
        /// <summary>
        /// Sort rules for delegation by delegation policy file path, i.e. Org/App/OfferedBy/CoveredBy
        /// </summary>
        /// <param name="rules">The list of rules to be sorted</param>
        /// <param name="unsortableRules">The list of rules not able to sort by org/app/offeredBy/CoveredBy</param>
        /// <returns>A dictionary with key being the filepath for the delegation policy file, and value being the list of rules to be written to the delegation policy</returns>
        public static Dictionary<string, List<Rule>> SortRulesByDelegationPolicyPath(List<Rule> rules, out List<Rule> unsortableRules)
        {
            Dictionary<string, List<Rule>> result = new Dictionary<string, List<Rule>>();
            unsortableRules = new List<Rule>();

            foreach (Rule rule in rules)
            {
                if (!TryGetDelegationPolicyPathFromRule(rule, out string path))
                {
                    unsortableRules.Add(rule);
                    continue;
                }

                if (result.ContainsKey(path))
                {
                    result[path].Add(rule);
                }
                else
                {
                    result.Add(path, new List<Rule> { rule });
                }
            }

            return result;
        }

        /// <summary>
        /// Tries to get the PartyId attribute value from a list of AttributeMatch models
        /// </summary>
        /// <returns>The true if party id is found as the single attribute in the collection</returns>
        public static bool TryGetPartyIdFromAttributeMatch(List<AttributeMatch> match, out int partyid)
        {
            partyid = 0;
            AttributeMatch partyIdAttribute = match?.Find(m => m.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute);
            if (partyIdAttribute != null)
            {
                return int.TryParse(partyIdAttribute.Value, out partyid) && partyid != 0;
            }

            return false;
        }

        /// <summary>
        /// Tries to get the Uuid attribute value from a list of AttributeMatch models and specifies which type it finds by setting based on the id in the attribute match
        /// </summary>
        /// <returns>The true if party id is found as the single attribute in the collection</returns>
        public static bool TryGetUuidFromAttributeMatch(List<AttributeMatch> match, out Guid uuid, out UuidType type)
        {
            uuid = Guid.Empty;
            type = UuidType.NotSpecified;

            if (match == null)
            {
                return false;
            }

            AttributeMatch currentAttributeMatch = match.Find(m => m.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.PersonUuid);
            if (currentAttributeMatch != null)
            {
                type = UuidType.Person;
                return Guid.TryParse(currentAttributeMatch.Value, out uuid) && uuid != Guid.Empty;
            }

            currentAttributeMatch = match.Find(m => m.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationUuid);
            if (currentAttributeMatch != null)
            {
                type = UuidType.Organization;
                return Guid.TryParse(currentAttributeMatch.Value, out uuid) && uuid != Guid.Empty;
            }

            currentAttributeMatch = match.Find(m => m.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.SystemUserUuid);
            if (currentAttributeMatch != null)
            {
                type = UuidType.SystemUser;
                return Guid.TryParse(currentAttributeMatch.Value, out uuid) && uuid != Guid.Empty;
            }

            currentAttributeMatch = match.Find(m => m.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.EnterpriseUserUuid);
            if (currentAttributeMatch != null)
            {
                type = UuidType.EnterpriseUser;
                return Guid.TryParse(currentAttributeMatch.Value, out uuid) && uuid != Guid.Empty;
            }

            return false;
        }

        /// <summary>
        /// Trys to get the UserId attribute value from a list of AttributeMatch models
        /// </summary>
        /// <returns>The true if user id is found as the single attribute in the collection</returns>
        public static bool TryGetUserIdFromAttributeMatch(List<AttributeMatch> match, out int userid)
        {
            userid = 0;
            AttributeMatch userIdAttribute = match?.Find(m => m.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute);
            if (userIdAttribute != null)
            {
                return int.TryParse(userIdAttribute.Value, out userid) && userid != 0;
            }

            return false;
        }

        /// <summary>
        /// Trys to get the organization number attribute value from a list of AttributeMatch models
        /// </summary>
        /// <returns>The true if organization number is found as the single attribute in the collection</returns>
        public static bool TryGetOrganizationNumberFromAttributeMatch(List<AttributeMatch> match, out string orgNo)
        {
            orgNo = string.Empty;
            if (match != null && match.Count == 1 && match[0].Id == AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationNumberAttribute)
            {
                orgNo = match[0].Value;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Trys to get the social security number attribute value from a list of AttributeMatch models
        /// </summary>
        /// <returns>The true if social security number is found as the single attribute in the collection</returns>
        public static bool TryGetSocialSecurityNumberAttributeMatch(List<AttributeMatch> match, out string ssn)
        {
            ssn = string.Empty;
            if (match != null && match.Count == 1 && match[0].Id == AltinnXacmlConstants.MatchAttributeIdentifiers.PersonId)
            {
                ssn = match[0].Value;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Trys to get both social security number and last name attribute value from a list of AttributeMatch models
        /// </summary>
        /// <returns>The true if both social security number and last name is found as the only attributes in the collection</returns>
        public static bool TryGetSocialSecurityNumberAndLastNameAttributeMatch(List<AttributeMatch> match, out string ssn, out string lastName)
        {
            ssn = match.Find(m => m.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.PersonId)?.Value;
            lastName = match.Find(m => m.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.PersonLastName)?.Value;

            if (match.Count == 2 && !string.IsNullOrWhiteSpace(ssn) && !string.IsNullOrWhiteSpace(lastName))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Trys to get both username and last name attribute value from a list of AttributeMatch models
        /// </summary>
        /// <returns>The true if both username and last name is found as the only attributes in the collection</returns>
        public static bool TryGetUsernameAndLastNameAttributeMatch(List<AttributeMatch> match, out string username, out string lastName)
        {
            username = match.Find(m => m.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.PersonUserName)?.Value;
            lastName = match.Find(m => m.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.PersonLastName)?.Value;

            if (match.Count == 2 && !string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(lastName))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Trys to get enterprise username attribute value from a list of AttributeMatch models
        /// </summary>
        /// <returns>The true if both enterprise username is found as the only attributes in the collection</returns>
        public static bool TryGetEnterpriseUserNameAttributeMatch(List<AttributeMatch> match, out string enterpriseUserName)
        {
            enterpriseUserName = string.Empty;
            if (match != null && match.Count == 1 && match[0].Id == AltinnXacmlConstants.MatchAttributeIdentifiers.EnterpriseUserName)
            {
                enterpriseUserName = match[0].Value;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Trys to get an single specific attribute value from a list of AttributeMatch models, if it's the only attribute in the list
        /// </summary>
        /// <returns>The true if person uuid is found as the only attributes in the collection</returns>
        public static bool TryGetSingleAttributeMatchValue(List<AttributeMatch> match, string matchAttributeIdentifier, out string value)
        {
            value = string.Empty;
            if (match != null && match.Count == 1 && match[0].Id == matchAttributeIdentifier)
            {
                value = match[0].Value;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if i given AttributeMatch list is in a list of defaultRights so it is valid for delegation to SystemUsers
        /// </summary>
        /// <param name="defaultRights">List of default rights to check if resource is included</param>
        /// <param name="resource">The rights to delegate to check for existence in list of defaultRights</param>
        /// <returns>true if all the rights is valid for in the given defaultRights list</returns>
        public static bool CheckResourceIsInListOfDefaultRights(List<DefaultRight> defaultRights, List<AttributeMatch> resource)
        {
            return defaultRights.Exists(defaultRight => CheckAllPartsInDefaultRightsIsInActualDelegatedResource(defaultRight.Resource, resource));
        }

        /// <summary>
        /// Compares a list of Resource details from allowed default rights with the list of Resource details in the actual delegation so all of the defined resource details
        /// in the allowed resource must be present in the actual delegation but the actual delegation can have more details narrowing the actual delegation more granulated than
        /// the allowed resource but not the other way round
        /// </summary>
        /// <param name="allowedResource">List describing the allowed resource</param>
        /// <param name="actualResource">List describing the actual delegation</param>
        /// <returns>True if the delegation is allowed false if not.</returns>
        public static bool CheckAllPartsInDefaultRightsIsInActualDelegatedResource(List<AttributeMatch> allowedResource, List<AttributeMatch> actualResource)
        {
            return allowedResource.TrueForAll(attributeMatch => actualResource.Exists(r => r.Id == attributeMatch.Id && r.Value == attributeMatch.Value));
        }

        /// <summary>
        /// Gets a int representation of the CoveredByUserId and CoverdByPartyId from an AttributeMatch list.
        /// This works under the asumptions that any valid search for å valid policy contains a CoveredBy and this must be in the form
        /// of a PartyId or a UserId. So any valid search containing a PartyId should not contain a USerId and vice versa.
        /// If the search does not contain any of those this should be considered as an invalid search.
        /// </summary>
        /// <param name="match">the list to fetch coveredBy from</param>
        /// <param name="coveredByUserId">The value for coveredByUserId or null if not present</param>
        /// <param name="coveredByPartyId">The value for coveredByPartyId or null if not present</param>
        /// <param name="coveredByUuid">The uuid of the covered by found in the AttributeMatch list</param>
        /// <param name="coveredByUuidType">The uuid type of the covered by found in the AttributeMatch list</param>
        /// <returns>The CoveredByUserId or CoveredByPartyId in the input AttributeMatch list as a string primarily used to create a policy path for fetching a delegated policy file.</returns>
        public static string GetCoveredByFromMatch(List<AttributeMatch> match, out int? coveredByUserId, out int? coveredByPartyId, out Guid? coveredByUuid, out UuidType coveredByUuidType)
        {
            bool validUser = TryGetUserIdFromAttributeMatch(match, out int coveredByUserIdTemp);
            bool validParty = TryGetPartyIdFromAttributeMatch(match, out int coveredByPartyIdTemp);
            bool validUuid = TryGetUuidFromAttributeMatch(match, out Guid coveredByUuidIdTemp, out coveredByUuidType);

            coveredByPartyId = validParty ? coveredByPartyIdTemp : null;
            coveredByUserId = validUser ? coveredByUserIdTemp : null;
            coveredByUuid = validUuid ? coveredByUuidIdTemp : null;

            if (validUser)
            {
                return coveredByUserIdTemp.ToString();
            }
            else if (validParty)
            {
                return coveredByPartyIdTemp.ToString();
            }
            else if (validUuid)
            {
                return coveredByUuidIdTemp.ToString();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the resource attribute values as out params from a Resource specified as a List of AttributeMatches
        /// </summary>
        /// <param name="input">The resource to fetch org and app from</param>
        /// <param name="resourceMatchType">the resource match type</param>
        /// <param name="resourceId">the resource id. Either a resource registry id or org/app</param>
        /// <param name="org">the org part of the resource</param>
        /// <param name="app">the app part of the resource</param>
        /// <param name="serviceCode">altinn 2 service code</param>
        /// <param name="serviceEditionCode">altinn 2 service edition code</param>
        /// <returns>A bool indicating whether params where found</returns>
        public static bool TryGetResourceFromAttributeMatch(List<AttributeMatch> input, out ResourceAttributeMatchType resourceMatchType, out string resourceId, out string org, out string app, out string serviceCode, out string serviceEditionCode)
        {
            resourceMatchType = ResourceAttributeMatchType.None;
            resourceId = null;
            org = null;
            app = null;
            serviceCode = null;
            serviceEditionCode = null;

            AttributeMatch resourceRegistryMatch = input.Find(am => am.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceRegistryAttribute);
            AttributeMatch orgMatch = input.Find(am => am.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute);
            AttributeMatch appMatch = input.Find(am => am.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute);
            AttributeMatch serviceCodeMatch = input.Find(am => am.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.ServiceCodeAttribute);
            AttributeMatch serviceEditionMatch = input.Find(am => am.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.ServiceEditionCodeAttribute);

            if (resourceRegistryMatch != null && orgMatch == null && appMatch == null)
            {
                resourceMatchType = ResourceAttributeMatchType.ResourceRegistry;
                resourceId = resourceRegistryMatch.Value;
                return true;
            }

            if (orgMatch != null && appMatch != null && resourceRegistryMatch == null)
            {
                resourceMatchType = ResourceAttributeMatchType.AltinnAppId;
                org = orgMatch.Value;
                app = appMatch.Value;
                resourceId = $"{org}/{app}";
                return true;
            }

            if (serviceCodeMatch != null && serviceEditionMatch != null && resourceRegistryMatch == null && orgMatch == null && appMatch == null)
            {
                resourceMatchType = ResourceAttributeMatchType.Altinn2Service;
                serviceCode = serviceCodeMatch.Value;
                serviceEditionCode = serviceEditionMatch.Value;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets ResourceType, ResourceRegistryId, Org, App, OfferedBy and CoveredBy as out params from a single Rule
        /// </summary>
        /// <returns>A bool indicating whether sufficent params where found</returns>
        public static bool TryGetDelegationParamsFromRule(Rule rule, out ResourceAttributeMatchType resourceMatchType, out string resourceId, out string org, out string app, out int offeredByPartyId, out Guid? fromUuid, out UuidType fromUuidType, out Guid? toUuid, out UuidType toUuidType, out int? coveredByPartyId, out int? coveredByUserId, out int? delegatedByUserId, out int? delegatedByPartyId, out DateTime delegatedDateTime)
        {
            resourceMatchType = ResourceAttributeMatchType.None;
            resourceId = null;
            org = null;
            app = null;
            offeredByPartyId = 0;
            fromUuid = null;
            fromUuidType = UuidType.NotSpecified;
            coveredByPartyId = null;
            coveredByUserId = null;
            toUuid = null;
            toUuidType = UuidType.NotSpecified;

            delegatedByUserId = null;
            delegatedByPartyId = null;
            delegatedDateTime = DateTime.UtcNow;

            try
            {
                TryGetResourceFromAttributeMatch(rule.Resource, out resourceMatchType, out resourceId, out org, out app, out string _, out string _);
                offeredByPartyId = rule.OfferedByPartyId;
                fromUuid = rule.OfferedByPartyUuid;
                fromUuidType = rule.OfferedByPartyType;
                coveredByPartyId = TryGetPartyIdFromAttributeMatch(rule.CoveredBy, out int coveredByParty) ? coveredByParty : null;
                coveredByUserId = TryGetUserIdFromAttributeMatch(rule.CoveredBy, out int coveredByUser) ? coveredByUser : null;
                bool validUuid = TryGetUuidFromAttributeMatch(rule.CoveredBy, out Guid toUuidTemp, out toUuidType);
                if (validUuid)
                {
                    toUuid = toUuidTemp;
                }

                delegatedByUserId = rule.DelegatedByUserId;
                delegatedByPartyId = rule.DelegatedByPartyId;
                delegatedDateTime = rule.DelegatedDateTime ?? DateTime.UtcNow;

                if (resourceMatchType != ResourceAttributeMatchType.None
                    && offeredByPartyId != 0
                    && (coveredByPartyId.HasValue || coveredByUserId.HasValue || toUuid.HasValue)
                    && (delegatedByUserId.HasValue || delegatedByPartyId.HasValue))
                {
                    return true;
                }
            }
            catch (Exception)
            {
                // Any exceptions here are caused by invalid input which should be handled and logged by the calling entity
            }

            return false;
        }

        /// <summary>
        /// Gets the delegation policy path for a single Rule
        /// </summary>
        /// <returns>A bool indicating whether necessary params to build the path where found</returns>
        public static bool TryGetDelegationPolicyPathFromRule(Rule rule, out string delegationPolicyPath)
        {
            delegationPolicyPath = null;
            if (TryGetDelegationParamsFromRule(rule, out ResourceAttributeMatchType resourceMatchType, out string resourceId, out string org, out string app, out int offeredBy, out Guid? fromUuid, out UuidType fromUuidType, out Guid? toUuid, out UuidType toUuidType, out int? coveredByPartyId, out int? coveredByUserId, out _, out _, out _))
            {
                try
                {
                    delegationPolicyPath = PolicyHelper.GetDelegationPolicyPath(resourceMatchType, resourceId, org, app, offeredBy.ToString(), coveredByUserId, coveredByPartyId, toUuid, toUuidType);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the delegation policy path for a single Rule
        /// </summary>
        /// <returns>A bool indicating whether necessary params to build the path where found</returns>
        public static bool TryGetDelegationPolicyPathFromInstanceRule(InstanceRight rule, out string instanceDelegationPolicyPath)
        {
            instanceDelegationPolicyPath = null;
            StringBuilder sb = new StringBuilder();

            sb.Append("resourceregistry");
            sb.Append('/');

            sb.Append(rule.ResourceId);
            sb.Append('/');

            sb.Append("instances");
            sb.Append('/');

            try
            {
                sb.Append(rule.InstanceId.AsFileName());
                sb.Append('/');
            }
            catch (Exception)
            {
                return false;
            }            
            
            sb.Append(rule.ToType);
            sb.Append('-');
            sb.Append(rule.ToUuid);
            sb.Append('/');

            sb.Append(rule.InstanceDelegationSource);
            sb.Append('/');
            
            sb.Append(rule.InstanceDelegationMode);
            sb.Append('/');

            sb.Append("delegationpolicy.xml");
            instanceDelegationPolicyPath = sb.ToString();

            return true;
        }

        /// <summary>
        /// Returns the count of unique Policies in a list of Rules
        /// </summary>
        /// <param name="rules">List of rules to check how many individual policies exist</param>
        /// <returns>count of policies</returns>
        public static int GetPolicyCount(List<Rule> rules)
        {
            List<string> policyPaths = new List<string>();
            foreach (Rule rule in rules)
            {
                bool pathOk = TryGetDelegationPolicyPathFromRule(rule, out string delegationPolicyPath);
                if (pathOk && !policyPaths.Contains(delegationPolicyPath))
                {
                    policyPaths.Add(delegationPolicyPath);
                }
            }

            return policyPaths.Count;
        }

        /// <summary>
        /// Returns the count of unique ruleids in a list dele
        /// </summary>
        /// <param name="rulesToDelete">List of rules and policies to check how many individual ruleids exist</param>
        /// <returns>count of rules</returns>
        public static int GetRulesCountToDeleteFromRequestToDelete(List<RequestToDelete> rulesToDelete)
        {
            int result = 0;
            foreach (RequestToDelete ruleToDelete in rulesToDelete)
            {
                result += ruleToDelete.RuleIds.Count;
            }

            return result;
        }

        /// <summary>
        /// Checks whether the provided XacmlPolicy contains a rule having an identical Resource signature and contains the Action from the rule,
        /// to be used for checking for duplicate rules in delegation, or that the rule exists in the Apps Xacml policy.
        /// </summary>
        /// <returns>A bool</returns>
        public static bool PolicyContainsMatchingInstanceRule(XacmlPolicy policy, InstanceRule rule)
        {
            string ruleResourceKey = GetAttributeMatchKey(rule.Resource.ToList());

            foreach (XacmlRule policyRule in policy.Rules)
            {
                if (!policyRule.Effect.Equals(XacmlEffectType.Permit) || policyRule.Target == null)
                {
                    continue;
                }

                bool matchingActionFound = MatchingActionFound(policyRule.Target.AnyOf, rule, out List<List<AttributeMatch>> policyResourceMatches);
                
                if (policyResourceMatches.Exists(resourceMatch => GetAttributeMatchKey(resourceMatch) == ruleResourceKey) && matchingActionFound)
                {
                    rule.RuleId = policyRule.RuleId;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns Xacml rule in the provided XacmlPolicy that contains a rule having an identical Resource signature and contains the Action from the InstanceRule null if no match,
        /// to be used for finding rules to revoke.
        /// </summary>
        /// <returns>A bool</returns>
        public static XacmlRule GetXamlRuleContainsMatchingInstanceRule(XacmlPolicy policy, InstanceRule rule)
        {
            string ruleResourceKey = GetAttributeMatchKey(rule.Resource.ToList());

            foreach (XacmlRule policyRule in policy.Rules)
            {
                if (!policyRule.Effect.Equals(XacmlEffectType.Permit) || policyRule.Target == null)
                {
                    continue;
                }

                bool matchingActionFound = MatchingActionFound(policyRule.Target.AnyOf, rule, out List<List<AttributeMatch>> policyResourceMatches);

                if (policyResourceMatches.Exists(resourceMatch => GetAttributeMatchKey(resourceMatch) == ruleResourceKey) && matchingActionFound)
                {
                    rule.RuleId = policyRule.RuleId;
                    return policyRule;
                }
            }

            return null;
        }

        private static bool MatchingActionFound(ICollection<XacmlAnyOf> input, InstanceRule rule, out List<List<AttributeMatch>> policyResourceMatches)
        {
            policyResourceMatches = [];
            bool matchingActionFound = false;

            foreach (XacmlAnyOf anyOf in input)
            {
                foreach (XacmlAllOf allOf in anyOf.AllOf)
                {
                    matchingActionFound = GetResourceMatch(allOf.Matches, rule, out List<AttributeMatch> resourceMatch);
                    
                    if (resourceMatch.Count > 0)
                    {
                        policyResourceMatches.Add(resourceMatch);
                    }
                }
            }

            return matchingActionFound;
        }

        private static bool GetResourceMatch(ICollection<XacmlMatch> input, InstanceRule rule, out List<AttributeMatch> resourceMatch)
        {
            resourceMatch = [];
            bool matchingActionFound = false;
            foreach (XacmlMatch xacmlMatch in input)
            {
                if (xacmlMatch.AttributeDesignator.Category.Equals(XacmlConstants.MatchAttributeCategory.Resource))
                {
                    resourceMatch.Add(new AttributeMatch { Id = xacmlMatch.AttributeDesignator.AttributeId.OriginalString, Value = xacmlMatch.AttributeValue.Value });
                }
                else if (xacmlMatch.AttributeDesignator.Category.Equals(XacmlConstants.MatchAttributeCategory.Action) &&
                    xacmlMatch.AttributeDesignator.AttributeId.OriginalString == rule.Action.PrefixSpan.ToString() &&
                    xacmlMatch.AttributeValue.Value == rule.Action.ValueSpan.ToString())
                {
                    matchingActionFound = true;
                }
            }

            return matchingActionFound;
        }

        /// <summary>
        /// Checks whether the provided XacmlPolicy contains a rule having an identical Resource signature and contains the Action from the rule,
        /// to be used for checking for duplicate rules in delegation, or that the rule exists in the Apps Xacml policy.
        /// </summary>
        /// <returns>A bool</returns>
        public static bool PolicyContainsMatchingRule(XacmlPolicy policy, Rule rule)
        {
            string ruleResourceKey = GetAttributeMatchKey(rule.Resource);

            foreach (XacmlRule policyRule in policy.Rules)
            {
                if (!policyRule.Effect.Equals(XacmlEffectType.Permit) || policyRule.Target == null)
                {
                    continue;
                }

                List<List<AttributeMatch>> policyResourceMatches = new List<List<AttributeMatch>>();
                bool matchingActionFound = false;
                foreach (XacmlAnyOf anyOf in policyRule.Target.AnyOf)
                {
                    foreach (XacmlAllOf allOf in anyOf.AllOf)
                    {
                        List<AttributeMatch> resourceMatch = new List<AttributeMatch>();
                        foreach (XacmlMatch xacmlMatch in allOf.Matches)
                        {
                            if (xacmlMatch.AttributeDesignator.Category.Equals(XacmlConstants.MatchAttributeCategory.Resource))
                            {
                                resourceMatch.Add(new AttributeMatch { Id = xacmlMatch.AttributeDesignator.AttributeId.OriginalString, Value = xacmlMatch.AttributeValue.Value });
                            }
                            else if (xacmlMatch.AttributeDesignator.Category.Equals(XacmlConstants.MatchAttributeCategory.Action) &&
                                xacmlMatch.AttributeDesignator.AttributeId.OriginalString == rule.Action.Id &&
                                xacmlMatch.AttributeValue.Value == rule.Action.Value)
                            {
                                matchingActionFound = true;
                            }
                        }

                        if (resourceMatch.Any())
                        {
                            policyResourceMatches.Add(resourceMatch);
                        }
                    }
                }

                if (policyResourceMatches.Any(resourceMatch => GetAttributeMatchKey(resourceMatch) == ruleResourceKey) && matchingActionFound)
                {
                    rule.RuleId = policyRule.RuleId;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets a string key representing the a list of attributematches
        /// </summary>
        /// <returns>A key string</returns>
        public static string GetAttributeMatchKey(List<AttributeMatch> attributeMatches)
        {
            return string.Concat(attributeMatches.OrderBy(r => r.Id).Select(r => r.Id + r.Value));
        }

        /// <summary>
        /// Gets a string key representing the a list of attributematches
        /// </summary>
        /// <returns>A key string</returns>
        public static string GetAttributeMatchKey(List<UrnJsonTypeValue> attributeMatches)
        {
            return string.Concat(attributeMatches.OrderBy(r => r.Value.KeySpan.ToString()).Select(r => r.Value.PrefixSpan.ToString() + r.Value.ValueSpan.ToString()));
        }

        /// <summary>
        /// Extract UuidType and Uuid from a AttributeMatch list
        /// </summary>
        /// <param name="performer">The lsit to fetch data from</param>
        /// <param name="id">The id of the party in the</param>
        /// <param name="type">The resulting type</param>
        /// <returns>true if a valid type and id was extracted else false</returns>
        public static bool TryGetPerformerFromAttributeMatches(IEnumerable<AttributeMatch> performer, out string id, out UuidType type)
        {
            string org = null, app = null, person = null, organization = null, enterpriseUser = null, systemUser = null;
            id = null;
            type = UuidType.NotSpecified;
            int counter = 0;

            foreach (AttributeMatch match in performer)
            {
                counter++;
                switch (match.Id)
                {
                    case AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute:
                        org = match.Value;
                        break;
                    case AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute:
                        app = match.Value;
                        break;
                    case AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationUuid:
                        organization = match.Value;
                        break;
                    case AltinnXacmlConstants.MatchAttributeIdentifiers.PersonUuid:
                        person = match.Value;
                        break;
                    case AltinnXacmlConstants.MatchAttributeIdentifiers.EnterpriseUserUuid:
                        enterpriseUser = match.Value;
                        break;
                    case AltinnXacmlConstants.MatchAttributeIdentifiers.SystemUserUuid:
                        systemUser = match.Value;
                        break;
                }
            }

            if (org != null && app != null && person == null && organization == null && enterpriseUser == null && systemUser == null && counter == 2)
            {
                id = $"app_{org}_{app}";
                type = UuidType.Resource;
                return true;
            }

            if (org == null && app == null && person != null && organization == null && enterpriseUser == null && systemUser == null && counter == 1)
            {
                id = person;
                type = UuidType.Person;
                return true;
            }
            
            if (org == null && app == null && person == null && organization != null && enterpriseUser == null && systemUser == null && counter == 1)
            {
                id = organization;
                type = UuidType.Organization;
                return true;
            }
            
            if (org == null && app == null && person == null && organization == null && enterpriseUser != null && systemUser == null && counter == 1)
            {
                id = enterpriseUser;
                type = UuidType.EnterpriseUser;
                return true;
            }
            
            if (org == null && app == null && person == null && organization == null && enterpriseUser == null && systemUser != null && counter == 1)
            {
                id = systemUser;
                type = UuidType.SystemUser;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the RuleType on each rule in the given list
        /// </summary>
        public static void SetRuleType(List<Rule> rulesList, int offeredByPartyId, List<int> keyRolePartyIds, List<AttributeMatch> coveredBy, int parentPartyId = 0)
        {
            foreach (Rule rule in rulesList)
            {
                if (TryGetDelegationParamsFromRule(rule, out _, out _, out _, out _, out _, out Guid? fromUuid, out UuidType fromUuidType, out Guid? toUuid, out UuidType toUuidType, out int? coveredByPartyId, out int? coveredByUserId, out _, out _, out _)
                    && rule.Type == RuleType.None)
                {
                    SetTypeForSingleRule(keyRolePartyIds, offeredByPartyId, coveredBy, parentPartyId, rule, coveredByPartyId, coveredByUserId);
                }
            }
        }

        /// <summary>
        /// Extracts the (assumed) party ID from the given 'who' string. 
        /// </summary>
        /// <param name="who">
        /// Who, valid values are an organization number, or a party ID (the letter R followed by 
        /// the party ID as used in SBL).
        /// </param>
        /// <returns>Party ID extracted from 'who', or NULL if 'who' contains no party id.</returns>
        public static int? TryParsePartyId(string who)
        {
            int partyId;
            if (!int.TryParse(who, out partyId))
            {
                return 0;
            }

            return partyId;
        }

        /// <summary>
        /// Gets the reference value for a given resourcereference type
        /// </summary>
        /// <param name="resource">resource</param>
        /// <param name="referenceSource">reference source</param>
        /// <param name="referenceType">reference type</param>
        public static string GetReferenceValue(ServiceResource resource, ReferenceSource referenceSource, ReferenceType referenceType)
        {
            ResourceReference reference = resource.ResourceReferences.Find(rf => rf.ReferenceSource == referenceSource && rf.ReferenceType == referenceType);
            return reference.Reference;
        }

        /// <summary>
        /// Builds a RequestToDelete request model for revoking all delegated rules for a resource registry service
        /// </summary>
        public static List<RequestToDelete> GetRequestToDeleteResourceRegistryService(int authenticatedUserId, string resourceRegistryId, int fromPartyId, int toPartyId)
        {
            return new List<RequestToDelete>
            {
                new RequestToDelete
                {
                    DeletedByUserId = authenticatedUserId,
                    PolicyMatch = new PolicyMatch
                    {
                        OfferedByPartyId = fromPartyId,
                        CoveredBy = new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, Value = toPartyId.ToString() }.SingleToList(),
                        Resource = new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceRegistryAttribute, Value = resourceRegistryId }.SingleToList()
                    }
                }
            };
        }

        /// <summary>
        /// Builds a RequestToDelete request model for revoking all delegated rules for the resource if delegated between the from and to parties
        /// </summary>
        public static List<RequestToDelete> GetRequestToDeleteResource(int authenticatedUserId, IEnumerable<AttributeMatch> resource, int fromPartyId, AttributeMatch to)
        {
            return new List<RequestToDelete>
            {
                new RequestToDelete
                {
                    DeletedByUserId = authenticatedUserId,
                    PolicyMatch = new PolicyMatch
                    {
                        OfferedByPartyId = fromPartyId,
                        CoveredBy = to.SingleToList(),
                        Resource = resource.ToList()
                    }
                }
            };
        }

        /// <summary>
        /// Gets the list of Rules as a list of RightDelegationResult
        /// </summary>
        /// <param name="rules">The rules output from a delegation to convert</param>
        /// <returns>List of RightDelegationResult</returns>
        public static List<RightDelegationResult> GetRightDelegationResultsFromRules(List<Rule> rules)
        {
            return rules.Select(rule => new RightDelegationResult
            {
                Resource = rule.Resource,
                Action = rule.Action,
                Status = rule.CreatedSuccessfully ? DelegationStatus.Delegated : DelegationStatus.NotDelegated
            }).ToList();
        }

        /// <summary>
        /// Gets the list of Rules as a list of RightDelegationResult
        /// </summary>
        /// <param name="rules">The rules output from a delegation to convert</param>
        /// <returns>List of RightDelegationResult</returns>
        public static IEnumerable<InstanceRightDelegationResult> GetRightDelegationResultsFromInstanceRules(InstanceRight rules)
        {
            return rules.InstanceRules.Select(rule => new InstanceRightDelegationResult
            {
                Resource = rule.Resource,
                Action = rule.Action,
                Status = rule.CreatedSuccessfully ? DelegationStatus.Delegated : DelegationStatus.NotDelegated
            });
        }

        /// <summary>
        /// Gets the list of Rules as a list of RightDelegationResult
        /// </summary>
        /// <param name="rules">The rules output from a delegation to convert</param>
        /// <returns>List of RightDelegationResult</returns>
        public static IEnumerable<InstanceRightRevokeResult> GetRightRevokeResultsFromInstanceRules(InstanceRight rules)
        {
            return rules.InstanceRules.Select(rule => new InstanceRightRevokeResult
            {
                Resource = rule.Resource,
                Action = rule.Action,
                Status = rule.CreatedSuccessfully ? RevokeStatus.Revoked : RevokeStatus.NotRevoked
            });
        }

        /// <summary>
        /// Gets the list of Rights as a list of RightDelegationResult
        /// </summary>
        /// <param name="rights">The rights to convert</param>
        /// <returns>List of RightDelegationResult</returns>
        public static List<RightDelegationResult> GetRightDelegationResultsFromFailedRights(List<Right> rights)
        {
            return rights.Select(right => new RightDelegationResult
            {
                Resource = right.Resource,
                Action = right.Action,
                Status = DelegationStatus.NotDelegated
            }).ToList();
        }

        /// <summary>
        /// Gets the list of Rights as a list of RightDelegationResult
        /// </summary>
        /// <param name="rights">The rights to convert</param>
        /// <returns>List of RightDelegationResult</returns>
        public static IEnumerable<InstanceRightDelegationResult> GetRightDelegationResultsFromFailedInternalRights(List<RightInternal> rights)
        {
            return rights.Select(right => new InstanceRightDelegationResult
            {
                Resource = right.Resource,
                Action = right.Action,
                Status = DelegationStatus.NotDelegated
            });
        }

        /// <summary>
        /// Evaluates the party type in order to return the correct uuid type and value for a party
        /// </summary>
        /// <param name="party">The party to get uuid info from</param>
        /// <returns>UuidType and Uuid</returns>
        public static (UuidType DelegationType, Guid Uuid) GetUuidTypeAndValueFromParty(Party party)
        {
            if (party?.Organization != null)
            {
                return (UuidType.Organization, party.PartyUuid.Value);
            }
            else if (party?.Person != null)
            {
                return (UuidType.Person, party.PartyUuid.Value);
            }

            return (UuidType.NotSpecified, Guid.Empty);
        }

        /// <summary>
        /// Gets the list of Rights as a list of RightDelegationResult
        /// </summary>
        /// <param name="rights">The rights to convert</param>
        /// <returns>List of RightDelegationResult</returns>
        public static IEnumerable<InstanceRightRevokeResult> GetRightRevokeResultsFromFailedInternalRights(List<RightInternal> rights)
        {
            return rights.Select(right => new InstanceRightRevokeResult
            {
                Resource = right.Resource,
                Action = right.Action,
                Status = RevokeStatus.NotRevoked
            });
        }

        private static void SetTypeForSingleRule(List<int> keyRolePartyIds, int offeredByPartyId, List<AttributeMatch> coveredBy, int parentPartyId, Rule rule, int? coveredByPartyId, int? coveredByUserId)
        {
            bool isUserId = TryGetUserIdFromAttributeMatch(coveredBy, out int coveredByUserIdFromRequest);
            bool isPartyId = TryGetPartyIdFromAttributeMatch(coveredBy, out int coveredByPartyIdFromRequest);

            if (((isUserId && coveredByUserIdFromRequest == coveredByUserId) || (isPartyId && coveredByPartyIdFromRequest == coveredByPartyId))
                && rule.OfferedByPartyId == offeredByPartyId)
            {
                rule.Type = RuleType.DirectlyDelegated;
            }
            else if (isUserId && keyRolePartyIds.Any(id => id == coveredByPartyId) && rule.OfferedByPartyId == offeredByPartyId)
            {
                rule.Type = RuleType.InheritedViaKeyRole;
            }
            else if (((isUserId && coveredByUserIdFromRequest == coveredByUserId) || (isPartyId && coveredByPartyIdFromRequest == coveredByPartyId))
                && parentPartyId != 0 && rule.OfferedByPartyId == parentPartyId)
            {
                rule.Type = RuleType.InheritedAsSubunit;
            }
            else if (isUserId && keyRolePartyIds.Any(id => id == coveredByPartyId) && parentPartyId != 0 && rule.OfferedByPartyId == parentPartyId)
            {
                rule.Type = RuleType.InheritedAsSubunitViaKeyrole;
            }
            else
            {
                rule.Type = RuleType.None;
            }
        }
    }
}
