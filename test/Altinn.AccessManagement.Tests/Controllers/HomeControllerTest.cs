using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Altinn.AccessManagement.Controllers;
using Altinn.AccessManagement.Core.Clients;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.AccessManagement.Tests.Util;
using Altinn.AccessManagement.Tests.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace Altinn.AccessManagement.Tests.Controllers
{
    /// <summary>
    /// Test class for <see cref="HomeController"></see>
    /// </summary>
    [Collection("HomeController Tests")]
    public class HomeControllerTest : IClassFixture<CustomWebApplicationFactory<HomeController>>
    {
        private readonly CustomWebApplicationFactory<HomeController> _factory;

        /// <summary>
        /// Constructor setting up factory, test client and dependencies
        /// </summary>
        /// <param name="factory">CustomWebApplicationFactory</param>
        public HomeControllerTest(CustomWebApplicationFactory<HomeController> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Test case: Checks if the altifirgery cookie is set when authenticated
        /// Expected: 
        /// </summary>
        [Fact]
        public async Task Index_Authenticated()
        {
            HttpClient client = SetupUtils.GetTestClient(_factory, false);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string token = PrincipalUtil.GetAccessToken("sbl.authorization");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            HttpResponseMessage response = await client.GetAsync($"accessmanagement/");
            string expectedCookie = "Antiforgery";
            string actualCookie = response.Headers.GetValues("Set-Cookie").FirstOrDefault();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(expectedCookie, actualCookie);
        }

        /// <summary>
        /// Test case: Checks if the user is redirected to authentication when not authenticated
        /// Expected: User is redirected to authentication
        /// </summary>
        [Fact]
        public async Task Index_NotAuthenticated()
        {
            // Arrange
            HttpClient client = SetupUtils.GetTestClient(_factory, true);
            string requestUrl = "http://localhost:5101/authentication/api/v1/authentication?goto=http%3a%2f%2flocalhost%2faccessmanagement";

            // Act
            HttpResponseMessage response = await client.GetAsync($"accessmanagement/");

            // Assert
            Assert.Equal(requestUrl, response.RequestMessage.RequestUri.ToString());
        }

        /// <summary>
        /// Test case : Authenticate with a cookie
        /// Expected : User is authenticated
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetHome_OK_WithAuthCookie()
        {
            string token = PrincipalUtil.GetAccessToken("sbl.authorization");

            HttpClient client = SetupUtils.GetTestClient(_factory, false);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "accessmanagement/");

            SetupUtils.AddAuthCookie(httpRequestMessage, token, "AltinnStudioRuntime");

            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);
            _ = await response.Content.ReadAsStringAsync();
            IEnumerable<string> cookieHeaders = response.Headers.GetValues("Set-Cookie");

            // Verify that 
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, cookieHeaders.Count());
            Assert.Contains("Antiforgery", cookieHeaders.ElementAt(0));
            Assert.StartsWith("XSR", cookieHeaders.ElementAt(1));
        }

        /// <summary>
        /// Test case : Authenticate with a invalid cookie
        /// Expected : User is redirected to authentication
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetHome_UnAuthorized_WithInvalidAuthCookie()
        {
            string token = "This is an invalid token";

            HttpClient client = SetupUtils.GetTestClient(_factory, true);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "accessmanagement/");

            SetupUtils.AddAuthCookie(httpRequestMessage, token, "AltinnStudioRuntime");

            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);
            string requestUrl = "http://localhost:5101/authentication/api/v1/authentication?goto=http%3a%2f%2flocalhost%2faccessmanagement";

            // Verify that 
            Assert.Equal(requestUrl, response.RequestMessage.RequestUri.ToString());
        }
    }
}
