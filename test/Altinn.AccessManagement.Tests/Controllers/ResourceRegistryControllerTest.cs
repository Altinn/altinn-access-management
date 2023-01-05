using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Altinn.AccessManagement.Controllers;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Models;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.AccessManagement.Tests.Util;
using Altinn.AccessManagement.Tests.Utils;
using Altinn.Common.PEP.Interfaces;
using AltinnCore.Authentication.JwtCookie;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Altinn.AccessManagement.Tests.Controllers
{
    /// <summary>
    /// Test class for <see cref="ResourceRegistryController"></see>
    /// </summary>
    [Collection("ResourceRegistryController Tests")]
    public class ResourceRegistryControllerTest : IClassFixture<CustomWebApplicationFactory<ResourceRegistryController>>
    {
        private readonly CustomWebApplicationFactory<ResourceRegistryController> _factory;
        private readonly HttpClient _client;

        private readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        /// <summary>
        /// Constructor setting up factory, test client and dependencies
        /// </summary>
        /// <param name="factory">CustomWebApplicationFactory</param>
        public ResourceRegistryControllerTest(CustomWebApplicationFactory<ResourceRegistryController> factory)
        {
            _factory = factory;
            _client = GetTestClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string token = PrincipalUtil.GetAccessToken("sbl.authorization");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private HttpClient GetTestClient()
        {
            HttpClient client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IPostConfigureOptions<JwtCookieOptions>, JwtCookiePostConfigureOptionsStub>();
                    services.AddSingleton<IResourceRegistryClient, ResourceRegistryClientMock>();
                    services.AddSingleton<IPDP, PdpPermitMock>();
                });
            }).CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            return client;
        }

        /// <summary>
        /// Test case: GetResources returns a list of resources 
        /// Expected: GetResources returns a list of resources filtered by resourcetype
        /// </summary>
        [Fact]
        public async Task GetResources_valid_resourcetype()
        {
            // Arrange
            List<ServiceResourceExternal> expectedResources = GetExpectedResources(ResourceType.MaskinportenSchema);

            // Act
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/52004219/resources/maskinportenschema");
            string responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<ServiceResourceExternal> actualResources = JsonSerializer.Deserialize<List<ServiceResourceExternal>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedResources, actualResources, AssertionUtil.AssertResourceExternalEqual);
        }

        private List<ServiceResourceExternal> GetExpectedResources(ResourceType resourceType)
        {
            List<ServiceResourceExternal> resources = new List<ServiceResourceExternal>();
            resources = TestDataUtil.GetResources(resourceType);
            return resources;
        }
    }
}
