using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Altinn.AccessManagement.Controllers;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Models;
using Altinn.AccessManagement.Tests.Data;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.AccessManagement.Tests.Utils;
using Altinn.Authorization.ProblemDetails;
using Altinn.Common.AccessToken.Services;
using Altinn.Common.PEP.Interfaces;
using AltinnCore.Authentication.JwtCookie;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Altinn.AccessManagement.Tests.Controllers;

public class AppsInstanceDelegationControllerTest : IClassFixture<CustomWebApplicationFactory<AppsInstanceDelegationController>>
{
    private readonly CustomWebApplicationFactory<AppsInstanceDelegationController> _factory;
    private readonly JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Constructor setting up factory, test client and dependencies
    /// </summary>
    /// <param name="factory">CustomWebApplicationFactory</param>
    public AppsInstanceDelegationControllerTest(CustomWebApplicationFactory<AppsInstanceDelegationController> factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Test case:  GET apps/instancedelegation/{resourceId}/{instanceId}/delegationcheck
    ///             with a valid PlatformAccessToken for an app having xacml rules specifying rights available for delegation
    /// Expected:   - Should return 200 OK
    /// Reason:     See testdata cases for details
    /// </summary>
    [Theory]
    [MemberData(nameof(TestDataAppsInstanceDelegation.DelegationCheck_Ok), MemberType = typeof(TestDataAppsInstanceDelegation))]
    public async Task DelegationCheck_ValidToken_OK(string platformToken, string resourceId, string instanceId, Paginated<ResourceRightDelegationCheckResultDto> expected)
    {
        var client = GetTestClient(platformToken);

        // Act
        HttpResponseMessage response = await client.GetAsync($"accessmanagement/api/v1/app/delegationcheck/resource/{resourceId}/instance/{instanceId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Paginated<ResourceRightDelegationCheckResultDto> actual = JsonSerializer.Deserialize<Paginated<ResourceRightDelegationCheckResultDto>>(await response.Content.ReadAsStringAsync(), options);

        AssertionUtil.AssertPagination(expected, actual, AssertionUtil.AssertResourceRightDelegationCheckResultDto);
    }

    /// <summary>
    /// Test case:  POST apps/instancedelegation/{resourceId}/{instanceId}
    ///             with a valid delegation
    /// Expected:   - Should return 200 OK
    /// Reason:     See testdat cases for details
    /// </summary>
    [Theory]
    [MemberData(nameof(TestDataAppsInstanceDelegation.DelegateParallelReadForAppNoExistingPolicy), MemberType = typeof(TestDataAppsInstanceDelegation))]
    [MemberData(nameof(TestDataAppsInstanceDelegation.DelegateParallelSignForAppExistingPolicy), MemberType = typeof(TestDataAppsInstanceDelegation))]
    [MemberData(nameof(TestDataAppsInstanceDelegation.DelegateNormalReadForAppNoExistingPolicy), MemberType = typeof(TestDataAppsInstanceDelegation))]
    [MemberData(nameof(TestDataAppsInstanceDelegation.DelegateNormalSignForAppExistingPolicy), MemberType = typeof(TestDataAppsInstanceDelegation))]
    [MemberData(nameof(TestDataAppsInstanceDelegation.DelegateNormalReadForAppNoExistingPolicyOrganizatonNumber), MemberType = typeof(TestDataAppsInstanceDelegation))]
    public async Task AppsInstanceDelegationController_ValidToken_Delegate_OK(string platformToken, AppsInstanceDelegationRequestDto request, string resourceId, string instanceId, AppsInstanceDelegationResponseDto expected)
    {
        var client = GetTestClient(platformToken);

        // Act
        HttpResponseMessage response = await client.PostAsync($"accessmanagement/api/v1/app/delegations/resource/{resourceId}/instance/{instanceId}", new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, MediaTypeNames.Application.Json));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        AppsInstanceDelegationResponseDto actual = JsonSerializer.Deserialize<AppsInstanceDelegationResponseDto>(await response.Content.ReadAsStringAsync(), options);
        AssertionUtil.AssertAppsInstanceDelegationResponseDto(expected, actual);
    }

    /// <summary>
    /// Test case:  POST apps/instancedelegation/{resourceId}/{instanceId}
    ///             with a valid delegation
    /// Expected:   - Should return 200 OK
    /// Reason:     See testdat cases for details
    /// </summary>
    [Theory]
    [MemberData(nameof(TestDataAppsInstanceDelegation.RevokeReadForAppOnlyExistingPolicyRevokeLast), MemberType = typeof(TestDataAppsInstanceDelegation))]
    [MemberData(nameof(TestDataAppsInstanceDelegation.RevokeReadForAppMultipleExistingPolicyRevoke), MemberType = typeof(TestDataAppsInstanceDelegation))]
    [MemberData(nameof(TestDataAppsInstanceDelegation.RevokeReadForAppNoExistingPolicyRevokeLast), MemberType = typeof(TestDataAppsInstanceDelegation))]
    public async Task AppsInstanceDelegationController_ValidToken_Revoke_OK(string platformToken, AppsInstanceDelegationRequestDto request, string resourceId, string instanceId, AppsInstanceRevokeResponseDto expected)
    {
        var client = GetTestClient(platformToken);

        // Act
        HttpResponseMessage response = await client.PostAsync($"accessmanagement/api/v1/app/delegationrevoke/resource/{resourceId}/instance/{instanceId}", new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, MediaTypeNames.Application.Json));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        AppsInstanceRevokeResponseDto actual = JsonSerializer.Deserialize<AppsInstanceRevokeResponseDto>(await response.Content.ReadAsStringAsync(), options);
        AssertionUtil.AssertAppsInstanceRevokeResponseDto(expected, actual);
    }

