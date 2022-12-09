using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Altinn.AccessManagement.Tests.Controllers
{
    /// <summary>
    /// Test class for <see cref="RightsController"></see>
    /// </summary>
    [Collection("RightsController Tests")]
    public class RightsControllerTest : IClassFixture<CustomWebApplicationFactory<RightsController>>
    {
        private readonly CustomWebApplicationFactory<RightsController> _factory;
        private readonly HttpClient _client;

        private readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        /// <summary>
        /// Constructor setting up factory, test client and dependencies
        /// </summary>
        /// <param name="factory">CustomWebApplicationFactory</param>
        public RightsControllerTest(CustomWebApplicationFactory<RightsController> factory)
        {
            _factory = factory;
            _client = GetTestClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Test case: RightsQuery returns a list of rights given from the offering partyid to the covered userid for the specified resource from the resource registry
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task RightsQuery_Delegated_ResourceRight_ReturnAllPolicyRights_False()
        {
            // Arrange
            Stream dataStream = File.OpenRead($"Data/Json/RightsQuery/jks_audi_etron_gt/user_20000095/party_50005545/RightsQuery.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            List<Right> expectedRights = GetExpectedRights("jks_audi_etron_gt", 50005545, 20000095, false);

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/", content);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Right> actualRights = JsonSerializer.Deserialize<List<Right>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightEqual);
        }

        /// <summary>
        /// Test case: RightsQuery returns a list of rights between an offering partyid to a covered userid, returnAllPolicyRights query param is set to true and operation should return all rights found in the resource registry XACML policy
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task RightsQuery_Delegated_ResourceRight_ReturnAllPolicyRights_True()
        {
            // Arrange
            Stream dataStream = File.OpenRead($"Data/Json/RightsQuery/jks_audi_etron_gt/user_20000095/party_50005545/RightsQuery.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            List<Right> expectedRights = GetExpectedRights("jks_audi_etron_gt", 50005545, 20000095, true);

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/?returnAllPolicyRights=true", content);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Right> actualRights = JsonSerializer.Deserialize<List<Right>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightEqual);
        }

        /// <summary>
        /// Test case: RightsQuery returns a list of rights given from the offering partyid to the covered userid for the specified altinn app
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task RightsQuery_Delegated_AppRight_ReturnAllPolicyRights_False()
        {
            // Arrange
            Stream dataStream = File.OpenRead($"Data/Json/RightsQuery/org1_app1/user_20001337/party_50001337/RightsQuery.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            List<Right> expectedRights = GetExpectedRights("org1_app1", 50001337, 20001337, false);

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/?returnAllPolicyRights=false", content);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Right> actualRights = JsonSerializer.Deserialize<List<Right>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightEqual);
        }

        /// <summary>
        /// Test case: RightsQuery returns a list of rights between an offering partyid to a covered userid, returnAllPolicyRights query param is set to true and operation should return all rights found in the altinn app XACML policy
        /// Note: This test scenario is setup using existing test data for Org1 App1 and offeredBy 50001337 and coveredby user 20001337, where the delegation policy contains rules for resources not in the App policy:
        /// ("rightKey": "app1,org1,task1:sign" and "rightKey": "app1,org1,task1:write"). This should normally not happen but can become an real scenario where delegations have been made and then the resource/app  policy is changed to remove some rights.
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task RightsQuery_Delegated_AppRight_ReturnAllPolicyRights_True()
        {
            // Arrange
            Stream dataStream = File.OpenRead($"Data/Json/RightsQuery/org1_app1/user_20001337/party_50001337/RightsQuery.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            List<Right> expectedRights = GetExpectedRights("org1_app1", 50001337, 20001337, true);

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/?returnAllPolicyRights=true", content);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Right> actualRights = JsonSerializer.Deserialize<List<Right>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightEqual);
        }

        /// <summary>
        /// Test case: RightsQuery returning the list of rights a DAGL has for its organization for the resource from the resource registry
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task RightsQuery_Resource_DAGL_ReturnAllPolicyRights_False()
        {
            // Arrange
            Stream dataStream = File.OpenRead($"Data/Json/RightsQuery/jks_audi_etron_gt/user_20000490/party_50005545/RightsQuery.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            List<Right> expectedRights = GetExpectedRights("jks_audi_etron_gt", 50005545, 20000490, false);

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/?returnAllPolicyRights=false", content);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Right> actualRights = JsonSerializer.Deserialize<List<Right>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightEqual);
        }

        /// <summary>
        /// Test case: RightsQuery returning the list of rights a DAGL has for its organization for the resource from the resource registry. returnAllPolicyRights query param is set to true and operation should return all rights found in the resource XACML policy
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task RightsQuery_Resource_DAGL_ReturnAllPolicyRights_True()
        {
            // Arrange
            Stream dataStream = File.OpenRead($"Data/Json/RightsQuery/jks_audi_etron_gt/user_20000490/party_50005545/RightsQuery.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            List<Right> expectedRights = GetExpectedRights("jks_audi_etron_gt", 50005545, 20000490, true);

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/?returnAllPolicyRights=true", content);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Right> actualRights = JsonSerializer.Deserialize<List<Right>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightEqual);
        }

        /// <summary>
        /// Test case: RightsQuery returning the list of rights a DAGL has for its organization for the Altinn App
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task RightsQueryd_AppRight_DAGL_ReturnAllPolicyRights_False()
        {
            // Arrange
            Stream dataStream = File.OpenRead($"Data/Json/RightsQuery/ttd_rf-0002/user_20000490/party_50005545/RightsQuery.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            List<Right> expectedRights = GetExpectedRights("ttd_rf-0002", 50005545, 20000490, false);

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/?returnAllPolicyRights=false", content);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Right> actualRights = JsonSerializer.Deserialize<List<Right>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightEqual);
        }

        /// <summary>
        /// Test case: RightsQuery returning the list of rights a DAGL has for its organization for the Altinn App. returnAllPolicyRights query param is set to true and operation should return all rights found in the apps XACML policy
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task RightsQueryd_AppRight_DAGL_ReturnAllPolicyRights_True()
        {
            // Arrange
            Stream dataStream = File.OpenRead($"Data/Json/RightsQuery/ttd_rf-0002/user_20000490/party_50005545/RightsQuery.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            List<Right> expectedRights = GetExpectedRights("ttd_rf-0002", 50005545, 20000490, true);

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/?returnAllPolicyRights=true", content);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Right> actualRights = JsonSerializer.Deserialize<List<Right>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightEqual);
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
                    services.AddSingleton<IPartiesClient, PartiesClientMock>();
                    services.AddSingleton<IResourceRegistryClient, ResourceRegistryClientMock>();
                    services.AddSingleton<IAltinnRolesClient, AltinnRolesClientMock>();
                });
            }).CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            return client;
        }

        private static List<Right> GetExpectedRights(string resourceId, int fromPartyId, int toUserId, bool returnAllPolicyRights)
        {
            List<Right> rights = new();

            string rightsPath = $"Data/Json/RightsQuery/{resourceId}/user_{toUserId}/party_{fromPartyId}/expected_rights_returnall_{returnAllPolicyRights}.json";
            if (File.Exists(rightsPath))
            {
                string content = File.ReadAllText(rightsPath);
                rights = (List<Right>)JsonSerializer.Deserialize(content, typeof(List<Right>), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            return rights;
        }
    }
}
