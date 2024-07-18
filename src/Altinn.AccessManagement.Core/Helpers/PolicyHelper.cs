using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Text;
using System.Xml;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Enums;
using Altinn.Authorization.ABAC.Constants;
using Altinn.Authorization.ABAC.Utils;
using Altinn.Authorization.ABAC.Xacml;

namespace Altinn.AccessManagement.Core.Helpers
{
    /// <summary>
    /// Policy helper methods
    /// </summary>
    public static class PolicyHelper
    {
        private const string CoveredByNotDefined = "CoveredBy was not defined";
        
        /// <summary>
        /// Extracts a list of all roles codes mentioned in a permit rule in a policy. 
        /// </summary>
        /// <param name="policy">The policy</param>
        /// <returns>List of role codes</returns>
        public static List<string> GetRolesWithAccess(XacmlPolicy policy)
        {
            HashSet<string> roleCodes = new HashSet<string>();

            foreach (XacmlRule rule in policy.Rules)
            {
                if (rule.Effect.Equals(XacmlEffectType.Permit) && rule.Target != null)
                {
                    foreach (XacmlAnyOf anyOf in rule.Target.AnyOf)
                    {
                        foreach (XacmlAllOf allOf in anyOf.AllOf)
                        {
                            foreach (XacmlMatch xacmlMatch in allOf.Matches)
                            {
                                if (xacmlMatch.AttributeDesignator.AttributeId.Equals(AltinnXacmlConstants.MatchAttributeIdentifiers.RoleAttribute))
                                {
                                    roleCodes.Add(xacmlMatch.AttributeValue.Value);
                                }
                            }
                        }
                    }
                }
            }

            return roleCodes.ToList();
        }

        /// <summary>
        /// Finds the correct policy path based on a XacmlContextRequest
        /// </summary>
        /// <param name="request">Xacml context request to use for finding the org and app for building the path</param>
        /// <returns></returns>
        public static string GetPolicyPath(XacmlContextRequest request)
        {
            string org = string.Empty;
            string app = string.Empty;

            foreach (XacmlContextAttributes attr in request.Attributes.Where(attr => attr.Category.OriginalString.Equals(XacmlConstants.MatchAttributeCategory.Resource)))
            {
                foreach (XacmlAttribute asd in attr.Attributes)
                {
                    if (asd.AttributeId.OriginalString.Equals(AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute))
                    {
                        XacmlAttributeValue orgAttr = asd.AttributeValues.FirstOrDefault();
                        org = orgAttr != null ? orgAttr.Value : string.Empty;
                    }

                    if (asd.AttributeId.OriginalString.Equals(AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute))
                    {
                        XacmlAttributeValue appAttr = asd.AttributeValues.FirstOrDefault();
                        app = appAttr != null ? appAttr.Value : string.Empty;
                    }
                }
            }

            return GetAltinnAppsPolicyPath(org, app);
        }

        /// <summary>
        /// Builds the policy path based on org and app names
        /// </summary>
        /// <param name="org">The organization name/identifier</param>
        /// <param name="app">The altinn app name</param>
        /// <returns></returns>
        public static string GetAltinnAppsPolicyPath(string org, string app)
        {
            if (string.IsNullOrWhiteSpace(org))
            {
                throw new ArgumentException("Org was not defined");
            }

            if (string.IsNullOrWhiteSpace(app))
            {
                throw new ArgumentException("App was not defined");
            }

            return $"{org.AsFileName()}/{app.AsFileName()}/policy.xml";
        }

        /// <summary>
        /// Builds the policy path based on resourceRegistryId
        /// </summary>
        /// <param name="resourceRegistryId">The resource registry Id</param>
        /// <returns>Returns the path to the policyfile.</returns>
        public static string GetResourceRegistryPolicyPath(string resourceRegistryId)
        {
            if (string.IsNullOrWhiteSpace(resourceRegistryId))
            {
                throw new ArgumentException("ResourceRegistryId was not defined");
            }

            return $"{resourceRegistryId.AsFileName()}/resourcepolicy.xml";
        }

        /// <summary>
        /// Creates a Rule representation based on a search and a xacmlRule found in a XacmlPolicyFile based on the search
        /// </summary>
        /// <param name="search">The search used to find the correct rule</param>
        /// <param name="xacmlRule">XacmlRule found by the search param to enrich the result with Action and Resource</param>
        /// <returns>The created Rule</returns>
        public static Rule CreateRuleFromPolicyAndRuleMatch(RequestToDelete search, XacmlRule xacmlRule)
        {
            Rule rule = new Rule
            {
                RuleId = xacmlRule.RuleId,
                CreatedSuccessfully = true,
                DelegatedByUserId = search.DeletedByUserId,
                OfferedByPartyId = search.PolicyMatch.OfferedByPartyId,
                CoveredBy = search.PolicyMatch.CoveredBy,
                Resource = GetResourceFromXcamlRule(xacmlRule),
                Action = GetActionValueFromRule(xacmlRule)
            };

            return rule;
        }

