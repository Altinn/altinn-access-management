using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Altinn.AccessManagement.Controllers;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Models;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.AccessManagement.Tests.Util;
using Altinn.AccessManagement.Tests.Utils;
using Altinn.Common.AccessToken.Services;
using Altinn.Common.PEP.Interfaces;
using AltinnCore.Authentication.JwtCookie;
using Microsoft.AspNetCore.Mvc;
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
            List<RightExternal> expectedRights = GetExpectedRights("jks_audi_etron_gt", "p50005545", "u20000095", false);
            StreamContent requestContent = GetRightsQueryRequestContent("jks_audi_etron_gt", "p50005545", "u20000095");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<RightExternal> actualRights = JsonSerializer.Deserialize<List<RightExternal>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightExternalEqual);
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
            List<RightExternal> expectedRights = GetExpectedRights("jks_audi_etron_gt", "p50005545", "u20000095", true);
            StreamContent requestContent = GetRightsQueryRequestContent("jks_audi_etron_gt", "p50005545", "u20000095");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/?returnAllPolicyRights=true", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<RightExternal> actualRights = JsonSerializer.Deserialize<List<RightExternal>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightExternalEqual);
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
            List<RightExternal> expectedRights = GetExpectedRights("jks_audi_etron_gt", "p50005545", "u20000490", false);
            StreamContent requestContent = GetRightsQueryRequestContent("jks_audi_etron_gt", "p50005545", "u20000490");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<RightExternal> actualRights = JsonSerializer.Deserialize<List<RightExternal>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightExternalEqual);
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
            List<RightExternal> expectedRights = GetExpectedRights("jks_audi_etron_gt", "p50005545", "u20000490", true);
            StreamContent requestContent = GetRightsQueryRequestContent("jks_audi_etron_gt", "p50005545", "u20000490");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/?returnAllPolicyRights=true", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<RightExternal> actualRights = JsonSerializer.Deserialize<List<RightExternal>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightExternalEqual);
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
            List<RightExternal> expectedRights = GetExpectedRights("digdirs_company_car", "p50005545", "u20001337", false);
            StreamContent requestContent = GetRightsQueryRequestContent("digdirs_company_car", "p50005545", "u20001337");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<RightExternal> actualRights = JsonSerializer.Deserialize<List<RightExternal>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightExternalEqual);
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
            List<RightExternal> expectedRights = GetExpectedRights("digdirs_company_car", "p50005545", "u20001337", true);
            StreamContent requestContent = GetRightsQueryRequestContent("digdirs_company_car", "p50005545", "u20001337");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/?returnAllPolicyRights=true", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<RightExternal> actualRights = JsonSerializer.Deserialize<List<RightExternal>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightExternalEqual);
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
            List<RightExternal> expectedRights = GetExpectedRights("jks_audi_etron_gt", "p50004221", "u20000490", false);
            StreamContent requestContent = GetRightsQueryRequestContent("jks_audi_etron_gt", "p50004221", "u20000490");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<RightExternal> actualRights = JsonSerializer.Deserialize<List<RightExternal>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightExternalEqual);
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
            List<RightExternal> expectedRights = GetExpectedRights("jks_audi_etron_gt", "p50004221", "u20000490", true);
            StreamContent requestContent = GetRightsQueryRequestContent("jks_audi_etron_gt", "p50004221", "u20000490");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/?returnAllPolicyRights=true", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<RightExternal> actualRights = JsonSerializer.Deserialize<List<RightExternal>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightExternalEqual);
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
            List<RightExternal> expectedRights = GetExpectedRights("org1_app1", "p50001337", "u20001337", false);
            StreamContent requestContent = GetRightsQueryRequestContent("org1_app1", "p50001337", "u20001337");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<RightExternal> actualRights = JsonSerializer.Deserialize<List<RightExternal>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightExternalEqual);
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
            List<RightExternal> expectedRights = GetExpectedRights("org1_app1", "p50001337", "u20001337", true);
            StreamContent requestContent = GetRightsQueryRequestContent("org1_app1", "p50001337", "u20001337");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/?returnAllPolicyRights=true", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<RightExternal> actualRights = JsonSerializer.Deserialize<List<RightExternal>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightExternalEqual);
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
            List<RightExternal> expectedRights = GetExpectedRights("ttd_rf-0002", "p50005545", "u20000490", false);
            StreamContent requestContent = GetRightsQueryRequestContent("ttd_rf-0002", "p50005545", "u20000490");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<RightExternal> actualRights = JsonSerializer.Deserialize<List<RightExternal>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightExternalEqual);
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
            List<RightExternal> expectedRights = GetExpectedRights("ttd_rf-0002", "p50005545", "u20000490", true);
            StreamContent requestContent = GetRightsQueryRequestContent("ttd_rf-0002", "p50005545", "u20000490");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/rights/?returnAllPolicyRights=true", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<RightExternal> actualRights = JsonSerializer.Deserialize<List<RightExternal>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightExternalEqual);
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
            List<RightExternal> expectedRights = GetExpectedDelegableRights("ttd_rf-0002", "p50005545", "u20000490", false);
            StreamContent requestContent = GetRightsQueryRequestContent("ttd_rf-0002", "p50005545", "u20000490");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/delegablerights/", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<RightExternal> actualRights = JsonSerializer.Deserialize<List<RightExternal>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightExternalEqual);
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
            List<RightExternal> expectedRights = GetExpectedDelegableRights("ttd_rf-0002", "p50005545", "u20000490", true);
            StreamContent requestContent = GetRightsQueryRequestContent("ttd_rf-0002", "p50005545", "u20000490");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/delegablerights/?returnAllPolicyRights=true", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<RightExternal> actualRights = JsonSerializer.Deserialize<List<RightExternal>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightExternalEqual);
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
            List<RightExternal> expectedRights = GetExpectedDelegableRights("jks_audi_etron_gt", "p50004221", "u20000490", false);
            StreamContent requestContent = GetRightsQueryRequestContent("jks_audi_etron_gt", "p50004221", "u20000490");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/delegablerights/", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<RightExternal> actualRights = JsonSerializer.Deserialize<List<RightExternal>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightExternalEqual);
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
            List<RightExternal> expectedRights = GetExpectedDelegableRights("jks_audi_etron_gt", "p50004221", "u20000490", true);
            StreamContent requestContent = GetRightsQueryRequestContent("jks_audi_etron_gt", "p50004221", "u20000490");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/delegablerights/?returnAllPolicyRights=true", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<RightExternal> actualRights = JsonSerializer.Deserialize<List<RightExternal>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightExternalEqual);
        }

        /// <summary>
        /// Test case: DelegableRightsQuery returns a list of rights the To userid 20001337 is able to delegate to others, for the From party 50005545 for the digdirs_company_car resource from the resource registry.
        ///            In this case:
        ///            - The user 20001337 is HADM for the From unit 50005545
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task DelegableRightsQuery_ResourceRight_HADM_ReturnAllPolicyRights_False()
        {
            // Arrange
            List<RightExternal> expectedRights = GetExpectedDelegableRights("digdirs_company_car", "p50005545", "u20001337", false);
            StreamContent requestContent = GetRightsQueryRequestContent("digdirs_company_car", "p50005545", "u20001337");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/delegablerights/", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<RightExternal> actualRights = JsonSerializer.Deserialize<List<RightExternal>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightExternalEqual);
        }

        /// <summary>
        /// Test case: DelegableRightsQuery returns a list of rights the To userid 20001337 is able to delegate to others, for the From party 50005545 for the digdirs_company_car resource from the resource registry.
        ///            In this case:
        ///            - The user 20001337 is HADM for the From unit 50005545
        ///            - The returnAllPolicyRights query param is set to True and operation should return all rights found in the resource registry XACML policy whether or not the user can delegate the right (which is indicated by the CanDelegate bool).
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task DelegableRightsQuery_ResourceRight_HADM_ReturnAllPolicyRights_True()
        {
            // Arrange
            List<RightExternal> expectedRights = GetExpectedDelegableRights("digdirs_company_car", "p50005545", "u20001337", true);
            StreamContent requestContent = GetRightsQueryRequestContent("digdirs_company_car", "p50005545", "u20001337");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/delegablerights/?returnAllPolicyRights=true", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<RightExternal> actualRights = JsonSerializer.Deserialize<List<RightExternal>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightExternalEqual);
        }

        /// <summary>
        /// Test case: DelegableRightsQuery returns a list of rights the To userid 20001337 is able to delegate to others, for the From party 50001337 for the Altinn App org1/app1.
        ///            In this case:
        ///            - The test scenario is setup using existing test data for Org1/App1, offeredBy 50001337 and coveredbyuser 20001337, where the delegation policy contains rules for resources not in the App policy:
        ///                 ("rightKey": "app1,org1,task1:sign" and "rightKey": "app1,org1,task1:write"). This should normally not happen but can become an real scenario where delegations have been made and then the resource/app policy is changed to remove some rights.
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task DelegableRightsQuery_AppRight_UserDelegation_ReturnAllPolicyRights_False()
        {
            // Arrange
            List<RightExternal> expectedRights = GetExpectedDelegableRights("org1_app1", "p50001337", "u20001337", false);
            StreamContent requestContent = GetRightsQueryRequestContent("org1_app1", "p50001337", "u20001337");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/delegablerights/", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<RightExternal> actualRights = JsonSerializer.Deserialize<List<RightExternal>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightExternalEqual);
        }

        /// <summary>
        /// Test case: DelegableRightsQuery returns a list of rights the To userid 20001337 is able to delegate to others, for the From party 50001337 for the Altinn App org1/app1.
        ///            In this case:
        ///            - The test scenario is setup using existing test data for Org1/App1, offeredBy 50001337 and coveredbyuser 20001337, where the delegation policy contains rules for resources not in the App policy:
        ///                 ("rightKey": "app1,org1,task1:sign" and "rightKey": "app1,org1,task1:write"). This should normally not happen but can become an real scenario where delegations have been made and then the resource/app policy is changed to remove some rights.
        ///            - The returnAllPolicyRights query param is set to True and operation should return all rights found in the resource registry XACML policy whether or not the user can delegate the right (which is indicated by the CanDelegate bool).
        /// Expected: GetRights returns a list of right matching expected
        /// </summary>
        [Fact]
        public async Task DelegableRightsQuery_AppRight_UserDelegation_ReturnAllPolicyRights_True()
        {
            // Arrange
            List<RightExternal> expectedRights = GetExpectedDelegableRights("org1_app1", "p50001337", "u20001337", true);
            StreamContent requestContent = GetRightsQueryRequestContent("org1_app1", "p50001337", "u20001337");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/internal/delegablerights/?returnAllPolicyRights=true", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<RightExternal> actualRights = JsonSerializer.Deserialize<List<RightExternal>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRights, actualRights, AssertionUtil.AssertRightExternalEqual);
        }

        /// <summary>
        /// Test case: DelegationCheck returns a list of rights the authenticated userid 20000490 is authorized to delegate on behalf of the reportee party 50005545 for the generic-access-resource from the resource registry.
        ///            In this case:
        ///            - The user 20000490 is DAGL for the From unit 50005545
        ///            - 5 out of 8 of the rights for the resource: generic-access-resource is delegable through having DAGL:
        ///                 - generic-access-resource:read
        ///                 - generic-access-resource,org-delegation-subtask:subunit-delegated-action
        ///                 - generic-access-resource,org-delegation-subtask:subunit-delegated-action-to-keyroleunit
        ///                 - generic-access-resource,org-delegation-subtask:mainunit-delegated-action
        ///                 - generic-access-resource,org-delegation-subtask:mainunit-delegated-action-to-keyroleunit
        /// Expected: DelegationCheck returns a list of RightDelegationStatus matching expected
        /// </summary>
        [Fact]
        public async Task DelegationCheck_GenericAccessResource_DAGL_HasDelegableRights()
        {
            // Arrange
            int userId = 20000490;
            int reporteePartyId = 50005545;
            string resourceId = "generic-access-resource";

            var token = PrincipalUtil.GetToken(userId, 0, 3);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            List<RightDelegationCheckResultExternal> expectedResponse = GetExpectedRightDelegationStatus($"u{userId}", $"p{reporteePartyId}", resourceId);
            StreamContent requestContent = GetDelegationCheckContent(resourceId);

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/{reporteePartyId}/rights/delegation/delegationcheck", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            List<RightDelegationCheckResultExternal> actualResponse = JsonSerializer.Deserialize<List<RightDelegationCheckResultExternal>>(responseContent, options);
            AssertionUtil.AssertCollections(expectedResponse, actualResponse, AssertionUtil.AssertRightDelegationStatusExternalEqual);
        }

        /// <summary>
        /// Test case: DelegationCheck returns a list of rights the authenticated userid 20000490 is authorized to delegate on behalf of itself (partyId 50002598) for the generic-access-resource from the resource registry.
        ///            In this case:
        ///            - The user 20000490 has PRIV role for itself (party 50002598)
        ///            - 4 out of 8 of the rights for the resource: generic-access-resource is delegable through having PRIV:
        ///                 - generic-access-resource:read
        ///                 - generic-access-resource:write
        ///                 - generic-access-resource,priv-delegation-subtask:delegated-action-to-user
        ///                 - generic-access-resource,priv-delegation-subtask:delegated-action-to-keyroleunit
        /// Expected: DelegationCheck returns a list of RightDelegationStatus matching expected
        /// </summary>
        [Fact]
        public async Task DelegationCheck_GenericAccessResource_PRIV_HasDelegableRights()
        {
            // Arrange
            int userId = 20000490;
            int reporteePartyId = 50002598;
            string resourceId = "generic-access-resource";

            var token = PrincipalUtil.GetToken(userId, 0, 3);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            List<RightDelegationCheckResultExternal> expectedResponse = GetExpectedRightDelegationStatus($"u{userId}", $"p{reporteePartyId}", resourceId);
            StreamContent requestContent = GetDelegationCheckContent(resourceId);

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/{reporteePartyId}/rights/delegation/delegationcheck", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            List<RightDelegationCheckResultExternal> actualResponse = JsonSerializer.Deserialize<List<RightDelegationCheckResultExternal>>(responseContent, options);
            AssertionUtil.AssertCollections(expectedResponse, actualResponse, AssertionUtil.AssertRightDelegationStatusExternalEqual);
        }

        /// <summary>
        /// Test case: DelegationCheck returns a list of rights the authenticated userid 20001337 is authorized to delegate on behalf of the reportee party 50005545 for the generic-access-resource from the resource registry.
        ///            In this case:
        ///            - The user 20001337 is HADM for the From unit 50005545
        ///            - 5 out of 8 of the rights for the resource: generic-access-resource is delegable through having HADM (which inheirits same rights for delegation as DAGL):
        ///                 - generic-access-resource:read
        ///                 - generic-access-resource,org-delegation-subtask:subunit-delegated-action
        ///                 - generic-access-resource,org-delegation-subtask:subunit-delegated-action-to-keyroleunit
        ///                 - generic-access-resource,org-delegation-subtask:mainunit-delegated-action
        ///                 - generic-access-resource,org-delegation-subtask:mainunit-delegated-action-to-keyroleunit
        /// Expected: DelegationCheck returns a list of RightDelegationStatus matching expected
        /// </summary>
        [Fact]
        public async Task DelegationCheck_GenericAccessResource_HADM_HasDelegableRights()
        {
            // Arrange
            int userId = 20001337;
            int reporteePartyId = 50005545;
            string resourceId = "generic-access-resource";

            var token = PrincipalUtil.GetToken(userId, 0, 3);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            List<RightDelegationCheckResultExternal> expectedResponse = GetExpectedRightDelegationStatus($"u{userId}", $"p{reporteePartyId}", resourceId);
            StreamContent requestContent = GetDelegationCheckContent(resourceId);

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/{reporteePartyId}/rights/delegation/delegationcheck", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            List<RightDelegationCheckResultExternal> actualResponse = JsonSerializer.Deserialize<List<RightDelegationCheckResultExternal>>(responseContent, options);
            AssertionUtil.AssertCollections(expectedResponse, actualResponse, AssertionUtil.AssertRightDelegationStatusExternalEqual);
        }

        /// <summary>
        /// Test case: DelegationCheck returns a list of rights the authenticated 20000490 is authorized to delegate on behalf of the reportee party 50004221 for the generic-access-resource from the resource registry.
        ///            In this case:
        ///            - The From unit (50004221) is a subunit of 500042222.
        ///            - The From unit (50004221) has delegated the "subunit-delegated-action" action directly to the user.
        ///            - The From unit (50004221) has delegated the "subunit-delegated-action-to-keyunit" action directly to the user.
        ///            - The main unit (50004222) has delegated the "mainunit-delegated-action" action to the user.
        ///            - The main unit (50004222) has delegated the "mainunit-delegated-action-to-keyunit" action to the party 50005545 where the user is DAGL and have keyrole privileges.
        ///            - 4 out of 8 rights are thus delegable and should contain the information of the actual recipient of the delegation
        /// Expected: DelegationCheck returns a list of RightDelegationStatus matching expected
        /// </summary>
        [Fact]
        public async Task DelegationCheck_GenericAccessResource_SubUnitToUserDelegation_SubUnitToKeyRoleUnitDelegation_MainUnitToUserDelegation_MainUnitToKeyRoleUnitDelegation_HasDelegableRights()
        {
            // Arrange
            int userId = 20000490;
            int reporteePartyId = 50004221;
            string resourceId = "generic-access-resource";

            var token = PrincipalUtil.GetToken(userId, 0, 3);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            List<RightDelegationCheckResultExternal> expectedResponse = GetExpectedRightDelegationStatus($"u{userId}", $"p{reporteePartyId}", resourceId);
            StreamContent requestContent = GetDelegationCheckContent(resourceId);

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/{reporteePartyId}/rights/delegation/delegationcheck", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            List<RightDelegationCheckResultExternal> actualResponse = JsonSerializer.Deserialize<List<RightDelegationCheckResultExternal>>(responseContent, options);
            AssertionUtil.AssertCollections(expectedResponse, actualResponse, AssertionUtil.AssertRightDelegationStatusExternalEqual);
        }

        /// <summary>
        /// Test case: DelegationCheck returns a list of rights the authenticated 20001337 is authorized to delegate on behalf of the reportee party 50001337 for the org1_app1 Altinn App.
        ///            In this case:
        ///            - The test scenario is setup using existing test data for Org1/App1, offeredBy 50001337 and coveredbyuser 20001337, where the delegation policy contains rules for resources not in the App policy:
        ///                 ("rightKey": "app1,org1,task1:sign" and "rightKey": "app1,org1,task1:write"). This should normally not happen but can become an real scenario where delegations have been made and then the resource/app policy is changed to remove some rights.
        /// Expected: DelegationCheck returns a list of RightDelegationStatus matching expected
        /// </summary>
        [Fact]
        public async Task DelegationCheck_AppRight_UserDelegation_HasDelegableRights()
        {
            // Arrange
            int userId = 20001337;
            int reporteePartyId = 50001337;
            string resourceId = "org1_app1";

            var token = PrincipalUtil.GetToken(userId, 0, 4);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            List<RightDelegationCheckResultExternal> expectedResponse = GetExpectedRightDelegationStatus($"u{userId}", $"p{reporteePartyId}", resourceId);
            StreamContent requestContent = GetDelegationCheckContent(resourceId);

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/{reporteePartyId}/rights/delegation/delegationcheck", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            List<RightDelegationCheckResultExternal> actualResponse = JsonSerializer.Deserialize<List<RightDelegationCheckResultExternal>>(responseContent, options);
            AssertionUtil.AssertCollections(expectedResponse, actualResponse, AssertionUtil.AssertRightDelegationStatusExternalEqual);
        }

        /// <summary>
        /// Test case: DelegationCheck returns a list of rights the authenticated 20001337 is authorized to delegate on behalf of the reportee party 50001337 for the 1337_1338 Altinn 2 service.
        ///            In this case:
        ///            - The test scenario is setup using existing test data for 1337/1338, offeredBy 50001337 and coveredbyuser 20001337, where the delegation policy contains rules for resources not in the App policy:
        ///                 ("rightKey": "1338,1337,task1:sign" and "rightKey": "1338,1337,task1:write"). This should normally not happen but can become an real scenario where delegations have been made and then the resource/app policy is changed to remove some rights.
        /// Expected: DelegationCheck returns a list of RightDelegationStatus matching expected
        /// </summary>
        [Fact]
        public async Task DelegationCheck_ServiceRight_UserDelegation_HasDelegableRights()
        {
            // Arrange
            int userId = 20001337;
            int reporteePartyId = 50001337;
            string resourceId = "se_1337_1338";

            var token = PrincipalUtil.GetToken(userId, 0, 4);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            List<RightDelegationCheckResultExternal> expectedResponse = GetExpectedRightDelegationStatus($"u{userId}", $"p{reporteePartyId}", resourceId);
            StreamContent requestContent = GetDelegationCheckContent(resourceId);

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/{reporteePartyId}/rights/delegation/delegationcheck", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            List<RightDelegationCheckResultExternal> actualResponse = JsonSerializer.Deserialize<List<RightDelegationCheckResultExternal>>(responseContent, options);
            AssertionUtil.AssertCollections(expectedResponse, actualResponse, AssertionUtil.AssertRightDelegationStatusExternalEqual);
        }

        /// <summary>
        /// Test case: DelegationCheck returns a list of rights the authenticated 20001337 is authorized to delegate on behalf of the reportee party 50001337 for the invalid resource non_existing_id
        ///            In this case:
        ///            - Since the resource is invalid a BadRequest response with a ValidationProblemDetails model response should be returned
        /// Expected: Responce error model is matching expected
        /// </summary>
        [Fact]
        public async Task DelegationCheck_InvalidResource_BadRequest()
        {
            // Arrange
            int userId = 20001337;
            int reporteePartyId = 50001337;
            string resourceId = "non_existing_id";

            var token = PrincipalUtil.GetToken(userId, 0, 4);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            ValidationProblemDetails expectedResponse = GetExpectedValidationError("DelegationCheck", $"u{userId}", $"p{reporteePartyId}", resourceId);
            StreamContent requestContent = GetDelegationCheckContent(resourceId);

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/{reporteePartyId}/rights/delegation/delegationcheck", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            ValidationProblemDetails actualResponse = JsonSerializer.Deserialize<ValidationProblemDetails>(responseContent, options);
            AssertionUtil.AssertValidationProblemDetailsEqual(expectedResponse, actualResponse);
        }

        /// <summary>
        /// Test case: DelegationCheck when the authenticated 20001337 is authorized to delegate on behalf of the reportee party 50001337 for the MaskinportenSchema resource jks_audi_etron_gt
        ///            In this case:
        ///            - Since the resource is a MaskinportenSchema a BadRequest response with a ValidationProblemDetails model response should be returned
        /// Expected: Responce error model is matching expected
        /// </summary>
        [Fact]
        public async Task DelegationCheck_MaskinportenSchema_BadRequest()
        {
            // Arrange
            int userId = 20001337;
            int reporteePartyId = 50001337;
            string resourceId = "jks_audi_etron_gt";

            var token = PrincipalUtil.GetToken(userId, 0, 4);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            ValidationProblemDetails expectedResponse = GetExpectedValidationError("DelegationCheck", $"u{userId}", $"p{reporteePartyId}", resourceId);
            StreamContent requestContent = GetDelegationCheckContent(resourceId);

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/{reporteePartyId}/rights/delegation/delegationcheck", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            ValidationProblemDetails actualResponse = JsonSerializer.Deserialize<ValidationProblemDetails>(responseContent, options);
            AssertionUtil.AssertValidationProblemDetailsEqual(expectedResponse, actualResponse);
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
                    services.AddSingleton<IPublicSigningKeyProvider, SigningKeyResolverMock>();
                    services.AddSingleton<IPartiesClient, PartiesClientMock>();
                    services.AddSingleton<IResourceRegistryClient, ResourceRegistryClientMock>();
                    services.AddSingleton<IAltinnRolesClient, AltinnRolesClientMock>();
                    services.AddSingleton<IPDP, PdpPermitMock>();
                    services.AddSingleton<IAltinn2RightsClient, Altinn2RightsClientMock>();
                });
            }).CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            return client;
        }

        private static List<RightExternal> GetExpectedRights(string resourceId, string from, string to, bool returnAllPolicyRights)
        {
            string rightsPath = $"Data/Json/RightsQuery/{resourceId}/from_{from}/to_{to}/expected_rights_returnall_{returnAllPolicyRights.ToString().ToLower()}.json";
            string content = File.ReadAllText(rightsPath);
            return (List<RightExternal>)JsonSerializer.Deserialize(content, typeof(List<RightExternal>), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private static List<RightExternal> GetExpectedDelegableRights(string resourceId, string from, string to, bool returnAllPolicyRights)
        {
            string rightsPath = $"Data/Json/DelegableRightsQuery/{resourceId}/from_{from}/to_{to}/expected_rights_returnall_{returnAllPolicyRights.ToString().ToLower()}.json";
            string content = File.ReadAllText(rightsPath);
            return (List<RightExternal>)JsonSerializer.Deserialize(content, typeof(List<RightExternal>), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private static List<RightDelegationCheckResultExternal> GetExpectedRightDelegationStatus(string user, string from, string resourceId)
        {
            string content = File.ReadAllText($"Data/Json/DelegationCheck/{resourceId}/from_{from}/authn_{user}.json");
            return (List<RightDelegationCheckResultExternal>)JsonSerializer.Deserialize(content, typeof(List<RightDelegationCheckResultExternal>), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private static ValidationProblemDetails GetExpectedValidationError(string operation, string user, string from, string resourceId)
        {
            string content = File.ReadAllText($"Data/Json/{operation}/{resourceId}/from_{from}/authn_{user}.json");
            return (ValidationProblemDetails)JsonSerializer.Deserialize(content, typeof(ValidationProblemDetails), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private static StreamContent GetRightsQueryRequestContent(string resourceId, string from, string to)
        {
            Stream dataStream = File.OpenRead($"Data/Json/RightsQuery/{resourceId}/from_{from}/to_{to}/RightsQuery.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return content;
        }

        private static StreamContent GetDelegationCheckContent(string resourceId)
        {
            Stream dataStream = File.OpenRead($"Data/Json/DelegationCheck/{resourceId}/request.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return content;
        }
    }
}
