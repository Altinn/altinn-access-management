using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Altinn.AccessManagement.Controllers;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Tests.Utils;
using Xunit;

namespace Altinn.AccessManagement.Tests.Services
{
    /// <summary>
    /// Test class for <see cref="DelegationRequestsController"></see>
    /// </summary>
    public class DelegationRequestTests : IClassFixture<CustomWebApplicationFactory<DelegationRequestsController>>
    {
        private readonly CustomWebApplicationFactory<DelegationRequestsController> factory;
        private readonly HttpClient client;

        /// <summary>
        /// Constructor setting up factory, test client and dependencies
        /// </summary>
        /// <param name="factory">CustomWebApplicationFactory</param>
        public DelegationRequestTests(CustomWebApplicationFactory<DelegationRequestsController> factory)
        {
            this.factory = factory;
            client = SetupUtils.GetTestClient(factory);
        }

        /// <summary>
        /// Test1
        /// </summary>
        /// <returns>Result</returns>
        [Fact(Skip = "Incomplete implementation")]
        public async Task Test1()
        {
            string requestUri = "/accessmanagement/api/v1/delegationrequests";
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri)
            {
            };

            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            string responseContent = await response.Content.ReadAsStringAsync();
            DelegationRequests delegationRequest = JsonSerializer.Deserialize<DelegationRequests>(responseContent, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) as DelegationRequests;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(delegationRequest);
        }

        /// <summary>
        /// Test2
        /// </summary>
        /// <returns>Result</returns>
        [Fact(Skip = "Incomplete implementation")]
        public async Task Test2()
        {
            string requestUri = "/accessmanagement/api/v1/delegationrequests/23";
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri)
            {
            };

            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            string responseContent = await response.Content.ReadAsStringAsync();
            DelegationRequest delegationRequest = JsonSerializer.Deserialize<DelegationRequest>(responseContent, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) as DelegationRequest;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(delegationRequest);
        }
    }
}
