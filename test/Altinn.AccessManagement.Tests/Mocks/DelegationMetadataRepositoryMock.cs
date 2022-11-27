using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Tests.Utils;

namespace Altinn.AccessManagement.Tests.Mocks
{
    /// <summary>
    /// Mock class for <see cref="IDelegationMetadataRepository"></see> interface
    /// </summary>
    public class DelegationMetadataRepositoryMock : IDelegationMetadataRepository
    {
        /// <summary>
        /// Property for storing delegation changes locally for verification later from the integration tests
        /// </summary>
        public Dictionary<string, List<DelegationChange>> MetadataChanges { get; set; }

        /// <summary>
        /// Constructor setting up dependencies
        /// </summary>
        public DelegationMetadataRepositoryMock()
        {
            MetadataChanges = new Dictionary<string, List<DelegationChange>>();
        }

        /// <inheritdoc/>
        public Task<DelegationChange> InsertDelegation(DelegationChange delegationChange)
        {
            List<DelegationChange> current;
            string coveredBy = delegationChange.CoveredByPartyId != null ? $"p{delegationChange.CoveredByPartyId}" : $"u{delegationChange.CoveredByUserId}";
            string key = string.Empty;

            if (delegationChange.ResourceType.Equals(ResourceAttributeMatchType.AltinnAppId.ToString()))
            {
                key = $"{delegationChange.ResourceId}/{delegationChange.OfferedByPartyId}/{coveredBy}";
            }
            else
            {
                key = $"resourceregistry/{delegationChange.ResourceId}/{delegationChange.OfferedByPartyId}/{coveredBy}";
            }

            if (MetadataChanges.ContainsKey(key))
            {
                current = MetadataChanges[key];
            }
            else
            {
                current = new List<DelegationChange>();
                MetadataChanges[key] = current;
            }

            DelegationChange currentDelegationChange = new DelegationChange
            {
                DelegationChangeId = 1337,
                DelegationChangeType = delegationChange.DelegationChangeType,
                ResourceId = delegationChange.ResourceId,
                ResourceType = delegationChange.ResourceType,
                OfferedByPartyId = delegationChange.OfferedByPartyId,
                CoveredByPartyId = delegationChange.CoveredByPartyId,
                CoveredByUserId = delegationChange.CoveredByUserId,
                PerformedByUserId = delegationChange.PerformedByUserId,
                BlobStoragePolicyPath = delegationChange.BlobStoragePolicyPath,
                BlobStorageVersionId = delegationChange.BlobStorageVersionId,
                Created = DateTime.Now                
            };
    
            current.Add(currentDelegationChange);

            if (delegationChange.ResourceId == "error/postgrewritechangefail")
            {
                currentDelegationChange.DelegationChangeId = 0;
                return Task.FromResult(currentDelegationChange);
            }

            return Task.FromResult(currentDelegationChange);
        }

