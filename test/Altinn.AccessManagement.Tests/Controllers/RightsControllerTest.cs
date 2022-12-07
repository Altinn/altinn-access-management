﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Altinn.AccessManagement.Controllers;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.AccessManagement.Tests.Util;
using Altinn.AccessManagement.Tests.Utils;
using AltinnCore.Authentication.JwtCookie;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Altinn.AccessManagement.Tests.Controllers
{
    /// <summary>
    /// Test class for <see cref="RightsController"></see>
    /// </summary>
    [Collection("RightsController Tests")]
    public class RightsControllerTest : IClassFixture<CustomWebApplicationFactory<AuthenticationController>>
    {
        private readonly CustomWebApplicationFactory<AuthenticationController> _factory;
        private readonly HttpClient _client;
        private readonly HttpClient _clientForNullToken;

        private readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        /// <summary>
        /// Constructor setting up factory, test client and dependencies
        /// </summary>
        /// <param name="factory">CustomWebApplicationFactory</param>
        public RightsControllerTest(CustomWebApplicationFactory<AuthenticationController> factory)
        {
            _factory = factory;
            _client = GetTestClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Test case: RightsQuery returns a list of rights given from the offering partyid to the covered userid
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task RightsQuery_Delegated_SingleRight_ReturnAllPolicyRights_False()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/RightsQuery/RightsQuery_SuccessRequest.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            List<Right> expectedRights = GetExpectedRights("jks_audi_etron_gt", 50005545, 20000095, false);

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/", content);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Right> actualRights = JsonSerializer.Deserialize<List<Right>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightEqual);
        }

        /// <summary>
        /// Test case: RightsQuery returns a list of rights given from the offering partyid to the covered userid
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task RightsQuery_Delegated_SingleRight_ReturnAllPolicyRights_True()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/RightsQuery/RightsQuery_SuccessRequest.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            List<Right> expectedRights = GetExpectedRights("jks_audi_etron_gt", 50005545, 20000095, true);

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/?returnAllPolicyRights=true", content);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Right> actualRights = JsonSerializer.Deserialize<List<Right>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightEqual);
        }

        private HttpClient GetTestClient()
        {
            HttpClient client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IPolicyRetrievalPoint, PolicyRetrievalPointMock>();
                    services.AddSingleton<IDelegationMetadataRepository, DelegationMetadataRepositoryMock>();
                    services.AddSingleton<IPolicyRepository, PolicyRepositoryMock>();
                    services.AddSingleton<IPartiesClient, PartiesClientMock>();
                    services.AddSingleton<IResourceRegistryClient, ResourceRegistryClientMock>();
                    services.AddSingleton<IAltinnRolesClient, AltinnRolesClientMock>();
                });
            }).CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            return client;
        }

        private static List<Right> GetExpectedRights(string resourceRegistryId, int fromPartyId, int toUserId, bool returnAllPolicyRights)
        {
            List<Right> rights = new();

            string rightsPath = GetRightsPath(resourceRegistryId, fromPartyId, toUserId, returnAllPolicyRights);
            if (File.Exists(rightsPath))
            {
                string content = File.ReadAllText(rightsPath);
                rights = (List<Right>)JsonSerializer.Deserialize(content, typeof(List<Right>), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            return rights;
        }

        private static string GetRightsPath(string resourceRegistryId, int fromPartyId, int toUserId, bool returnAllPolicyRights)
        {
            string? unitTestFolder = Path.GetDirectoryName(new Uri(typeof(RightsControllerTest).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, "Data", "Rights", $"{resourceRegistryId}", $"user_{toUserId}", $"party_{fromPartyId}", $"rights_returnall_{returnAllPolicyRights}.json");
        }
    }
}