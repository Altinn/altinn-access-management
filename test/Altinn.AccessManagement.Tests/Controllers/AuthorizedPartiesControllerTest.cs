using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Altinn.AccessManagement.Controllers;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.AccessManagement.Tests.Utils;
using Altinn.Common.AccessToken.Services;
using Altinn.Common.PEP.Interfaces;
using AltinnCore.Authentication.JwtCookie;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Altinn.AccessManagement.Tests.Controllers;

/// <summary>
/// Test class for <see cref="AuthorizedPartiesController"></see>
/// </summary>
[Collection("AuthorizedPartiesController Tests")]
public class AuthorizedPartiesControllerTest : IClassFixture<CustomWebApplicationFactory<AuthorizedPartiesController>>
{
    private readonly CustomWebApplicationFactory<AuthorizedPartiesController> _factory;
    private readonly JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Constructor setting up factory, test client and dependencies
    /// </summary>
    /// <param name="factory">CustomWebApplicationFactory</param>
    public AuthorizedPartiesControllerTest(CustomWebApplicationFactory<AuthorizedPartiesController> factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Test case:  Get AuthorizedParties for an unauthenticated user
    /// Expected:   - Should return 401 Unauthorized
    /// </summary>
    /// <returns></returns>
    [Theory]
    [MemberData(nameof(TestDataAuthorizedParties.UnauthenticatedNoValidToken), MemberType = typeof(TestDataAuthorizedParties))]
    [MemberData(nameof(TestDataAuthorizedParties.UnauthenticatedValidTokenWithOutUserContext), MemberType = typeof(TestDataAuthorizedParties))]
    public async Task GetAuthorizedParties_UnauthenticatedUser_Unauthorized(string userToken)
    {
        var client = GetTestClient(userToken);

        // Act
        HttpResponseMessage response = await client.GetAsync($"accessmanagement/api/v1/authorizedparties?includeAltinn2={false}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Test case:  Get AuthorizedParties for authenticated user
    /// Expected:   - Should return 200 OK
    ///             - Should include expected authorized parties incl. authorized resources for each party
    /// </summary>
    /// <returns></returns>
    [Theory]
    [MemberData(nameof(TestDataAuthorizedParties.KasperOnlyAltinn3AuthorizedParties), MemberType = typeof(TestDataAuthorizedParties))]
    [MemberData(nameof(TestDataAuthorizedParties.KasperBothAltinn3AndAltinn2AuthorizedParties), MemberType = typeof(TestDataAuthorizedParties))]
    [MemberData(nameof(TestDataAuthorizedParties.PaulaOnlyAltinn3AuthorizedParties), MemberType = typeof(TestDataAuthorizedParties))]
    [MemberData(nameof(TestDataAuthorizedParties.PaulaBothAltinn3AndAltinn2AuthorizedParties), MemberType = typeof(TestDataAuthorizedParties))]
    public async Task GetAuthorizedParties_AuthenticatedUser_Ok(string userToken, bool includeAltinn2, List<AuthorizedParty> expected)
    {
        var client = GetTestClient(userToken, WithPDPMock);

        // Act
        HttpResponseMessage response = await client.GetAsync($"accessmanagement/api/v1/authorizedparties?includeAltinn2={includeAltinn2}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<AuthorizedParty> actual = JsonSerializer.Deserialize<List<AuthorizedParty>>(await response.Content.ReadAsStringAsync(), options);
        AssertionUtil.AssertCollections(expected, actual, AssertionUtil.AssertAuthorizedPartyEqual);
    }

    private void WithPDPMock(IServiceCollection services) => services.AddSingleton(new PepWithPDPAuthorizationMock());

    private HttpClient GetTestClient(string token, params Action<IServiceCollection>[] actions)
    {
        HttpClient client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IPolicyRetrievalPoint, PolicyRetrievalPointMock>();
                services.AddSingleton<IDelegationMetadataRepository, DelegationMetadataRepositoryMock>();
                services.AddSingleton<IPolicyRepository, PolicyRepositoryMock>();
                services.AddSingleton<IPostConfigureOptions<JwtCookieOptions>, JwtCookiePostConfigureOptionsStub>();
                services.AddSingleton<IPublicSigningKeyProvider, SigningKeyResolverMock>();
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
        return client;
    }
}
