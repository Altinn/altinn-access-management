using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Altinn.AccessManagement.Controllers;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Models;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.AccessManagement.Tests.Util;
using Altinn.AccessManagement.Tests.Utils;
using Altinn.AccessManagement.Utilities;
using Altinn.Common.AccessToken.Services;
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
    [Collection("LookupController Tests")]
    public class LookupControllerTest : IClassFixture<CustomWebApplicationFactory<LookupController>>
    {
        private readonly CustomWebApplicationFactory<LookupController> _factory;
        private readonly HttpClient _client;

        private readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        /// <summary>
        /// Constructor setting up factory, test client and dependencies
        /// </summary>
        /// <param name="factory">CustomWebApplicationFactory</param>
        public LookupControllerTest(CustomWebApplicationFactory<LookupController> factory)
        {
            _factory = factory;
            _client = GetTestClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string token = PrincipalUtil.GetAccessToken("sbl.authorization");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Test case: GetParty for partyId that corresponds to a party in partylist for authenticated user.
        /// Expected: GetParty returns list with party.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetParty_Valid_PartyId_is_person()
        {
            // Arrange
            PartyExternal expectedParty = GetExpectedParty(50002182);
            expectedParty.SSN = IdentifierUtil.MaskSSN(expectedParty.SSN);
            int userId = 50002182;
            string token = PrincipalUtil.GetToken(userId, 50002182, 2);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/lookup/reportee/{expectedParty.PartyId}");
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PartyExternal actualParty = JsonSerializer.Deserialize<PartyExternal>(responseContent, serializerOptions);
            AssertionUtil.AssertPartyEqual(expectedParty, actualParty);
        }

        /// <summary>
        /// Test case: GetParty for partyId that corresponds to a party in partylist for authenticated user.
        /// Expected: GetParty returns list with party.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetParty_Valid_PartyId_is_Org()
        {
            // Arrange
            PartyExternal expectedParty = GetExpectedParty(50004226);
            int userId = 50004226;
            string token = PrincipalUtil.GetToken(userId, 50004226, 2);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/lookup/reportee/{expectedParty.PartyId}");
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PartyExternal actualParty = JsonSerializer.Deserialize<PartyExternal>(responseContent, serializerOptions);
            AssertionUtil.AssertPartyEqual(expectedParty, actualParty);
        }

        /// <summary>
        /// Test case: GetParty for partyId that corresponds to a party in partylist for authenticated user.
        /// Expected: GetParty returns list with party.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetParty_Valid_PartyId_is_Subunit()
        {
            // Arrange
            PartyExternal expectedParty = GetExpectedParty(50004646, true);
            
            int userId = 50004646;
            string token = PrincipalUtil.GetToken(userId, 50004646, 2);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/lookup/reportee/{expectedParty.PartyId}");
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PartyExternal actualParty = JsonSerializer.Deserialize<PartyExternal>(responseContent, serializerOptions);
            AssertionUtil.AssertPartyEqual(expectedParty, actualParty);
        }

        /// <summary>
        /// Test case: GetParty for partyId that does not correspond to a party in partylist for authenticated user.
        /// Expected: GetParty returns 404 not found.
        /// </summary>
        /// <returns>404 not found</returns>
        [Fact]
        public async Task GetParty_Invalid_partyId()
        {
            // Arrange
            int userId = 1234;
            string token = PrincipalUtil.GetToken(userId, 50002182, 2);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/lookup/reportee/{54321}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private HttpClient GetTestClient()
        {
            HttpClient client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IPostConfigureOptions<JwtCookieOptions>, JwtCookiePostConfigureOptionsStub>();
                    services.AddSingleton<IPartiesClient, PartiesClientMock>();
                    services.AddSingleton<IPDP, PdpPermitMock>();
                    services.AddSingleton<IPublicSigningKeyProvider, SigningKeyResolverMock>();
                });
            }).CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            return client;
        }

        private static PartyExternal GetExpectedOrganisation(string orgNummer)
        {
            PartyExternal party = TestDataUtil.GetOrganisation(orgNummer);
            return party;
        }

        private static PartyExternal GetExpectedParty(int partyId, bool isSubUnit = false)
        {
            if (isSubUnit)
            {
                return TestDataUtil.GetTestPartyWithSubUnit(partyId);
            }

            return TestDataUtil.GetTestParty(partyId);
        }
    }
}