        /// <summary>
        /// Builds the delegation policy path based on org and app names, as well as identifiers for the delegating and receiving entities
        /// </summary>
        /// <param name="resourceMatchType">the resource match type</param>
        /// <param name="resourceId">The id of the resource. Either a resource registry id or org/app</param>
        /// <param name="org">The organization name/identifier</param>
        /// <param name="app">The altinn app name</param>
        /// <param name="offeredBy">The party id of the entity offering the delegated the policy</param>
        /// <param name="coveredByUserId">The user id of the entity having received the delegated policy or null if party id</param>
        /// <param name="coveredByPartyId">The party id of the entity having received the delegated policy or null if user id</param>
        /// <param name="coveredByUuid">the uuid of the coveredBy only valid value when the receiver is a system user</param>
        /// <param name="uuidType">the type of uuid to set as prefix for the coveredBy</param> 
        /// <returns>policypath matching input data</returns>
        public static string GetDelegationPolicyPath(ResourceAttributeMatchType resourceMatchType, string resourceId, string org, string app, string offeredBy, int? coveredByUserId, int? coveredByPartyId, Guid? coveredByUuid, UuidType uuidType)
        {
            if (string.IsNullOrWhiteSpace(offeredBy))
            {
                throw new ArgumentException("OfferedBy was not defined");
            }

            if (coveredByPartyId == null && coveredByUserId == null && coveredByUuid == null)
            {
                throw new ArgumentException(CoveredByNotDefined);
            }

            if (coveredByPartyId <= 0)
            {
                throw new ArgumentException(CoveredByNotDefined);
            }

            if (coveredByUserId <= 0)
            {
                throw new ArgumentException(CoveredByNotDefined);
            }

            if (coveredByUuid == Guid.Empty)
            {
                throw new ArgumentException(CoveredByNotDefined);
            }

            string coveredByPrefix;
            string coveredBy;
            if (coveredByPartyId != null)
            {
                coveredByPrefix = "p";
                coveredBy = coveredByPartyId.ToString();
            }
            else if (coveredByUserId != null)
            {
                coveredByPrefix = "u";
                coveredBy = coveredByUserId.ToString();
            }
            else
            {
                coveredByPrefix = uuidType.ToString();
                coveredBy = coveredByUuid.ToString();
            }

            if (resourceMatchType == ResourceAttributeMatchType.None)
            {
                throw new ArgumentException("Resource could not be identified. Resource must be for either a single resource from the resource registry or for a single Altinn app identified by org owner and app name");
            }

            if (resourceMatchType == ResourceAttributeMatchType.ResourceRegistry)
            {
                if (string.IsNullOrWhiteSpace(resourceId))
                {
                    throw new ArgumentException("ResourceRegistryId was not defined");
                }

                return $"resourceregistry/{resourceId.AsFileName()}/{offeredBy.AsFileName()}/{coveredByPrefix}{coveredBy.AsFileName()}/delegationpolicy.xml";
            }

            if (resourceMatchType == ResourceAttributeMatchType.AltinnAppId)
            {
                if (string.IsNullOrWhiteSpace(org))
                {
                    throw new ArgumentException("Org was not defined");
                }

                if (string.IsNullOrWhiteSpace(app))
                {
                    throw new ArgumentException("App was not defined");
                }

                return $"{org.AsFileName()}/{app.AsFileName()}/{offeredBy.AsFileName()}/{coveredByPrefix}{coveredBy.AsFileName()}/delegationpolicy.xml";
            }

            throw new ArgumentException("Unable to build a valid delegation policy path from the provided parameters");
        }

        /// <summary>
        /// Builds the delegation policy path based on input policyMatch
        /// </summary>
        /// <param name="policyMatch">param to build policypath from</param>
        /// <returns>policypath matching input data</returns>
        public static string GetAltinnAppDelegationPolicyPath(PolicyMatch policyMatch)
        {
            DelegationHelper.TryGetResourceFromAttributeMatch(policyMatch.Resource, out ResourceAttributeMatchType resourceMatchType, out string resourceId, out string org, out string app, out string _, out string _);
            DelegationHelper.GetCoveredByFromMatch(policyMatch.CoveredBy, out int? coveredByUserId, out int? coveredByPartyId, out Guid? coveredByUuid, out UuidType coveredByUuidType);

            return GetDelegationPolicyPath(resourceMatchType, resourceId, org, app, policyMatch.OfferedByPartyId.ToString(), coveredByUserId, coveredByPartyId, coveredByUuid, coveredByUuidType);
        }

        /// <summary>
        /// Takes the file IO stream and parses the policy file to a XacmlPolicy <see cref="XacmlPolicy"/>
        /// </summary>
        /// <param name="stream">The file IO stream</param>
        /// <returns>XacmlPolicy</returns>
        public static XacmlPolicy ParsePolicy(Stream stream)
        {
            stream.Position = 0;
            XacmlPolicy policy;
            using (XmlReader reader = XmlReader.Create(stream))
            {
                policy = XacmlParser.ParseXacmlPolicy(reader);
            }

            return policy;
        }

