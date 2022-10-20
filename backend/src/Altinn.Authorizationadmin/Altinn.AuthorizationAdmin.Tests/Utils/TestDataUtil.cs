using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
        /// <param name="coveredBy">coveredBy</param>
        /// <param name="coveredByAttributeType">coveredByAttributeType</param>
        /// <param name="action">action</param>
        /// <param name="org">org</param>
        /// <param name="app">app</param>
        /// <param name="task">task</param>
        /// <param name="appresource">appresource</param>
        /// <param name="createdSuccessfully">createdSuccessfully</param>
        /// <param name="ruleType">ruleType</param>
        /// <param name="resourceRegistryId">resourceregistry id.</param>
        /// <returns>Rule model</returns>
        public static Rule GetRuleModel(int delegatedByUserId, int offeredByPartyId, string coveredBy, string coveredByAttributeType, string action, string org, string app, string task = null, string appresource = null, bool createdSuccessfully = false, RuleType ruleType = RuleType.None, string resourceRegistryId = null)
        {
            Rule rule;

            if (!string.IsNullOrEmpty(resourceRegistryId))
            {
                rule = new Rule
                {
                    DelegatedByUserId = delegatedByUserId,
                    OfferedByPartyId = offeredByPartyId,
                    CoveredBy = new List<AttributeMatch> { new AttributeMatch { Id = coveredByAttributeType, Value = coveredBy } },
                    Resource = new List<AttributeMatch> { new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceRegistryAttribute, Value = resourceRegistryId } },
                    Action = new AttributeMatch { Id = XacmlConstants.MatchAttributeIdentifiers.ActionId, Value = action },
                    CreatedSuccessfully = createdSuccessfully,
                    Type = ruleType
                };
            }
            else
            {
                rule = new Rule
                {
                    DelegatedByUserId = delegatedByUserId,
                    OfferedByPartyId = offeredByPartyId,
                    CoveredBy = new List<AttributeMatch> { new AttributeMatch { Id = coveredByAttributeType, Value = coveredBy } },
                    Resource = new List<AttributeMatch> { new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute, Value = org }, new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute, Value = app } },
                    Action = new AttributeMatch { Id = XacmlConstants.MatchAttributeIdentifiers.ActionId, Value = action },
                    CreatedSuccessfully = createdSuccessfully,
                    Type = ruleType
                };
            }

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
        public static DelegationChange GetDelegationChange(string altinnAppId, int offeredByPartyId, int? coveredByUserId = null, int? coveredByPartyId = null, int performedByUserId = 20001336, DelegationChangeType changeType = DelegationChangeType.Grant, int changeId = 1337, string? resourceId = null, string? resourceType = null)
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
                Created = DateTime.Now,
                ResourceId = resourceId,
                ResourceType = resourceType
            };
        }

        /// <summary>
        /// Sets up mock data for delegation list 
        /// </summary>
        /// <param name="offeredByPartyId">partyid of the reportee that delegated the resource</param>
        /// <param name="resourceId">resource identifier</param>
        /// <param name="resourceName">Resource name</param>
        /// <returns></returns>
        public static List<Delegation> GetDelegations(int offeredByPartyId, string resourceId, string resourceName, int performedByUserId)
        {
            List<Delegation> delegations = new List<Delegation>();
            Encoding enc = new UTF8Encoding(true, true);
            delegations.Add(new Delegation
            {
                OfferedByPartyId = offeredByPartyId,
                PerformedByUserId = performedByUserId,
                CoveredByName = enc.GetString(enc.GetBytes("KOLSÅS OG FLÅM")),
                CoveredByOrganizationNumber = 810418192,
                CoveredByPartyId = 50004219,

            });
            delegations.Add(new Delegation
            {
                OfferedByPartyId = offeredByPartyId,
                PerformedByUserId = performedByUserId,
                CoveredByName = enc.GetString(enc.GetBytes("NORDRE FROGN OG MORTENHALS")),
                CoveredByOrganizationNumber = 810418362,
                CoveredByPartyId = 50004220,
            });
            delegations.Add(new Delegation
            {
                OfferedByPartyId = offeredByPartyId,
                PerformedByUserId = performedByUserId,
                CoveredByName = enc.GetString(enc.GetBytes("LUNDAMO OG FLEINVÆR")),
                CoveredByOrganizationNumber = 810418532,
                CoveredByPartyId = 50004221,
            });
            return delegations;
        }

        /// <summary>
        /// Creates a ServiceResource model.
        /// </summary>
        /// <param name="resourceId">ResourceId.</param>
        /// <param name="resourceTitle">title of the resource</param>
        /// <returns>Returns the newly created ServiceResource.</returns>
        public static ServiceResource GetResource(string resourceId, string resourceTitle)
        {
            return new ServiceResource
            {
                Identifier = resourceId,
                Title = new Dictionary<string, string>
                {
                    { "en", resourceTitle },
                    { "nb-no", resourceTitle },
                    { "nn-no", resourceTitle },
                },
                Description = new Dictionary<string, string>
                {
                    { "Description", resourceTitle }
                },
                ValidFrom = DateTime.Now,
                ValidTo = DateTime.Now.AddDays(1),
            };
        }

        /// <summary>
        /// Gets offered delegation model for the given input
        /// </summary>
        /// <param name="offeredByPartyId">party that delegated the resources</param>
        /// <param name="resourceId">the resource that was delegated</param>
        /// <param name="resourceName">the resource name that was delegated</param>
        /// <param name="performedByUserId">id of the user who perfoemed the delegation</param>
        /// <returns></returns>
        public static OfferedDelegations GetDelegatedResourcesModel(int offeredByPartyId, string resourceId, string resourceName, int performedByUserId)
        {
            OfferedDelegations offeredDelegation = new OfferedDelegations
            {
                ResourceId = resourceId,
                ResourceTitle = resourceName
            };
            offeredDelegation.Delegations = new List<Delegation>();
            offeredDelegation.Delegations.AddRange(GetDelegations(offeredByPartyId, resourceId, resourceName, performedByUserId));
            return offeredDelegation;
        }

        /// <summary>
        /// Creates a DelegationChange model from the input.
        /// </summary>
        /// <returns>DelegationChange.</returns>
        public static DelegationChange GetResourceDelegationChange(string resourceRegistryId, int offeredByPartyId, int? coveredByUserId = null, int? coveredByPartyId = null, int performedByUserId = 20001336, DelegationChangeType changeType = DelegationChangeType.Grant, int changeId = 1337)
        {
            string coveredBy = coveredByPartyId != null ? $"p{coveredByPartyId}" : $"u{coveredByUserId}";
            return new DelegationChange
            {
                DelegationChangeId = changeId,
                DelegationChangeType = changeType,
                AltinnAppId = null,
                OfferedByPartyId = offeredByPartyId,
                CoveredByPartyId = coveredByPartyId,
                CoveredByUserId = coveredByUserId,
                PerformedByUserId = performedByUserId,
                BlobStoragePolicyPath = $"{resourceRegistryId}/{offeredByPartyId}/{coveredBy}/delegationpolicy.xml",
                BlobStorageVersionId = "CorrectLeaseId",
                Created = DateTime.Now,
                ResourceId = resourceRegistryId,
            };
        }

        /// <summary>
        /// Creates a list of roles.
        /// </summary>
        /// <returns>The newly created list of roles.</returns>
        public static List<string> GetRolesWithAccess()
        {
            List<string> roles = new List<string>();
            roles.Add("BEST");
            roles.Add("BOBE");
            roles.Add("DAGL");
            roles.Add("DTPR");
            roles.Add("DTSO");
            roles.Add("INNH");
            roles.Add("KEMN");
            roles.Add("KOMP");
            roles.Add("LEDE");
            roles.Add("REPR");

            return roles;
        }
    }
}
