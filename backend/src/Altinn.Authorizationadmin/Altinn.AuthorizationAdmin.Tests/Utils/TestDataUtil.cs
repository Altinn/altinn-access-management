using System;
using System.Collections.Generic;
using Altinn.Authorization.ABAC.Constants;
using Altinn.AuthorizationAdmin.Core.Constants;
using Altinn.AuthorizationAdmin.Core.Models;
using Altinn.AuthorizationAdmin.Core.Models.ResourceRegistry;

namespace Altinn.AuthorizationAdmin.Tests.Utils
{
    /// <summary>
    /// Mock class for helping setup test data
    /// </summary>
    public static class TestDataUtil
    {
        /// <summary>
        /// Creates a rule model from the input
        /// </summary>
        /// <param name="delegatedByUserId">delegatedByUserId</param>
        /// <param name="offeredByPartyId">offeredByPartyId</param>
        /// <param name="coveredBy">coveredBy/param>
        /// <param name="coveredByAttributeType">coveredByAttributeType</param>
        /// <param name="action">action</param>
        /// <param name="org">org</param>
        /// <param name="app">app</param>
        /// <param name="task">task</param>
        /// <param name="appresource">appresource</param>
        /// <param name="createdSuccessfully">createdSuccessfully</param>
        /// <param name="ruleType">ruleType</param>
        /// <returns>Rule model</returns>
        public static Rule GetRuleModel(int delegatedByUserId, int offeredByPartyId, string coveredBy, string coveredByAttributeType, string action, string org, string app, string task = null, string appresource = null, bool createdSuccessfully = false, RuleType ruleType = RuleType.None)
        {
            Rule rule = new Rule
            {
                DelegatedByUserId = delegatedByUserId,
                OfferedByPartyId = offeredByPartyId,
                CoveredBy = new List<AttributeMatch> { new AttributeMatch { Id = coveredByAttributeType, Value = coveredBy } },
                Resource = new List<AttributeMatch> { new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute, Value = org }, new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute, Value = app } },
                Action = new AttributeMatch { Id = XacmlConstants.MatchAttributeIdentifiers.ActionId, Value = action },
                CreatedSuccessfully = createdSuccessfully,
                Type = ruleType
            };

