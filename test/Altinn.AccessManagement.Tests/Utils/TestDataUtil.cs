using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Enums.ResourceRegistry;
using Altinn.AccessManagement.Models;
using Altinn.AccessManagement.Tests.Controllers;
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
        /// <param name="delegatedBy">delegatedBy</param>
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
        /// <param name="delegatedByParty">Value indicating delegatedBy is party</param>
        /// <returns>Rule model</returns>
        public static Rule GetRuleModel(int delegatedBy, int offeredByPartyId, string coveredBy, string coveredByAttributeType, string action, string org, string app, string task = null, string appresource = null, bool createdSuccessfully = false, RuleType ruleType = RuleType.None, string resourceRegistryId = null, bool delegatedByParty = false)
        {
            Rule rule;

            if (!string.IsNullOrEmpty(resourceRegistryId))
            {
                rule = new Rule
                {
                    DelegatedByUserId = delegatedByParty ? null : delegatedBy,
                    DelegatedByPartyId = delegatedByParty ? delegatedBy : null,
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
                    DelegatedByUserId = delegatedByParty ? null : delegatedBy,
                    DelegatedByPartyId = delegatedByParty ? delegatedBy : null,
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
        public static DelegationChange GetAltinnAppDelegationChange(string altinnAppId, int offeredByPartyId, int? coveredByUserId = null, int? coveredByPartyId = null, int performedByUserId = 20001336, DelegationChangeType changeType = DelegationChangeType.Grant, int changeId = 1337)
        {
            string coveredBy = coveredByPartyId != null ? $"p{coveredByPartyId}" : $"u{coveredByUserId}";
            return new DelegationChange
            {
                DelegationChangeId = changeId,
                DelegationChangeType = changeType,
                ResourceId = altinnAppId,
                ResourceType = ResourceAttributeMatchType.AltinnAppId.ToString(),
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
        /// <param name="resourceId">resource identifier</param>
        /// <param name="resourceName">Resource name</param>
        /// <param name="performedByUserId">who performed the delegation</param>
        /// <returns></returns>
        public static List<Delegation> GetDelegations(int offeredByPartyId, string resourceId, string resourceName, int performedByUserId)
        {
            List<Delegation> delegations = new List<Delegation>();
            delegations.Add(new Delegation
            {
                OfferedByPartyId = offeredByPartyId,
                PerformedByUserId = performedByUserId,
                CoveredByName = "KOLSAAS OG FLAAM",
                CoveredByOrganizationNumber = "810418192",
                CoveredByPartyId = 50004219,
            });
            delegations.Add(new Delegation
            {
                OfferedByPartyId = offeredByPartyId,
                PerformedByUserId = performedByUserId,
                CoveredByName = "NORDRE FROGN OG MORTENHALS",
                CoveredByOrganizationNumber = "810418362",
                CoveredByPartyId = 50004220,
            });
            delegations.Add(new Delegation
            {
                OfferedByPartyId = offeredByPartyId,
                PerformedByUserId = performedByUserId,
                CoveredByName = "LUNDAMO OG FLEINVAR",
                CoveredByOrganizationNumber = "810418532",
                CoveredByPartyId = 50004221,
            });
            return delegations;
        }

        /// <summary>
        /// Creates a ServiceResource model.
        /// </summary>
        /// <param name="resourceId">ResourceId.</param>
        /// <param name="resourceTitle">title of the resource</param>
        /// <param name="resourceType">Type of the resource</param>
        /// <param name="description">Description of the resource</param>
        /// <param name="validFrom">The valid from date</param>
        /// <param name="validTo">The valid to date</param>
        /// <param name="status">The status of resource</param>
        /// <returns>Returns the newly created ServiceResource.</returns>
        public static ServiceResource GetResource(string resourceId, string resourceTitle, ResourceType resourceType, string description = "Test", DateTime? validFrom = null, DateTime? validTo = null, string status = "Active")
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
                    Status = "NA",
                    ResourceType = resourceType,
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
                    { "en", description },
                    { "nb-no", description },
                    { "nn-no", description }
                },
                    Status = status,
                    ResourceType = resourceType,
                };
            }
        }

        /// <summary>
        /// Creates a DelegationChange model from the input.
        /// </summary>
        /// <returns>DelegationChange.</returns>
        public static DelegationChange GetResourceRegistryDelegationChange(string resourceRegistryId, ResourceType resourceType, int offeredByPartyId, DateTime? created, int? coveredByUserId = null, int? coveredByPartyId = null, int performedByUserId = 20001336, DelegationChangeType changeType = DelegationChangeType.Grant, int changeId = 1337)
        {
            string coveredBy = coveredByPartyId != null ? $"p{coveredByPartyId}" : $"u{coveredByUserId}";

            return new DelegationChange
            {
                DelegationChangeId = changeId,
                DelegationChangeType = changeType,
                ResourceId = resourceRegistryId,
                ResourceType = resourceType.ToString(),
                OfferedByPartyId = offeredByPartyId,
                CoveredByPartyId = coveredByPartyId,
                CoveredByUserId = coveredByUserId,
                PerformedByUserId = performedByUserId,
                BlobStoragePolicyPath = $"resourceregistry/{resourceRegistryId}/{offeredByPartyId}/{coveredBy}/delegationpolicy.xml",
                BlobStorageVersionId = "CorrectLeaseId",
                Created = created
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
        /// Sets up mock data for offered maskinporten schema delegations
        /// </summary>
        /// <param name="offeredByPartyId">The party id of the reportee to retrieve offered delegations for</param>
        /// <returns>Offered maskinporten schema delegations</returns>
        public static List<MaskinportenSchemaDelegationExternal> GetOfferedMaskinportenSchemaDelegations(int offeredByPartyId)
        {
            List<MaskinportenSchemaDelegationExternal> delegations = null;

            string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(DelegationsControllerTest).Assembly.Location).LocalPath);
            string path = Path.Combine(unitTestFolder, "Data", "Json", "MaskinportenSchema", "Offered.json");
            if (File.Exists(path))
            {
                string content = File.ReadAllText(path);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                try
                {
                    delegations = JsonSerializer.Deserialize<List<MaskinportenSchemaDelegationExternal>>(content, options);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                if (offeredByPartyId != 0)
                {
                    return delegations.FindAll(d => d.OfferedByPartyId == offeredByPartyId);
                }
            }

            return delegations;
        }

        /// <summary>
        /// Sets up mock data for received maskinporten schema delegations
        /// </summary>
        /// <param name="coveredByPartyId">The party id of the reportee to retrieve received delegations for</param>
        /// <returns>Received maskinporten schema delegations</returns>
        public static List<MaskinportenSchemaDelegationExternal> GetReceivedMaskinportenSchemaDelegations(int coveredByPartyId)
        {
            List<MaskinportenSchemaDelegationExternal> delegations = null;

            string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(DelegationsControllerTest).Assembly.Location).LocalPath);
            string path = Path.Combine(unitTestFolder, "Data", "Json", "MaskinportenSchema", "Received.json");
            if (File.Exists(path))
            {
                string content = File.ReadAllText(path);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                try
                {
                    delegations = JsonSerializer.Deserialize<List<MaskinportenSchemaDelegationExternal>>(content, options);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                if (coveredByPartyId != 0)
                {
                    return delegations.FindAll(d => d.CoveredByPartyId == coveredByPartyId);
                }
            }

            return delegations;
        }

        /// <summary>
        /// Sets up mock data for admin delegation list 
        /// </summary>
        /// <param name="supplierOrg">partyid of the reportee that delegated the resource</param>
        /// <param name="consumerOrg">partyid of the reportee that received the delegation</param>
        /// <param name="resourceIds">resource id</param>
        /// <returns>Received delegations</returns>
        public static List<MPDelegationExternal> GetAdminDelegations(string supplierOrg, string consumerOrg, List<string> resourceIds = null)
        {
            List<MPDelegationExternal> delegations = null;
            List<MPDelegationExternal> filteredDelegations = new List<MPDelegationExternal>();
            string fileName = "admindelegations";
            string path = GetDelegationPath();
            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path);

                foreach (string file in files)
                {
                    if (file.Contains(fileName))
                    {
                        string content = File.ReadAllText(Path.Combine(path, file));
                        try
                        {
                            delegations = JsonSerializer.Deserialize<List<MPDelegationExternal>>(content);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                }

                foreach (MPDelegationExternal delegation in delegations)
                {
                    if (delegation.SupplierOrg == supplierOrg && delegation.ConsumerOrg == consumerOrg && resourceIds.Contains(delegation.ResourceId))
                    {
                        filteredDelegations.Add(delegation);
                    }
                }
            }

            return filteredDelegations;
        }

        /// <summary>
        /// Gets the organisation information
        /// </summary>
        /// <param name="orgNummer">the organisation number</param>
        /// <returns>organisation information</returns>
        public static PartyExternal GetOrganisation(string orgNummer)
        {
            List<PartyExternal> partyList = new List<PartyExternal>();
            PartyExternal party = null;

            string path = GetPartiesPath();
            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path);

                foreach (string file in files)
                {
                    if (file.Contains("parties"))
                    {
                        string content = File.ReadAllText(Path.Combine(path, file));
                        partyList = JsonSerializer.Deserialize<List<PartyExternal>>(content);
                    }
                }

                party = partyList.Find(p => p.Organization?.OrgNumber == orgNummer);
            }

            return party;
        }

        /// <summary>
        /// Gets the party information
        /// </summary>
        /// <param name="partyId">The party id</param>
        /// <returns>Party information</returns>
        public static PartyExternal GetTestParty(int partyId)
        {
            List<PartyExternal> partyList = new List<PartyExternal>();
            PartyExternal party = null;

            string path = GetPartiesPath();
            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path);

                foreach (string file in files)
                {
                    if (file.Contains("parties"))
                    {
                        string content = File.ReadAllText(Path.Combine(path, file));
                        partyList = JsonSerializer.Deserialize<List<PartyExternal>>(content);
                    }
                }

                party = partyList.Find(p => p.PartyId == partyId);
            }

            return party;
        }

        /// <summary>
        /// Gets the party information for a party with subunit
        /// </summary>
        /// <param name="partyId">The party id</param>
        /// <returns>Party information</returns>
        public static PartyExternal GetTestPartyWithSubUnit(int partyId)
        {
            List<PartyExternal> partyList = new List<PartyExternal>();

            string path = GetPartiesPath();
            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path);

                foreach (string file in files)
                {
                    if (file.Contains("parties"))
                    {
                        string content = File.ReadAllText(Path.Combine(path, file));
                        partyList = JsonSerializer.Deserialize<List<PartyExternal>>(content);
                    }
                }

                foreach (PartyExternal party in partyList)
                {
                    if (party != null && party.PartyId == partyId)
                    {
                        return party;
                    }
                    else if (party != null && party.ChildParties != null && party.ChildParties.Count > 0)
                    {
                        foreach (Party childParty in party.ChildParties)
                        {
                            if (childParty.PartyId == partyId)
                            {
                                return MapPartyToPartyExternal(childParty);
                            }
                        }
                    }
                }
            }

            return null;
        }

        private static PartyExternal MapPartyToPartyExternal(Party party)
        {
            PartyExternal partyExternal = new PartyExternal
            {
                PartyId = party.PartyId,
                PartyTypeName = party.PartyTypeName,
                OrgNumber = party.OrgNumber,
                SSN = party.SSN,
                UnitType = party.UnitType,
                Name = party.Name,
                IsDeleted = party.IsDeleted,
                OnlyHierarchyElementWithNoAccess = party.OnlyHierarchyElementWithNoAccess,
                Person = party.Person,
                Organization = party.Organization,
                ChildParties = party.ChildParties
            };

            return partyExternal;
        }

        private static string GetDelegationPath()
        {
            string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(DelegationsControllerTest).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, "Data", "Json", "MaskinportenSchema");
        }

        private static string GetPartiesPath()
        {
            string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(PartiesClientMock).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, "Data", "Parties");
        }
    }
}