    [Theory]
    [MemberData(nameof(TestDataAppsInstanceDelegation.RevokeAll), MemberType = typeof(TestDataAppsInstanceDelegation))]
    public async Task AppsInstanceDelegationController_ValidToken_RevokeAll_OK(string platformToken, string resourceId, string instanceId, Paginated<AppsInstanceRevokeResponseDto> expected)
    {
        var client = GetTestClient(platformToken);

        // Act
        HttpResponseMessage response = await client.DeleteAsync($"accessmanagement/api/v1/app/delegationrevoke/resource/{resourceId}/instance/{instanceId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Paginated<AppsInstanceRevokeResponseDto> actual = JsonSerializer.Deserialize<Paginated<AppsInstanceRevokeResponseDto>>(await response.Content.ReadAsStringAsync(), options);
        AssertionUtil.AssertPagination(expected, actual, AssertionUtil.AssertAppsInstanceRevokeResponseDto);        
    }

    [Theory]
    [MemberData(nameof(TestDataAppsInstanceDelegation.RevokeAllUnathorized), MemberType = typeof(TestDataAppsInstanceDelegation))]
    public async Task AppsInstanceDelegationController_NoToken_RevokeAll_Unauthorized(string resourceId, string instanceId)
    {
        var client = GetTestClient(null);

        // Act
        HttpResponseMessage response = await client.DeleteAsync($"accessmanagement/api/v1/app/delegationrevoke/resource/{resourceId}/instance/{instanceId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);                
    }

    /// <summary>
    /// Test case:  POST apps/instancedelegation/{resourceId}/{instanceId}
    ///             with a valid delegation
    /// Expected:   - Should return 200 OK
    /// Reason:     See testdat cases for details
    /// </summary>
    [Theory]
    [MemberData(nameof(TestDataAppsInstanceDelegation.DelegateReadForAppNoExistingPolicyNoResponceDBWrite), MemberType = typeof(TestDataAppsInstanceDelegation))]
    public async Task AppsInstanceDelegationController_ValidToken_Delegate_DBWriteFails(string platformToken, AppsInstanceDelegationRequestDto request, string resourceId, string instanceId, AppsInstanceDelegationResponseDto expected)
    {
        var client = GetTestClient(platformToken);

        // Act
        HttpResponseMessage response = await client.PostAsync($"accessmanagement/api/v1/app/delegations/resource/{resourceId}/instance/{instanceId}", new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, MediaTypeNames.Application.Json));

        // Assert
        Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);

        AppsInstanceDelegationResponseDto actual = JsonSerializer.Deserialize<AppsInstanceDelegationResponseDto>(await response.Content.ReadAsStringAsync(), options);
        AssertionUtil.AssertAppsInstanceDelegationResponseDto(expected, actual);
    }

