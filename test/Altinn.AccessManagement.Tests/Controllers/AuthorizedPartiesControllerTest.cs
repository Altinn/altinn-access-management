using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Altinn.AccessManagement.Controllers;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Tests.Data;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.AccessManagement.Tests.Utils;
using Altinn.Common.AccessToken.Services;
using Altinn.Common.PEP.Interfaces;
using AltinnCore.Authentication.JwtCookie;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
    /// Test case:  GET /authorizedparties?includeAltinn2={includeAltinn2}
    ///             for an unauthenticated user
    /// Expected:   - Should return 401 Unauthorized
    /// Reason:     Operation requires valid user authentication
    /// </summary>
    [Theory]
    [MemberData(nameof(TestDataAuthorizedParties.UnauthenticatedNoValidToken), MemberType = typeof(TestDataAuthorizedParties))]
    [MemberData(nameof(TestDataAuthorizedParties.UnauthenticatedValidTokenMissingUserContext), MemberType = typeof(TestDataAuthorizedParties))]
    public async Task GetAuthorizedParties_UnauthenticatedUser_Unauthorized(string userToken)
    {
        var client = GetTestClient(userToken);

        // Act
        HttpResponseMessage response = await client.GetAsync($"accessmanagement/api/v1/authorizedparties?includeAltinn2={true}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Test case:  GET /authorizedparty/{partyId}?includeAltinn2={includeAltinn2}
    ///             for an unauthenticated user
    /// Expected:   - Should return 401 Unauthorized
    /// Reason:     Operation requires valid user authentication
    /// </summary>
    [Theory]
    [MemberData(nameof(TestDataAuthorizedParties.UnauthenticatedNoValidToken), MemberType = typeof(TestDataAuthorizedParties))]
    [MemberData(nameof(TestDataAuthorizedParties.UnauthenticatedValidTokenMissingUserContext), MemberType = typeof(TestDataAuthorizedParties))]
    public async Task GetAuthorizedParty_UnauthenticatedUser_Unauthorized(string userToken)
    {
        var client = GetTestClient(userToken);

        // Act
        HttpResponseMessage response = await client.GetAsync($"accessmanagement/api/v1/authorizedparty/{123}?includeAltinn2={true}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Test case:  GET /authorizedparties?includeAltinn2={includeAltinn2}
    ///             with an authenticated user
    /// Expected:   - Should return 200 OK
    ///             - Should return the expected list of authorized party models
    /// Reason:     See individual test case description in <see cref="TestDataAuthorizedParties"></see>
    /// </summary>
    [Theory]
    [MemberData(nameof(TestDataAuthorizedParties.PersonToPerson), MemberType = typeof(TestDataAuthorizedParties))]
    [MemberData(nameof(TestDataAuthorizedParties.PersonToPersonInclA2), MemberType = typeof(TestDataAuthorizedParties))]
    [MemberData(nameof(TestDataAuthorizedParties.PersonToOrg), MemberType = typeof(TestDataAuthorizedParties))]
    [MemberData(nameof(TestDataAuthorizedParties.PersonToOrgInclA2), MemberType = typeof(TestDataAuthorizedParties))]
    [MemberData(nameof(TestDataAuthorizedParties.MainUnitAndSubUnitToPerson), MemberType = typeof(TestDataAuthorizedParties))]
    [MemberData(nameof(TestDataAuthorizedParties.MainUnitAndSubUnitToPersonInclA2), MemberType = typeof(TestDataAuthorizedParties))]
    [MemberData(nameof(TestDataAuthorizedParties.MainUnitAndSubUnitToOrg), MemberType = typeof(TestDataAuthorizedParties))]
    [MemberData(nameof(TestDataAuthorizedParties.MainUnitAndSubUnitToOrgInclA2), MemberType = typeof(TestDataAuthorizedParties))]
    [MemberData(nameof(TestDataAuthorizedParties.SubUnitToPerson), MemberType = typeof(TestDataAuthorizedParties))]
    public async Task GetAuthorizedParties_AuthenticatedUser_Ok(string userToken, bool includeAltinn2, List<AuthorizedPartyExternal> expected)
    {
        var client = GetTestClient(userToken);

        // Act
        HttpResponseMessage response = await client.GetAsync($"accessmanagement/api/v1/authorizedparties?includeAltinn2={includeAltinn2}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<AuthorizedPartyExternal> actual = JsonSerializer.Deserialize<List<AuthorizedPartyExternal>>(await response.Content.ReadAsStringAsync(), options);
        AssertionUtil.AssertCollections(expected, actual, TestDataAuthorizedParties.AssertAuthorizedPartyExternalEqual);
    }

    /// <summary>
    /// Test case:  GET /authorizedparty/{partyId}?includeAltinn2={includeAltinn2}
    ///             with an authenticated user
    /// Expected:   - Should return 200 OK
    ///             - Should return the expected authorized party model
    /// Reason:     See individual test case description in <see cref="TestDataAuthorizedParties"></see>
    /// </summary>
    [Theory]
    [MemberData(nameof(TestDataAuthorizedParties.PersonGettingSelfInclA2_Success), MemberType = typeof(TestDataAuthorizedParties))]
    [MemberData(nameof(TestDataAuthorizedParties.PersonGettingA3Delegator_Success), MemberType = typeof(TestDataAuthorizedParties))]
    [MemberData(nameof(TestDataAuthorizedParties.PersonGettingA3DelegatorInclA2_Success), MemberType = typeof(TestDataAuthorizedParties))]
    public async Task GetAuthorizedParty_AuthenticatedUser_Ok(string userToken, int partyId, bool includeAltinn2, AuthorizedPartyExternal expected)
    {
        var client = GetTestClient(userToken);

        // Act
        HttpResponseMessage response = await client.GetAsync($"accessmanagement/api/v1/authorizedparty/{partyId}?includeAltinn2={includeAltinn2}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        AuthorizedPartyExternal actual = JsonSerializer.Deserialize<AuthorizedPartyExternal>(await response.Content.ReadAsStringAsync(), options);
        TestDataAuthorizedParties.AssertAuthorizedPartyExternalEqual(expected, actual);
    }

    /// <summary>
    /// Test case:  GET /authorizedparty/{partyId}?includeAltinn2={includeAltinn2}
    ///             with an authenticated user
    /// Expected:   - Should return 400 BadRequest
    ///             - Should return the expected ValidationProblemDetails response
    /// Reason:     See individual test case description in <see cref="TestDataAuthorizedParties"></see>
    /// </summary>
    [Theory]
    [MemberData(nameof(TestDataAuthorizedParties.PersonGettingSelf_BadRequest), MemberType = typeof(TestDataAuthorizedParties))]
    public async Task GetAuthorizedParty_AuthenticatedUser_BadRequest(string userToken, int partyId, bool includeAltinn2, ValidationProblemDetails expected)
    {
        var client = GetTestClient(userToken);

        // Act
        HttpResponseMessage response = await client.GetAsync($"accessmanagement/api/v1/authorizedparty/{partyId}?includeAltinn2={includeAltinn2}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        ValidationProblemDetails actual = JsonSerializer.Deserialize<ValidationProblemDetails>(await response.Content.ReadAsStringAsync(), options);
        AssertionUtil.AssertValidationProblemDetailsEqual(expected, actual);
    }

    /// <summary>
    /// Test case:  GET {party}/authorizedparties?includeAltinn2={includeAltinn2}
    ///             with an authenticated and authorized Access Manager for the {party}
    /// Expected:   - Should return 200 OK
    ///             - Should return the expected list of authorized party models
    /// Reason:     See individual test case description in <see cref="TestDataAuthorizedParties"></see>
    /// </summary>
    [Theory]
    [MemberData(nameof(TestDataAuthorizedParties.PersonGettingOwnList_Success), MemberType = typeof(TestDataAuthorizedParties))]
    [MemberData(nameof(TestDataAuthorizedParties.AccessManagerGettingOrgList_Success), MemberType = typeof(TestDataAuthorizedParties))]
    public async Task GetAuthorizedParties_AsAccessManager_Ok(string userToken, int partyId, bool includeAltinn2, List<AuthorizedPartyExternal> expected)
    {
        var client = GetTestClient(userToken, WithPDPMock);

        // Act
        HttpResponseMessage response = await client.GetAsync($"accessmanagement/api/v1/{partyId}/authorizedparties?includeAltinn2={includeAltinn2}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<AuthorizedPartyExternal> actual = JsonSerializer.Deserialize<List<AuthorizedPartyExternal>>(await response.Content.ReadAsStringAsync(), options);
        AssertionUtil.AssertCollections(expected, actual, TestDataAuthorizedParties.AssertAuthorizedPartyExternalEqual);
    }

    /// <summary>
    /// Test case:  GET {party}/authorizedparties?includeAltinn2={includeAltinn2}
    ///             with an authenticated and authorized Access Manager for the {party} which is a person
    /// Expected:   - Should return 403 Forbidden
    /// Reason:     See individual test case description in <see cref="TestDataAuthorizedParties"></see>
    /// </summary>
    [Theory]
    [MemberData(nameof(TestDataAuthorizedParties.AccessManagerGettingPersonList_Forbidden), MemberType = typeof(TestDataAuthorizedParties))]
    public async Task GetAuthorizedParties_AsAccessManager_Forbidden(string userToken, int partyId, bool includeAltinn2)
    {
        var client = GetTestClient(userToken);

        // Act
        HttpResponseMessage response = await client.GetAsync($"accessmanagement/api/v1/{partyId}/authorizedparties?includeAltinn2={includeAltinn2}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
