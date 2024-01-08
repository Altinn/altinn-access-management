﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Altinn.AccessManagement.Models;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.AccessManagement.Tests.Util;
using Altinn.AccessManagement.Tests.Utils;
using Altinn.AccessManagement.Utilities;
using Altinn.Common.AccessToken.Services;
using Altinn.Common.PEP.Interfaces;
using AltinnCore.Authentication.JwtCookie;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
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
        private readonly string sblInternalToken = PrincipalUtil.GetAccessToken("sbl.authorization");

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
            HttpResponseMessage response = await GetTestClient(sblInternalToken).PostAsync($"accessmanagement/api/v1/internal/rights/", requestContent);
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
            HttpResponseMessage response = await GetTestClient(sblInternalToken).PostAsync($"accessmanagement/api/v1/internal/rights/?returnAllPolicyRights=true", requestContent);
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
            HttpResponseMessage response = await GetTestClient(sblInternalToken).PostAsync($"accessmanagement/api/v1/internal/rights/", requestContent);
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
            HttpResponseMessage response = await GetTestClient(sblInternalToken).PostAsync($"accessmanagement/api/v1/internal/rights/?returnAllPolicyRights=true", requestContent);
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
            HttpResponseMessage response = await GetTestClient(sblInternalToken).PostAsync($"accessmanagement/api/v1/internal/rights/", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            List<RightExternal> actualRights = JsonSerializer.Deserialize<List<RightExternal>>(responseContent, options);
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
            HttpResponseMessage response = await GetTestClient(sblInternalToken).PostAsync($"accessmanagement/api/v1/internal/rights/?returnAllPolicyRights=true", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            List<RightExternal> actualRights = JsonSerializer.Deserialize<List<RightExternal>>(responseContent, options);
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
            HttpResponseMessage response = await GetTestClient(sblInternalToken).PostAsync($"accessmanagement/api/v1/internal/rights/", requestContent);
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
            HttpResponseMessage response = await GetTestClient(sblInternalToken).PostAsync($"accessmanagement/api/v1/internal/rights/?returnAllPolicyRights=true", requestContent);
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
            HttpResponseMessage response = await GetTestClient(sblInternalToken).PostAsync($"accessmanagement/api/v1/internal/rights/", requestContent);
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
            HttpResponseMessage response = await GetTestClient(sblInternalToken).PostAsync($"accessmanagement/api/v1/internal/rights/?returnAllPolicyRights=true", requestContent);
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
            HttpResponseMessage response = await GetTestClient(sblInternalToken).PostAsync($"accessmanagement/api/v1/internal/rights/", requestContent);
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
            HttpResponseMessage response = await GetTestClient(sblInternalToken).PostAsync($"accessmanagement/api/v1/internal/rights/?returnAllPolicyRights=true", requestContent);
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
            HttpResponseMessage response = await GetTestClient(sblInternalToken).PostAsync($"accessmanagement/api/v1/internal/delegablerights/", requestContent);
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
            HttpResponseMessage response = await GetTestClient(sblInternalToken).PostAsync($"accessmanagement/api/v1/internal/delegablerights/?returnAllPolicyRights=true", requestContent);
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
            HttpResponseMessage response = await GetTestClient(sblInternalToken).PostAsync($"accessmanagement/api/v1/internal/delegablerights/", requestContent);
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
            HttpResponseMessage response = await GetTestClient(sblInternalToken).PostAsync($"accessmanagement/api/v1/internal/delegablerights/?returnAllPolicyRights=true", requestContent);
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
            HttpResponseMessage response = await GetTestClient(sblInternalToken).PostAsync($"accessmanagement/api/v1/internal/delegablerights/", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            List<RightExternal> actualRights = JsonSerializer.Deserialize<List<RightExternal>>(responseContent, options);
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
            HttpResponseMessage response = await GetTestClient(sblInternalToken).PostAsync($"accessmanagement/api/v1/internal/delegablerights/?returnAllPolicyRights=true", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            List<RightExternal> actualRights = JsonSerializer.Deserialize<List<RightExternal>>(responseContent, options);
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
            HttpResponseMessage response = await GetTestClient(sblInternalToken).PostAsync($"accessmanagement/api/v1/internal/delegablerights/", requestContent);
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
            HttpResponseMessage response = await GetTestClient(sblInternalToken).PostAsync($"accessmanagement/api/v1/internal/delegablerights/?returnAllPolicyRights=true", requestContent);
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
        ///            - 6 out of 9 of the rights for the resource: generic-access-resource is delegable through having DAGL:
        ///                 - generic-access-resource:read
        ///                 - generic-access-resource:admai-delegated-action
        ///                 - generic-access-resource,org-delegation-subtask:subunit-delegated-action
        ///                 - generic-access-resource,org-delegation-subtask:subunit-delegated-action-to-keyroleunit
        ///                 - generic-access-resource,org-delegation-subtask:mainunit-delegated-action
        ///                 - generic-access-resource,org-delegation-subtask:mainunit-delegated-action-to-keyroleunit
        /// Expected: DelegationCheck returns a list of RightDelegationCheckResult matching expected
        /// </summary>
        [Fact]
        public async Task DelegationCheck_GenericAccessResource_DAGL_HasDelegableRights()
        {
            // Arrange
            int userId = 20000490;
            int reporteePartyId = 50005545;
            string resourceId = "generic-access-resource";

            var token = PrincipalUtil.GetToken(userId, 0, 3);

            List<RightDelegationCheckResultExternal> expectedResponse = GetExpectedRightDelegationStatus($"u{userId}", $"p{reporteePartyId}", resourceId);
            StreamContent requestContent = GetDelegationCheckContent(resourceId);

            // Act
            HttpResponseMessage response = await GetTestClient(token).PostAsync($"accessmanagement/api/v1/{reporteePartyId}/rights/delegation/delegationcheck", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            List<RightDelegationCheckResultExternal> actualResponse = JsonSerializer.Deserialize<List<RightDelegationCheckResultExternal>>(responseContent, options);
            AssertionUtil.AssertCollections(expectedResponse, actualResponse, AssertionUtil.AssertRightDelegationCheckExternalEqual);
        }

        /// <summary>
        /// Test case: DelegationCheck returns a list of rights the authenticated userid 20000490 is authorized to delegate on behalf of itself (partyId 50002598) for the generic-access-resource from the resource registry.
        ///            In this case:
        ///            - The user 20000490 has PRIV role for itself (party 50002598)
        ///            - 5 out of 9 of the rights for the resource: generic-access-resource is delegable through having PRIV:
        ///                 - generic-access-resource:read
        ///                 - generic-access-resource:write
        ///                 - generic-access-resource:admai-delegated-action
        ///                 - generic-access-resource,priv-delegation-subtask:delegated-action-to-user
        ///                 - generic-access-resource,priv-delegation-subtask:delegated-action-to-keyroleunit
        /// Expected: DelegationCheck returns a list of RightDelegationCheckResult matching expected
        /// </summary>
        [Fact]
        public async Task DelegationCheck_GenericAccessResource_PRIV_HasDelegableRights()
        {
            // Arrange
            int userId = 20000490;
            int reporteePartyId = 50002598;
            string resourceId = "generic-access-resource";

            var token = PrincipalUtil.GetToken(userId, 0, 3);

            List<RightDelegationCheckResultExternal> expectedResponse = GetExpectedRightDelegationStatus($"u{userId}", $"p{reporteePartyId}", resourceId);
            StreamContent requestContent = GetDelegationCheckContent(resourceId);

            // Act
            HttpResponseMessage response = await GetTestClient(token).PostAsync($"accessmanagement/api/v1/{reporteePartyId}/rights/delegation/delegationcheck", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            List<RightDelegationCheckResultExternal> actualResponse = JsonSerializer.Deserialize<List<RightDelegationCheckResultExternal>>(responseContent, options);
            AssertionUtil.AssertCollections(expectedResponse, actualResponse, AssertionUtil.AssertRightDelegationCheckExternalEqual);
        }

        /// <summary>
        /// Test case: DelegationCheck returns a list of rights the authenticated userid 20001337 is authorized to delegate on behalf of the reportee party 50005545 for the generic-access-resource from the resource registry.
        ///            In this case:
        ///            - The user 20001337 is HADM for the From unit 50005545
        ///            - 6 out of 9 of the rights for the resource: generic-access-resource is delegable through having HADM (which inheirits same rights for delegation as DAGL):
        ///                 - generic-access-resource:read
        ///                 - generic-access-resource:admai-delegated-action
        ///                 - generic-access-resource,org-delegation-subtask:subunit-delegated-action
        ///                 - generic-access-resource,org-delegation-subtask:subunit-delegated-action-to-keyroleunit
        ///                 - generic-access-resource,org-delegation-subtask:mainunit-delegated-action
        ///                 - generic-access-resource,org-delegation-subtask:mainunit-delegated-action-to-keyroleunit
        /// Expected: DelegationCheck returns a list of RightDelegationCheckResult matching expected
        /// </summary>
        [Fact]
        public async Task DelegationCheck_GenericAccessResource_HADM_HasDelegableRights()
        {
            // Arrange
            int userId = 20001337;
            int reporteePartyId = 50005545;
            string resourceId = "generic-access-resource";

            var token = PrincipalUtil.GetToken(userId, 0, 3);

            List<RightDelegationCheckResultExternal> expectedResponse = GetExpectedRightDelegationStatus($"u{userId}", $"p{reporteePartyId}", resourceId);
            StreamContent requestContent = GetDelegationCheckContent(resourceId);

            // Act
            HttpResponseMessage response = await GetTestClient(token).PostAsync($"accessmanagement/api/v1/{reporteePartyId}/rights/delegation/delegationcheck", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            List<RightDelegationCheckResultExternal> actualResponse = JsonSerializer.Deserialize<List<RightDelegationCheckResultExternal>>(responseContent, options);
            AssertionUtil.AssertCollections(expectedResponse, actualResponse, AssertionUtil.AssertRightDelegationCheckExternalEqual);
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
        /// Expected: DelegationCheck returns a list of RightDelegationCheckResult matching expected
        /// </summary>
        [Fact]
        public async Task DelegationCheck_GenericAccessResource_SubUnitToUserDelegation_SubUnitToKeyRoleUnitDelegation_MainUnitToUserDelegation_MainUnitToKeyRoleUnitDelegation_HasDelegableRights()
        {
            // Arrange
            int userId = 20000490;
            int reporteePartyId = 50004221;
            string resourceId = "generic-access-resource";

            var token = PrincipalUtil.GetToken(userId, 0, 3);

            List<RightDelegationCheckResultExternal> expectedResponse = GetExpectedRightDelegationStatus($"u{userId}", $"p{reporteePartyId}", resourceId);
            StreamContent requestContent = GetDelegationCheckContent(resourceId);

            // Act
            HttpResponseMessage response = await GetTestClient(token).PostAsync($"accessmanagement/api/v1/{reporteePartyId}/rights/delegation/delegationcheck", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            List<RightDelegationCheckResultExternal> actualResponse = JsonSerializer.Deserialize<List<RightDelegationCheckResultExternal>>(responseContent, options);
            AssertionUtil.AssertCollections(expectedResponse, actualResponse, AssertionUtil.AssertRightDelegationCheckExternalEqual);
        }

        /// <summary>
        /// Test case: DelegationCheck returns a list of rights the authenticated 20001337 is authorized to delegate on behalf of the reportee party 50001337 for the org1_app1 Altinn App.
        ///            In this case:
        ///            - The test scenario is setup using existing test data for Org1/App1, offeredBy 50001337 and coveredbyuser 20001337, where the delegation policy contains rules for resources not in the App policy:
        ///                 ("rightKey": "app1,org1,task1:sign" and "rightKey": "app1,org1,task1:write"). This should normally not happen but can become an real scenario where delegations have been made and then the resource/app policy is changed to remove some rights.
        /// Expected: DelegationCheck returns a list of RightDelegationCheckResult matching expected
        /// </summary>
        [Fact]
        public async Task DelegationCheck_AppRight_UserDelegation_HasDelegableRights()
        {
            // Arrange
            int userId = 20001337;
            int reporteePartyId = 50001337;
            string resourceId = "org1_app1";

            var token = PrincipalUtil.GetToken(userId, 0, 4);

            List<RightDelegationCheckResultExternal> expectedResponse = GetExpectedRightDelegationStatus($"u{userId}", $"p{reporteePartyId}", resourceId);
            StreamContent requestContent = GetDelegationCheckContent(resourceId);

            // Act
            HttpResponseMessage response = await GetTestClient(token).PostAsync($"accessmanagement/api/v1/{reporteePartyId}/rights/delegation/delegationcheck", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            List<RightDelegationCheckResultExternal> actualResponse = JsonSerializer.Deserialize<List<RightDelegationCheckResultExternal>>(responseContent, options);
            AssertionUtil.AssertCollections(expectedResponse, actualResponse, AssertionUtil.AssertRightDelegationCheckExternalEqual);
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

            List<RightDelegationCheckResultExternal> expectedResponse = GetExpectedRightDelegationStatus($"u{userId}", $"p{reporteePartyId}", resourceId);
            StreamContent requestContent = GetDelegationCheckContent(resourceId);

            // Act
            HttpResponseMessage response = await GetTestClient(token).PostAsync($"accessmanagement/api/v1/{reporteePartyId}/rights/delegation/delegationcheck", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            List<RightDelegationCheckResultExternal> actualResponse = JsonSerializer.Deserialize<List<RightDelegationCheckResultExternal>>(responseContent, options);
            AssertionUtil.AssertCollections(expectedResponse, actualResponse, AssertionUtil.AssertRightDelegationCheckExternalEqual);
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

            ValidationProblemDetails expectedResponse = GetExpectedValidationError("DelegationCheck", $"u{userId}", $"p{reporteePartyId}", resourceId);
            StreamContent requestContent = GetDelegationCheckContent(resourceId);

            // Act
            HttpResponseMessage response = await GetTestClient(token).PostAsync($"accessmanagement/api/v1/{reporteePartyId}/rights/delegation/delegationcheck", requestContent);
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

            ValidationProblemDetails expectedResponse = GetExpectedValidationError("DelegationCheck", $"u{userId}", $"p{reporteePartyId}", resourceId);
            StreamContent requestContent = GetDelegationCheckContent(resourceId);

            // Act
            HttpResponseMessage response = await GetTestClient(token).PostAsync($"accessmanagement/api/v1/{reporteePartyId}/rights/delegation/delegationcheck", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            ValidationProblemDetails actualResponse = JsonSerializer.Deserialize<ValidationProblemDetails>(responseContent, options);
            AssertionUtil.AssertValidationProblemDetailsEqual(expectedResponse, actualResponse);
        }

        /// <summary>
        /// Test case: Delegation returns a list of rights the authenticated userid 20000490 is authorized to delegate on behalf of the reportee party 50005545 for the generic-access-resource from the resource registry.
        ///            In this case:
        ///            - The user 20000490 is DAGL for the From unit 50005545
        ///            - The user is delegating:
        ///                 - generic-access-resource,org-delegation-subtask:subunit-delegated-action
        ///                 - generic-access-resource,org-delegation-subtask:subunit-delegated-action-to-keyroleunit
        ///                 - generic-access-resource,org-delegation-subtask:mainunit-delegated-action
        ///                 - generic-access-resource,org-delegation-subtask:mainunit-delegated-action-to-keyroleunit
        /// Expected: Delegation returns a RightsDelegationResponse matching expected
        /// </summary>
        [Fact]
        public async Task Delegation_GenericAccessResource_FromOrg_ToPerson_ByDagl_Success()
        {
            // Arrange
            string resourceId = "generic-access-resource";
            (string OrgNo, string Ssn, int PartyId, string Uuid, int UuidType) from = ("910459880", string.Empty, 50005545, string.Empty, 0);
            (string OrgNo, string Ssn, string Uuid, int UuidType) to = (string.Empty, "27099450067", string.Empty, 0);
            (int UserId, int PartyId, string OrgNo, string Ssn, string Username, string Uuid, int UuidType) by = (20000490, 0, string.Empty, "07124912037", string.Empty, string.Empty, 0);
            string scenario = "success";

            string token = PrincipalUtil.GetToken(by.UserId, 0, 3);

            RightsDelegationResponseExternal expectedResponse = GetExpectedRightsDelegationResponse(resourceId, from.OrgNo, to.Ssn, by.Ssn, scenario);
            StreamContent requestContent = GetRightsDelegationContent(resourceId, from.OrgNo, to.Ssn, by.Ssn, scenario);

            // Act
            HttpResponseMessage response = await GetTestClient(token).PostAsync($"accessmanagement/api/v1/{from.PartyId}/rights/delegation/offered", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            RightsDelegationResponseExternal actualResponse = JsonSerializer.Deserialize<RightsDelegationResponseExternal>(responseContent, options);
            AssertionUtil.AssertRightsDelegationResponseExternalEqual(expectedResponse, actualResponse);
        }

        /// <summary>
        /// Test case: Delegation returns validation problem details when trying to delegate to SSN without specifying last name of recipient
        /// Expected: Delegation returns validation problem details
        /// </summary>
        [Fact]
        public async Task Delegation_GenericAccessResource_FromOrg_ToPerson_ByDagl_MissingToLastName()
        {
            // Arrange
            string resourceId = "generic-access-resource";
            (string OrgNo, string Ssn, int PartyId, string Uuid, int UuidType) from = ("910459880", string.Empty, 50005545, string.Empty, 0);
            (string OrgNo, string Ssn, string Uuid, int UuidType) to = (string.Empty, "27099450067", string.Empty, 0);
            (int UserId, int PartyId, string OrgNo, string Ssn, string Username, string Uuid, int UuidType) by = (20000490, 0, string.Empty, "07124912037", string.Empty, string.Empty, 0);
            string scenario = "missing-to-lastname";

            string token = PrincipalUtil.GetToken(by.UserId, 0, 3);

            ValidationProblemDetails expectedResponse = GetExpectedRightsDelegationError(resourceId, from.OrgNo, to.Ssn, by.Ssn, scenario);
            StreamContent requestContent = GetRightsDelegationContent(resourceId, from.OrgNo, to.Ssn, by.Ssn, scenario);

            // Act
            HttpResponseMessage response = await GetTestClient(token).PostAsync($"accessmanagement/api/v1/{from.PartyId}/rights/delegation/offered", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            ValidationProblemDetails actualResponse = JsonSerializer.Deserialize<ValidationProblemDetails>(responseContent, options);
            AssertionUtil.AssertValidationProblemDetailsEqual(expectedResponse, actualResponse);
        }

        /// <summary>
        /// Test case: Delegation returns a list of rights the authenticated userid 20000490 is authorized to delegate on behalf of itself (partyId 50002598) for the generic-access-resource from the resource registry.
        ///            In this case:
        ///            - The user 20000490 has PRIV role for itself (party 50002598)
        ///            - The user delegates:
        ///                 - generic-access-resource:read
        ///                 - generic-access-resource:write
        /// Expected: Delegation returns a RightsDelegationResponse matching expected
        /// </summary>
        [Fact]
        public async Task Delegation_GenericAccessResource_FromPerson_ToPerson_ByPriv_Success()
        {
            // Arrange
            string resourceId = "generic-access-resource";
            (string OrgNo, string Ssn, int PartyId, string Uuid, int UuidType) from = (string.Empty, "07124912037", 50002598, string.Empty, 0);
            (string OrgNo, string Ssn, string Uuid, int UuidType) to = (string.Empty, "27099450067", string.Empty, 0);
            (int UserId, int PartyId, string OrgNo, string Ssn, string Username, string Uuid, int UuidType) by = (20000490, 0, string.Empty, "07124912037", string.Empty, string.Empty, 0);
            string scenario = "success";

            var token = PrincipalUtil.GetToken(by.UserId, 0, 3);

            RightsDelegationResponseExternal expectedResponse = GetExpectedRightsDelegationResponse(resourceId, from.Ssn, to.Ssn, by.Ssn, scenario);
            StreamContent requestContent = GetRightsDelegationContent(resourceId, from.Ssn, to.Ssn, by.Ssn, scenario);

            // Act
            HttpResponseMessage response = await GetTestClient(token).PostAsync($"accessmanagement/api/v1/{from.PartyId}/rights/delegation/offered", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            RightsDelegationResponseExternal actualResponse = JsonSerializer.Deserialize<RightsDelegationResponseExternal>(responseContent, options);
            AssertionUtil.AssertRightsDelegationResponseExternalEqual(expectedResponse, actualResponse);
        }

        /// <summary>
        /// Test case: Delegation returns a list of rights the authenticated userid 20001337 is authorized to delegate on behalf of the reportee party 50005545 for the generic-access-resource from the resource registry.
        ///            In this case:
        ///            - The user 20001337 is HADM for the From unit 50005545
        ///            - The user is using HADM privilege to self delegate rights 
        ///            - 5 out of 8 of the rights for the resource: generic-access-resource is delegable through having HADM (which inheirits same rights for delegation as DAGL):
        ///                 - generic-access-resource:read
        ///                 - generic-access-resource,org-delegation-subtask:subunit-delegated-action
        ///                 - generic-access-resource,org-delegation-subtask:subunit-delegated-action-to-keyroleunit
        ///                 - generic-access-resource,org-delegation-subtask:mainunit-delegated-action
        ///                 - generic-access-resource,org-delegation-subtask:mainunit-delegated-action-to-keyroleunit
        /// Expected: Delegation returns a RightsDelegationResponse matching expected
        /// </summary>
        [Fact]
        public async Task Delegation_GenericAccessResource_FromOrg_ToPerson_ByHadm_Success()
        {
            // Arrange
            string resourceId = "generic-access-resource";
            (string OrgNo, string Ssn, int PartyId, string Uuid, int UuidType) from = ("910459880", string.Empty, 50005545, string.Empty, 0);
            (string OrgNo, string Ssn, string Uuid, int UuidType) to = (string.Empty, "27099450067", string.Empty, 0);
            (int UserId, int PartyId, string OrgNo, string Ssn, string Username, string Uuid, int UuidType) by = (20001337, 0, string.Empty, "27099450067", string.Empty, string.Empty, 0);
            string scenario = "success";

            var token = PrincipalUtil.GetToken(by.UserId, 0, 3);

            RightsDelegationResponseExternal expectedResponse = GetExpectedRightsDelegationResponse(resourceId, from.OrgNo, to.Ssn, by.Ssn, scenario);
            StreamContent requestContent = GetRightsDelegationContent(resourceId, from.OrgNo, to.Ssn, by.Ssn, scenario);

            // Act
            HttpResponseMessage response = await GetTestClient(token).PostAsync($"accessmanagement/api/v1/{from.PartyId}/rights/delegation/offered", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            RightsDelegationResponseExternal actualResponse = JsonSerializer.Deserialize<RightsDelegationResponseExternal>(responseContent, options);
            AssertionUtil.AssertRightsDelegationResponseExternalEqual(expectedResponse, actualResponse);
        }

        /// <summary>
        /// Test case: Delegation returns a list of rights the authenticated 20000490 is authorized to delegate on behalf of the reportee party 50004221 for the generic-access-resource from the resource registry.
        ///            In this case:
        ///            - The From unit (50004221) is a subunit of 500042222.
        ///            - The From unit (50004221) has delegated the "subunit-delegated-action" action directly to the user.
        ///            - The From unit (50004221) has delegated the "subunit-delegated-action-to-keyunit" action directly to the user.
        ///            - The main unit (50004222) has delegated the "mainunit-delegated-action" action to the user.
        ///            - The main unit (50004222) has delegated the "mainunit-delegated-action-to-keyunit" action to the party 50005545 where the user is DAGL and have keyrole privileges.
        ///            - 4 out of 8 rights are thus delegable and should contain the information of the actual recipient of the delegation
        /// Expected: Delegation returns a RightsDelegationResponse matching expected
        /// </summary>
        [Fact]
        public async Task Delegation_GenericAccessResource_FromSubunit_ToOrg_ByDelegation_Success()
        {
            // Arrange
            string resourceId = "generic-access-resource";
            (string OrgNo, string Ssn, int PartyId, string Uuid, int UuidType) from = ("810418532", string.Empty, 50004221, string.Empty, 0);
            (string OrgNo, string Ssn, string Uuid, int UuidType) to = ("810418672", string.Empty, string.Empty, 0);
            (int UserId, int PartyId, string OrgNo, string Ssn, string Username, string Uuid, int UuidType) by = (20000490, 0, string.Empty, "07124912037", string.Empty, string.Empty, 0);
            string scenario = "success";

            var token = PrincipalUtil.GetToken(by.UserId, 0, 3);

            RightsDelegationResponseExternal expectedResponse = GetExpectedRightsDelegationResponse(resourceId, from.OrgNo, to.OrgNo, by.Ssn, scenario);
            StreamContent requestContent = GetRightsDelegationContent(resourceId, from.OrgNo, to.OrgNo, by.Ssn, scenario);

            // Act
            HttpResponseMessage response = await GetTestClient(token).PostAsync($"accessmanagement/api/v1/{from.PartyId}/rights/delegation/offered", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            RightsDelegationResponseExternal actualResponse = JsonSerializer.Deserialize<RightsDelegationResponseExternal>(responseContent, options);
            AssertionUtil.AssertRightsDelegationResponseExternalEqual(expectedResponse, actualResponse);
        }

        /// <summary>
        /// Test case: Delegation returns a list of rights the authenticated userid 20000095 is authorized to delegate on behalf of the reportee party 50005545 for the generic-access-resource from the resource registry.
        ///            In this case:
        ///            - The user 20000490 has the roles ADMAI and UTINN for the From unit 50005545
        ///            - The UTINN gives the user access to delegate:
        ///                 - generic-access-resource:admai-delegated-action
        /// Expected: Delegation returns a RightsDelegationResponse matching expected
        /// </summary>
        [Fact]
        public async Task Delegation_GenericAccessResource_FromOrg_ToEcUser_ByAdmai_Success()
        {
            // Arrange
            string resourceId = "generic-access-resource";
            (string OrgNo, string Ssn, int PartyId, string Uuid, int UuidType) from = ("910459880", string.Empty, 50005545, string.Empty, 0);
            (string OrgNo, string Ssn, string Username, string Uuid, int UuidType) to = (string.Empty, string.Empty, "OrstaECUser", string.Empty, 0);
            (int UserId, int PartyId, string OrgNo, string Ssn, string Username, string Uuid, int UuidType) by = (20000095, 0, string.Empty, "02056260016", string.Empty, string.Empty, 0);
            string scenario = "success";

            var token = PrincipalUtil.GetToken(by.UserId, 0, 3);

            RightsDelegationResponseExternal expectedResponse = GetExpectedRightsDelegationResponse(resourceId, from.OrgNo, to.Username, by.Ssn, scenario);
            StreamContent requestContent = GetRightsDelegationContent(resourceId, from.OrgNo, to.Username, by.Ssn, scenario);

            // Act
            HttpResponseMessage response = await GetTestClient(token).PostAsync($"accessmanagement/api/v1/{from.PartyId}/rights/delegation/offered", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            RightsDelegationResponseExternal actualResponse = JsonSerializer.Deserialize<RightsDelegationResponseExternal>(responseContent, options);
            AssertionUtil.AssertRightsDelegationResponseExternalEqual(expectedResponse, actualResponse);
        }

        /// <summary>
        /// Test case: Delegation returns a list of rights the authenticated 20001337 is authorized to delegate on behalf of the reportee party 50001337 for the org1_app1 Altinn App.
        ///            In this case:
        ///            - The test scenario is setup using existing test data for Org1/App1, offeredBy 50001337 and coveredbyuser 20001337, where the delegation policy contains rules for resources not in the App policy:
        ///                 ("rightKey": "app1,org1,task1:sign" and "rightKey": "app1,org1,task1:write"). This should normally not happen but can become an real scenario where delegations have been made and then the resource/app policy is changed to remove some rights.
        ///            - The user is trying to delegate:
        ///                 - app1,org1:read
        ///                 - app1,org1:write
        ///            - The user only have delegable rights for app1,org1:read
        /// Expected: Delegation returns a RightsDelegationResponse matching expected
        /// </summary>
        [Fact]
        public async Task Delegation_AppRight_PartialSuccess()
        {
            // Arrange
            string resourceId = "app_org1_app1";
            (string OrgNo, string Ssn, int PartyId, string Uuid, int UuidType) from = ("910001337", string.Empty, 50001337, string.Empty, 0);
            (string OrgNo, string Ssn, string Uuid, int UuidType) to = ("810418672", string.Empty, string.Empty, 0);
            (int UserId, int PartyId, string OrgNo, string Ssn, string Username, string Uuid, int UuidType) by = (20001337, 0, string.Empty, "27099450067", string.Empty, string.Empty, 0);
            string scenario = "success";

            var token = PrincipalUtil.GetToken(by.UserId, 0, 3);

            RightsDelegationResponseExternal expectedResponse = GetExpectedRightsDelegationResponse(resourceId, from.OrgNo, to.OrgNo, by.Ssn, scenario);
            StreamContent requestContent = GetRightsDelegationContent(resourceId, from.OrgNo, to.OrgNo, by.Ssn, scenario);

            // Act
            HttpResponseMessage response = await GetTestClient(token).PostAsync($"accessmanagement/api/v1/{from.PartyId}/rights/delegation/offered", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            RightsDelegationResponseExternal actualResponse = JsonSerializer.Deserialize<RightsDelegationResponseExternal>(responseContent, options);
            AssertionUtil.AssertRightsDelegationResponseExternalEqual(expectedResponse, actualResponse);
        }

        /// <summary>
        /// Test case: Delegation returns a list of rights the authenticated 20001337 is authorized to delegate on behalf of the reportee party 50001337 for the org1_app1 Altinn App.
        ///            In this case:
        ///            - The test scenario is setup using existing test data for Org1/App1, offeredBy 50001337 and coveredbyuser 20001337, where the delegation policy contains rules for resources not in the App policy:
        ///                 ("rightKey": "app1,org1,task1:sign" and "rightKey": "app1,org1,task1:write"). This should normally not happen but can become an real scenario where delegations have been made and then the resource/app policy is changed to remove some rights.
        ///            - The user is trying to delegate:
        ///                 - app1,org1:read
        ///                 - app1,org1:write
        ///            - The user only have delegable rights for app1,org1:read
        /// Expected: Delegation returns a RightsDelegationResponse matching expected
        /// </summary>
        [Fact]
        public async Task Delegation_Altinn2Service_Success()
        {
            // Arrange
            string resourceId = "se_2802_2203";
            (string OrgNo, string Ssn, int PartyId, string Uuid, int UuidType) from = ("910459880", string.Empty, 50005545, string.Empty, 0);
            (string OrgNo, string Ssn, string Uuid, int UuidType) to = (string.Empty, "27099450067", string.Empty, 0);
            (int UserId, int PartyId, string OrgNo, string Ssn, string Username, string Uuid, int UuidType) by = (20000490, 0, string.Empty, "07124912037", string.Empty, string.Empty, 0);
            string scenario = "success";

            var token = PrincipalUtil.GetToken(by.UserId, 0, 3);

            RightsDelegationResponseExternal expectedResponse = GetExpectedRightsDelegationResponse(resourceId, from.OrgNo, to.Ssn, by.Ssn, scenario);
            StreamContent requestContent = GetRightsDelegationContent(resourceId, from.OrgNo, to.Ssn, by.Ssn, scenario);

            // Act
            HttpResponseMessage response = await GetTestClient(token).PostAsync($"accessmanagement/api/v1/{from.PartyId}/rights/delegation/offered", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            RightsDelegationResponseExternal actualResponse = JsonSerializer.Deserialize<RightsDelegationResponseExternal>(responseContent, options);
            AssertionUtil.AssertRightsDelegationResponseExternalEqual(expectedResponse, actualResponse);
        }

        /// <summary>
        /// Test case: Delegation when the authenticated 20001337 is authorized to delegate on behalf of the reportee party 50001337 for the invalid resource non_existing_id 
        ///            In this case:
        ///            - Since the resource is invalid a BadRequest response with a ValidationProblemDetails model response should be returned
        /// Expected: Responce error model is matching expected
        /// </summary>
        [Fact]
        public async Task Delegation_InvalidResource_BadRequest()
        {
            // Arrange
            string resourceId = "non_existing_id";
            (string OrgNo, string Ssn, int PartyId, string Uuid, int UuidType) from = ("910001337", string.Empty, 50001337, string.Empty, 0);
            (string OrgNo, string Ssn, string Uuid, int UuidType) to = ("810418672", string.Empty, string.Empty, 0);
            (int UserId, int PartyId, string OrgNo, string Ssn, string Username, string Uuid, int UuidType) by = (20001337, 0, string.Empty, "27099450067", string.Empty, string.Empty, 0);
            string scenario = "invalid-resource";

            var token = PrincipalUtil.GetToken(by.UserId, 0, 3);

            ValidationProblemDetails expectedResponse = GetExpectedRightsDelegationError(resourceId, from.OrgNo, to.OrgNo, by.Ssn, scenario);
            StreamContent requestContent = GetRightsDelegationContent(resourceId, from.OrgNo, to.OrgNo, by.Ssn, scenario);

            // Act
            HttpResponseMessage response = await GetTestClient(token).PostAsync($"accessmanagement/api/v1/{from.PartyId}/rights/delegation/offered", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            ValidationProblemDetails actualResponse = JsonSerializer.Deserialize<ValidationProblemDetails>(responseContent, options);
            AssertionUtil.AssertValidationProblemDetailsEqual(expectedResponse, actualResponse);
        }

        /// <summary>
        /// Test case: Delegation when the authenticated 20001337 is authorized to delegate on behalf of the reportee party 50001337 for the MaskinportenSchema resource jks_audi_etron_gt
        ///            In this case:
        ///            - Since the resource is a MaskinportenSchema a BadRequest response with a ValidationProblemDetails model response should be returned
        /// Expected: Responce error model is matching expected
        /// </summary>
        [Fact]
        public async Task Delegation_MaskinportenSchema_BadRequest()
        {
            // Arrange
            string resourceId = "jks_audi_etron_gt";
            (string OrgNo, string Ssn, int PartyId, string Uuid, int UuidType) from = ("910001337", string.Empty, 50001337, string.Empty, 0);
            (string OrgNo, string Ssn, string Uuid, int UuidType) to = ("810418672", string.Empty, string.Empty, 0);
            (int UserId, int PartyId, string OrgNo, string Ssn, string Username, string Uuid, int UuidType) by = (20001337, 0, string.Empty, "27099450067", string.Empty, string.Empty, 0);
            string scenario = "invalid-resource-maskinportenschema";

            var token = PrincipalUtil.GetToken(by.UserId, 0, 3);

            ValidationProblemDetails expectedResponse = GetExpectedRightsDelegationError(resourceId, from.OrgNo, to.OrgNo, by.Ssn, scenario);
            StreamContent requestContent = GetRightsDelegationContent(resourceId, from.OrgNo, to.OrgNo, by.Ssn, scenario);

            // Act
            HttpResponseMessage response = await GetTestClient(token).PostAsync($"accessmanagement/api/v1/{from.PartyId}/rights/delegation/offered", requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            ValidationProblemDetails actualResponse = JsonSerializer.Deserialize<ValidationProblemDetails>(responseContent, options);
            AssertionUtil.AssertValidationProblemDetailsEqual(expectedResponse, actualResponse);
        }

        /// <summary>
        /// Test case: Get all delegations offered from party 20001337 for systemresource and altinnapps
        ///             In this case:
        ///            - Should use the partyID in the URL
        /// Expected: - Should return 200 Ok and a list containing three delegations from party 20001337
        /// </summary>
        [Fact]
        public async Task RightsOfferedDelegations_PartyInURL_ReturnOK()
        {
            var token = PrincipalUtil.GetToken(20001337, 50002203, 3);
            var client = GetTestClient(token);

            // Act
            HttpResponseMessage response = await client.GetAsync($"accessmanagement/api/v1/{20001337}/rights/delegation/offered");
            string responseContent = await response.Content.ReadAsStringAsync();
            List<RightDelegationExternal> actualRights = JsonSerializer.Deserialize<List<RightDelegationExternal>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3, actualRights.Count);
        }

        /// <summary>
        /// Test case: Get all delegations offered from party 20001337 for systemresource and altinnapps
        /// Expected: - Should return 200 Ok and a list containing three delegations from party 20001337
        /// </summary>
        [Fact]
        public async Task RightsOfferedDelegations_PartyInOrganizationHeader_ReturnOK()
        {
            var token = PrincipalUtil.GetToken(20001337, 50002203, 3);
            var client = GetTestClient(token);
            client.DefaultRequestHeaders.Add(IdentifierUtil.OrganizationNumberHeader, "927144913");

            // Act
            HttpResponseMessage response = await client.GetAsync($"accessmanagement/api/v1/organization/rights/delegation/offered");
            string responseContent = await response.Content.ReadAsStringAsync();
            List<RightDelegationExternal> actualRights = JsonSerializer.Deserialize<List<RightDelegationExternal>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3, actualRights.Count);
        }

        /// <summary>
        /// Test case: Get all delegations offered from party 20001337 for systemresource and altinnapps
        /// Expected: - Should return 200 Ok and a list containing three delegations from party 20001337
        /// </summary>
        [Fact]
        public async Task RightsOfferedDelegations_PartyInPersonHeader_ReturnOK()
        {
            var token = PrincipalUtil.GetToken(20001337, 50002203, 3);
            var client = GetTestClient(token, WithHttpContextAccessorMock("party", "20001337"), WithPDPMock);
            client.DefaultRequestHeaders.Add(IdentifierUtil.PersonHeader, "21033041133");

            // Act
            HttpResponseMessage response = await client.GetAsync($"accessmanagement/api/v1/person/rights/delegation/offered");
            string responseContent = await response.Content.ReadAsStringAsync();
            List<RightDelegationExternal> actualRights = JsonSerializer.Deserialize<List<RightDelegationExternal>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3, actualRights.Count);
        }

        /// <summary>
        /// Test case: Get all delegations offered from person 22093229405
        /// Expected: - Should return 500 as person with SSN 22093229405 don't exist
        /// </summary>
        [Fact]
        public async Task RightsOfferedDelegations_NoneExistingPartyInPersonHeader_ReturnInternalServerError()
        {
            var token = PrincipalUtil.GetToken(20001337, 50002203, 3);
            var client = GetTestClient(token, WithHttpContextAccessorMock("party", "20001337"), WithPDPMock);
            client.DefaultRequestHeaders.Add(IdentifierUtil.PersonHeader, "22093229405");

            // Act
            HttpResponseMessage response = await client.GetAsync($"accessmanagement/api/v1/person/rights/delegation/offered");

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        private static Action<IServiceCollection> WithHttpContextAccessorMock(string partytype, string id)
        {
            return services => 
            {
                HttpContext httpContext = new DefaultHttpContext();
                httpContext.Request.RouteValues.Add(partytype, id);

                var mock = new Mock<IHttpContextAccessor>();
                mock.Setup(h => h.HttpContext).Returns(httpContext);
                services.AddSingleton(mock.Object);
            };
        }

        private void WithPDPMock(IServiceCollection services) => services.AddSingleton(new PepWithPDPAuthorizationMock());

        private HttpClient GetTestClient(string token, params Action<IServiceCollection>[] actions)
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
                    services.AddSingleton<IProfileClient, ProfileClientMock>();
                    services.AddSingleton<IResourceRegistryClient, ResourceRegistryClientMock>();
                    services.AddSingleton<IAltinnRolesClient, AltinnRolesClientMock>();
                    services.AddSingleton<IPDP, PdpPermitMock>();
                    services.AddSingleton<IAltinn2RightsClient, Altinn2RightsClientMock>();
                    services.AddSingleton<IDelegationChangeEventQueue>(new DelegationChangeEventQueueMock());
                
                    foreach (var action in actions)
                    {
                        action(services);
                    }
                });

            }).CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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

        private static ValidationProblemDetails GetExpectedRightsDelegationError(string resourceId, string from, string to, string by, string scenario)
        {
            string content = File.ReadAllText($"Data/Json/RightsDelegation/{resourceId}/from_{from}/to_{to}/by_{by}/{scenario}_response.json");
            return (ValidationProblemDetails)JsonSerializer.Deserialize(content, typeof(ValidationProblemDetails), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private static RightsDelegationResponseExternal GetExpectedRightsDelegationResponse(string resourceId, string from, string to, string by, string scenario)
        {
            string content = File.ReadAllText($"Data/Json/RightsDelegation/{resourceId}/from_{from}/to_{to}/by_{by}/{scenario}_response.json");
            return (RightsDelegationResponseExternal)JsonSerializer.Deserialize(content, typeof(RightsDelegationResponseExternal), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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

        private static StreamContent GetRightsDelegationContent(string resourceId, string from, string to, string by, string scenario)
        {
            Stream dataStream = File.OpenRead($"Data/Json/RightsDelegation/{resourceId}/from_{from}/to_{to}/by_{by}/{scenario}_request.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return content;
        }
    }
}
