using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Altinn.AccessManagement.Controllers;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Models;
using Altinn.AccessManagement.Tests.Data;
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
/// Test class for <see cref="ResourceOwnerAuthorizedPartiesController"></see>
/// </summary>
[Collection("ResourceOwnerAuthorizedPartiesController Tests")]
public class ResourceOwnerAuthorizedPartiesControllerTest : IClassFixture<CustomWebApplicationFactory<ResourceOwnerAuthorizedPartiesController>>
{
    private readonly CustomWebApplicationFactory<ResourceOwnerAuthorizedPartiesController> _factory;
    private readonly JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Constructor setting up factory, test client and dependencies
    /// </summary>
    /// <param name="factory">CustomWebApplicationFactory</param>
    public ResourceOwnerAuthorizedPartiesControllerTest(CustomWebApplicationFactory<ResourceOwnerAuthorizedPartiesController> factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Test case:  POST resourceowner/authorizedparties?includeAltinn2={includeAltinn2}
    ///             without valid token
    /// Expected:   - Should return 401 Unauthorized
    /// Reason:     Operation requires valid token
    /// </summary>
    [Theory]
    [MemberData(nameof(TestDataAuthorizedParties.UnauthenticatedNoValidToken), MemberType = typeof(TestDataAuthorizedParties))]
    public async Task PostResourceOwnerAuthorizedParties_InvalidToken_Unauthorized(string resourceOwnerToken)
    {
        var client = GetTestClient(resourceOwnerToken);

        // Act
        HttpResponseMessage response = await client.PostAsync($"accessmanagement/api/v1/resourceowner/authorizedparties?includeAltinn2={true}", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Test case:  POST resourceowner/authorizedparties?includeAltinn2={includeAltinn2}
    ///             with a valid token but missing required scope
    /// Expected:   - Should return 403 Forbidden
    /// Reason:     Operation requires valid token with authorized scope
    /// </summary>
    [Theory]
    [MemberData(nameof(TestDataAuthorizedParties.ValidResourceOwnerTokenMissingScope), MemberType = typeof(TestDataAuthorizedParties))]
    public async Task PostResourceOwnerAuthorizedParties_ValidToken_MissingAuthorizationScope_Forbidden(string resourceOwnerToken)
    {
        var client = GetTestClient(resourceOwnerToken, WithPDPMock);

        // Act
        HttpResponseMessage response = await client.PostAsync($"accessmanagement/api/v1/resourceowner/authorizedparties?includeAltinn2={true}", null);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Test case:  POST resourceowner/authorizedparties?includeAltinn2={includeAltinn2}
    ///             with a valid token including authorized scope
    /// Expected:   - Should return 200 OK
    ///             - Should return the expected Authorized Party list for the requested party
    /// Reason:     See individual test case description in <see cref="TestDataAuthorizedParties"></see>
    /// </summary>
    [Theory]
    [MemberData(nameof(TestDataAuthorizedParties.ResourceOwner_GetPersonList_ByPersonId), MemberType = typeof(TestDataAuthorizedParties))]
    [MemberData(nameof(TestDataAuthorizedParties.ResourceOwner_GetPersonList_ByPersonUuid), MemberType = typeof(TestDataAuthorizedParties))]
    [MemberData(nameof(TestDataAuthorizedParties.ResourceOwner_GetPersonList_ByPartyId), MemberType = typeof(TestDataAuthorizedParties))]
    [MemberData(nameof(TestDataAuthorizedParties.ResourceOwner_GetPersonList_ByUserId), MemberType = typeof(TestDataAuthorizedParties))]
    [MemberData(nameof(TestDataAuthorizedParties.ResourceOwner_GetPersonList_ByPersonId_InclResourcesThroughRoles), MemberType = typeof(TestDataAuthorizedParties))]
    [MemberData(nameof(TestDataAuthorizedParties.ResourceOwner_GetOrgList_ByOrganizationNumber), MemberType = typeof(TestDataAuthorizedParties))]
    [MemberData(nameof(TestDataAuthorizedParties.ResourceOwner_GetOrgList_ByOrganizationUuid), MemberType = typeof(TestDataAuthorizedParties))]
    [MemberData(nameof(TestDataAuthorizedParties.ResourceOwner_GetOrgList_ByPartyId), MemberType = typeof(TestDataAuthorizedParties))]
    [MemberData(nameof(TestDataAuthorizedParties.ResourceOwner_GetOrgList_ByOrganizationNumber_InclResourcesThroughRoles), MemberType = typeof(TestDataAuthorizedParties))]
    [MemberData(nameof(TestDataAuthorizedParties.ResourceOwner_GetEnterpriseUserList_ByEnterpriseUserUsername), MemberType = typeof(TestDataAuthorizedParties))]
    [MemberData(nameof(TestDataAuthorizedParties.ResourceOwner_GetEnterpriseUserList_ByEnterpriseUserUuid), MemberType = typeof(TestDataAuthorizedParties))]
    [MemberData(nameof(TestDataAuthorizedParties.ResourceOwner_GetEnterpriseUserList_ByUserId), MemberType = typeof(TestDataAuthorizedParties))]
    [MemberData(nameof(TestDataAuthorizedParties.ResourceOwner_GetEnterpriseUserList_ByEnterpriseUserUsername_InclResourcesThroughRoles), MemberType = typeof(TestDataAuthorizedParties))]
    public async Task PostResourceOwnerAuthorizedParties_Ok(string resourceOwnerToken, BaseAttributeExternal attributeExt, bool inclA2, bool inclRoleResources, List<AuthorizedPartyExternal> expected)
    {
        var client = GetTestClient(resourceOwnerToken);

        // Act
        HttpResponseMessage response = await client.PostAsync(
            $"accessmanagement/api/v1/resourceowner/authorizedparties?includeAltinn2={inclA2}&includeAuthorizedResourcesThroughRoles={inclRoleResources}",
            new StringContent(JsonSerializer.Serialize(attributeExt), Encoding.UTF8, MediaTypeNames.Application.Json));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<AuthorizedPartyExternal> actual = JsonSerializer.Deserialize<List<AuthorizedPartyExternal>>(await response.Content.ReadAsStringAsync(), options);
        AssertionUtil.AssertCollections(expected, actual, TestDataAuthorizedParties.AssertAuthorizedPartyExternalEqual);
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
