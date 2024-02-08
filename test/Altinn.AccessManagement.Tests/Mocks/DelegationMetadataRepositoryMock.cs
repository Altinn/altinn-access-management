using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Tests.Utils;

namespace Altinn.AccessManagement.Tests.Mocks;

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
    public Task<DelegationChange> InsertDelegation(ResourceAttributeMatchType resourceMatchType, DelegationChange delegationChange)
    {
        List<DelegationChange> current;
        string coveredBy = delegationChange.CoveredByPartyId != null ? $"p{delegationChange.CoveredByPartyId}" : $"u{delegationChange.CoveredByUserId}";
        string key = string.Empty;

        if (resourceMatchType == ResourceAttributeMatchType.AltinnAppId)
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
            ResourceType = resourceMatchType == ResourceAttributeMatchType.AltinnAppId ? ResourceAttributeMatchType.AltinnAppId.ToString() : ResourceType.MaskinportenSchema.ToString(),
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
        DateTime created = Convert.ToDateTime("2022-09-27T13:02:23.786072Z");

        switch (resourceId)
        {
            case "app_org1_app1":
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
                result = TestDataUtil.GetResourceRegistryDelegationChange(resourceId, ResourceType.MaskinportenSchema, offeredByPartyId, created, coveredByUserId, coveredByPartyId);
                break;
            case "resource2":
                result = TestDataUtil.GetResourceRegistryDelegationChange(resourceId, ResourceType.MaskinportenSchema, offeredByPartyId, created, coveredByUserId, coveredByPartyId);
                break;
            case "jks_audi_etron_gt":
                result = TestDataUtil.GetResourceRegistryDelegationChange(resourceId, ResourceType.MaskinportenSchema, offeredByPartyId, created, coveredByUserId, coveredByPartyId);
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
    public Task<List<DelegationChange>> GetAllCurrentAppDelegationChanges(List<int> offeredByPartyIds, List<string> altinnAppIds, List<int> coveredByPartyIds, List<int> coveredByUserIds, CancellationToken cancellationToken = default)
    {
        List<DelegationChange> result = new List<DelegationChange>();
        altinnAppIds ??= [];
        if (altinnAppIds.Count == 0 && offeredByPartyIds.Contains(20001337))
        {
            result.Add(TestDataUtil.GetAltinnAppDelegationChange("org1/app1", 20001337, 20001336));
        }

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
    public Task<List<DelegationChange>> GetAllCurrentResourceRegistryDelegationChanges(List<int> offeredByPartyIds, List<string> resourceRegistryIds = null, List<int> coveredByPartyIds = null, int? coveredByUserId = null)
    {
        List<DelegationChange> result = new List<DelegationChange>();

        foreach (string resourceRegistryId in resourceRegistryIds)
        {
            foreach (int offeredByPartyId in offeredByPartyIds)
            {
                if (coveredByPartyIds != null)
                {
                    foreach (int coveredByPartyId in coveredByPartyIds)
                    {
                        string delegationChangePath = GetResourceRegistryDelegationPath_ForCoveredByPartyId(resourceRegistryId, offeredByPartyId, coveredByPartyId);
                        if (File.Exists(delegationChangePath))
                        {
                            string content = File.ReadAllText(delegationChangePath);
                            DelegationChange delegationChange = (DelegationChange)JsonSerializer.Deserialize(content, typeof(DelegationChange), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            result.Add(delegationChange);
                        }
                    }
                }

                if (coveredByUserId.HasValue)
                {
                    string delegationChangePath = GetResourceRegistryDelegationPath_ForCoveredByUserId(resourceRegistryId, offeredByPartyId, coveredByUserId.Value);
                    if (File.Exists(delegationChangePath))
                    {
                        string content = File.ReadAllText(delegationChangePath);
                        DelegationChange delegationChange = (DelegationChange)JsonSerializer.Deserialize(content, typeof(DelegationChange), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        result.Add(delegationChange);
                    }
                }
            }
        }

        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public Task<List<DelegationChange>> GetOfferedResourceRegistryDelegations(int offeredByPartyId, List<string> resourceRegistryIds = null, List<ResourceType> resourceTypes = null, CancellationToken cancellationToken = default)
    {
        List<DelegationChange> result = new List<DelegationChange>();
        DateTime created = Convert.ToDateTime("2022-09-27T13:02:23.786072Z");
        if (offeredByPartyId == 20001337)
        {
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("altinn_access_management", ResourceType.Systemresource, 20001337, created, null, 50004219, 20000002, DelegationChangeType.Grant, 1234));
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("app_org1_app1", ResourceType.AltinnApp, 20001337, created, null, 50004220, 20000002, DelegationChangeType.Grant, 1235));
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("scope-access-schema", ResourceType.MaskinportenSchema, 20001337, created, null, 50004221, 20000002, DelegationChangeType.Grant, 1236));
        }

        if (offeredByPartyId == 50004223 && resourceTypes.Count == 1 && resourceTypes.First() == ResourceType.MaskinportenSchema)
        {
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("nav_aa_distribution", ResourceType.MaskinportenSchema, 50004223, created, null, 50004219, 20000002, DelegationChangeType.Grant, 1234));
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("nav_aa_distribution", ResourceType.MaskinportenSchema, 50004223, created, null, 50004220, 20000002, DelegationChangeType.Grant, 1235));
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("nav_aa_distribution", ResourceType.MaskinportenSchema, 50004223, created, null, 50004221, 20000002, DelegationChangeType.Grant, 1236));
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("skd_1", ResourceType.MaskinportenSchema, 50004223, created, null, 50004219, 20000002, DelegationChangeType.Grant, 1234));
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("skd_1", ResourceType.MaskinportenSchema, 50004223, created, null, 50004220, 20000002, DelegationChangeType.Grant, 1234));
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("skd_1", ResourceType.MaskinportenSchema, 50004223, created, null, 50004221, 20000002, DelegationChangeType.Grant, 1234));
        }
        else if (offeredByPartyId == 50004226 && resourceTypes.Count == 1 && resourceTypes.First() == ResourceType.MaskinportenSchema)
        {
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("nav1_aa_distribution", ResourceType.MaskinportenSchema, 50004226, created, null, 50004219, 20000002, DelegationChangeType.Grant, 1234));
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("nav1_aa_distribution", ResourceType.MaskinportenSchema, 50004226, created, null, 50004220, 20000002, DelegationChangeType.Grant, 1235));
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("nav1_aa_distribution", ResourceType.MaskinportenSchema, 50004226, created, null, 50004221, 20000002, DelegationChangeType.Grant, 1236));
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("skd_1", ResourceType.MaskinportenSchema, 50004226, created, null, 50004219, 20000002, DelegationChangeType.Grant, 1234));
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("skd_1", ResourceType.MaskinportenSchema, 50004226, created, null, 50004220, 20000002, DelegationChangeType.Grant, 1234));
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("skd_1", ResourceType.MaskinportenSchema, 50004226, created, null, 50004221, 20000002, DelegationChangeType.Grant, 1234));
        }

        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public Task<List<DelegationChange>> GetReceivedResourceRegistryDelegationsForCoveredByPartys(List<int> coveredByPartyIds, List<int> offeredByPartyIds, List<string> resourceRegistryIds = null, List<ResourceType> resourceTypes = null)
    {
        List<DelegationChange> result = new List<DelegationChange>();
        DateTime created = DateTime.Parse("2022-09-27T13:02:23.786072Z");
        if (coveredByPartyIds.Count == 1 && coveredByPartyIds.First() == 50004219 && resourceTypes.Count == 1 && resourceTypes.First() == ResourceType.MaskinportenSchema)
        {
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("nav_aa_distribution", ResourceType.MaskinportenSchema, 50004222, created, null, 50004219, 20000008, DelegationChangeType.Grant, 1234));
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("skd_1", ResourceType.MaskinportenSchema, 50004222, created, null, 50004219, 20000008, DelegationChangeType.Grant, 1234));
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("nav_aa_distribution", ResourceType.MaskinportenSchema, 50004220, created, null, 50004219, 20000007, DelegationChangeType.Grant, 1234));
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("skd_1", ResourceType.MaskinportenSchema, 50004220, created, null, 50004219, 20000007, DelegationChangeType.Grant, 1234));
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("nav_aa_distribution", ResourceType.MaskinportenSchema, 50004221, created, null, 50004219, 20000002, DelegationChangeType.Grant, 1234));
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("skd_1", ResourceType.MaskinportenSchema, 50004221, created, null, 50004219, 20000002, DelegationChangeType.Grant, 1234));
        }
        else if (coveredByPartyIds.Count == 1 && coveredByPartyIds.First() == 50004216 && resourceTypes.Count == 1 && resourceTypes.First() == ResourceType.MaskinportenSchema)
        {
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("nav_aa_distribution", ResourceType.MaskinportenSchema, 50004222, created, null, 50004216, 20000008, DelegationChangeType.Grant, 1234));
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("skd_1", ResourceType.MaskinportenSchema, 50004222, created, null, 50004216, 20000008, DelegationChangeType.Grant, 1234));
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("nav1_aa_distribution", ResourceType.MaskinportenSchema, 50004226, created, null, 50004216, 20000002, DelegationChangeType.Grant, 1234));
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("skd_1", ResourceType.MaskinportenSchema, 50004226, created, null, 50004216, 20000002, DelegationChangeType.Grant, 1234));
        }

        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public Task<List<DelegationChange>> GetResourceRegistryDelegationChanges(List<string> resourceIds, int offeredByPartyid, int coveredByPartyId, ResourceType resourceType)
    {
        List<DelegationChange> result = new List<DelegationChange>();
        if (offeredByPartyid == 50004222 && coveredByPartyId == 50004219 && (resourceIds != null && resourceIds.Count > 0))
        {
            DateTime created = Convert.ToDateTime("2022-09-27T13:02:23.786072Z");
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("nav_aa_distribution", resourceType, offeredByPartyid, created, null, coveredByPartyId, 20000008, DelegationChangeType.Grant));
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("appid-123", resourceType, offeredByPartyid, created, null, coveredByPartyId, 20000008, DelegationChangeType.Grant));
        }

        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public Task<List<DelegationChange>> GetReceivedResourceRegistryDelegationsForCoveredByUser(int coveredByUserId, List<int> offeredByPartyIds, List<string> resourceRegistryIds = null, List<ResourceType> resourceTypes = null)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<List<DelegationChange>> GetAllDelegationChangesForAuthorizedParties(List<int> coveredByUserIds, List<int> coveredByPartyIds, CancellationToken cancellationToken = default)
    {
        List<DelegationChange> result = new List<DelegationChange>();
        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public Task<List<DelegationChange>> GetReceivedDelegations(List<int> offeredByPartyIds, CancellationToken cancellationToken = default)
    {
        var result = new List<DelegationChange>();
        foreach (var offeredBy in offeredByPartyIds)
        {
            if (offeredBy == 50002203)
            {
                result.Add(TestDataUtil.GetResourceRegistryDelegationChange("altinn_access_management", ResourceType.Systemresource, offeredBy, DateTime.Now, coveredByPartyId: 50005545));
                result.Add(TestDataUtil.GetResourceRegistryDelegationChange("org1/app1", ResourceType.AltinnApp, offeredBy, DateTime.Now, coveredByPartyId: 50005545));
            }

            if (offeredBy == 50005545)
            {
                result.Add(TestDataUtil.GetResourceRegistryDelegationChange("altinn_access_management", ResourceType.Systemresource, offeredBy, DateTime.Now, coveredByUserId: 20000095));
                result.Add(TestDataUtil.GetResourceRegistryDelegationChange("org1/app1", ResourceType.AltinnApp, offeredBy, DateTime.Now, coveredByUserId: 20000095));
            }
        }

        return Task.FromResult(result);
    }

    private static string GetResourceRegistryDelegationPath_ForCoveredByPartyId(string resourceRegistryId, int offeredByPartyId, int coveredByPartyId)
    {
        string? unitTestFolder = Path.GetDirectoryName(new Uri(typeof(DelegationMetadataRepositoryMock).Assembly.Location).LocalPath);
        return Path.Combine(unitTestFolder, "Data", "ResourceRegistryDelegationChanges", $"{resourceRegistryId}", $"{offeredByPartyId}", $"p{coveredByPartyId}", "delegationchange.json");
    }

    private static string GetResourceRegistryDelegationPath_ForCoveredByUserId(string resourceRegistryId, int offeredByPartyId, int coveredByUserId)
    {
        string? unitTestFolder = Path.GetDirectoryName(new Uri(typeof(DelegationMetadataRepositoryMock).Assembly.Location).LocalPath);
        return Path.Combine(unitTestFolder, "Data", "ResourceRegistryDelegationChanges", $"{resourceRegistryId}", $"{offeredByPartyId}", $"u{coveredByUserId}", "delegationchange.json");
    }
}
