using System.Text.Json;
using Altinn.AccessManagement.Models;
using Altinn.AccessManagement.Tests.Util;
using Altinn.AccessManagement.Tests.Utils;
using Altinn.Authorization.ProblemDetails;
using Dapper;

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

    private static readonly string InstanceIdInvalidParty = "00000000-0000-0000-0000-000000000006";

    private static readonly string InstanceIdNormalNewPolicyOrgNumber = "00000000-0000-0000-0000-000000000007";

    private static readonly string ListOfDelegationsForAnInstance = "00000000-0000-0000-0000-000000000008";

    private static readonly string RevokeOneOfExistingDelegations = "00000000-0000-0000-0000-000000000009";

    private static readonly string RevokeAllInstance = "00000000-0000-0000-0000-000000000010";

    /// <summary>
    /// Test case:  GET v1/apps/instancedelegation/{resourceId}/{instanceId}/delegationcheck
    ///             with: 
    ///                - a valid app authenticated as the delegater, authenticated with PlatformAccessToken
    ///                - valid resource for instance delegation
    ///                - xacml policy file for resource to be delegated configured with rights available for delegation by the authenticated app
    /// Expected:   - Should return 200 OK
    ///             - Should include the delegated rights
    /// Reason:     Apps defined in the policy file should be able to delegate the defined rights
    /// </summary>
    public static TheoryData<string, string, string, Paginated<ResourceRightDelegationCheckResultDto>> DelegationCheck_Ok() => new()
    {
        {
            PrincipalUtil.GetAccessToken("ttd", "am-devtest-instancedelegation"),
            AppId,
            InstanceIdParallelNewPolicy,
            GetExpectedResponse<Paginated<ResourceRightDelegationCheckResultDto>>("DelegationCheck", AppId, InstanceIdParallelNewPolicy)
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
    public static TheoryData<string, AppsInstanceDelegationRequestDto, string, string, AppsInstanceDelegationResponseDto> DelegateParallelReadForAppNoExistingPolicy() => new()
    {
        {
            PrincipalUtil.GetAccessToken("ttd", "am-devtest-instancedelegation"),
            GetRequest<AppsInstanceDelegationRequestDto>("Delegation", AppId, InstanceIdParallelNewPolicy),
            AppId,
            InstanceIdParallelNewPolicy,
            GetExpectedResponse<AppsInstanceDelegationResponseDto>("Delegation", AppId, InstanceIdParallelNewPolicy)
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
            GetRequest<AppsInstanceDelegationRequestDto>("Delegation", AppId, InstanceIdParallelExistingPolicy),
            AppId,
            InstanceIdParallelExistingPolicy,
            GetExpectedResponse<AppsInstanceDelegationResponseDto>("Delegation", AppId, InstanceIdParallelExistingPolicy)
        }
    };

    public static TheoryData<string, AppsInstanceDelegationRequestDto, string, string, AppsInstanceRevokeResponseDto> RevokeReadForAppOnlyExistingPolicyRevokeLast() => new()
    {
        {
            PrincipalUtil.GetAccessToken("ttd", "am-devtest-instancedelegation"),
            GetRequest<AppsInstanceDelegationRequestDto>("Revoke", AppId, InstanceIdParallelExistingPolicy),
            AppId,
            InstanceIdParallelExistingPolicy,
            GetExpectedResponse<AppsInstanceRevokeResponseDto>("Revoke", AppId, InstanceIdParallelExistingPolicy)
        }
    };

    public static TheoryData<string, AppsInstanceDelegationRequestDto, string, string, AppsInstanceRevokeResponseDto> RevokeReadForAppMultipleExistingPolicyRevoke() => new()
    {
        {
            PrincipalUtil.GetAccessToken("ttd", "am-devtest-instancedelegation"),
            GetRequest<AppsInstanceDelegationRequestDto>("Revoke", AppId, RevokeOneOfExistingDelegations),
            AppId,
            RevokeOneOfExistingDelegations,
            GetExpectedResponse<AppsInstanceRevokeResponseDto>("Revoke", AppId, RevokeOneOfExistingDelegations)
        }
    };

    public static TheoryData<string, string, string, Paginated<AppsInstanceRevokeResponseDto>> RevokeAll() => new()
    {
        {
            PrincipalUtil.GetAccessToken("ttd", "am-devtest-instancedelegation"),
            AppId,
            RevokeAllInstance,
            GetExpectedResponse<Paginated<AppsInstanceRevokeResponseDto>>("Revoke", AppId, RevokeAllInstance)
        }
    };

    public static TheoryData<string, string> RevokeAllUnathorized() => new()
    {
        {
            AppId,
            RevokeAllInstance
        }
    }; 

    public static TheoryData<string, AppsInstanceDelegationRequestDto, string, string, AppsInstanceRevokeResponseDto> RevokeReadForAppNoExistingPolicyRevokeLast() => new()
    {
        {
            PrincipalUtil.GetAccessToken("ttd", "am-devtest-instancedelegation"),
            GetRequest<AppsInstanceDelegationRequestDto>("Revoke", AppId, InstanceIdNewPolicyNoResponceOnWrite),
            AppId,
            InstanceIdNewPolicyNoResponceOnWrite,
            GetExpectedResponse<AppsInstanceRevokeResponseDto>("Revoke", AppId, InstanceIdNewPolicyNoResponceOnWrite)
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
            GetRequest<AppsInstanceDelegationRequestDto>("Delegation", AppId, InstanceIdNewPolicyNoResponceOnWrite),
            AppId,
            InstanceIdNewPolicyNoResponceOnWrite,
            GetExpectedResponse<AppsInstanceDelegationResponseDto>("Delegation", AppId, InstanceIdNewPolicyNoResponceOnWrite)
        }
    };

    public static TheoryData<string, AppsInstanceDelegationRequestDto, string, string, AltinnProblemDetails> DelegateToPartyNotExisting() => new()
    {
        {
            PrincipalUtil.GetAccessToken("ttd", "am-devtest-instancedelegation"),
            GetRequest<AppsInstanceDelegationRequestDto>("Delegation", AppId, InstanceIdInvalidParty),
            AppId,
            InstanceIdInvalidParty,
            GetExpectedResponse<AltinnProblemDetails>("Delegation", AppId, InstanceIdInvalidParty)
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
            GetRequest<AppsInstanceDelegationRequestDto>("Delegation", AppId, InstanceIdNormalNewPolicy),
            AppId,
            InstanceIdNormalNewPolicy,
            GetExpectedResponse<AppsInstanceDelegationResponseDto>("Delegation", AppId, InstanceIdNormalNewPolicy)
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
    public static TheoryData<string, AppsInstanceDelegationRequestDto, string, string, AppsInstanceDelegationResponseDto> DelegateNormalReadForAppNoExistingPolicyOrganizatonNumber() => new()
    {
        {
            PrincipalUtil.GetAccessToken("ttd", "am-devtest-instancedelegation"),
            GetRequest<AppsInstanceDelegationRequestDto>("Delegation", AppId, InstanceIdNormalNewPolicyOrgNumber),
            AppId,
            InstanceIdNormalNewPolicyOrgNumber,
            GetExpectedResponse<AppsInstanceDelegationResponseDto>("Delegation", AppId, InstanceIdNormalNewPolicyOrgNumber)
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
            GetRequest<AppsInstanceDelegationRequestDto>("Delegation", AppId, InstanceIdNormalExistingPolicy),
            AppId,
            InstanceIdNormalExistingPolicy,
            GetExpectedResponse<AppsInstanceDelegationResponseDto>("Delegation", AppId, InstanceIdNormalExistingPolicy)
        }
    };

    public static TheoryData<string, string, string, Paginated<AppsInstanceDelegationResponseDto>> GetAllAppDelegatedInstances() => new()
    {
        {
            PrincipalUtil.GetAccessToken("ttd", "am-devtest-instancedelegation"),
            AppId,
            ListOfDelegationsForAnInstance,
            GetExpectedResponse<Paginated<AppsInstanceDelegationResponseDto>>("Get", AppId, ListOfDelegationsForAnInstance)
        }
    };

    public static void AssertAltinnProblemDetailsEqual(AltinnProblemDetails expected, AltinnProblemDetails actual)
    {
        Assert.NotNull(actual);
        Assert.NotNull(expected);

        Assert.Equal(expected.Instance, actual.Instance);
        Assert.Equal(expected.Detail, actual.Detail);
        Assert.Equal(expected.Type, actual.Type);
        Assert.Equal(expected.Title, actual.Title);
        Assert.Equal(expected.ErrorCode, actual.ErrorCode);
        AssertionUtil.AssertCollections(expected.Extensions.ToDictionary(), actual.Extensions.ToDictionary(), AssertProblemDetailsExtensionEqual);        
    }

    public static void AssertProblemDetailsExtensionEqual(KeyValuePair<string, object> expected, KeyValuePair<string, object> actual)
    {
        Assert.Equal(expected.Key, actual.Key);
        JsonElement? actualJson = actual.Value as JsonElement?;
        JsonElement? expectedJson = expected.Value as JsonElement?;
        
        if (actualJson == null)
        {
            Assert.Null(expectedJson);
            return;
        }

        Assert.NotNull(actualJson);
        Assert.NotNull(expectedJson);
        
        var actualExtensionList = actualJson.Value.EnumerateArray().AsList();
        var expectedExtensionList = expectedJson.Value.EnumerateArray().AsList();
        Assert.Equal(expectedExtensionList.Count, actualExtensionList.Count);
        
        for (int i = 0; i < actualExtensionList.Count; i++)
        {
            ErrorDetails expectedDetail = JsonSerializer.Deserialize<ErrorDetails>(expectedExtensionList[i].GetRawText(), JsonOptions);
            ErrorDetails actualDetail = JsonSerializer.Deserialize<ErrorDetails>(actualExtensionList[i].GetRawText(), JsonOptions);
            Assert.Equal(expectedDetail.Code, actualDetail.Code);
            Assert.Equal(expectedDetail.Detail, actualDetail.Detail);
            
            if (expectedDetail.Paths == null)
            {
                Assert.Null(expectedDetail.Paths);
                return;
            }

            Assert.NotNull(actualDetail.Paths);
            Assert.NotNull(expectedDetail.Paths);

            Assert.Equal(expectedDetail.Paths.Count, actualDetail.Paths.Count);
            for (int j = 0; j < expectedDetail.Paths.Count; j++)
            {
                Assert.Equal(expectedDetail.Paths[j], actualDetail.Paths[j]);
            }
        }
    }

    internal class ErrorDetails
    {
        public string Code { get; set; }

        public string Detail { get; set; }

        public List<string> Paths { get; set; }
    }

    private static T GetExpectedResponse<T>(string operation, string appId, string instanceId)
    {
        string content = File.ReadAllText($"Data/Json/AppsInstanceDelegation/{operation}/{appId}/{instanceId}/response.json");
        return (T)JsonSerializer.Deserialize(content, typeof(T), JsonOptions);
    }

    private static T GetRequest<T>(string operation, string appId, string instanceId)
    {
        string content = File.ReadAllText($"Data/Json/AppsInstanceDelegation/{operation}/{appId}/{instanceId}/request.json");
        return (T)JsonSerializer.Deserialize(content, typeof(T), JsonOptions);
    }
}