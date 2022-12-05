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
using Altinn.AccessManagement.Tests.Util;
using AltinnCore.Authentication.JwtCookie;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Altinn.AccessManagement.Tests.Controllers
{
    /// <summary>
    /// Tests for AccessManagmet Resource metadata
    /// </summary>
    [Collection("ResourceController Tests")]
    public class ResourceControllerTest : IClassFixture<CustomWebApplicationFactory<ResourceController>>
    {
        private readonly CustomWebApplicationFactory<ResourceController> _factory;
        private readonly HttpClient _client;

        private readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            
        };

        /// <summary>
        /// Constructor setting up factory, test client and dependencies
        /// </summary>
        /// <param name="factory">CustomWebApplicationFactory</param>
        public ResourceControllerTest(CustomWebApplicationFactory<ResourceController> factory)
        {
            _factory = factory;
            _client = GetTestClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string token = PrincipalUtil.GetAccessToken("internal.authorization");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Test calling InsertAccessManagementResource with invalid token
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task InsertAccessManagementResource_ResourceStored()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/InsertAccessManagementResource/input1.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            string dataElement1 = await File.OpenText("Data/Json/InsertAccessManagementResource/InsertData_string.json").ReadToEndAsync();
            string dataElement2 = await File.OpenText("Data/Json/InsertAccessManagementResource/InsertData_string2.json").ReadToEndAsync();
            string expectedText = $"[{dataElement1},{dataElement2}]";
            List<AccessManagementResource> expected = JsonSerializer.Deserialize<List<AccessManagementResource>>(expectedText, options);

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/resources", content);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<AccessManagementResource> actual = JsonSerializer.Deserialize<List<AccessManagementResource>>(responseContent, options);
            
            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            ListAccessManagementResourceAreEqual(expected, actual);
        }

        /// <summary>
        /// Test calling InsertAccessManagementResource with invalid token
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task InsertAccessManagementResource_MissingBearerToken()
        {
            // Arrange
            _client.DefaultRequestHeaders.Remove("Authorization");
            
            Stream dataStream = File.OpenRead("Data/Json/InsertAccessManagementResource/input1.json");
            StreamContent content = new StreamContent(dataStream);

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/resources", content);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        /// <summary>
        /// Test calling InsertAccessManagementResource with invalid token
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task InsertAccessManagementResource_InvalidBearerToken()
        {
            // Arrange
            _client.DefaultRequestHeaders.Remove("Authorization");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "This is an invalid token");
            
            Stream dataStream = File.OpenRead("Data/Json/InsertAccessManagementResource/input1.json");
            StreamContent content = new StreamContent(dataStream);

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/resources", content);
            
            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        /// <summary>
        /// tEST
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Test_OK()
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/internal/test");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private HttpClient GetTestClient()
        {
            HttpClient client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IResourceMetadataRepository, ResourceMetadataRepositoryMock>();
                    services.AddSingleton<IPostConfigureOptions<JwtCookieOptions>, JwtCookiePostConfigureOptionsStub>();
                });
            }).CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            return client;
        }

        private void ListAccessManagementResourceAreEqual(List<AccessManagementResource> expected, List<AccessManagementResource> actual)
        {
            Assert.Equal(expected.Count, actual.Count);
            for (int i = 0; i < actual.Count; i++)
            {
                AccessManagementResource currentExpectedElement = expected[i];
                AccessManagementResource currentActualElement = actual[i];

                Assert.Equal(currentExpectedElement.Created, currentActualElement.Created);
                Assert.Equal(currentExpectedElement.Modified, currentActualElement.Modified);
                Assert.Equal(currentExpectedElement.ResourceRegistryId, currentActualElement.ResourceRegistryId);
                Assert.Equal(currentExpectedElement.ResourceId, currentActualElement.ResourceId);
                Assert.Equal(currentExpectedElement.ResourceType, currentActualElement.ResourceType);
            }
        }
    }
}
