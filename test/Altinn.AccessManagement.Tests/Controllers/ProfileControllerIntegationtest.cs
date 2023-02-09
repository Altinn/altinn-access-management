#nullable enable
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Altinn.AccessManagement.Controllers;
using Altinn.AccessManagement.Integration.Clients;
using Altinn.AccessManagement.Interfaces;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.AccessManagement.Tests.Util;
using Altinn.Common.AccessTokenClient.Services;
using Altinn.Common.PEP.Interfaces;
using Altinn.Platform.Profile.Models;
using AltinnCore.Authentication.JwtCookie;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Altinn.AccessManagement.Tests.Controllers
{
    /// <summary>
    /// Integrationtests of ProfileController
    /// </summary>
    [Collection("ProfileController integrationtests")]
    public partial class ProfileControllerIntegationtest: IClassFixture<CustomWebApplicationFactory<ProfileController>>
    {
        private readonly CustomWebApplicationFactory<ProfileController> _factory;
        private readonly Mock<ILogger<ProfileClient>> _logger;
        
        /// <summary>
        /// Integration test of ProfileController
        /// </summary>
        /// <param name="factory">CustomWebApplicationFactory</param>
        public ProfileControllerIntegationtest(CustomWebApplicationFactory<ProfileController> factory)
        {
            _factory = factory;
            _logger = new Mock<ILogger<ProfileClient>>();
        }

        private HttpClient GetClient(int userId, MessageHandlerMock handler)
        {
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                    services.AddSingleton<IPostConfigureOptions<JwtCookieOptions>, JwtCookiePostConfigureOptionsStub>();
                    services.AddSingleton<IAccessTokenGenerator, AccessTokenGenerator>();
                    services.AddSingleton(typeof(ILogger<ProfileClient>),  _logger.Object);
                    services.AddHttpClient<IProfileClient, ProfileClient>().AddHttpMessageHandler<MessageHandlerMock>();
                    services.AddSingleton<MessageHandlerMock>(handler);
                    services.AddSingleton<IPDP, PdpPermitMock>();
                });
            }).CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var token = PrincipalUtil.GetToken(userId, 1234, 2);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        /// <summary>
        ///  Get user profile should return user profile
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task GetUserProfile_ReturnsUserProfile()
        {
            const int userId = 1234;
            const HttpStatusCode expectedStatusCode = HttpStatusCode.OK;
            var userProfile = new UserProfile { UserId = userId };
            var handler = new MessageHandlerMock(expectedStatusCode, JsonContent.Create(userProfile));
            var client = GetClient(userId, handler);

            var response = await client.GetAsync("accessmanagement/api/v1/profile/user");
            
            Assert.Equal(expectedStatusCode, response.StatusCode);
            var userProfileResult = await response.Content.ReadAsAsync<UserProfile>();
            Assert.Equivalent(userProfile, userProfileResult);
        }

        /// <summary>
        /// Message should be logged when user not found by Profile service
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task GetUserProfile_UserNotFound()
        {
            const int userId = 1234;
            const HttpStatusCode expectedStatusCode = HttpStatusCode.NotFound;
            var expectedErrorMessage =
                $"Getting user profile with userId {userId} failed with statuscode {expectedStatusCode}";
            var userProfile = new UserProfile { UserId = userId };
            var handler = new MessageHandlerMock(expectedStatusCode, JsonContent.Create(userProfile));
            
            _logger.Setup(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
            var client = GetClient(userId, handler);

            var response = await client.GetAsync("accessmanagement/api/v1/profile/user");

            // Verify log is called with loglevel error once
            _logger.Verify(
                m => m.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            
            // Verify errormessage
            var loggedErrorMessages = _logger.Invocations
                .Where(x => (LogLevel)x.Arguments[0] == LogLevel.Error)
                .Select(x => x.Arguments[2].ToString()).First();
            
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(expectedErrorMessage, loggedErrorMessages);
        }
    }
}