using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Altinn.AccessManagement.Controllers;
using Altinn.AccessManagement.Interfaces;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.AccessManagement.Tests.Util;
using Altinn.Common.PEP.Interfaces;
using Altinn.Platform.Profile.Enums;
using Altinn.Platform.Profile.Models;
using Altinn.Platform.Register.Models;
using AltinnCore.Authentication.JwtCookie;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Altinn.AccessManagement.Tests.Controllers
{
    /// <summary>
    /// Test class for <see cref="DelegationsController"/>
    /// </summary>
    [Collection("ProfileController Tests")]
    public class ProfileControllerTest : IClassFixture<CustomWebApplicationFactory<ProfileController>>
    {
        private readonly CustomWebApplicationFactory<ProfileController> _factory;
        private readonly HttpClient _client;
        private readonly IProfileClient _profileClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileControllerTest"/> class.
        /// </summary>
        /// <param name="factory">CustomWebApplicationFactory</param>
        public ProfileControllerTest(CustomWebApplicationFactory<ProfileController> factory)
        {
            _factory = factory;
            _profileClient = Mock.Of<IProfileClient>();
            _client = GetTestClient();
        }

        private HttpClient GetTestClient()
        {
            var httpClient = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IProfileClient>(sp => _profileClient);
                    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                    services.AddSingleton<IPostConfigureOptions<JwtCookieOptions>, JwtCookiePostConfigureOptionsStub>();
                    services.AddSingleton<IPDP, PdpPermitMock>();
                });
            }).CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return httpClient;
        }

        private void SetupProfileClientMock(UserProfile userProfile)
        {
            Mock.Get(_profileClient).Setup(m => m.GetUserProfile(It.IsAny<int>())).ReturnsAsync(userProfile);
        }

        private UserProfile GetUserProfile(int id)
        {
            return new UserProfile
            {
                UserId = id,
                Email = "email@domain.com",
                ExternalIdentity = "SomeId",
                PartyId = 1234,
                PhoneNumber = "12345678",
                UserName = "UserName",
                UserType = UserType.None,
                Party = new Party(),
                ProfileSettingPreference = new ProfileSettingPreference
                {
                    DoNotPromptForParty = false, Language = "Norwegian", LanguageType = "NB", PreSelectedPartyId = 1
                }
            };
        }

        /// <summary>
        /// Assert that OK and user is returned upon user found
        /// </summary>
        [Fact]
        public async Task GetUser_UserFound_ReturnsUserProfile()
        {
            const int userId = 1234;                            
            SetupProfileClientMock(GetUserProfile(userId));                  
            var token = PrincipalUtil.GetToken(userId, 1234, 2);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var response = await _client.GetAsync("accessmanagement/api/v1/profile/user");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var userProfile = await response.Content.ReadAsAsync<UserProfile>();
            Assert.Equal(userId, userProfile.UserId);
            Mock.Get(_profileClient).Verify(p => p.GetUserProfile(It.Is<int>(i => i == userId)), Times.Once());
        }

        /// <summary>
        /// Assert that BadRequest is returned when userId is 0
        /// </summary>
        [Fact]
        public async Task GetUser_UserIdNotSet_ReturnsBadRequest()
        {
            const int userId = 0;                                                                     
            SetupProfileClientMock(GetUserProfile(userId));                                                           
            var token = PrincipalUtil.GetToken(userId, 1234, 2);                                         
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var response = await _client.GetAsync("accessmanagement/api/v1/profile/user");
            
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// Assert that NotFound is returned upon user not found by Profile service
        /// </summary>
        [Fact]
        public async Task GetUser_UserNotFoundByProfoileService_ReturnsNotFound()
        {
            const int userId = 1234;                                                                     
            SetupProfileClientMock(null);                                                           
            var token = PrincipalUtil.GetToken(userId, 1234, 2); 
            
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var response = await _client.GetAsync("accessmanagement/api/v1/profile/user");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Assert that InternalServiceError is returned upon exception
        /// </summary>
        [Fact]
        public async Task GetUser_ExceptionInProfileService_ReturnsInternalServiceError()
        {
            const int userId = 1234;
            var token = PrincipalUtil.GetToken(userId, 1234, 2);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            Mock.Get(_profileClient).Setup(m => m.GetUserProfile(It.IsAny<int>()))
                .Throws(new Exception("Something failed"));
            
            var response = await _client.GetAsync("accessmanagement/api/v1/profile/user");

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }
    }
}