        /// <summary>
        /// Serializes the XacmlPolicy <see cref="XacmlPolicy"/> to Xml and returns it as a Memory stream
        /// </summary>
        /// <param name="policy">The XacmlPolicy model to serialize to a memory stream</param>
        /// <returns>MemoryStream of the Xml serialized policy</returns>
        public static MemoryStream GetXmlMemoryStreamFromXacmlPolicy(XacmlPolicy policy)
        {
            MemoryStream stream = new MemoryStream();
            XmlWriter writer = XmlWriter.Create(stream);

            XacmlSerializer.WritePolicy(writer, policy);

            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Check the input and returns a vale for CoveredBy and a urn to be used when creating a Attribute match given taht the covered by could be a user, party or SystemUser.
        /// </summary>
        /// <param name="coveredByPartyId">PartyId to evaluate for coveredBy</param>
        /// <param name="coveredByUserId">UserId to evaluate for coveredBy</param>
        /// <param name="toUuid">Uuid to evaluate for coveredBy</param>
        /// <param name="toType">The type of covered by to evaluate for type and chose what input to use for covered by value</param>
        /// <returns>coveredBy value and type of value</returns>
        /// <exception cref="ArgumentException">When no valid coveredBy is defined</exception>
        public static (string CoveredBy, string CoveredByType) GetCoveredByAndType(int? coveredByPartyId, int? coveredByUserId, Guid? toUuid, UuidType toType)
        {
            string coveredBy = null;
            string coveredByType = null;

            if (coveredByPartyId.HasValue)
            {
                coveredBy = coveredByPartyId.Value.ToString();
                coveredByType = AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute;
            }
            else if (coveredByUserId.HasValue)
            {
                coveredBy = coveredByUserId.Value.ToString();
                coveredByType = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute;
            }
            else if (toType.Equals(UuidType.SystemUser))
            {
                coveredBy = toUuid.ToString();
                coveredByType = toType.EnumMemberAttributeValueOrName();
            }

            if (coveredBy == null)
            {
                throw new ArgumentException($"No valid coveredBy was provided");
            }

            return (coveredBy, coveredByType);
        }
        
        /// <summary>
        /// Builds a XacmlPolicy <see cref="XacmlPolicy"/> representation based on the DelegationPolicy input
        /// </summary>
        /// <param name="resourceId">The identifier of the resource, either a resource in the resource registry or altinn app</param>
        /// <param name="offeredByPartyId">The party id of the entity offering the delegated the policy</param>
        /// <param name="coveredByPartyId">The party of the entity having received the delegated policy, if the receiving entity is an organization</param>
        /// <param name="coveredByUserId">The user id of the entity having received the delegated policy, if the receiving entity is a user</param>
        /// <param name="toUuid">The uuid id of the entity having received the delegated policy</param>
        /// <param name="toType">The uuid type id of the entity having received the delegated policy</param>
        /// <param name="rules">The set of rules to be delegated</param>
        public static XacmlPolicy BuildDelegationPolicy(string resourceId, int offeredByPartyId, int? coveredByPartyId, int? coveredByUserId, Guid? toUuid, UuidType toType, IList<Rule> rules)
        {
            XacmlPolicy delegationPolicy = new XacmlPolicy(new Uri($"{AltinnXacmlConstants.Prefixes.PolicyId}{1}"), new Uri(XacmlConstants.CombiningAlgorithms.PolicyDenyOverrides), new XacmlTarget(new List<XacmlAnyOf>()));
            delegationPolicy.Version = "1.0";

            (string coveredBy, string coveredByType) = GetCoveredByAndType(coveredByPartyId, coveredByUserId, toUuid, toType);

            delegationPolicy.Description = $"Delegation policy containing all delegated rights/actions from {offeredByPartyId} to {coveredBy}, for the resource; {resourceId}";

            foreach (Rule rule in rules)
            {
                if (!DelegationHelper.PolicyContainsMatchingRule(delegationPolicy, rule))
                {
                    delegationPolicy.Rules.Add(BuildDelegationRule(resourceId, offeredByPartyId, coveredBy, coveredByType, rule));
                }
            }

            return delegationPolicy;
        }

        /// <summary>
        /// Builds a XacmlRule <see cref="XacmlRule"/> representation based on the Rule input
        /// </summary>
        /// <param name="resourceId">The identifier of the resource, either a resource in the resource registry or altinn app</param>
        /// <param name="offeredByPartyId">The party id of the entity offering the delegated the policy</param>
        /// <param name="coveredBy">The id of the entity having received the delegated policy</param>
        /// <param name="toType">The type of the entity having received the delegated policy</param>
        /// <param name="rule">The rule to be delegated</param>
        public static XacmlRule BuildDelegationRule(string resourceId, int offeredByPartyId, string coveredBy, string toType, Rule rule)
        {
            rule.RuleId = Guid.NewGuid().ToString();
            
            XacmlRule delegationRule = new XacmlRule(rule.RuleId, XacmlEffectType.Permit)
            {
                Description = $"Delegation of a right/action from {offeredByPartyId} to {coveredBy}, for the resource: {resourceId}, by user; {rule.DelegatedByUserId}",
                Target = BuildDelegationRuleTarget(coveredBy, toType, rule)
            };
            return delegationRule;
        }

        /// <summary>
        /// Builds a XacmlTarget <see cref="XacmlTarget"/> representation based on the Rule input
        /// </summary>
        /// <param name="coveredBy">The the entity having received the delegated policy</param>
        /// <param name="toType">The type of identifier received the delegated policy user, party or system user</param>
        /// <param name="rule">The rule to be delegated</param>
        public static XacmlTarget BuildDelegationRuleTarget(string coveredBy, string toType, Rule rule)
        {
            List<XacmlAnyOf> targetList = new List<XacmlAnyOf>();

            // Build Subject
            List<XacmlAllOf> subjectAllOfs = new List<XacmlAllOf>();
            
            subjectAllOfs.Add(new XacmlAllOf(new List<XacmlMatch>
            {
                new XacmlMatch(
                    new Uri(XacmlConstants.AttributeMatchFunction.StringEqual),
                    new XacmlAttributeValue(new Uri(XacmlConstants.DataTypes.XMLString), coveredBy),
                    new XacmlAttributeDesignator(new Uri(XacmlConstants.MatchAttributeCategory.Subject), new Uri(toType), new Uri(XacmlConstants.DataTypes.XMLString), false))
            }));
            
            // Build Resource
            List<XacmlMatch> resourceMatches = new List<XacmlMatch>();
            foreach (AttributeMatch resourceMatch in rule.Resource)
            {
                resourceMatches.Add(
                    new XacmlMatch(
                        new Uri(XacmlConstants.AttributeMatchFunction.StringEqual),
                        new XacmlAttributeValue(new Uri(XacmlConstants.DataTypes.XMLString), resourceMatch.Value),
                        new XacmlAttributeDesignator(new Uri(XacmlConstants.MatchAttributeCategory.Resource), new Uri(resourceMatch.Id), new Uri(XacmlConstants.DataTypes.XMLString), false)));
            }

            List<XacmlAllOf> resourceAllOfs = new List<XacmlAllOf> { new XacmlAllOf(resourceMatches) };

            // Build Action
            List<XacmlAllOf> actionAllOfs = new List<XacmlAllOf>();
            actionAllOfs.Add(new XacmlAllOf(new List<XacmlMatch>
            {
                new XacmlMatch(
                        new Uri(XacmlConstants.AttributeMatchFunction.StringEqual),
                        new XacmlAttributeValue(new Uri(XacmlConstants.DataTypes.XMLString), rule.Action.Value),
                        new XacmlAttributeDesignator(new Uri(XacmlConstants.MatchAttributeCategory.Action), new Uri(XacmlConstants.MatchAttributeIdentifiers.ActionId), new Uri(XacmlConstants.DataTypes.XMLString), false))
            }));

            targetList.Add(new XacmlAnyOf(subjectAllOfs));
            targetList.Add(new XacmlAnyOf(resourceAllOfs));
            targetList.Add(new XacmlAnyOf(actionAllOfs));

            return new XacmlTarget(targetList);
        }

        /// <summary>
        /// Builds a XacmlMatch <see cref="XacmlMatch"/> model
        /// </summary>
        /// <param name="function">The compare function type</param>
        /// <param name="datatype">The attribute data type</param>
        /// <param name="attributeValue">The attribute value</param>
        /// <param name="attributeId">The attribute id</param>
        /// <param name="category">The attribute category</param>
        /// <param name="mustBePresent">Whether the attribute value must be present</param>
        public static XacmlMatch BuildDelegationPolicyMatch(string function, string datatype, string attributeValue, string attributeId, string category, bool mustBePresent = false)
        {
            return new XacmlMatch(
                new Uri(function),
                new XacmlAttributeValue(new Uri(datatype), attributeValue),
                new XacmlAttributeDesignator(new Uri(category), new Uri(attributeId), new Uri(datatype), mustBePresent));
        }

        /// <summary>
        /// Gets the entire policy as a list of <see cref="ResourcePolicy"/>. 
        /// </summary>
        /// <param name="policy">The policy</param>
        /// <param name="language">The language (not in use yet; exactly how is yet to be determined)</param>
        /// <returns>List of resource policies</returns>
        public static List<ResourcePolicy> GetResourcePoliciesFromXacmlPolicy(XacmlPolicy policy, string language)
        {
            Dictionary<string, ResourcePolicy> resourcePolicies = new Dictionary<string, ResourcePolicy>();

            foreach (XacmlRule rule in policy.Rules)
            {
                if (rule.Effect.Equals(XacmlEffectType.Permit) && rule.Target != null)
                {
                    List<RoleGrant> roles = GetRolesFromRule(rule);
                    if (roles.Count == 0)
                    {
                        continue;
                    }

                    List<string> policyKeys = GetResourcePoliciesFromRule(resourcePolicies, rule);
                    List<ResourceAction> actions = GetActionsFromRule(rule, roles);

                    foreach (string policyKey in policyKeys)
                    {
                        ResourcePolicy resourcePolicy = resourcePolicies.GetValueOrDefault(policyKey);

                        if (policy.Description != null && resourcePolicy.Description == null)
                        {
                            resourcePolicy.Description = policy.Description;
                        }

                        AddActionsToResourcePolicy(actions, resourcePolicy);
                    }
                }
            }

            return resourcePolicies.Values.ToList();
        }

        /// <summary>
        /// Gets the authentication level requirement from the obligation expression of the XacmlPolicy if specified 
        /// </summary>
        /// <param name="policy">The policy</param>
        /// <returns>Minimum authentication level requirement</returns>
        public static int GetMinimumAuthenticationLevelFromXacmlPolicy(XacmlPolicy policy)
        {
            foreach (XacmlObligationExpression oblExpr in policy.ObligationExpressions)
            {
                foreach (XacmlAttributeAssignmentExpression attrExpr in oblExpr.AttributeAssignmentExpressions)
                {
                    if (attrExpr.Category.OriginalString == AltinnXacmlConstants.MatchAttributeCategory.MinimumAuthenticationLevel &&
                        attrExpr.Property is XacmlAttributeValue attrValue &&
                        int.TryParse(attrValue.Value, out int minAuthLevel))
                    {
                        return minAuthLevel;
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// Decomposes the provided XacmlPolicy with individual XacmlRules pr. Subject, Resource and Action AllOf combinations
        /// </summary>
        /// <param name="policy">The XacmlPolicy to decompose</param>
        /// <returns>A decomposed XacmlPolicy</returns>
        public static XacmlPolicy GetDecomposedXacmlPolicy(XacmlPolicy policy)
        {
            XacmlPolicy decomposedPolicy = new XacmlPolicy(new Uri($"{policy.PolicyId}_decomposed"), policy.RuleCombiningAlgId, policy.Target);
            decomposedPolicy.Description = $"Decomposed policy of policyid: {policy.PolicyId}. Original description: {policy.Description}";

            foreach (XacmlRule rule in policy.Rules)
            {
                ICollection<XacmlAllOf> subjectAllOfs = GetAllOfsByCategory(rule, XacmlConstants.MatchAttributeCategory.Subject);
                ICollection<XacmlAllOf> resourceAllOfs = GetAllOfsByCategory(rule, XacmlConstants.MatchAttributeCategory.Resource);
                ICollection<XacmlAllOf> actionAllOfs = GetAllOfsByCategory(rule, XacmlConstants.MatchAttributeCategory.Action);

                int decomposedRuleCount = 0;
                foreach (XacmlAllOf subject in subjectAllOfs)
                {
                    foreach (XacmlAllOf resource in resourceAllOfs)
                    {
                        foreach (XacmlAllOf action in actionAllOfs)
                        {
                            decomposedRuleCount++;

                            XacmlRule decomposedRule = new XacmlRule($"{rule.RuleId}_{decomposedRuleCount}", rule.Effect);
                            decomposedRule.Description = $"Decomposed rule from policyid: {policy.PolicyId} ruleid: {rule.RuleId}. Original description: {rule.Description}";
                            decomposedRule.Target = new XacmlTarget(new List<XacmlAnyOf>
                            {
                                new XacmlAnyOf(new List<XacmlAllOf> { subject }),
                                new XacmlAnyOf(new List<XacmlAllOf> { resource }),
                                new XacmlAnyOf(new List<XacmlAllOf> { action })
                            });

                            decomposedPolicy.Rules.Add(decomposedRule);
                        }
                    }
                }
            }

            return decomposedPolicy;
        }

        /// <summary>
        /// Builds a collection of XacmlContextAttributes which can be used for a decision request, based on a list of subject attributes and an already decomposed XacmlRule (has a single combination of subject, resource and action AllOfs) 
        /// </summary>
        /// <param name="subjects">The list of subject values to add to the context (Roles, access groups, delegation recipients etc.)</param>
        /// <param name="decomposedRule">The decomposed XacmlRule (has a single combination of subject, resource and action AllOfs)</param>
        /// <returns>A collection of XacmlContextAttributes which can be used for a decision request</returns>
        public static ICollection<XacmlContextAttributes> GetContextAttributes(List<AttributeMatch> subjects, XacmlRule decomposedRule)
        {
            ICollection<XacmlAllOf> resourceAllOfs = GetAllOfsByCategory(decomposedRule, XacmlConstants.MatchAttributeCategory.Resource);
            ICollection<XacmlAllOf> actionAllOfs = GetAllOfsByCategory(decomposedRule, XacmlConstants.MatchAttributeCategory.Action);

            List<AttributeMatch> resource = GetAttributeMatchFromXacmlAllOfs(resourceAllOfs.FirstOrDefault());
            List<AttributeMatch> action = GetAttributeMatchFromXacmlAllOfs(actionAllOfs.FirstOrDefault());

            return GetContextAttributes(subjects, resource, action);
        }

        /// <summary>
        /// Takes an already decomposed XacmlRule (has a single combination of subject, resource and action AllOfs) and builds a collection of XacmlContextAttributes which can be used for a decision request
        /// </summary>
        /// <param name="subjects">The list of subject values to add to the context (Roles, access groups, delegation recipients etc.)</param>
        /// <param name="resource">The list of attribute values identifying a single resource to add to the context (Org/App, ResourceRegistryId)</param>
        /// <param name="action">The list of action attribute values identifying a single action to add to the context (Read, Write etc.)</param>
        /// <returns>A collection of XacmlContextAttributes which can be used for a decision request</returns>
        public static ICollection<XacmlContextAttributes> GetContextAttributes(List<AttributeMatch> subjects, List<AttributeMatch> resource, List<AttributeMatch> action)
        {
            ICollection<XacmlContextAttributes> contextAttributes = new Collection<XacmlContextAttributes>();

            ICollection<XacmlAttribute> subjectsAttributes = new Collection<XacmlAttribute>();
            foreach (AttributeMatch subjectMatch in subjects)
            {
                XacmlAttribute subjectAttribute = new XacmlAttribute(new Uri(subjectMatch.Id), true);
                subjectAttribute.AttributeValues.Add(new XacmlAttributeValue(new Uri(XacmlConstants.DataTypes.XMLString), subjectMatch.Value));
                subjectsAttributes.Add(subjectAttribute);
            }

            ICollection<XacmlAttribute> resourceAttributes = new Collection<XacmlAttribute>();
            foreach (AttributeMatch resourceMatch in resource)
            {
                XacmlAttribute resourceAttribute = new XacmlAttribute(new Uri(resourceMatch.Id), true);
                resourceAttribute.AttributeValues.Add(new XacmlAttributeValue(new Uri(XacmlConstants.DataTypes.XMLString), resourceMatch.Value));
                resourceAttributes.Add(resourceAttribute);
            }

            ICollection<XacmlAttribute> actionAttributes = new Collection<XacmlAttribute>();
            foreach (AttributeMatch actionMatch in action)
            {
                XacmlAttribute actionAttribute = new XacmlAttribute(new Uri(actionMatch.Id), true);
                actionAttribute.AttributeValues.Add(new XacmlAttributeValue(new Uri(XacmlConstants.DataTypes.XMLString), actionMatch.Value));
                actionAttributes.Add(actionAttribute);
            }

            contextAttributes.Add(new XacmlContextAttributes(new Uri(XacmlConstants.MatchAttributeCategory.Subject), subjectsAttributes));
            contextAttributes.Add(new XacmlContextAttributes(new Uri(XacmlConstants.MatchAttributeCategory.Resource), resourceAttributes));
            contextAttributes.Add(new XacmlContextAttributes(new Uri(XacmlConstants.MatchAttributeCategory.Action), actionAttributes));
            return contextAttributes;
        }

        /// <summary>
        /// Creates a collection of Rights (single Resource and Action combinations) from the provided collection of XacmlRules
        /// </summary>
        /// <param name="xacmlRules">The collection of XacmlRules</param>
        /// <returns>A collection of Rights</returns>
        public static ICollection<Right> GetRightsFromXacmlRules(ICollection<XacmlRule> xacmlRules)
        {
            Dictionary<string, Right> rights = new Dictionary<string, Right>();

            foreach (XacmlRule rule in xacmlRules)
            {
                ICollection<XacmlAllOf> resourceAllOfs = GetAllOfsByCategory(rule, XacmlConstants.MatchAttributeCategory.Resource);
                ICollection<XacmlAllOf> actionAllOfs = GetAllOfsByCategory(rule, XacmlConstants.MatchAttributeCategory.Action);

                foreach (XacmlAllOf resource in resourceAllOfs)
                {
                    foreach (XacmlAllOf action in actionAllOfs)
                    {
                        Right right = new Right
                        {
                            RightSources = new List<RightSource>(),
                            Resource = GetAttributeMatchFromXacmlAllOfs(resource),
                            Action = GetAttributeMatchFromXacmlAllOfs(action).FirstOrDefault()
                        };

                        if (!rights.ContainsKey(right.RightKey))
                        {
                            rights.Add(right.RightKey, right);
                        }
                    }
                }
            }

            return rights.Values;
        }

        /// <summary>
        /// Gets a collection of distinct AttributeId used in XacmlMatch instances matching the specified attribute category. 
        /// </summary>
        /// <param name="rule">The xacml rule to find match attribute ids in</param>
        /// <param name="category">The attribute category to match</param>
        /// <returns>Collection of AttributeId</returns>
        public static ICollection<string> GetRuleMatchAttributeIdsForCategory(XacmlRule rule, string category)
        {
            SortedList<string, string> attributeIds = new SortedList<string, string>();

            foreach (XacmlAnyOf anyOf in rule.Target.AnyOf)
            {
                foreach (XacmlAllOf allOf in anyOf.AllOf)
                {
                    foreach (XacmlMatch xacmlMatch in allOf.Matches)
                    {
                        if (xacmlMatch.AttributeDesignator.Category.Equals(category) && !attributeIds.ContainsKey(xacmlMatch.AttributeDesignator.AttributeId.OriginalString))
                        {
                            attributeIds.Add(xacmlMatch.AttributeDesignator.AttributeId.OriginalString, xacmlMatch.AttributeDesignator.AttributeId.OriginalString);
                        }
                    }
                }
            }

            return attributeIds.Keys.ToList();
        }

        /// <summary>
        /// Gets a nested list of AttributeMatche models for all XacmlMatch instances matching the specified attribute category. 
        /// </summary>
        /// <param name="rule">The xacml rule to process</param>
        /// <param name="category">The attribute category to match</param>
        /// <returns>Nested list of PolicyAttributeMatch models</returns>
        public static List<List<PolicyAttributeMatch>> GetRulePolicyAttributeMatchesForCategory(XacmlRule rule, string category)
        {
            List<List<PolicyAttributeMatch>> ruleAttributeMatches = new();

            foreach (XacmlAnyOf anyOf in rule.Target.AnyOf)
            {
                foreach (XacmlAllOf allOf in anyOf.AllOf)
                {
                    List<PolicyAttributeMatch> anyOfAttributeMatches = new();
                    foreach (XacmlMatch xacmlMatch in allOf.Matches)
                    {
                        if (xacmlMatch.AttributeDesignator.Category.Equals(category))
                        {
                            anyOfAttributeMatches.Add(new PolicyAttributeMatch { Id = xacmlMatch.AttributeDesignator.AttributeId.OriginalString, Value = xacmlMatch.AttributeValue.Value });
                        }
                    }

                    if (anyOfAttributeMatches.Any())
                    {
                        ruleAttributeMatches.Add(anyOfAttributeMatches);
                    }
                }                
            }

            return ruleAttributeMatches;
        }

        private static List<AttributeMatch> GetAttributeMatchFromXacmlAllOfs(XacmlAllOf allOf)
        {
            List<AttributeMatch> attributeMatches = new List<AttributeMatch>();
            foreach (XacmlMatch xacmlMatch in allOf.Matches)
            {
                attributeMatches.Add(new AttributeMatch
                {
                    Id = xacmlMatch.AttributeDesignator.AttributeId.OriginalString,
                    Value = xacmlMatch.AttributeValue.Value
                });
            }     

            return attributeMatches;
        }

        /// <summary>
        /// Gets a collection of all XacmlAllOfs containing all XacmlMatch instances matching the specified attribute category, from a given XacmlRule
        /// </summary>
        /// <param name="rule">The xacml rule</param>
        /// <param name="category">The attribute category to match</param>
        /// <returns>Collection of AllOfs</returns>
        private static ICollection<XacmlAllOf> GetAllOfsByCategory(XacmlRule rule, string category)
        {
            ICollection<XacmlAllOf> allOfs = new Collection<XacmlAllOf>();

            foreach (XacmlAnyOf anyOf in rule.Target.AnyOf)
            {
                foreach (XacmlAllOf allOf in anyOf.AllOf)
                {
                    ICollection<XacmlMatch> allOfMatchesFound = new Collection<XacmlMatch>();

                    foreach (XacmlMatch xacmlMatch in allOf.Matches)
                    {
                        if (xacmlMatch.AttributeDesignator.Category.Equals(category))
                        {
                            allOfMatchesFound.Add(xacmlMatch);
                        }
                    }

                    if (allOfMatchesFound.Count > 0)
                    {
                        allOfs.Add(new XacmlAllOf(allOfMatchesFound));
                    }
                }
            }

            return allOfs;
        }

        private static void AddActionsToResourcePolicy(List<ResourceAction> actions, ResourcePolicy resourcePolicy)
        {
            if (resourcePolicy.Actions == null)
            {
                resourcePolicy.Actions = new List<ResourceAction>();
                resourcePolicy.Actions.AddRange(actions);
            }
            else
            {
                foreach (ResourceAction resourceAction in actions)
                {
                    if (!resourcePolicy.Actions.Any(action => action.Match.Value == resourceAction.Match.Value && action.Match.Id == resourceAction.Match.Id))
                    {
                        resourcePolicy.Actions.Add(resourceAction);
                    }
                    else
                    {
                        ResourceAction existingAction = resourcePolicy.Actions.First(action => action.Match.Value == resourceAction.Match.Value && action.Match.Id == resourceAction.Match.Id);
                        existingAction.RoleGrants.AddRange(resourceAction.RoleGrants.Where(roleGrant => !existingAction.RoleGrants.Any(existingRoleGrant => existingRoleGrant.RoleTypeCode == roleGrant.RoleTypeCode)));
                    }
                }
            }
        }

        private static AttributeMatch GetActionValueFromRule(XacmlRule rule)
        {
            foreach (XacmlAnyOf anyOf in rule.Target.AnyOf)
            {
                foreach (XacmlAllOf allOf in anyOf.AllOf)
                {
                    XacmlMatch action = allOf.Matches.FirstOrDefault(m => m.AttributeDesignator.Category.Equals(XacmlConstants.MatchAttributeCategory.Action));

                    if (action != null)
                    {
                        return new AttributeMatch { Id = action.AttributeDesignator.AttributeId.OriginalString, Value = action.AttributeValue.Value };
                    }                    
                }
            }

            return null;
        }

        private static List<AttributeMatch> GetResourceFromXcamlRule(XacmlRule rule)
        {
            List<AttributeMatch> result = new List<AttributeMatch>();
            foreach (XacmlAnyOf anyOf in rule.Target.AnyOf)
            {
                foreach (XacmlAllOf allOf in anyOf.AllOf)
                {
                    foreach (XacmlMatch xacmlMatch in allOf.Matches.Where(m => m.AttributeDesignator.Category.Equals(XacmlConstants.MatchAttributeCategory.Resource)))
                    {
                        result.Add(new AttributeMatch { Id = xacmlMatch.AttributeDesignator.AttributeId.OriginalString, Value = xacmlMatch.AttributeValue.Value });                        
                    }
                }
            }

            return result;
        }

        private static List<ResourceAction> GetActionsFromRule(XacmlRule rule, List<RoleGrant> roles)
        {
            List<ResourceAction> actions = new List<ResourceAction>();
            foreach (XacmlAnyOf anyOf in rule.Target.AnyOf)
            {
                foreach (XacmlAllOf allOf in anyOf.AllOf)
                {
                    AttributeMatch actionAttributeMatch = new AttributeMatch();
                    foreach (XacmlMatch xacmlMatch in allOf.Matches)
                    {
                        if (xacmlMatch.AttributeDesignator.Category.Equals(XacmlConstants.MatchAttributeCategory.Action))
                        {
                            actionAttributeMatch.Id = xacmlMatch.AttributeDesignator.AttributeId.OriginalString;
                            actionAttributeMatch.Value = xacmlMatch.AttributeValue.Value;
                            ResourceAction resourceAction = new ResourceAction
                            {
                                Match = actionAttributeMatch,
                                RoleGrants = new List<RoleGrant>(),
                                Title = xacmlMatch.AttributeValue.Value
                            };
                            resourceAction.RoleGrants.AddRange(roles);
                            if (!actions.Contains(resourceAction))
                            {
                                actions.Add(resourceAction);
                            }
                        }
                    }
                }
            }

            return actions;
        }

        private static List<RoleGrant> GetRolesFromRule(XacmlRule rule)
        {
            List<RoleGrant> roles = new List<RoleGrant>();
            foreach (XacmlAnyOf anyOf in rule.Target.AnyOf)
            {
                foreach (XacmlAllOf allOf in anyOf.AllOf)
                {
                    foreach (XacmlMatch xacmlMatch in allOf.Matches)
                    {
                        if (xacmlMatch.AttributeDesignator.Category.Equals(XacmlConstants.MatchAttributeCategory.Subject) && xacmlMatch.AttributeDesignator.AttributeId.Equals(XacmlRequestAttribute.RoleAttribute))
                        {
                            roles.Add(new RoleGrant { RoleTypeCode = xacmlMatch.AttributeValue.Value, IsDelegable = true });
                        }
                    }
                }
            }

            return roles;
        }

        private static List<string> GetResourcePoliciesFromRule(Dictionary<string, ResourcePolicy> resourcePolicies, XacmlRule rule)
        {
            List<string> policyKeys = new List<string>();
            foreach (XacmlAnyOf anyOf in rule.Target.AnyOf)
            {
                foreach (XacmlAllOf allOf in anyOf.AllOf)
                {
                    StringBuilder bld = new StringBuilder();
                    string resourceKey = string.Empty;
                    List<AttributeMatch> resourceMatches = new List<AttributeMatch>();
                    foreach (XacmlMatch xacmlMatch in allOf.Matches)
                    {
                        if (xacmlMatch.AttributeDesignator.Category.Equals(XacmlConstants.MatchAttributeCategory.Resource))
                        {
                            bld.Append(xacmlMatch.AttributeDesignator.AttributeId);
                            bld.Append(xacmlMatch.AttributeValue.Value);
                            resourceKey = bld.ToString();
                            resourceMatches.Add(new AttributeMatch { Id = xacmlMatch.AttributeDesignator.AttributeId.OriginalString, Value = xacmlMatch.AttributeValue.Value });
                        }
                    }

                    CreateUniqueResourcePolicy(resourceKey, policyKeys, resourcePolicies, resourceMatches);
                }
            }

            return policyKeys;
        }

        private static void CreateUniqueResourcePolicy(string resourceKey, List<string> policyKeys, Dictionary<string, ResourcePolicy> resourcePolicies, List<AttributeMatch> resourceMatches)
        {
            if (!string.IsNullOrEmpty(resourceKey))
            {
                policyKeys.Add(resourceKey);

                if (!resourcePolicies.ContainsKey(resourceKey))
                {
                    string title = string.Join("/", resourceMatches.Select(rm => rm.Value));
                    ResourcePolicy newPolicy = new ResourcePolicy
                    {
                        Resource = resourceMatches,
                        Title = title
                    };

                    resourcePolicies.Add(resourceKey, newPolicy);
                }
            }
        }
    }
}
