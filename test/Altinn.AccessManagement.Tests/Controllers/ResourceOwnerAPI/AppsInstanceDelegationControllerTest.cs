using Altinn.AccessManagement.Tests.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.Common.AccessToken.Services;
using Altinn.Common.PEP.Interfaces;
using AltinnCore.Authentication.JwtCookie;
using Microsoft.Extensions.Options;
using Altinn.AccessManagement.Controllers;
using System.Text.Json;
using Microsoft.AspNetCore.TestHost;
using Altinn.AccessManagement.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Text;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Tests.Utils;

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
    /// Test case:  POST resourceowner/authorizedparties?includeAltinn2={includeAltinn2}
    ///             with a valid token but missing required scope
    /// Expected:   - Should return 403 Forbidden
    /// Reason:     Operation requires valid token with authorized scope
    /// </summary>
    [Theory]
    [MemberData(nameof(TestDataAppsInstanceDelegation.DelegateReadForApp), MemberType = typeof(TestDataAppsInstanceDelegation))]
    public async Task AppsInstanceDelegationController_ValidToken_Delegate_OK(string platformToken, AppsInstanceDelegationRequestDto request, string resourceId, string instanceId, AppsInstanceDelegationResponseDto expected)
    {
        var client = GetTestClient(platformToken);

        // Act
        HttpResponseMessage response = await client.PostAsync($"accessmanagement/api/v1/apps/instancedelegation/{resourceId}/{instanceId}", new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, MediaTypeNames.Application.Json));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        AppsInstanceDelegationResponseDto actual = JsonSerializer.Deserialize<AppsInstanceDelegationResponseDto>(await response.Content.ReadAsStringAsync(), options);
        TestDataAppsInstanceDelegation.AssertAppsInstanceDelegationResponseDtoEqual(expected, actual);
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
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        client.DefaultRequestHeaders.Add("PlatformAccessToken", token);
        return client;
    }
}