            if (task != null)
            {
                rule.Resource.Add(new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.TaskAttribute, Value = task });
            }

            if (appresource != null)
            {
                rule.Resource.Add(new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.AppResourceAttribute, Value = appresource });
            }

            return rule;
        }

        /// <summary>
        /// Creates a RequestToDelete model from the input
        /// </summary>
        /// <param name="lastChangedByUserId">lastChangedByUserId</param>
        /// <param name="offeredByPartyId">offeredByPartyId</param>
        /// <param name="org">org</param>
        /// <param name="app">app</param>
        /// <param name="ruleIds">ruleIds</param>
        /// <param name="coveredByPartyId">coveredByPartyId</param>
        /// <param name="coveredByUserId">coveredByUserId</param>
        /// <returns></returns>
        public static RequestToDelete GetRequestToDeleteModel(int lastChangedByUserId, int offeredByPartyId, string org, string app, List<string> ruleIds = null, int? coveredByPartyId = null, int? coveredByUserId = null)
        {
            AttributeMatch coveredBy = new AttributeMatch();
            if (coveredByUserId == null)
            {
                coveredBy.Id = AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute;
                coveredBy.Value = coveredByPartyId.ToString();
            }
            else
            {
                coveredBy.Id = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute;
                coveredBy.Value = coveredByUserId.ToString();
            }

            RequestToDelete requestToDelete = new RequestToDelete
            {
                DeletedByUserId = lastChangedByUserId,
                PolicyMatch = new PolicyMatch
                {
                    CoveredBy = new List<AttributeMatch> { coveredBy },
                    OfferedByPartyId = offeredByPartyId,
                    Resource = new List<AttributeMatch> { new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute, Value = org }, new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute, Value = app } }
                },
                RuleIds = ruleIds
            };

            return requestToDelete;
        }

        /// <summary>
        /// Creates a DelegationChange model from the input
        /// </summary>
        /// <param name="altinnAppId">altinnAppId</param>
        /// <param name="offeredByPartyId">offeredByPartyId</param>
        /// <param name="coveredByUserId">coveredByUserId</param>
        /// <param name="coveredByPartyId">coveredByPartyId</param>
        /// <param name="performedByUserId">performedByUserId</param>
        /// <param name="changeType">changeType</param>
        /// <param name="changeId">changeId</param>
        /// <returns></returns>
        public static DelegationChange GetDelegationChange(string altinnAppId, int offeredByPartyId, int? coveredByUserId = null, int? coveredByPartyId = null, int performedByUserId = 20001336, DelegationChangeType changeType = DelegationChangeType.Grant, int changeId = 1337)
        {
            string coveredBy = coveredByPartyId != null ? $"p{coveredByPartyId}" : $"u{coveredByUserId}";
            return new DelegationChange
            {
                DelegationChangeId = changeId,
                DelegationChangeType = changeType,
                AltinnAppId = altinnAppId,
                OfferedByPartyId = offeredByPartyId,
                CoveredByPartyId = coveredByPartyId,
                CoveredByUserId = coveredByUserId,
                PerformedByUserId = performedByUserId,
                BlobStoragePolicyPath = $"{altinnAppId}/{offeredByPartyId}/{coveredBy}/delegationpolicy.xml",
                BlobStorageVersionId = "CorrectLeaseId",
                Created = DateTime.Now
            };
        }

        /// <summary>
        /// Sets up mock data for delegation list 
        /// </summary>
        /// <param name="offeredByPartyId">partyid of the reportee that delegated the resource</param>
        /// <returns></returns>
        public static List<Delegation> GetDelegations(int offeredByPartyId, string resourceId, string resourceName)
        {
            List<Delegation> delegations = new List<Delegation>();
            delegations.Add(new Delegation
            {
                OfferedByPartyId = offeredByPartyId,
                CoveredByName = "THOMAS T�NDER",
                CoveredByPartyId = 50002111,
                ResourceId = resourceId,
                ResourceTitle = new Dictionary<string, string>
                {
                    { "en", resourceName },
                    { "nb_no", resourceName },
                    { "nn_no", resourceName}
                }
            });
            delegations.Add(new Delegation
            {
                OfferedByPartyId = offeredByPartyId,
                CoveredByName = "HANNAH TUFT",
                CoveredByPartyId = 50002112,
                ResourceId = resourceId,
                ResourceTitle = new Dictionary<string, string>
                {
                    { "en", resourceName },
                    { "nb_no", resourceName },
                    { "nn_no", resourceName }
                }
            });
            delegations.Add(new Delegation
            {
                OfferedByPartyId = offeredByPartyId,
                CoveredByName = "KLEVEN ALMA",
                CoveredByPartyId = 50002113,
                ResourceId = resourceId,
                ResourceTitle = new Dictionary<string, string>
                {
                    { "en", resourceName },
                    { "nb_no", resourceName },
                    { "nn_no", resourceName }
                }
            });
            delegations.Add(new Delegation
            {
                OfferedByPartyId = offeredByPartyId,
                CoveredByName = "ELENA FJ�R",
                CoveredByPartyId = 50002114,
                ResourceId = resourceId,
                ResourceTitle = new Dictionary<string, string>
                {
                    { "en", resourceName },
                    { "nb_no", resourceName },
                    { "nn_no", resourceName }
                }
            });
            delegations.Add(new Delegation
            {
                OfferedByPartyId = offeredByPartyId,
                CoveredByName = "MARGRETHE THORUD",
                CoveredByPartyId = 50002115,
                ResourceId = resourceId,
                ResourceTitle = new Dictionary<string, string>
                {
                    { "en", resourceName },
                    { "nb_no", resourceName},
                    { "nn_no", resourceName }
                }
            });
            return delegations;
        }

        /// <summary>
        /// Sets mock data for service resource
        /// </summary>
        /// <returns></returns>
        public static List<ServiceResource> GetResources(int offeredByPartyId)
        {
            List<ServiceResource> resources = new List<ServiceResource>();
            resources.Add(new ServiceResource
            {
                Identifier = "nav_aa_distribution",
                Title = new Dictionary<string, string>
                {
                    { "en", "NAV aa distribution" },
                    { "nb_no", "NAV aa distribution" },
                    { "nn_no", "NAV aa distribution" }
                }
            });
            resources.Add(new ServiceResource
            {
                Identifier = "skd_1",
                Title = new Dictionary<string, string>
                {
                    { "en", "SKD 1" },
                    { "nb_no", "SKD 1" },
                    { "nn_no", "SKD 1" }
                }
            });
            return resources;
        }

        /// <summary>
        /// Gets resourcedelegation model for the given input
        /// </summary>
        /// <param name="offeredByPartyId">party that delegated the resources</param>
        /// <param name="resourceId">the resource that was delegated</param>
        /// <param name="resourceName">the resource name that was delegated</param>
        /// <returns></returns>
        public static ResourceDelegation GetResourceDelegationModel(int offeredByPartyId, string resourceId, string resourceName)
        {
            ResourceDelegation resourceDelegation = new ResourceDelegation
            {
                ResourceId = resourceId,
                ResourceName = resourceName
            };
            resourceDelegation.Delegations = new List<Delegation>();
            resourceDelegation.Delegations.AddRange(GetDelegations(50002110, resourceId, resourceName));
            return resourceDelegation;
        }
    }
}
