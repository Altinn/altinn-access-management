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
using Altinn.AccessManagement.Models;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.AccessManagement.Tests.Utils;
using Altinn.Common.AccessToken.Services;
using Altinn.Common.PEP.Interfaces;
using AltinnCore.Authentication.JwtCookie;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Altinn.AccessManagement.Tests.Controllers
{
    /// <summary>
    /// Test class for <see cref="PolicyInformationPointController"></see>
    /// </summary>
    public class PolicyInformationPointControllerTest : IClassFixture<CustomWebApplicationFactory<PolicyInformationPointController>>
    {
        private HttpClient _client;
        private readonly CustomWebApplicationFactory<PolicyInformationPointController> _factory;
        
        private readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyInformationPointControllerTest"/> class.
        /// </summary>
        /// <param name="factory">CustomWebApplicationFactory</param>
        public PolicyInformationPointControllerTest(CustomWebApplicationFactory<PolicyInformationPointController> factory)
        {
            _factory = factory;
            _client = GetTestClient();
        }
        
        private HttpClient GetTestClient(IPDP pdpMock = null, IHttpContextAccessor httpContextAccessor = null, IDelegationMetadataRepository delegationMetadataRepositoryMock = null)
        {
            pdpMock ??= new PepWithPDPAuthorizationMock();
            httpContextAccessor ??= new HttpContextAccessor();
            delegationMetadataRepositoryMock ??= new DelegationMetadataRepositoryMock();

            HttpClient client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IPolicyRetrievalPoint, PolicyRetrievalPointMock>();
                    services.AddSingleton(delegationMetadataRepositoryMock);
                    services.AddSingleton<IPolicyRepository, PolicyRepositoryMock>();
                    services.AddSingleton<IDelegationChangeEventQueue, DelegationChangeEventQueueMock>();
                    services.AddSingleton<IPostConfigureOptions<JwtCookieOptions>, JwtCookiePostConfigureOptionsStub>();
                    services.AddSingleton<IPartiesClient, PartiesClientMock>();
                    services.AddSingleton<IResourceRegistryClient, ResourceRegistryClientMock>(); 
                    services.AddSingleton(pdpMock);
                    services.AddSingleton(httpContextAccessor);
                    services.AddSingleton<IAltinnRolesClient, AltinnRolesClientMock>();
                    services.AddSingleton<IPublicSigningKeyProvider, SigningKeyResolverMock>();
                });
            }).CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            return client;
        }

        /// <summary>
        /// Test case: Tests if you can get all delegation changes for a user
        /// Expected: 
        /// </summary>
        [Fact]
        public async Task GetDelegationChanges_ValidResponse_Resource()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/DelegationChangeInput/resource.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            
            string expectedContent = File.ReadAllText("Data/ResourceRegistryDelegationChanges/ExpectedResponses/jks_audi_etron_gt/50004221/20000490/delegationchange.json");
            List<DelegationChange> expectedResponse = (List<DelegationChange>)JsonSerializer.Deserialize(expectedContent, typeof(List<DelegationChange>), options);

            // Act
            HttpResponseMessage actualResponse = await _client.PostAsync($"accessmanagement/api/v1/policyinformation/getdelegationchanges", content);
            string responseContent = await actualResponse.Content.ReadAsStringAsync();
            List<DelegationChange> actualDelegationChanges = JsonSerializer.Deserialize<List<DelegationChange>>(responseContent, options);
            
            Assert.Equal(HttpStatusCode.OK, actualResponse.StatusCode);
            AssertionUtil.AssertEqual(expectedResponse, actualDelegationChanges);
        }
        
        /// <summary>
        /// Test case: Tests if you can get all delegation changes for a user
        /// Expected: 
        /// </summary>
        [Fact]
        public async Task GetDelegationChanges_ValidResponse_App()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/DelegationChangeInput/app.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            
            string expectedContent = File.ReadAllText("Data/ResourceRegistryDelegationChanges/ExpectedResponses/app/app.json");
            List<DelegationChange> expectedResponse = (List<DelegationChange>)JsonSerializer.Deserialize(expectedContent, typeof(List<DelegationChange>), options);

            // Act
            HttpResponseMessage actualResponse = await _client.PostAsync($"accessmanagement/api/v1/policyinformation/getdelegationchanges", content);
            string responseContent = await actualResponse.Content.ReadAsStringAsync();
            List<DelegationChange> actualDelegationChanges = JsonSerializer.Deserialize<List<DelegationChange>>(responseContent, options);
            
            Assert.Equal(HttpStatusCode.OK, actualResponse.StatusCode);
            AssertionUtil.AssertEqual(expectedResponse, actualDelegationChanges);
        }
    }
}
