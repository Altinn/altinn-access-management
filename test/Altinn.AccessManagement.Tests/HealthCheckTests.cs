using System.Net;
using Altinn.AccessManagement.Health;
using Altinn.AccessManagement.Tests.Fixtures;

namespace Altinn.AccessManagement.Tests.Health
{
    /// <summary>
    /// Health check 
    /// </summary>
    /// 
    public class HealthCheckTests(WebApplicationFixture fixture) : IClassFixture<WebApplicationFixture>
    {
        public WebApplicationFixture Fixture { get; } = fixture;

        /// <summary>
        /// Verify that component responds on health check
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task VerifyHealthCheck_OK()
        {
            HttpClient client = Fixture.ConfigureHostBuilderWithScenarios().Client;

            var request = new HttpRequestMessage(HttpMethod.Get, "/health");

            HttpResponseMessage response = await client.SendAsync(request);
            Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// Verify that component responds on health check
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task VerifyAliveCheck_OK()
        {
            HttpClient client = Fixture.ConfigureHostBuilderWithScenarios().Client;

            var request = new HttpRequestMessage(HttpMethod.Get, "/alive");

            HttpResponseMessage response = await client.SendAsync(request);
            await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
