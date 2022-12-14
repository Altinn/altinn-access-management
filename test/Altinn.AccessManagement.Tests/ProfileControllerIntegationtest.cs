using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Altinn.AccessManagement.Controllers;
using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Integration.Clients;
using Altinn.AccessManagement.Integration.Configuration;
using Altinn.AccessManagement.Interfaces;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.AccessManagement.Tests.Util;
using Altinn.Common.AccessTokenClient.Services;
using Altinn.Platform.Profile.Models;
using AltinnCore.Authentication.JwtCookie;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace Altinn.AccessManagement.Tests
{
    /// <summary>
    /// Integrationtests of ProfileController
    /// </summary>
    [Collection("ProfileController integrationtests")]
    public class ProfileControllerIntegationtest: IClassFixture<CustomWebApplicationFactory<ProfileController>>
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
            Mock.Of<IKeyVaultService>();          
            Mock.Of<IAccessTokenGenerator>();
            _logger = new Mock<ILogger<ProfileClient>>();
        }

        private HttpClient GetTestClient(IProfileClient profileClient, int userId)
        {
            var messageHandlerMock = new Mock<HttpMessageHandler>();
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IProfileClient>(sp => profileClient);
                    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                    services.AddSingleton<IPostConfigureOptions<JwtCookieOptions>, JwtCookiePostConfigureOptionsStub>();
                });
            }).CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var token = PrincipalUtil.GetToken(userId, 1234, 2);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        private IProfileClient GetProfileClient(HttpStatusCode expectedHttpStatusCode, UserProfile expectedUserProfile)
        {
            var platformSettings = Options.Create(new PlatformSettings { ProfileApiEndpoint = "http://www.test.no/", SubscriptionKeyHeaderName = "SubscriptionKeyHeaderName"});
            var keyVaultSettings = Options.Create(new KeyVaultSettings { KeyVaultURI = "spotify:track:6KDCteFISA2GEHoVANwBvn?si=c5ce80373d8c46e7", PlatformCertSecretId = "PlatformCertSecretId"});
            var generalSettings = Mock.Of<IOptionsMonitor<GeneralSettings>>(optionsMonitor => optionsMonitor.CurrentValue == new GeneralSettings() { RuntimeCookieName = "RuntimeCookieName" });
            
            var keyVaultServiceMock = Mock.Of<IKeyVaultService>();
            Mock.Get(keyVaultServiceMock).Setup(m => m.GetCertificateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(GetTestCert);

            var accessTokenGeneratorMock = Mock.Of<IAccessTokenGenerator>();
            Mock.Get(accessTokenGeneratorMock)
                .Setup(m => m.GenerateAccessToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<X509Certificate2>())).Returns("SomeStringRepresentingAccessToken");

            _logger.Setup(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
            
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = expectedHttpStatusCode,
                    Content = JsonContent.Create(expectedUserProfile)
                });

            var httpClient = new HttpClient(handler.Object);
            var profileClient = new ProfileClient(platformSettings, keyVaultSettings, _logger.Object, new HttpContextAccessor(), generalSettings, httpClient, accessTokenGeneratorMock, keyVaultServiceMock);

            return profileClient;
        }

        private static string GetTestCert()
        {
            var x509Certificate2 = new X509Certificate2("selfSignedTestCertificate.pfx", "qwer1234");
            return Convert.ToBase64String(x509Certificate2.Export(X509ContentType.Cert));
        }

        /// <summary>
        ///  Get user profile should return user profile
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task GetUserProfile_ReturnsUserProfile()
        {
            const int userId = 1234;
            var userProfile = new UserProfile { UserId = userId };
            var profileClient = GetProfileClient(HttpStatusCode.OK, userProfile);
            var client = GetTestClient(profileClient, userId);

            var response = await client.GetAsync("accessmanagement/api/v1/profile/user");
            
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
            var profileClient = GetProfileClient(expectedStatusCode, userProfile);
            var client = GetTestClient(profileClient, userId);

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
            
            Assert.Equal(expectedErrorMessage, loggedErrorMessages);
        }
    }
}