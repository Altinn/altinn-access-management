using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Altinn.Authorizationadmin.Controllers;
using Altinn.AuthorizationAdmin.Core.Models;
using Altinn.AuthorizationAdmin.Tests.Utils;
using Xunit;

namespace Altinn.AuthorizationAdmin.Tests
{
    public class DelegationRequestTests: IClassFixture<CustomWebApplicationFactory<DelegationRequestsController>>
    {
        private readonly CustomWebApplicationFactory<DelegationRequestsController> _factory;

        public DelegationRequestTests(CustomWebApplicationFactory<DelegationRequestsController> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Test1()
        {
            HttpClient client = SetupUtils.GetTestClient(_factory);

            string requestUri = "/api/DelegationRequests";
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri)
            {
            };

            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            string responseContent = await response.Content.ReadAsStringAsync();
            DelegationRequests? delegationRequest = System.Text.Json.JsonSerializer.Deserialize<DelegationRequests>(responseContent, new System.Text.Json.JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) as DelegationRequests;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(delegationRequest);
        }

        [Fact]
        public async Task Test2()
        {
            HttpClient client = SetupUtils.GetTestClient(_factory);

            string requestUri = "/api/DelegationRequests/23";
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri)
            {
            };

            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            string responseContent = await response.Content.ReadAsStringAsync();
            DelegationRequest? delegationRequest = System.Text.Json.JsonSerializer.Deserialize<DelegationRequest>(responseContent, new System.Text.Json.JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) as DelegationRequest;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(delegationRequest);
        }
    }
}
