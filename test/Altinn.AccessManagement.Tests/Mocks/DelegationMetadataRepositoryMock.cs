using System.Text;
using System.Text.Json;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Enums;
using Altinn.AccessManagement.Tests.Data;
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
    public Task<DelegationChange> InsertDelegation(ResourceAttributeMatchType resourceMatchType, DelegationChange delegationChange, CancellationToken cancellationToken = default)
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

    public Task<InstanceDelegationChange> GetLastInstanceDelegationChange(InstanceDelegationChangeRequest request, CancellationToken cancellationToken = default)
    {
        Random random = new Random();
        switch (request.Instance)
        {
            case "00000000-0000-0000-0000-000000000001":
            case "00000000-0000-0000-0000-000000000009":
            case "00000000-0000-0000-0000-000000000010":
                return Task.FromResult(new InstanceDelegationChange
                {
                    FromUuidType = request.FromType,
                    FromUuid = request.FromUuid,
                    ToUuidType = request.ToType,
                    ToUuid = request.ToUuid,
                    PerformedBy = request.Resource,
                    PerformedByType = UuidType.Resource,
                    BlobStoragePolicyPath = BuildPolicyPath(request.ToUuid, request.InstanceDelegationMode, request.Resource, request.Instance),
                    BlobStorageVersionId = "2024-09-13T16:59:13.123Z",
                    Created = new DateTime(2024, 9, 13, 16, 59, 13, 347, DateTimeKind.Utc),
                    InstanceId = request.Instance,
                    DelegationChangeType = DelegationChangeType.Grant,
                    InstanceDelegationChangeId = random.Next(1, 1000),
                    InstanceDelegationMode = InstanceDelegationMode.ParallelSigning,
                    ResourceId = request.Resource
                });
            default:
                return Task.FromResult((InstanceDelegationChange)null);
        }
    }

    public Task<InstanceDelegationChange> InsertInstanceDelegation(InstanceDelegationChange instanceDelegationChange, CancellationToken cancellationToken = default)
    {
        Random random = new();
        string path = GetDelegationPolicyPathFromInstanceRule(instanceDelegationChange);
        InstanceDelegationChange result = instanceDelegationChange.InstanceId switch
        {
            "00000000-0000-0000-0000-000000000002" => null,
            _ => new InstanceDelegationChange
            {
                InstanceDelegationChangeId = random.Next(0, 1000),
                DelegationChangeType = instanceDelegationChange.DelegationChangeType,
                InstanceDelegationMode = instanceDelegationChange.InstanceDelegationMode,
                ResourceId = instanceDelegationChange.ResourceId,
                InstanceId = instanceDelegationChange.InstanceId,
                FromUuid = instanceDelegationChange.FromUuid,
                FromUuidType = instanceDelegationChange.FromUuidType,
                ToUuid = instanceDelegationChange.ToUuid,
                ToUuidType = instanceDelegationChange.ToUuidType,
                PerformedBy = instanceDelegationChange.PerformedBy,
                PerformedByType = instanceDelegationChange.PerformedByType,
                BlobStoragePolicyPath = path,
                BlobStorageVersionId = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                Created = DateTime.Now
            },
        };

        return Task.FromResult(result);
    }

    private static string GetDelegationPolicyPathFromInstanceRule(InstanceDelegationChange change)
    {
        StringBuilder sb = new StringBuilder();

        sb.Append(change.ResourceId);

        sb.Append('/');

        sb.Append(change.FromUuidType);
        sb.Append('-');
        sb.Append(change.FromUuid);
        sb.Append('/');

        sb.Append(change.ToUuidType);
        sb.Append('-');
        sb.Append(change.ToUuid);
        sb.Append('/');

        sb.Append(change.InstanceId.AsFileName(false));

        sb.Append('/');
        sb.Append(change.InstanceDelegationMode);

        sb.Append("/delegationpolicy.xml");
        return sb.ToString();
    }

    /// <inheritdoc/>
    public Task<DelegationChange> GetCurrentDelegationChange(ResourceAttributeMatchType resourceMatchType, string resourceId, int offeredByPartyId, int? coveredByPartyId, int? coveredByUserId, Guid? toUuid, UuidType toUuidType, CancellationToken cancellationToken = default)
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
    public Task<List<DelegationChange>> GetAllAppDelegationChanges(string altinnAppId, int offeredByPartyId, int? coveredByPartyId, int? coveredByUserId, CancellationToken cancellationToken = default)
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
    public Task<List<DelegationChange>> GetAllCurrentAppDelegationChanges(List<string> altinnAppIds, List<int> fromPartyIds, UuidType toUuidType, Guid toUuid, CancellationToken cancellationToken = default)
    {
        List<DelegationChange> result = new List<DelegationChange>();
        if (altinnAppIds.Contains("ttd/apps-test") && fromPartyIds.Contains(50005545) && toUuid == new Guid("a6355a68-86b8-4344-8a81-0248cb461468"))
        {
            result.Add(new DelegationChange
            {
                DelegationChangeId = 1337,
                DelegationChangeType = DelegationChangeType.Grant,
                ResourceId = "ttd/apps-test",
                ResourceType = ResourceAttributeMatchType.AltinnAppId.ToString(),
                OfferedByPartyId = 50005545,
                FromUuid = new Guid("00000000-0000-0000-0005-000000005545"),
                FromUuidType = UuidType.Organization,
                CoveredByPartyId = null,
                CoveredByUserId = null,
                ToUuid = new Guid("a6355a68-86b8-4344-8a81-0248cb461468"),
                ToUuidType = UuidType.SystemUser,
                PerformedByUserId = 20000490,
                PerformedByUuid = null,
                PerformedByUuidType = UuidType.NotSpecified,
                BlobStoragePolicyPath = $"ttd/apps-test/50005545/SystemUsera6355a68-86b8-4344-8a81-0248cb461468/delegationpolicy.xml",
                BlobStorageVersionId = "2024-07-18T13:37:00.1337Z",
                Created = Convert.ToDateTime("2024-07-18T13:37:00.1337Z")
            });
        }

        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public Task<List<DelegationChange>> GetAllCurrentResourceRegistryDelegationChanges(List<int> offeredByPartyIds, List<string> resourceRegistryIds = null, List<int> coveredByPartyIds = null, int? coveredByUserId = null, CancellationToken cancellationToken = default)
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
    public Task<List<DelegationChange>> GetAllCurrentResourceRegistryDelegationChanges(List<string> resourceRegistryIds, List<int> fromPartyIds, UuidType toUuidType, Guid toUuid, CancellationToken cancellationToken = default)
    {
        List<DelegationChange> result = new List<DelegationChange>();
        if (resourceRegistryIds.Contains("generic-access-resource") && fromPartyIds.Contains(50005545) && toUuid == new Guid("a6355a68-86b8-4344-8a81-0248cb461468"))
        {
            result.Add(new DelegationChange
            {
                ResourceRegistryDelegationChangeId = 1337,
                DelegationChangeType = DelegationChangeType.Grant,
                ResourceId = "generic-access-resource",
                ResourceType = ResourceType.GenericAccessResource.ToString().ToLower(),
                OfferedByPartyId = 50005545,
                FromUuid = new Guid("00000000-0000-0000-0005-000000005545"),
                FromUuidType = UuidType.Organization,
                CoveredByPartyId = null,
                CoveredByUserId = null,
                ToUuid = new Guid("a6355a68-86b8-4344-8a81-0248cb461468"),
                ToUuidType = UuidType.SystemUser,
                PerformedByUserId = 20000490,
                PerformedByUuid = null,
                PerformedByUuidType = UuidType.NotSpecified,
                BlobStoragePolicyPath = $"resourceregistry/generic-access-resource/50005545/SystemUsera6355a68-86b8-4344-8a81-0248cb461468/delegationpolicy.xml",
                BlobStorageVersionId = "2024-07-18T13:37:00.1337Z",
                Created = Convert.ToDateTime("2024-07-18T13:37:00.1337Z")
            });
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
    public Task<List<DelegationChange>> GetReceivedResourceRegistryDelegationsForCoveredByPartys(List<int> coveredByPartyIds, List<int> offeredByPartyIds, List<string> resourceRegistryIds = null, List<ResourceType> resourceTypes = null, CancellationToken cancellationToken = default)
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
    public Task<List<DelegationChange>> GetResourceRegistryDelegationChanges(List<string> resourceIds, int offeredByPartyid, int coveredByPartyId, ResourceType resourceType, CancellationToken cancellationToken = default)
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
    public Task<List<DelegationChange>> GetReceivedResourceRegistryDelegationsForCoveredByUser(int coveredByUserId, List<int> offeredByPartyIds, List<string> resourceRegistryIds = null, List<ResourceType> resourceTypes = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<List<DelegationChange>> GetAllDelegationChangesForAuthorizedParties(List<int> coveredByUserIds, List<int> coveredByPartyIds, CancellationToken cancellationToken = default)
    {
        List<DelegationChange> result = new List<DelegationChange>();
        DateTime created = Convert.ToDateTime("2024-02-05T21:05:00.00Z");
        if (coveredByPartyIds != null && coveredByPartyIds.Contains(50005545))
        {
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("altinn_access_management", ResourceType.Systemresource, 50002203, DateTime.Now, coveredByPartyId: 50005545));
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("org1/app1", ResourceType.AltinnApp, 50002203, DateTime.Now, coveredByPartyId: 50005545));
        }
        else if (coveredByUserIds != null && coveredByUserIds.Contains(20000095))
        {
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("altinn_access_management", ResourceType.Systemresource, 50002203, DateTime.Now, coveredByUserId: 20000095));
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("org1/app1", ResourceType.AltinnApp, 50002203, DateTime.Now, coveredByUserId: 20000095));
        }
        else if (coveredByUserIds != null && coveredByUserIds.Contains(TestDataAuthorizedParties.PersonToPerson_ToUserId))
        {
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("devtest_gar_authparties-person-to-person", ResourceType.GenericAccessResource, TestDataAuthorizedParties.PersonToPerson_FromPartyId, created, coveredByUserId: TestDataAuthorizedParties.PersonToPerson_ToUserId, performedByUserId: TestDataAuthorizedParties.PersonToPerson_FromUserId, changeType: DelegationChangeType.Grant));
            result.Add(TestDataUtil.GetAltinnAppDelegationChange("ttd/am-devtest-person-to-person", TestDataAuthorizedParties.PersonToPerson_FromPartyId, coveredByUserId: TestDataAuthorizedParties.PersonToPerson_ToUserId, performedByUserId: TestDataAuthorizedParties.PersonToPerson_FromUserId, changeType: DelegationChangeType.Grant));
        }
        else if (coveredByPartyIds != null && coveredByPartyIds.Contains(TestDataAuthorizedParties.PersonToOrg_ToOrgPartyId))
        {
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("devtest_gar_authparties-person-to-org", ResourceType.GenericAccessResource, TestDataAuthorizedParties.PersonToOrg_FromPartyId, created, coveredByPartyId: TestDataAuthorizedParties.PersonToOrg_ToOrgPartyId, performedByUserId: TestDataAuthorizedParties.PersonToOrg_FromUserId, changeType: DelegationChangeType.Grant));
            result.Add(TestDataUtil.GetAltinnAppDelegationChange("ttd/am-devtest-person-to-org", TestDataAuthorizedParties.PersonToOrg_FromPartyId, coveredByPartyId: TestDataAuthorizedParties.PersonToOrg_ToOrgPartyId, performedByUserId: TestDataAuthorizedParties.PersonToOrg_FromUserId, changeType: DelegationChangeType.Grant));
        }
        else if (coveredByUserIds != null && coveredByUserIds.Contains(TestDataAuthorizedParties.MainUnitAndSubUnitToPerson_ToUserId))
        {
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("devtest_gar_authparties-main-to-person", ResourceType.GenericAccessResource, TestDataAuthorizedParties.MainUnit_PartyId, created, coveredByUserId: TestDataAuthorizedParties.MainUnitAndSubUnitToPerson_ToUserId, changeType: DelegationChangeType.Grant));
            result.Add(TestDataUtil.GetAltinnAppDelegationChange("ttd/am-devtest-sub-to-person", TestDataAuthorizedParties.SubUnit_PartyId, coveredByUserId: TestDataAuthorizedParties.MainUnitAndSubUnitToPerson_ToUserId, changeType: DelegationChangeType.Grant));
        }
        else if (coveredByPartyIds != null && coveredByPartyIds.Contains(TestDataAuthorizedParties.MainUnitAndSubUnitToOrg_ToOrgPartyId))
        {
            result.Add(TestDataUtil.GetAltinnAppDelegationChange("ttd/am-devtest-sub-to-org", TestDataAuthorizedParties.SubUnit_PartyId, coveredByPartyId: TestDataAuthorizedParties.PersonToOrg_ToOrgPartyId, changeType: DelegationChangeType.Grant));
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("devtest_gar_authparties-main-to-org", ResourceType.GenericAccessResource, TestDataAuthorizedParties.MainUnit_PartyId, created, coveredByPartyId: TestDataAuthorizedParties.PersonToOrg_ToOrgPartyId, changeType: DelegationChangeType.Grant));
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("devtest_gar_authparties-sub-to-org", ResourceType.GenericAccessResource, TestDataAuthorizedParties.SubUnit_PartyId, created, coveredByPartyId: TestDataAuthorizedParties.PersonToOrg_ToOrgPartyId, changeType: DelegationChangeType.Grant));
        }
        else if (coveredByUserIds != null && coveredByUserIds.Contains(TestDataAuthorizedParties.SubUnitToPerson_ToUserId))
        {
            result.Add(TestDataUtil.GetAltinnAppDelegationChange("ttd/am-devtest-sub-to-person", TestDataAuthorizedParties.SubUnit_PartyId, coveredByUserId: TestDataAuthorizedParties.SubUnitToPerson_ToUserId, changeType: DelegationChangeType.Grant));
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("devtest_gar_authparties-sub-to-person", ResourceType.GenericAccessResource, TestDataAuthorizedParties.SubUnit_PartyId, created, coveredByUserId: TestDataAuthorizedParties.SubUnitToPerson_ToUserId, changeType: DelegationChangeType.Grant));
            result.Add(TestDataUtil.GetResourceRegistryDelegationChange("devtest_gar_authparties-sub2-to-person", ResourceType.GenericAccessResource, TestDataAuthorizedParties.SubUnitTwo_PartyId, created, coveredByUserId: TestDataAuthorizedParties.SubUnitToPerson_ToUserId, changeType: DelegationChangeType.Grant));
        }

        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public Task<List<DelegationChange>> GetOfferedDelegations(List<int> offeredByPartyIds, CancellationToken cancellationToken = default)
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

    /// <inheritdoc />
    public Task<IEnumerable<InstanceDelegationChange>> GetActiveInstanceDelegations(List<string> resourceIds, Guid from, List<Guid> to, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<InstanceDelegationChange>>(new List<InstanceDelegationChange>());
    }

    private static string GetResourceRegistryDelegationPath_ForCoveredByPartyId(string resourceRegistryId, int offeredByPartyId, int coveredByPartyId, CancellationToken cancellationToken = default)
    {
        string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(DelegationMetadataRepositoryMock).Assembly.Location).LocalPath);
        return Path.Combine(unitTestFolder, "Data", "ResourceRegistryDelegationChanges", $"{resourceRegistryId}", $"{offeredByPartyId}", $"p{coveredByPartyId}", "delegationchange.json");
    }

    private static string GetResourceRegistryDelegationPath_ForCoveredByUserId(string resourceRegistryId, int offeredByPartyId, int coveredByUserId, CancellationToken cancellationToken = default)
    {
        string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(DelegationMetadataRepositoryMock).Assembly.Location).LocalPath);
        return Path.Combine(unitTestFolder, "Data", "ResourceRegistryDelegationChanges", $"{resourceRegistryId}", $"{offeredByPartyId}", $"u{coveredByUserId}", "delegationchange.json");
    }

    private static InstanceDelegationChange CreateInstanceDelegationChange(InstanceDelegationSource source, string resourceId, string instanceId, Guid toUuid)
    {
        Random random = new Random();
        InstanceDelegationChange result = new InstanceDelegationChange
        {
            FromUuidType = UuidType.Organization,
            FromUuid = Guid.Parse("B537C953-03C4-4822-B028-C15182ADC356"),
            ToUuidType = UuidType.Person,
            ToUuid = toUuid,
            PerformedBy = "app_ttd_am-devtest-instancedelegation",
            PerformedByType = UuidType.Resource,
            BlobStoragePolicyPath = BuildPolicyPath(toUuid, InstanceDelegationMode.Normal, resourceId, instanceId),
            BlobStorageVersionId = "2024-09-13T16:59:13.123Z",
            Created = new DateTime(2024, 9, 13, 16, 59, 13, 347, DateTimeKind.Utc),
            InstanceId = instanceId,
            DelegationChangeType = DelegationChangeType.Grant,
            InstanceDelegationChangeId = random.Next(1, 1000),
            InstanceDelegationMode = InstanceDelegationMode.Normal,
            ResourceId = resourceId
        };

        return result;
    }

    private static string BuildPolicyPath(Guid to, InstanceDelegationMode mode, string resourceId, string instanceId)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"Instance/{resourceId}");
        sb.Append('/');
        sb.Append(instanceId.ToUpper().AsSpan(24));
        sb.Append('/');
        sb.Append(mode.ToString().ToUpper().AsSpan(0, 1));
        sb.Append('/');
        sb.Append(to.ToString().ToUpper().AsSpan(24));
        sb.Append("/delegationpolicy.xml");
        return sb.ToString();
    }

    public Task<List<InstanceDelegationChange>> GetAllLatestInstanceDelegationChanges(InstanceDelegationSource source, string resourceID, string instanceID, CancellationToken cancellationToken = default)
    {
        List<InstanceDelegationChange> result = new List<InstanceDelegationChange>();
        switch (instanceID)
        {
            case "00000000-0000-0000-0000-000000000008":
                result.Add(CreateInstanceDelegationChange(source, resourceID, instanceID, Guid.Parse("CE4BA72B-D111-404F-95B5-313FB3847FA1")));
                result.Add(CreateInstanceDelegationChange(source, resourceID, instanceID, Guid.Parse("0268B99A-5817-4BBF-9B62-D90B16D527EA")));
                return Task.FromResult(result);
            case "00000000-0000-0000-0000-000000000010":
                result.Add(CreateInstanceDelegationChange(source, resourceID, instanceID, Guid.Parse("CE4BA72B-D111-404F-95B5-313FB3847FA1")));
                result.Add(CreateInstanceDelegationChange(source, resourceID, instanceID, Guid.Parse("0268B99A-5817-4BBF-9B62-D90B16D527EA")));
                result.Add(CreateInstanceDelegationChange(source, resourceID, instanceID, Guid.Parse("00000000-0000-0000-0001-000000000012")));
                result.Add(CreateInstanceDelegationChange(source, resourceID, instanceID, Guid.Parse("00000000-0000-0000-0001-000000000010")));
                return Task.FromResult(result);
            default:
                return Task.FromResult(result);
        }
    }

    public Task<List<InstanceDelegationChange>> GetAllCurrentReceivedInstanceDelegations(List<Guid> toUuid, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new List<InstanceDelegationChange>());
    }

    public Task<bool> InsertMultipleInstanceDelegations(List<PolicyWriteOutput> policyWriteOutputs, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}