    /// <summary>
    /// Test case:  POST apps/instancedelegation/{resourceId}/{instanceId}
    ///             with a valid delegation
    /// Expected:   - Should return 200 OK
    /// Reason:     See testdat cases for details
    /// </summary>
    [Theory]
    [MemberData(nameof(TestDataAppsInstanceDelegation.DelegateToPartyNotExisting), MemberType = typeof(TestDataAppsInstanceDelegation))]
    public async Task AppsInstanceDelegationController_ValidToken_Delegate_InvalidParty(string platformToken, AppsInstanceDelegationRequestDto request, string resourceId, string instanceId, AltinnProblemDetails expected)
    {
        var client = GetTestClient(platformToken);

        // Act
        HttpResponseMessage response = await client.PostAsync($"accessmanagement/api/v1/app/delegations/resource/{resourceId}/instance/{instanceId}", new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, MediaTypeNames.Application.Json));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        AltinnProblemDetails actual = JsonSerializer.Deserialize<AltinnProblemDetails>(await response.Content.ReadAsStringAsync(), options);
        TestDataAppsInstanceDelegation.AssertAltinnProblemDetailsEqual(expected, actual);
    }

    [Theory]
    [MemberData(nameof(TestDataAppsInstanceDelegation.GetAllAppDelegatedInstances), MemberType = typeof(TestDataAppsInstanceDelegation))]
    public async Task AppsInstanceDelegationController_ValidToken_Get_OK(string platformToken, string resourceId, string instanceId, Paginated<AppsInstanceDelegationResponseDto> expected)
    {
        var client = GetTestClient(platformToken);

        // Act
        HttpResponseMessage response = await client.GetAsync($"accessmanagement/api/v1/app/delegations/resource/{resourceId}/instance/{instanceId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Paginated<AppsInstanceDelegationResponseDto> actual = JsonSerializer.Deserialize<Paginated<AppsInstanceDelegationResponseDto>>(await response.Content.ReadAsStringAsync(), options);
        AssertionUtil.AssertPagination(expected, actual, AssertionUtil.AssertAppsInstanceDelegationResponseDto);
    }

    private static void WithPDPMock(IServiceCollection services) => services.AddSingleton(new PepWithPDPAuthorizationMock());

    private HttpClient GetTestClient(string token, params Action<IServiceCollection>[] actions)
    {
        HttpClient client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IPolicyRetrievalPoint, PolicyRetrievalPointMock>();
                services.AddSingleton<IDelegationMetadataRepository, DelegationMetadataRepositoryMock>();
                services.AddSingleton<IPolicyFactory, PolicyFactoryMock>();
                services.AddSingleton<IPublicSigningKeyProvider, SigningKeyResolverMock>();
                services.AddSingleton<IPostConfigureOptions<JwtCookieOptions>, JwtCookiePostConfigureOptionsStub>();
                services.AddSingleton<IPartiesClient, PartiesClientMock>();
                services.AddSingleton<IProfileClient, ProfileClientMock>();
                services.AddSingleton<IResourceRegistryClient, ResourceRegistryClientMock>();
                services.AddSingleton<IAltinnRolesClient, AltinnRolesClientMock>();
                services.AddSingleton<IPDP, PdpPermitMock>();
                services.AddSingleton<IAltinn2RightsClient, Altinn2RightsClientMock>();

                foreach (var action in actions)
                {
                    action(services);
                }
            });
        }).CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (token != null)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        client.DefaultRequestHeaders.Add("PlatformAccessToken", token);
        return client;
    }
}