        /// <inheritdoc/>
        public Task<DelegationChange> GetCurrentDelegationChange(ResourceAttributeMatchType resourceMatchType, string resourceId, int offeredByPartyId, int? coveredByPartyId, int? coveredByUserId)
        {
            DelegationChange result = null;

            switch (resourceId)
            {
                case "org1/app1":
                case "org1/app3":
                case "org2/app3":
                case "org1/app4":
                case "error/blobstorageleaselockwritefail":
                case "error/postgrewritechangefail":
                    result = TestDataUtil.GetAltinnAppDelegationChange(resourceId, offeredByPartyId, coveredByUserId, coveredByPartyId);
                    break;
                case "org1/app5":
                    result = TestDataUtil.GetAltinnAppDelegationChange(resourceId, offeredByPartyId, coveredByUserId, coveredByPartyId, changeType: DelegationChangeType.RevokeLast);
                    break;
                case "error/postgregetcurrentfail":
                    throw new Exception("Some exception happened");
                case "error/delegationeventfail":
                    result = TestDataUtil.GetAltinnAppDelegationChange(resourceId, offeredByPartyId, coveredByUserId, coveredByPartyId, changeType: DelegationChangeType.Grant);
                    break;
                case "resource1":
                    result = TestDataUtil.GetResourceRegistryDelegationChange(resourceId, ResourceType.MaskinportenSchema, offeredByPartyId, coveredByUserId, coveredByPartyId);
                    break;
                case "resource2":
                    result = TestDataUtil.GetResourceRegistryDelegationChange(resourceId, ResourceType.MaskinportenSchema, offeredByPartyId, coveredByUserId, coveredByPartyId);
                    break;
                default:
                    result = null;
                    break;
            }

            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task<List<DelegationChange>> GetAllAppDelegationChanges(string altinnAppId, int offeredByPartyId, int? coveredByPartyId, int? coveredByUserId)
        {
            return Task.FromResult(new List<DelegationChange>());
        }

        /// <inheritdoc/>
        public Task<List<DelegationChange>> GetAllCurrentAppDelegationChanges(List<int> offeredByPartyIds, List<string> altinnAppIds, List<int> coveredByPartyIds, List<int> coveredByUserIds)
        {
            List<DelegationChange> result = new List<DelegationChange>();

            if (altinnAppIds.Any(appId => appId == "org1/app1") && offeredByPartyIds.Contains(50001337) && (coveredByUserIds != null && coveredByUserIds.Contains(20001337)))
            {
                result.Add(TestDataUtil.GetAltinnAppDelegationChange("org1/app1", 50001337, coveredByUserId: 20001337));
            }

            if (altinnAppIds.Any(appId => appId == "skd/taxreport") && offeredByPartyIds.Contains(1000) && (coveredByUserIds != null && coveredByUserIds.Contains(20001337)))
            {
                result.Add(TestDataUtil.GetAltinnAppDelegationChange("skd/taxreport", 1000, coveredByUserId: 20001337));
            }

            if (altinnAppIds.Any(appId => appId == "org1/app1") && offeredByPartyIds.Contains(50001337) && (coveredByUserIds != null && coveredByUserIds.Contains(20001338)))
            {
                DelegationChange delegation = TestDataUtil.GetAltinnAppDelegationChange("org1/app1", 50001337, coveredByUserId: 20001338);
                delegation.DelegationChangeType = DelegationChangeType.RevokeLast;
                result.Add(delegation);
            }

            if (altinnAppIds.Any(appId => appId == "org1/app1") && offeredByPartyIds.Contains(50001337) && (coveredByPartyIds != null && coveredByPartyIds.Contains(50001336)))
            {
                result.Add(TestDataUtil.GetAltinnAppDelegationChange("org1/app1", 50001337, coveredByPartyId: 50001336));
            }

            if (altinnAppIds.Contains("skd/taxreport") && offeredByPartyIds.Contains(50001337) && (coveredByUserIds != null && coveredByUserIds.Contains(20001336)))
            {
                result.Add(TestDataUtil.GetAltinnAppDelegationChange("skd/taxreport", 50001337, coveredByUserId: 20001336, performedByUserId: 20001337, changeType: DelegationChangeType.Grant));
            }

            if (altinnAppIds.Contains("skd/taxreport") && offeredByPartyIds.Contains(50001337) && (coveredByUserIds != null && coveredByUserIds.Contains(20001335)))
            {
                result.Add(TestDataUtil.GetAltinnAppDelegationChange("skd/taxreport", 50001337, coveredByPartyId: 50001335, performedByUserId: 20001337, changeType: DelegationChangeType.Grant));
            }

            if (altinnAppIds.Contains("skd/taxreport") && offeredByPartyIds.Contains(50001337) && (coveredByPartyIds != null && coveredByPartyIds.Contains(50001336)))
            {
                result.Add(TestDataUtil.GetAltinnAppDelegationChange("skd/taxreport", 50001337, coveredByPartyId: 50001338, performedByUserId: 20001337, changeType: DelegationChangeType.Grant));
            }

            if (altinnAppIds.Contains("skd/taxreport") && offeredByPartyIds.Contains(50001338) && (coveredByPartyIds != null && coveredByPartyIds.Contains(50001339)))
            {
                result.Add(TestDataUtil.GetAltinnAppDelegationChange("skd/taxreport", 50001338, coveredByPartyId: 50001339, performedByUserId: 20001338, changeType: DelegationChangeType.Grant));
            }

            if (altinnAppIds.Contains("skd/taxreport") && offeredByPartyIds.Contains(50001338) && (coveredByPartyIds != null && coveredByPartyIds.Contains(50001340)))
            {
                result.Add(TestDataUtil.GetAltinnAppDelegationChange("skd/taxreport", 50001338, coveredByPartyId: 50001340, performedByUserId: 20001338, changeType: DelegationChangeType.Grant));
            }

            if (altinnAppIds.Contains("skd/taxreport") && offeredByPartyIds.Contains(50001338) && (coveredByPartyIds != null && coveredByPartyIds.Contains(50001336)))
            {
                result.Add(TestDataUtil.GetAltinnAppDelegationChange("skd/taxreport", 50001338, coveredByPartyId: 50001336, performedByUserId: 20001339, changeType: DelegationChangeType.Grant));
            }

            if (altinnAppIds.Contains("skd/taxreport") && offeredByPartyIds.Contains(50001339) && (coveredByPartyIds != null && coveredByUserIds.Contains(20001336)))
            {
                result.Add(TestDataUtil.GetAltinnAppDelegationChange("skd/taxreport", 50001335, coveredByPartyId: 50001337, performedByUserId: 20001339, changeType: DelegationChangeType.Grant));
            }

            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task<List<DelegationChange>> GetOfferedResourceRegistryDelegations(int offeredByPartyId, ResourceType resourceType)
        {
            List<DelegationChange> result = new List<DelegationChange>();

            if (offeredByPartyId == 50004223 && resourceType == ResourceType.MaskinportenSchema)
            {
                result.Add(TestDataUtil.GetResourceRegistryDelegationChange("nav_aa_distribution", ResourceType.MaskinportenSchema, offeredByPartyId, null, 50004219, 20000002, DelegationChangeType.Grant, 1234));
                result.Add(TestDataUtil.GetResourceRegistryDelegationChange("nav_aa_distribution", ResourceType.MaskinportenSchema, offeredByPartyId, null, 50004220, 20000002, DelegationChangeType.Grant, 1235));
                result.Add(TestDataUtil.GetResourceRegistryDelegationChange("nav_aa_distribution", ResourceType.MaskinportenSchema, offeredByPartyId, null, 50004221, 20000002, DelegationChangeType.Grant, 1236));
                result.Add(TestDataUtil.GetResourceRegistryDelegationChange("skd_1", ResourceType.MaskinportenSchema, offeredByPartyId, null, 50004219, 20000002, DelegationChangeType.Grant, 1234));
                result.Add(TestDataUtil.GetResourceRegistryDelegationChange("skd_1", ResourceType.MaskinportenSchema, offeredByPartyId, null, 50004220, 20000002, DelegationChangeType.Grant, 1234));
                result.Add(TestDataUtil.GetResourceRegistryDelegationChange("skd_1", ResourceType.MaskinportenSchema, offeredByPartyId, null, 50004221, 20000002, DelegationChangeType.Grant, 1234));
            }
            else if (offeredByPartyId == 50004226 && resourceType == ResourceType.MaskinportenSchema)
            {
                result.Add(TestDataUtil.GetResourceRegistryDelegationChange("nav1_aa_distribution", ResourceType.MaskinportenSchema, offeredByPartyId, null, 50004219, 20000002, DelegationChangeType.Grant, 1234));
                result.Add(TestDataUtil.GetResourceRegistryDelegationChange("nav1_aa_distribution", ResourceType.MaskinportenSchema, offeredByPartyId, null, 50004220, 20000002, DelegationChangeType.Grant, 1235));
                result.Add(TestDataUtil.GetResourceRegistryDelegationChange("nav1_aa_distribution", ResourceType.MaskinportenSchema, offeredByPartyId, null, 50004221, 20000002, DelegationChangeType.Grant, 1236));
                result.Add(TestDataUtil.GetResourceRegistryDelegationChange("skd_1", ResourceType.MaskinportenSchema, offeredByPartyId, null, 50004219, 20000002, DelegationChangeType.Grant, 1234));
                result.Add(TestDataUtil.GetResourceRegistryDelegationChange("skd_1", ResourceType.MaskinportenSchema, offeredByPartyId, null, 50004220, 20000002, DelegationChangeType.Grant, 1234));
                result.Add(TestDataUtil.GetResourceRegistryDelegationChange("skd_1", ResourceType.MaskinportenSchema, offeredByPartyId, null, 50004221, 20000002, DelegationChangeType.Grant, 1234));
            }

            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task<List<DelegationChange>> GetReceivedResourceRegistryDelegationsForCoveredByPartys(int coveredByPartyId, ResourceType resourceType)
        {
            List<DelegationChange> result = new List<DelegationChange>();
            if (coveredByPartyId == 50004219 && resourceType == ResourceType.MaskinportenSchema)
            {
                result.Add(TestDataUtil.GetResourceRegistryDelegationChange("nav_aa_distribution", ResourceType.MaskinportenSchema, 50004222, null, coveredByPartyId, 20000008, DelegationChangeType.Grant, 1234));
                result.Add(TestDataUtil.GetResourceRegistryDelegationChange("skd_1", ResourceType.MaskinportenSchema, 50004222, null, coveredByPartyId, 20000008, DelegationChangeType.Grant, 1234));
                result.Add(TestDataUtil.GetResourceRegistryDelegationChange("nav_aa_distribution", ResourceType.MaskinportenSchema, 50004220, null, coveredByPartyId, 20000007, DelegationChangeType.Grant, 1234));
                result.Add(TestDataUtil.GetResourceRegistryDelegationChange("skd_1", ResourceType.MaskinportenSchema, 50004220, null, coveredByPartyId, 20000007, DelegationChangeType.Grant, 1234));
                result.Add(TestDataUtil.GetResourceRegistryDelegationChange("nav_aa_distribution", ResourceType.MaskinportenSchema, 50004221, null, coveredByPartyId, 20000002, DelegationChangeType.Grant, 1234));
                result.Add(TestDataUtil.GetResourceRegistryDelegationChange("skd_1", ResourceType.MaskinportenSchema, 50004221, null, coveredByPartyId, 20000002, DelegationChangeType.Grant, 1234));
            }
            else if (coveredByPartyId == 50004216 && resourceType == ResourceType.MaskinportenSchema)
            {
                result.Add(TestDataUtil.GetResourceRegistryDelegationChange("nav_aa_distribution", ResourceType.MaskinportenSchema, 50004222, null, coveredByPartyId, 20000008, DelegationChangeType.Grant, 1234));
                result.Add(TestDataUtil.GetResourceRegistryDelegationChange("skd_1", ResourceType.MaskinportenSchema, 50004222, null, coveredByPartyId, 20000008, DelegationChangeType.Grant, 1234));
                result.Add(TestDataUtil.GetResourceRegistryDelegationChange("nav1_aa_distribution", ResourceType.MaskinportenSchema, 50004226, null, coveredByPartyId, 20000002, DelegationChangeType.Grant, 1234));
                result.Add(TestDataUtil.GetResourceRegistryDelegationChange("skd_1", ResourceType.MaskinportenSchema, 50004226, null, coveredByPartyId, 20000002, DelegationChangeType.Grant, 1234));
            }

            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task<List<DelegationChange>> GetResourceRegistryDelegationChangesForAdmin(List<string> resourceIds, int offeredByPartyid, int coveredByPartyId, ResourceType resourceType)
        {
            List<DelegationChange> result = new List<DelegationChange>();
            if (offeredByPartyid == 50004222 && coveredByPartyId == 50004219 && (resourceIds != null && resourceIds.Count > 0))
            {
                result.Add(TestDataUtil.GetResourceRegistryDelegationChange("appid-119", resourceType, offeredByPartyid, null, coveredByPartyId, 20000008, DelegationChangeType.Grant));
                result.Add(TestDataUtil.GetResourceRegistryDelegationChange("appid-122", resourceType, offeredByPartyid, null, coveredByPartyId, 20000008, DelegationChangeType.Grant));
            }

            return Task.FromResult(result);
        }
    }
}
