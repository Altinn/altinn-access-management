using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Altinn.AccessManagement.Controllers.BFF;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Models;
using Altinn.AccessManagement.Models.Bff;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.AccessManagement.Tests.Util;
using Altinn.AccessManagement.Tests.Utils;
using Altinn.Common.PEP.Interfaces;
using AltinnCore.Authentication.JwtCookie;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Altinn.AccessManagement.Tests.Controllers
{
    /// <summary>
    /// Test class for <see cref="DelegationsController"></see>
    /// </summary>
    [Collection("DelegationController for frontend tests")]
    public class BFFDelegationsControllerTest : IClassFixture<CustomWebApplicationFactory<DelegationsController>>
    {
        private readonly CustomWebApplicationFactory<DelegationsController> _factory;
        private readonly HttpClient _client;

        private readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        /// <summary>
        /// Constructor setting up factory, test client and dependencies
        /// </summary>
        /// <param name="factory">CustomWebApplicationFactory</param>
        public BFFDelegationsControllerTest(CustomWebApplicationFactory<DelegationsController> factory)
        {
            _factory = factory;
            _client = GetTestClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string token = PrincipalUtil.GetAccessToken("sbl.authorization");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Test case: GetOfferedMaskinportenSchemaDelegations returns a list of delegations offeredby has given coveredby
        /// Expected: GetOfferedMaskinportenSchemaDelegations returns a list of delegations offeredby has given coveredby
        /// </summary>
        [Fact]
        public async Task GetOfferedMaskinportenSchemaDelegations_Valid_OfferedByParty()
        {
            // Arrange
            List<DelegationBff> expectedDelegations = GetExpectedOutboundDelegationsForParty(50004223);

            // Act
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/bff/50004223/delegations/maskinportenschema/offered");
            string responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<DelegationBff> actualDelegations = JsonSerializer.Deserialize<List<DelegationBff>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedDelegations, actualDelegations, AssertionUtil.AssertDelegationEqual);
        }

        /// <summary>
        /// Test case: GetOfferedMaskinportenSchemaDelegations returns a list of delegations offeredby has given coveredby
        /// Expected: GetOfferedMaskinportenSchemaDelegations returns a list of delegations offeredby has given coveredby
        /// </summary>
        [Fact]
        public async Task GetOfferedMaskinportenSchemaDelegations_Valid_OfferedByOrg()
        {
            // Arrange
            List<DelegationBff> expectedDelegations = GetExpectedOutboundDelegationsForParty(50004223);
            var token = PrincipalUtil.GetToken(1234, 12345678, 2);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _client.DefaultRequestHeaders.Add("party-organizationumber", "810418982");

            // Act
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/bff/organization/delegations/maskinportenschema/offered");
            string responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<DelegationBff> actualDelegations = JsonSerializer.Deserialize<List<DelegationBff>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedDelegations, actualDelegations, AssertionUtil.AssertDelegationEqual);
        }

        /// <summary>
        /// Test case: GetOfferedMaskinportenSchemaDelegations returns notfound when the query parameter is missing
        /// Expected: GetOfferedMaskinportenSchemaDelegations returns notfound
        /// </summary>
        [Fact]
        public async Task GetOfferedMaskinportenSchemaDelegations_Notfound_MissingOfferedBy()
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/bff//delegations/maskinportenschema/offered");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Test case: GetOfferedMaskinportenSchemaDelegations returns badrequest when the query parameter is invalid
        /// Expected: GetOfferedMaskinportenSchemaDelegations returns badrequest
        /// </summary>
        [Fact(Skip = "Bad test scenario. Will give not authorized not bad request")]
        public async Task GetOfferedMaskinportenSchemaDelegations_BadRequest_InvalidOfferedBy()
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/bff/123/delegations/maskinportenschema/offered");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// Test case: GetOfferedMaskinportenSchemaDelegations returns 200 with response message "No delegations found" when there are no delegations for the reportee
        /// Expected: GetOfferedMaskinportenSchemaDelegations returns 200 with response message "No delegations found" when there are no delegations for the reportee
        /// </summary>
        [Fact]
        public async Task GetOfferedMaskinportenSchemaDelegations_OfferedBy_NoDelegations()
        {
            // Arrange
            string expected = "[]";

            // Act
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/bff/50004225/delegations/maskinportenschema/offered");
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(expected, responseContent);
        }

        /// <summary>
        /// Test case: GetOfferedMaskinportenSchemaDelegations returns list of resources that were delegated. The resource metadata is set to not available if the resource in a delegation for some reason is  not found in resource registry
        /// Expected: GetOfferedMaskinportenSchemaDelegations returns list of resources that were delegated. The resource metadata is set to not available if the resource in a delegation for some reason is  not found in resource registry
        /// </summary>
        [Fact]
        public async Task GetOfferedMaskinportenSchemaDelegations_ResourceMetadataNotFound()
        {
            // Arrange
            List<DelegationBff> expectedDelegations = GetExpectedOutboundDelegationsForParty(50004226);

            // Act
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/bff/50004226/delegations/maskinportenschema/offered");
            string responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<DelegationBff> actualDelegations = JsonSerializer.Deserialize<List<DelegationBff>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedDelegations, actualDelegations, AssertionUtil.AssertDelegationEqual);
        }

        /// <summary>
        /// Test case: GetOfferedMaskinportenSchemaDelegations returns unauthorized when the bearer token is not set
        /// Expected: GetOfferedMaskinportenSchemaDelegations returns unauthorized when the bearer token is not set
        /// </summary>
        [Fact]
        public async Task GetOfferedMaskinportenSchemaDelegations_MissingBearerToken()
        {
            _client.DefaultRequestHeaders.Remove("Authorization");

            // Act
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/bff/50004223/delegations/maskinportenschema/offered");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        /// <summary>
        /// Test case: GetOfferedMaskinportenSchemaDelegations returns unauthorized when the bearer token is not valid
        /// Expected: GetOfferedMaskinportenSchemaDelegations returns unauthorized when the bearer token is not valid
        /// </summary>
        [Fact]
        public async Task GetOfferedMaskinportenSchemaDelegations_InvalidBearerToken()
        {
            // Arrange
            _client.DefaultRequestHeaders.Remove("Authorization");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "This is an invalid token");

            // Act
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/bff/50004223/delegations/maskinportenschema/offered");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        /// <summary>
        /// Test case: GetReceivedMaskinportenSchemaDelegations returns a list of delegations received by coveredby
        /// Expected: GetReceivedMaskinportenSchemaDelegations returns a list of delegations received by coveredby
        /// </summary>
        [Fact]
        public async Task GetReceivedMaskinportenSchemaDelegations_Valid_CoveredBy()
        {
            // Arrange
            List<DelegationBff> expectedDelegations = GetExpectedInboundDelegationsForParty(50004219);

            // Act
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/bff/50004219/delegations/maskinportenschema/received");
            string responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<DelegationBff> actualDelegations = JsonSerializer.Deserialize<List<DelegationBff>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedDelegations, actualDelegations, AssertionUtil.AssertDelegationEqual);
        }

        /// <summary>
        /// Test case: GetReceivedMaskinportenSchemaDelegations returns a list of delegations received by coveredby when the coveredby is an organisation number
        /// Expected: GetReceivedMaskinportenSchemaDelegations returns a list of delegations received by coveredby
        /// </summary>
        [Fact]
        public async Task GetReceivedMaskinportenSchemaDelegations_Valid_CoveredByOrg()
        {
            // Arrange
            List<DelegationBff> expectedDelegations = GetExpectedInboundDelegationsForParty(50004219);
            var token = PrincipalUtil.GetToken(1234, 12345678, 2);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _client.DefaultRequestHeaders.Add("party-organizationumber", "810418192");

            // Act
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/bff/organization/delegations/maskinportenschema/received");
            string responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<DelegationBff> actualDelegations = JsonSerializer.Deserialize<List<DelegationBff>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedDelegations, actualDelegations, AssertionUtil.AssertDelegationEqual);
        }

        /// <summary>
        /// Test case: GetReceivedMaskinportenSchemaDelegations returns notfound when the query parameter is missing
        /// Expected: GetReceivedMaskinportenSchemaDelegations returns notfound when the query parameter is missing
        /// </summary>
        [Fact]
        public async Task GetReceivedMaskinportenSchemaDelegations_Missing_CoveredBy()
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/bff//delegations/maskinportenschema/received");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Test case: GetReceivedMaskinportenSchemaDelegations returns badrequest when the query parameter is invalid
        /// Expected: GetReceivedMaskinportenSchemaDelegations returns badrequest
        /// </summary>
        [Fact(Skip = "Bad test scenario. Will give not authorized not bad request")]
        public async Task GetReceivedMaskinportenSchemaDelegations_Invalid_CoveredBy()
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/bff/1234/delegations/maskinportenschema/received");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// Test case: GetReceivedMaskinportenSchemaDelegations returns 200 with response message "No delegations found" when there are no delegations received for the reportee
        /// Expected: GetReceivedMaskinportenSchemaDelegations returns 200 with response message "No delegations found" when there are no delegations received for the reportee
        /// </summary>
        [Fact]
        public async Task GetReceivedMaskinportenSchemaDelegations_CoveredBy_NoDelegations()
        {
            // Arrange
            string expected = "[]";

            // Act
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/bff/50004225/delegations/maskinportenschema/received");
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(expected, responseContent);
        }

        /// <summary>
        /// Test case: GetReceivedMaskinportenSchemaDelegations returns list of resources that were delegated. The resource metadata is set to not available if the resource in a delegation for some reason is  not found in resource registry
        /// Expected: GetReceivedMaskinportenSchemaDelegations returns list of resources that were delegated. The resource metadata is set to not available if the resource in a delegation for some reason is  not found in resource registry
        /// </summary>
        [Fact]
        public async Task GetReceivedMaskinportenSchemaDelegations_ResourceMetadataNotFound()
        {
            // Arrange
            List<DelegationBff> expectedDelegations = GetExpectedInboundDelegationsForParty(50004216);

            // Act
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/bff/50004216/delegations/maskinportenschema/received");
            string responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<DelegationBff> actualDelegations = JsonSerializer.Deserialize<List<DelegationBff>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedDelegations, actualDelegations, AssertionUtil.AssertDelegationEqual);
        }

        /// <summary>
        /// Test case: GetReceivedMaskinportenSchemaDelegations returns unauthorized when the bearer token is not set
        /// Expected: GetReceivedMaskinportenSchemaDelegations returns unauthorized when the bearer token is not set
        /// </summary>
        [Fact]
        public async Task GetReceivedMaskinportenSchemaDelegations_MissingBearerToken()
        {
            _client.DefaultRequestHeaders.Remove("Authorization");

            // Act
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/bff/50004223/delegations/maskinportenschema/received");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        /// <summary>
        /// Test case: GetReceivedMaskinportenSchemaDelegations returns unauthorized when the bearer token is not valid
        /// Expected: GetReceivedMaskinportenSchemaDelegations returns unauthorized when the bearer token is not valid
        /// </summary>
        [Fact]
        public async Task GetReceivedMaskinportenSchemaDelegations_InvalidBearerToken()
        {
            _client.DefaultRequestHeaders.Remove("Authorization");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "This is an invalid token");

            // Act
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/bff/50004223/delegations/maskinportenschema/received");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        private static List<DelegationBff> GetExpectedOutboundDelegationsForParty(int offeredByPartyId)
        {
            List<DelegationBff> outboundDelegations = new List<DelegationBff>();
            outboundDelegations = TestDataUtil.GetBffDelegations(offeredByPartyId, 0);
            return outboundDelegations;
        }

        private static List<DelegationBff> GetExpectedInboundDelegationsForParty(int covererdByPartyId)
        {
            List<DelegationBff> inboundDelegations = new List<DelegationBff>();
            inboundDelegations = TestDataUtil.GetBffDelegations(0, covererdByPartyId);
            return inboundDelegations;
        }

        private HttpClient GetTestClient()
        {
            HttpClient client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IPolicyRetrievalPoint, PolicyRetrievalPointMock>();
                    services.AddSingleton<IDelegationMetadataRepository, DelegationMetadataRepositoryMock>();
                    services.AddSingleton<IPolicyRepository, PolicyRepositoryMock>();
                    services.AddSingleton<IDelegationChangeEventQueue, DelegationChangeEventQueueMock>();
                    services.AddSingleton<IPostConfigureOptions<JwtCookieOptions>, JwtCookiePostConfigureOptionsStub>();
                    services.AddSingleton<IPartiesClient, PartiesClientMock>();
                    services.AddSingleton<IResourceRegistryClient, ResourceRegistryClientMock>();
                    services.AddSingleton<IPDP, PdpPermitMock>();
                });
            }).CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            return client;
        }
    }
}
