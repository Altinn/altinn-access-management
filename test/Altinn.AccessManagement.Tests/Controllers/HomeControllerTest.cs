using Altinn.AccessManagement.Controllers;
using Altinn.AccessManagement.Core.Clients;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.AccessManagement.Tests.Util;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
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
        private readonly HttpClient _client;

        /// <summary>
        /// Constructor setting up factory, test client and dependencies
        /// </summary>
        /// <param name="factory">CustomWebApplicationFactory</param>
        public HomeControllerTest(CustomWebApplicationFactory<HomeController> factory)
        {
            _factory = factory;
            _client = GetTestClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string token = PrincipalUtil.GetAccessToken("sbl.authorization");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Test case: 
        /// Expected: 
        /// </summary>
        [Fact]
        public async Task Index_Authenticated()
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/");
            string expectedCookie = "Antiforgery";
            string actualCookie = response.Headers.GetValues("Set-Cookie").FirstOrDefault();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(expectedCookie, actualCookie);
        }

        /// <summary>
        /// Test case: 
        /// Expected: 
        /// </summary>
        [Fact]
        public async Task Index_NotAuthenticated()
        {
            // Arrange
            _client.DefaultRequestHeaders.Remove("Authorization");
            string requestUrl = "http://localhost:5101/authentication/api/v1/authentication?goto=http%3a%2f%2flocalhost%2faccessmanagement";

            // Act
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/");

            // Assert
            Assert.Equal(requestUrl, response.RequestMessage.RequestUri.ToString());
        }

        private HttpClient GetTestClient()
        {
            HttpClient client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IAuthenticationClient, AuthenticationMock>();
                });
            }).CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = true });

            return client;
        }
    }
}
