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

    private static readonly string InstanceId = "0191579e-72bc-7977-af5d-f9e92af4393b";

    /// <summary>
    /// Sets up a request with a valid token but missing a valid authorization scope for authorized party list API
    /// </summary>
    public static TheoryData<string> ValidResourceOwnerTokenMissingScope() => new()
    {
        {
            PrincipalUtil.GetAccessToken("ttd", "am-devtest-instancedelegation")
        }
    };

    /// <summary>
    /// Test case:  POST resourceowner/authorizedparties?includeAltinn2={includeAltinn2}
    ///             with a valid resource owner token with the scope: altinn:accessmanagement/authorizedparties.resourceowner
    ///             getting authorized party list for an organization identified by urn:altinn:enterpriseuser:username
    /// Expected:   - Should return 200 OK
    ///             - Should include expected authorized party list of the requested party
    /// Reason:     Authenticated resource owner organizations authorized with scope: altinn:accessmanagement/authorizedparties.resourceowner
    ///             are authorized to get authorized party list of any person, user or organization in Altinn
    /// </summary>
    public static TheoryData<string, AppsInstanceDelegationRequestDto, string, string, AppsInstanceDelegationResponseDto> DelegateReadForApp() => new()
    {
        {
            PrincipalUtil.GetAccessToken("ttd", "am-devtest-instancedelegation"),
            GetRequest<AppsInstanceDelegationRequestDto>(AppId, InstanceId),
            AppId,
            InstanceId,
            GetExpectedResponse<AppsInstanceDelegationResponseDto>(AppId, InstanceId)
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
        Assert.Equal(expected.Resource, actual.Resource);
        Assert.Equal(expected.Instance, actual.Instance);
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