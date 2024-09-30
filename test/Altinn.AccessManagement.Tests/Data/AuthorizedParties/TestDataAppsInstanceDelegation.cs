using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.Register;
using Altinn.AccessManagement.Core.Models.Rights;
using Altinn.AccessManagement.Models;
using Altinn.AccessManagement.Tests.Util;
using Altinn.AccessManagement.Tests.Utils;
using Altinn.Urn.Json;
using System.Text.Json;

namespace Altinn.AccessManagement.Tests.Data;

public static class TestDataAppsInstanceDelegation
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    private static readonly string AppId = "app_ttd_am-devtest-instancedelegation";

    private static readonly string InstanceIdParallelNewPolicy = "0191579e-72bc-7977-af5d-f9e92af4393b";

    private static readonly string InstanceIdParallelExistingPolicy = "00000000-0000-0000-0000-000000000001";

    private static readonly string InstanceIdNewPolicyNoResponceOnWrite = "00000000-0000-0000-0000-000000000002";

    private static readonly string InstanceIdNormalNewPolicy = "00000000-0000-0000-0000-000000000003";

    private static readonly string InstanceIdNormalExistingPolicy = "00000000-0000-0000-0000-000000000005";

    /// <summary>
    /// Test case:  POST v1/apps/instancedelegation/{resourceId}/{instanceId}
    ///             with: 
    ///                - a valid app as the delegater
    ///                - valid resource for instance delegation
    ///                - Instancedelegation mode set to ParallelSigning
    ///                - no policy file with existing rights delegated
    /// Expected:   - Should return 200 OK
    ///             - Should include the delegated rights
    /// Reason:     Apps defined in the policy file should be able to delegate the defined rights
    /// </summary>
    public static TheoryData<string, AppsInstanceDelegationRequestDto, string, string, AppsInstanceDelegationResponseDto> DelegateParallelReadForAppNoExistingPolicy() => new()
    {
        {
            PrincipalUtil.GetAccessToken("ttd", "am-devtest-instancedelegation"),
            GetRequest<AppsInstanceDelegationRequestDto>(AppId, InstanceIdParallelNewPolicy),
            AppId,
            InstanceIdParallelNewPolicy,
            GetExpectedResponse<AppsInstanceDelegationResponseDto>(AppId, InstanceIdParallelNewPolicy)
        }
    };

    /// <summary>
    /// Test case:  POST v1/apps/instancedelegation/{resourceId}/{instanceId}
    ///             with: 
    ///                - a valid app as the delegater
    ///                - valid resource for instance delegation
    ///                - Instancedelegation mode set to ParallelSigning
    ///                - existing policy existing with rights delegated
    /// Expected:   - Should return 200 OK
    ///             - Should include the delegated rights
    /// Reason:     Apps defined in the policy file should be able to delegate the defined rights
    /// </summary>
    public static TheoryData<string, AppsInstanceDelegationRequestDto, string, string, AppsInstanceDelegationResponseDto> DelegateParallelSignForAppExistingPolicy() => new()
    {
        {            
            PrincipalUtil.GetAccessToken("ttd", "am-devtest-instancedelegation"),
            GetRequest<AppsInstanceDelegationRequestDto>(AppId, InstanceIdParallelExistingPolicy),
            AppId,
            InstanceIdParallelExistingPolicy,
            GetExpectedResponse<AppsInstanceDelegationResponseDto>(AppId, InstanceIdParallelExistingPolicy)
        }
    };

    /// <summary>
    /// Test case:  POST v1/apps/instancedelegation/{resourceId}/{instanceId}
    ///             with: 
    ///                - a valid app as the delegater
    ///                - valid resource for instance delegation
    ///                - Instancedelegation mode set to Normal
    ///                - no policy file with existing rights delegated
    /// Expected:   - Should return 200 OK
    ///             - Should include the delegated rights
    /// Reason:     Apps defined in the policy file should be able to delegate the defined rights
    /// </summary>
    public static TheoryData<string, AppsInstanceDelegationRequestDto, string, string, AppsInstanceDelegationResponseDto> DelegateReadForAppNoExistingPolicyNoResponceDBWrite() => new()
    {
        {
            PrincipalUtil.GetAccessToken("ttd", "am-devtest-instancedelegation"),
            GetRequest<AppsInstanceDelegationRequestDto>(AppId, InstanceIdNewPolicyNoResponceOnWrite),
            AppId,
            InstanceIdNewPolicyNoResponceOnWrite,
            GetExpectedResponse<AppsInstanceDelegationResponseDto>(AppId, InstanceIdNewPolicyNoResponceOnWrite)
        }
    };

    /// <summary>
    /// Test case:  POST v1/apps/instancedelegation/{resourceId}/{instanceId}
    ///             with: 
    ///                - a valid app as the delegater
    ///                - valid resource for instance delegation
    ///                - Instancedelegation mode set to ParallelSigning
    ///                - no policy file with existing rights delegated
    /// Expected:   - Should return 200 OK
    ///             - Should include the delegated rights
    /// Reason:     Apps defined in the policy file should be able to delegate the defined rights
    /// </summary>
    public static TheoryData<string, AppsInstanceDelegationRequestDto, string, string, AppsInstanceDelegationResponseDto> DelegateNormalReadForAppNoExistingPolicy() => new()
    {
        {
            PrincipalUtil.GetAccessToken("ttd", "am-devtest-instancedelegation"),
            GetRequest<AppsInstanceDelegationRequestDto>(AppId, InstanceIdParallelNewPolicy),
            AppId,
            InstanceIdParallelNewPolicy,
            GetExpectedResponse<AppsInstanceDelegationResponseDto>(AppId, InstanceIdParallelNewPolicy)
        }
    };

    /// <summary>
    /// Test case:  POST v1/apps/instancedelegation/{resourceId}/{instanceId}
    ///             with: 
    ///                - a valid app as the delegater
    ///                - valid resource for instance delegation
    ///                - Instancedelegation mode set to ParallelSigning
    ///                - existing policy existing with rights delegated
    /// Expected:   - Should return 200 OK
    ///             - Should include the delegated rights
    /// Reason:     Apps defined in the policy file should be able to delegate the defined rights
    /// </summary>
    public static TheoryData<string, AppsInstanceDelegationRequestDto, string, string, AppsInstanceDelegationResponseDto> DelegateNormalSignForAppExistingPolicy() => new()
    {
        {
            PrincipalUtil.GetAccessToken("ttd", "am-devtest-instancedelegation"),
            GetRequest<AppsInstanceDelegationRequestDto>(AppId, InstanceIdParallelExistingPolicy),
            AppId,
            InstanceIdParallelExistingPolicy,
            GetExpectedResponse<AppsInstanceDelegationResponseDto>(AppId, InstanceIdParallelExistingPolicy)
        }
    };

    /// <summary>
    /// Assert that two <see cref="AppsInstanceDelegationResponseDto"/> have the same property in the same positions.
    /// </summary>
    /// <param name="expected">An instance with the expected values.</param>
    /// <param name="actual">The instance to verify.</param>
    public static void AssertAppsInstanceDelegationResponseDtoEqual(AppsInstanceDelegationResponseDto expected, AppsInstanceDelegationResponseDto actual)
    {
        Assert.NotNull(actual);
        Assert.NotNull(expected);

        AssertPartyUrn(expected.From, actual.From);
        Assert.Equal(expected.To.Value, actual.To.Value);
        Assert.Equal(expected.ResourceId, actual.ResourceId);
        Assert.Equal(expected.InstanceId, actual.InstanceId);
        Assert.Equal(expected.InstanceDelegationMode, actual.InstanceDelegationMode);
        AssertionUtil.AssertCollections(expected.Rights.ToList(), actual.Rights.ToList(), AssertRightsEqual);
    }

    public static void AssertPartyUrn(UrnJsonTypeValue<PartyUrn> expected, UrnJsonTypeValue<PartyUrn> actual)
    {
        Assert.True(actual.HasValue);
        Assert.True(expected.HasValue);

        Assert.Equal(expected.Value.Urn, actual.Value.Urn);
    }

    public static void AssertActionUrn(UrnJsonTypeValue<ActionUrn> expected, UrnJsonTypeValue<ActionUrn> actual)
    {
        Assert.True(actual.HasValue);
        Assert.True(expected.HasValue);

        Assert.Equal(expected.Value.Urn, actual.Value.Urn);
    }

    /// <summary>
    /// Assert that two <see cref="RightDelegationResultDto"/> have the same property in the same positions.
    /// </summary>
    /// <param name="expected">An instance with the expected values.</param>
    /// <param name="actual">The instance to verify.</param>
    public static void AssertRightsEqual(RightDelegationResultDto expected, RightDelegationResultDto actual)
    {
        Assert.NotNull(actual);
        Assert.NotNull(expected);

        AssertActionUrn(expected.Action, actual.Action);
        Assert.Equal(expected.Status, actual.Status);
        AssertionUtil.AssertCollections(expected.Resource.ToList(), actual.Resource.ToList(), AssertResourceEqual);
    }

    /// <summary>
    /// Assert that two <see cref="RightDelegationResultDto"/> have the same property in the same positions.
    /// </summary>
    /// <param name="expected">An instance with the expected values.</param>
    /// <param name="actual">The instance to verify.</param>
    public static void AssertResourceEqual(UrnJsonTypeValue expected, UrnJsonTypeValue actual)
    {
        Assert.True(actual.HasValue);
        Assert.True(expected.HasValue);

        Assert.Equal(expected.Value, actual.Value);
    }

    private static T GetExpectedResponse<T>(string appId, string instanceId)
    {
        string content = File.ReadAllText($"Data/Json/AppsInstanceDelegation/{appId}/{instanceId}/response.json");
        return (T)JsonSerializer.Deserialize(content, typeof(T), JsonOptions);
    }

    private static T GetRequest<T>(string appId, string instanceId)
    {
        string content = File.ReadAllText($"Data/Json/AppsInstanceDelegation/{appId}/{instanceId}/request.json");
        return (T)JsonSerializer.Deserialize(content, typeof(T), JsonOptions);
    }
}