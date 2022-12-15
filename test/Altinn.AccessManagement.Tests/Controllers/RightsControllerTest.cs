using System;
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
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.AccessManagement.Tests.Util;
using Altinn.AccessManagement.Tests.Utils;
using AltinnCore.Authentication.JwtCookie;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Altinn.AccessManagement.Tests.Controllers
{
    /// <summary>
    /// Test class for <see cref="RightsController"></see>
    /// </summary>
    [Collection("RightsController Tests")]
    public class RightsControllerTest : IClassFixture<CustomWebApplicationFactory<RightsController>>
    {
        private readonly CustomWebApplicationFactory<RightsController> _factory;
        private readonly HttpClient _client;

        private readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        /// <summary>
        /// Constructor setting up factory, test client and dependencies
        /// </summary>
        /// <param name="factory">CustomWebApplicationFactory</param>
        public RightsControllerTest(CustomWebApplicationFactory<RightsController> factory)
        {
            _factory = factory;
            _client = GetTestClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string token = PrincipalUtil.GetAccessToken("sbl.authorization");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Test case: RightsQuery returns a list of rights the To userid 20000095 have for the From party 50005545 for the jks_audi_etron_gt resource from the resource registry.
        ///            In this case:
        ///            - The From unit (50005545) has delegated the "Park" action directly to the user.
        ///            - The From unit (50005545) has delegated the "Drive" action to the party 50005545 where the user is DAGL and have keyrole privileges.
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task RightsQuery_ResourceRight_UserDelegation_KeyRoleUnitDelegation_ReturnAllPolicyRights_False()
        {
            // Arrange
            List<Right> expectedRights = GetExpectedRights("jks_audi_etron_gt", "p50005545", "u20000095", false);
            StreamContent requestContent = GetRequestContent("jks_audi_etron_gt", "p50005545", "u20000095");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Right> actualRights = JsonSerializer.Deserialize<List<Right>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightEqual);
        }

        /// <summary>
        /// Test case: RightsQuery returns a list of rights the To userid 20000095 have for the From party 50005545 for the jks_audi_etron_gt resource from the resource registry.
        ///            In this case:
        ///            - The From unit (50005545) has delegated the "Park" action directly to the user.
        ///            - The From unit (50005545) has delegated the "Drive" action to the party 50005545 where the user is DAGL and have keyrole privileges.
        ///            - The returnAllPolicyRights query param is set to True and operation should return all rights found in the resource registry XACML policy whether or not the user has Permit for the rights.
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task RightsQuery_ResourceRight_UserDelegation_KeyRoleUnitDelegation_ReturnAllPolicyRights_True()
        {
            // Arrange
            List<Right> expectedRights = GetExpectedRights("jks_audi_etron_gt", "p50005545", "u20000095", true);
            StreamContent requestContent = GetRequestContent("jks_audi_etron_gt", "p50005545", "u20000095");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/?returnAllPolicyRights=true", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Right> actualRights = JsonSerializer.Deserialize<List<Right>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightEqual);
        }

        /// <summary>
        /// Test case: RightsQuery returns a list of rights the To userid 20000490 have for the From party 50005545 for the jks_audi_etron_gt resource from the resource registry.
        ///            In this case:
        ///            - The user 20000490 is DAGL for the From unit 50005545
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task RightsQuery_ResourceRight_DAGL_ReturnAllPolicyRights_False()
        {
            // Arrange
            List<Right> expectedRights = GetExpectedRights("jks_audi_etron_gt", "p50005545", "u20000490", false);
            StreamContent requestContent = GetRequestContent("jks_audi_etron_gt", "p50005545", "u20000490");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Right> actualRights = JsonSerializer.Deserialize<List<Right>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightEqual);
        }

        /// <summary>
        /// Test case: RightsQuery returns a list of rights the To userid 20000490 have for the From party 50005545 for the jks_audi_etron_gt resource from the resource registry.
        ///            In this case:
        ///            - The user 20000490 is DAGL for the From unit 50005545
        ///            - The returnAllPolicyRights query param is set to True and operation should return all rights found in the resource registry XACML policy whether or not the user has Permit for the rights.
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task RightsQuery_ResourceRight_DAGL_ReturnAllPolicyRights_True()
        {
            // Arrange
            List<Right> expectedRights = GetExpectedRights("jks_audi_etron_gt", "p50005545", "u20000490", true);
            StreamContent requestContent = GetRequestContent("jks_audi_etron_gt", "p50005545", "u20000490");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/?returnAllPolicyRights=true", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Right> actualRights = JsonSerializer.Deserialize<List<Right>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightEqual);
        }

        /// <summary>
        /// Test case: RightsQuery returns a list of rights the To userid 20001337 have for the From party 50005545 for the digdirs_company_car resource from the resource registry.
        ///            In this case:
        ///            - The user 20001337 is HADM for the From unit 50005545
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task RightsQuery_ResourceRight_HADM_ReturnAllPolicyRights_False()
        {
            // Arrange
            List<Right> expectedRights = GetExpectedRights("digdirs_company_car", "p50005545", "u20001337", false);
            StreamContent requestContent = GetRequestContent("digdirs_company_car", "p50005545", "u20001337");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Right> actualRights = JsonSerializer.Deserialize<List<Right>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightEqual);
        }

        /// <summary>
        /// Test case: RightsQuery returns a list of rights the To userid 20001337 have for the From party 50005545 for the digdirs_company_car resource from the resource registry.
        ///            In this case:
        ///            - The user 20001337 is HADM for the From unit 50005545
        ///            - The returnAllPolicyRights query param is set to True and operation should return all rights found in the resource registry XACML policy whether or not the user has Permit for the rights.
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task RightsQuery_ResourceRight_HADM_ReturnAllPolicyRights_True()
        {
            // Arrange
            List<Right> expectedRights = GetExpectedRights("digdirs_company_car", "p50005545", "u20001337", true);
            StreamContent requestContent = GetRequestContent("digdirs_company_car", "p50005545", "u20001337");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/?returnAllPolicyRights=true", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Right> actualRights = JsonSerializer.Deserialize<List<Right>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightEqual);
        }

        /// <summary>
        /// Test case: RightsQuery returns a list of rights the To userid 20000490 have for the From party 50004221 for the jks_audi_etron_gt resource from the resource registry.
        ///            In this case:
        ///            - The From unit (50004221) is a subunit of 500042222.
        ///            - The From unit (50004221) has delegated the "Race" action directly to the user.
        ///            - The main unit (50004222) has delegated the "Park" action to the user.
        ///            - The main unit (50004222) has delegated the "Drive" action to the party 50005545 where the user is DAGL and have keyrole privileges.
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task RightsQuery_ResourceRight_UserDelegation_MainUnitToUserDelegation_MainUnitToKeyRoleDelegation_ReturnAllPolicyRights_False()
        {
            // Arrange
            List<Right> expectedRights = GetExpectedRights("jks_audi_etron_gt", "p50004221", "u20000490", false);
            StreamContent requestContent = GetRequestContent("jks_audi_etron_gt", "p50004221", "u20000490");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Right> actualRights = JsonSerializer.Deserialize<List<Right>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightEqual);
        }

        /// <summary>
        /// Test case: RightsQuery returns a list of rights the To userid 20000490 have for the From party 50004221 for the jks_audi_etron_gt resource from the resource registry.
        ///            In this case:
        ///            - The From unit (50004221) is a subunit of 500042222.
        ///            - The From unit (50004221) has delegated the "Race" action directly to the user.
        ///            - The main unit (50004222) has delegated the "Park" action to the user.
        ///            - The main unit (50004222) has delegated the "Drive" action to the party 50005545 where the user is DAGL and have keyrole privileges.
        ///            - The returnAllPolicyRights query param is set to True and operation should return all rights found in the resource registry XACML policy whether or not the user has Permit for the rights.
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task RightsQuery_ResourceRight_UserDelegation_MainUnitToUserDelegation_MainUnitToKeyRoleDelegation_ReturnAllPolicyRights_True()
        {
            // Arrange
            List<Right> expectedRights = GetExpectedRights("jks_audi_etron_gt", "p50004221", "u20000490", true);
            StreamContent requestContent = GetRequestContent("jks_audi_etron_gt", "p50004221", "u20000490");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/?returnAllPolicyRights=true", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Right> actualRights = JsonSerializer.Deserialize<List<Right>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightEqual);
        }

        /// <summary>
        /// Test case: RightsQuery returns a list of rights the To userid 20001337 have for the From party 50001337 for the Altinn App org1/app1.
        ///            In this case:
        ///            - The test scenario is setup using existing test data for Org1/App1, offeredBy 50001337 and coveredbyuser 20001337, where the delegation policy contains rules for resources not in the App policy:
        ///                 ("rightKey": "app1,org1,task1:sign" and "rightKey": "app1,org1,task1:write"). This should normally not happen but can become an real scenario where delegations have been made and then the resource/app policy is changed to remove some rights.
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task RightsQuery_AppRight_UserDelegation_ReturnAllPolicyRights_False()
        {
            // Arrange
            List<Right> expectedRights = GetExpectedRights("org1_app1", "p50001337", "u20001337", false);
            StreamContent requestContent = GetRequestContent("org1_app1", "p50001337", "u20001337");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Right> actualRights = JsonSerializer.Deserialize<List<Right>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightEqual);
        }

        /// <summary>
        /// Test case: RightsQuery returns a list of rights the To userid 20001337 have for the From party 50001337 for the Altinn App org1/app1.
        ///            In this case:
        ///            - The test scenario is setup using existing test data for Org1/App1, offeredBy 50001337 and coveredbyuser 20001337, where the delegation policy contains rules for resources not in the App policy:
        ///                 ("rightKey": "app1,org1,task1:sign" and "rightKey": "app1,org1,task1:write"). This should normally not happen but can become an real scenario where delegations have been made and then the resource/app policy is changed to remove some rights.
        ///            - The returnAllPolicyRights query param is set to True and operation should return all rights found in the resource registry XACML policy whether or not the user has Permit for the rights.
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task RightsQuery_AppRight_UserDelegation_ReturnAllPolicyRights_True()
        {
            // Arrange
            List<Right> expectedRights = GetExpectedRights("org1_app1", "p50001337", "u20001337", true);
            StreamContent requestContent = GetRequestContent("org1_app1", "p50001337", "u20001337");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/?returnAllPolicyRights=true", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Right> actualRights = JsonSerializer.Deserialize<List<Right>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightEqual);
        }

        /// <summary>
        /// Test case: RightsQuery returns a list of rights the To userid 20000490 have for the From party 50005545 for the Altinn App ttd/rf-0002.
        ///            In this case:
        ///            - The user 20000490 is DAGL for the From unit 50005545
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task RightsQuery_AppRight_DAGL_ReturnAllPolicyRights_False()
        {
            // Arrange
            List<Right> expectedRights = GetExpectedRights("ttd_rf-0002", "p50005545", "u20000490", false);
            StreamContent requestContent = GetRequestContent("ttd_rf-0002", "p50005545", "u20000490");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Right> actualRights = JsonSerializer.Deserialize<List<Right>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightEqual);
        }

        /// <summary>
        /// Test case: RightsQuery returns a list of rights the To userid 20000490 have for the From party 50005545 for the Altinn App ttd/rf-0002.
        ///            In this case:
        ///            - The user 20000490 is DAGL for the From unit 50005545
        ///            - The returnAllPolicyRights query param is set to True and operation should return all rights found in the resource registry XACML policy whether or not the user has Permit for the rights.
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task RightsQuery_AppRight_DAGL_ReturnAllPolicyRights_True()
        {
            // Arrange
            List<Right> expectedRights = GetExpectedRights("ttd_rf-0002", "p50005545", "u20000490", true);
            StreamContent requestContent = GetRequestContent("ttd_rf-0002", "p50005545", "u20000490");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/?returnAllPolicyRights=true", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Right> actualRights = JsonSerializer.Deserialize<List<Right>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightEqual);
        }

        /// <summary>
        /// Test case: DelegableRightsQuery returns a list of rights the To userid 20000490 is able to delegate to others, for the From party 50005545 for the Altinn App ttd/rf-0002.
        ///            In this case:
        ///            - The user 20000490 is DAGL for the From unit 50005545
        /// Expected: GetDelegableRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task DelegableRightsQuery_AppRight_DAGL_ReturnAllPolicyRights_False()
        {
            // Arrange
            List<Right> expectedRights = GetExpectedDelegableRights("ttd_rf-0002", "p50005545", "u20000490", false);
            StreamContent requestContent = GetRequestContent("ttd_rf-0002", "p50005545", "u20000490");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/delegablerights/", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Right> actualRights = JsonSerializer.Deserialize<List<Right>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightEqual);
        }

        /// <summary>
        /// Test case: DelegableRightsQuery returns a list of rights the To userid 20000490 is able to delegate to others, for the From party 50005545 for the Altinn App ttd/rf-0002.
        ///            In this case:
        ///            - The user 20000490 is DAGL for the From unit 50005545
        ///            - The returnAllPolicyRights query param is set to True and operation should return all rights found in the resource registry XACML policy whether or not the user can delegate the right (which is indicated by the CanDelegate bool).
        /// Expected: GetDelegableRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task DelegableRightsQuery_AppRight_DAGL_ReturnAllPolicyRights_True()
        {
            // Arrange
            List<Right> expectedRights = GetExpectedDelegableRights("ttd_rf-0002", "p50005545", "u20000490", true);
            StreamContent requestContent = GetRequestContent("ttd_rf-0002", "p50005545", "u20000490");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/delegablerights/?returnAllPolicyRights=true", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Right> actualRights = JsonSerializer.Deserialize<List<Right>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightEqual);
        }

        /// <summary>
        /// Test case: DelegableRightsQuery returns a list of rights the To userid 20000490 is able to delegate to others, for the From party 50004221 for the jks_audi_etron_gt resource from the resource registry.
        ///            In this case:
        ///            - The From unit (50004221) is a subunit of 500042222.
        ///            - The From unit (50004221) has delegated the "Race" action directly to the user.
        ///            - The main unit (50004222) has delegated the "Park" action to the user.
        ///            - The main unit (50004222) has delegated the "Drive" action to the party 50005545 where the user is DAGL and have keyrole privileges.
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task DelegableRightsQuery_ResourceRight_UserDelegation_MainUnitToUserDelegation_MainUnitToKeyRoleDelegation_ReturnAllPolicyRights_False()
        {
            // Arrange
            List<Right> expectedRights = GetExpectedDelegableRights("jks_audi_etron_gt", "p50004221", "u20000490", false);
            StreamContent requestContent = GetRequestContent("jks_audi_etron_gt", "p50004221", "u20000490");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/delegablerights/", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Right> actualRights = JsonSerializer.Deserialize<List<Right>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightEqual);
        }

        /// <summary>
        /// Test case: DelegableRightsQuery returns a list of rights the To userid 20000490 is able to delegate to others, for the From party 50004221 for the jks_audi_etron_gt resource from the resource registry.
        ///            In this case:
        ///            - The From unit (50004221) is a subunit of 500042222.
        ///            - The From unit (50004221) has delegated the "Race" action directly to the user.
        ///            - The main unit (50004222) has delegated the "Park" action to the user.
        ///            - The main unit (50004222) has delegated the "Drive" action to the party 50005545 where the user is DAGL and have keyrole privileges.
        ///            - The returnAllPolicyRights query param is set to True and operation should return all rights found in the resource registry XACML policy whether or not the user can delegate the right (which is indicated by the CanDelegate bool).
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task DelegableRightsQuery_ResourceRight_UserDelegation_MainUnitToUserDelegation_MainUnitToKeyRoleDelegation_ReturnAllPolicyRights_True()
        {
            // Arrange
            List<Right> expectedRights = GetExpectedDelegableRights("jks_audi_etron_gt", "p50004221", "u20000490", true);
            StreamContent requestContent = GetRequestContent("jks_audi_etron_gt", "p50004221", "u20000490");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/delegablerights/?returnAllPolicyRights=true", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Right> actualRights = JsonSerializer.Deserialize<List<Right>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightEqual);
        }

        /// <summary>
        /// Test case: DelegableRightsQuery returns a list of rights the To userid 20001337 have for the From party 50005545 for the digdirs_company_car resource from the resource registry.
        ///            In this case:
        ///            - The user 20001337 is HADM for the From unit 50005545
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task DelegableRightsQuery_ResourceRight_HADM_ReturnAllPolicyRights_False()
        {
            // Arrange
            List<Right> expectedRights = GetExpectedDelegableRights("digdirs_company_car", "p50005545", "u20001337", false);
            StreamContent requestContent = GetRequestContent("digdirs_company_car", "p50005545", "u20001337");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/delegablerights/", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Right> actualRights = JsonSerializer.Deserialize<List<Right>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightEqual);
        }

        /// <summary>
        /// Test case: DelegableRightsQuery returns a list of rights the To userid 20001337 have for the From party 50005545 for the digdirs_company_car resource from the resource registry.
        ///            In this case:
        ///            - The user 20001337 is HADM for the From unit 50005545
        ///            - The returnAllPolicyRights query param is set to True and operation should return all rights found in the resource registry XACML policy whether or not the user has Permit for the rights.
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task DelegableRightsQuery_ResourceRight_HADM_ReturnAllPolicyRights_True()
        {
            // Arrange
            List<Right> expectedRights = GetExpectedDelegableRights("digdirs_company_car", "p50005545", "u20001337", true);
            StreamContent requestContent = GetRequestContent("digdirs_company_car", "p50005545", "u20001337");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/delegablerights/?returnAllPolicyRights=true", requestContent);
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
                    services.AddSingleton<IPostConfigureOptions<JwtCookieOptions>, JwtCookiePostConfigureOptionsStub>();
                    services.AddSingleton<IPartiesClient, PartiesClientMock>();
                    services.AddSingleton<IResourceRegistryClient, ResourceRegistryClientMock>();
                    services.AddSingleton<IAltinnRolesClient, AltinnRolesClientMock>();
                });
            }).CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            return client;
        }

        private static List<Right> GetExpectedRights(string resourceId, string from, string to, bool returnAllPolicyRights)
        {
            string rightsPath = $"Data/Json/RightsQuery/{resourceId}/from_{from}/to_{to}/expected_rights_returnall_{returnAllPolicyRights.ToString().ToLower()}.json";
            string content = File.ReadAllText(rightsPath);
            return (List<Right>)JsonSerializer.Deserialize(content, typeof(List<Right>), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private static List<Right> GetExpectedDelegableRights(string resourceId, string from, string to, bool returnAllPolicyRights)
        {
            string rightsPath = $"Data/Json/DelegableRightsQuery/{resourceId}/from_{from}/to_{to}/expected_rights_returnall_{returnAllPolicyRights.ToString().ToLower()}.json";
            string content = File.ReadAllText(rightsPath);
            return (List<Right>)JsonSerializer.Deserialize(content, typeof(List<Right>), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private static StreamContent GetRequestContent(string resourceId, string from, string to)
        {
            Stream dataStream = File.OpenRead($"Data/Json/RightsQuery/{resourceId}/from_{from}/to_{to}/RightsQuery.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return content;
        }
    }
}
