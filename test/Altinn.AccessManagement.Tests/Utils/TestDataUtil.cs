using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Models;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.Authorization.ABAC.Constants;
using Altinn.Platform.Register.Models;

namespace Altinn.AccessManagement.Tests.Utils
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
            delegations.Add(new Delegation
            {
                OfferedByPartyId = offeredByPartyId,
                PerformedByUserId = performedByUserId,
                CoveredByName = "KOLSAAS OG FLAAM",
                CoveredByOrganizationNumber = 810418192,
                CoveredByPartyId = 50004219,
            });
            delegations.Add(new Delegation
            {
                OfferedByPartyId = offeredByPartyId,
                PerformedByUserId = performedByUserId,
                CoveredByName = "NORDRE FROGN OG MORTENHALS",
                CoveredByOrganizationNumber = 810418362,
                CoveredByPartyId = 50004220,
            });
            delegations.Add(new Delegation
            {
                OfferedByPartyId = offeredByPartyId,
                PerformedByUserId = performedByUserId,
                CoveredByName = "LUNDAMO OG FLEINVAR",
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
            if (resourceId == "nav1_aa_distribution")
            {
                return new ServiceResource
                {
                    Identifier = resourceId,
                    Title = new Dictionary<string, string>
                {
                    { "en", "Not Available" },
                    { "nb-no", "ikke tilgjengelig" },
                    { "nn-no", "ikkje tilgjengelig" },
                },
                    Description = new Dictionary<string, string>
                {
                    { "Description", resourceTitle }
                },
                    ValidFrom = DateTime.Now,
                    ValidTo = DateTime.Now.AddDays(1),
                };
            }
            else
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
                BlobStoragePolicyPath = $"resourceregistry/{resourceRegistryId}/{offeredByPartyId}/{coveredBy}/delegationpolicy.xml",
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

        /// <summary>
        /// Sets up mock data for delegation list 
        /// </summary>
        /// <param name="offeredByPartyId">partyid of the reportee that delegated the resource</param>
        /// <param name="coveredByPartyId">partyid of the reportee that received the delegation</param>
        /// <param name="resourceIds">resource id</param>
        /// <returns>Received delegations</returns>
        public static List<DelegationExternal> GetDelegations(int offeredByPartyId, int coveredByPartyId, List<string> resourceIds = null)
        {
            List<DelegationExternal> delegations = null;
            List<DelegationExternal> filteredDelegations = new List<DelegationExternal>();
            string fileName;

            if (resourceIds != null)
            {
                fileName = "delegations";
            }
            else
            {
                fileName = offeredByPartyId != 0 ? "outbounddelegation" : "inbounddelegation";
            }
            
            string path = GetDelegationPath();
            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path);

                foreach (string file in files)
                {
                    if (file.Contains(fileName))
                    {
                        string content = File.ReadAllText(Path.Combine(path, file));
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                        };
                        try
                        {
                            delegations = JsonSerializer.Deserialize<List<DelegationExternal>>(content, options);
                        }
                        catch (Exception ex)
                        { 
                            Console.WriteLine(ex);
                        }
                    }
                }

                if (offeredByPartyId != 0 && coveredByPartyId != 0)
                {
                    filteredDelegations.AddRange(delegations?.FindAll(od => od.OfferedByPartyId == offeredByPartyId && od.CoveredByPartyId == coveredByPartyId && resourceIds.Contains(od.ResourceId)));
                }
                else if (offeredByPartyId != 0)
                {
                    filteredDelegations.AddRange(delegations.FindAll(od => od.OfferedByPartyId == offeredByPartyId));
                }
                else if (coveredByPartyId != 0)
                {
                    filteredDelegations.AddRange(delegations.FindAll(od => od.CoveredByPartyId == coveredByPartyId));
                }
            }

            return filteredDelegations;
        }

        private static string GetDelegationPath()
        {
            string? unitTestFolder = Path.GetDirectoryName(new Uri(typeof(DelegationsControllerTest).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, "..", "..", "..", "Data", "Json", "Delegation");
        }
    }
}
