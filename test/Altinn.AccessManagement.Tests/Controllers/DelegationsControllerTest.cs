using System;
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
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Models;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.AccessManagement.Tests.Util;
using Altinn.AccessManagement.Tests.Utils;
using Altinn.Common.AccessToken.Services;
using Altinn.Common.PEP.Interfaces;
using AltinnCore.Authentication.JwtCookie;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Altinn.AccessManagement.Tests.Controllers
{
    /// <summary>
    /// Test class for <see cref="DelegationsController"></see>
    /// </summary>
    [Collection("DelegationController Tests")]
    public class DelegationsControllerTest : IClassFixture<CustomWebApplicationFactory<DelegationsController>>
    {
        private readonly CustomWebApplicationFactory<DelegationsController> _factory;
        private HttpClient _client;

        private readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        /// <summary>
        /// Constructor setting up factory, test client and dependencies
        /// </summary>
        /// <param name="factory">CustomWebApplicationFactory</param>
        public DelegationsControllerTest(CustomWebApplicationFactory<DelegationsController> factory)
        {
            _factory = factory;
            _client = GetTestClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string token = PrincipalUtil.GetAccessToken("sbl.authorization");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Test case: Calling the POST operation for DeleteRules to perform a valid deletion of org1/app3
        /// Expected: DeleteRules returns status code 201 and list of rules created match expected
        /// 
        /// Scenario:
        /// Calling the POST operation for DeleteRules to perform a valid deletion
        /// Input:
        /// List of two one rule in one policy for deletion of the app org1/app3 between for a single offeredby/coveredby combination resulting in a single policyfile beeing updated.
        /// Expected Result:
        /// Rules are deleted and returned with the CreatedSuccessfully flag set and rule ids
        /// Success Criteria:
        /// DeleteRules returns status code 201 and list of rules deleted to match expected
        /// </summary>
        [Fact]
        public async Task Post_DeleteRules_Success()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/DeleteRules/ReadOrg1App3_50001337_20001337.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(20001336, 50001337, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", "org1", "app3", createdSuccessfully: true),
            };

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/DeleteRules", content);

            string responseContent = await response.Content.ReadAsStringAsync();
            List<Rule> actual = JsonSerializer.Deserialize<List<Rule>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(actual.TrueForAll(a => a.CreatedSuccessfully));
            Assert.True(actual.TrueForAll(a => !string.IsNullOrEmpty(a.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
        }

        /// <summary>
        /// Test case: Calling the POST operation for DeleteRules to perform a valid deletion of resourceRegistryId
        /// Expected: DeleteRules returns status code 201 and list of rules created match expected
        /// 
        /// Scenario:
        /// Calling the POST operation for DeleteRules to perform a valid deletion
        /// Input:
        /// List of two one rule in one policy for deletion of the resource between for a single offeredby/coveredby combination resulting in a single policyfile beeing updated.
        /// Expected Result:
        /// Rules are deleted and returned with the CreatedSuccessfully flag set and rule ids
        /// Success Criteria:
        /// DeleteRules returns status code 201 and list of rules deleted to match expected
        /// </summary>
        [Fact]
        public async Task Post_DeleteResourceRegistryRules_Success()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/DeleteRules/ReadResource2_50001337_20001337.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(20001336, 50001337, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "write", null, null, createdSuccessfully: true, resourceRegistryId: "resource2"),
            };

            string expectedRuleId = "99e5cced-3bcb-42b6-9089-63c834f89e77";

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/DeleteRules", content);

            string responseContent = await response.Content.ReadAsStringAsync();
            List<Rule> actual = JsonSerializer.Deserialize<List<Rule>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(actual[0].RuleId, expectedRuleId);
            Assert.True(actual.TrueForAll(a => a.CreatedSuccessfully));
            Assert.True(actual.TrueForAll(a => !string.IsNullOrEmpty(a.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
        }

        /// <summary>
        /// Test case: Calling the POST operation for DeleteRules to perform a valid deletion of org1/app3
        /// Expected: DeleteRules returns status code 206 and list of rules created match expected one of the rules does not exist
        /// 
        /// Scenario:
        /// Calling the POST operation for DeleteRules to perform a valid deletion and one not existing 
        /// Input:
        /// List of two one rule in one policy for deletion of the app org1/app3 between for a single offeredby/coveredby combination resulting in a single policyfile beeing updated.
        /// Expected Result:
        /// Rules are deleted and returned with the CreatedSuccessfully flag set and rule id deleted
        /// Success Criteria:
        /// DeleteRules returns status code 206 and list of rules deleted to match expected (one rule)
        /// </summary>
        [Fact]
        public async Task Post_DeleteRulesNotExistingRuleId_PartialSuccess()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/DeleteRules/ReadOrg1App3_50001337_20001337RuleIdDoesNotExist.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(20001336, 50001337, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", "org1", "app3", createdSuccessfully: true),
            };

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/DeleteRules", content);

            string responseContent = await response.Content.ReadAsStringAsync();
            List<Rule> actual = JsonSerializer.Deserialize<List<Rule>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
            Assert.True(actual.TrueForAll(a => a.CreatedSuccessfully));
            Assert.True(actual.TrueForAll(a => !string.IsNullOrEmpty(a.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
        }

        /// <summary>
        /// Test case: Calling the POST operation for DeleteRules to perform a valid deletion of org1/app3 without a valid bearertoken
        /// Expected: DeleteRules returns status code 401
        /// 
        /// Scenario:
        /// Calling the POST operation for DeleteRules to perform a valid deletion
        /// Input:
        /// List of two one rule in one policy for deletion of the app org1/app3 between for a single offeredby/coveredby combination resulting in a single policyfile beeing updated.
        /// Expected Result:
        /// Responce declined as it is not Authorized
        /// Success Criteria:
        /// DeleteRules returns status code 401
        /// </summary>
        [Fact]
        public async Task Post_DeleteRules_WithoutAuthorization()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/DeleteRules/ReadOrg1App3_50001337_20001337.json");
            StreamContent content = new StreamContent(dataStream);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "ThisIsNotAValidToken");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/DeleteRules", content);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        /// <summary>
        /// Test case: Calling the POST operation for DeleteRules to perform a valid deletion of org1/app3 org1/app4 org1/app8
        /// Expected: DeleteRules returns status code 206 and list of rules created match expected
        /// 
        /// Scenario:
        /// Calling the POST operation for DeleteRules to perform a valid deletion but one of the policy files was not found and some rules was therfore not deleted
        /// Input:
        /// List of four rules for deletion spread accross 3 policy files of the app org1/app3 org1/app4 and org1/app8 between for a single offeredby/coveredby combination resulting in two policyfile beeing updated.
        /// Expected Result:
        /// Rules are deleted and returned with the CreatedSuccessfully flag set and rule ids but not all rules is retuned
        /// Success Criteria:
        /// DeleteRules returns status code 206 and list of rules dleted to match expected
        /// </summary>
        [Fact]
        public async Task Post_DeleteRules_OnePolicyMissing_PartialSucess()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/DeleteRules/ReadOrg1App3App4App8_50001337_20001337.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(20001336, 50001337, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(20001336, 50001337, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", "org1", "app4", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(20001336, 50001337, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "write", "org1", "app4", createdSuccessfully: true),
            };

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/DeleteRules", content);

            string responseContent = await response.Content.ReadAsStringAsync();
            List<Rule> actual = JsonSerializer.Deserialize<List<Rule>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
            Assert.True(actual.TrueForAll(a => a.CreatedSuccessfully));
            Assert.True(actual.TrueForAll(a => !string.IsNullOrEmpty(a.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
        }

        /// <summary>
        /// Test case: Calling the POST operation for DeleteRules to perform a deletion of org1/app4 and org1/app3 without rules defined
        /// Expected: DeleteRules returns status code 500 and no list of rules as one of the policies had no ruleids defined
        /// 
        /// Scenario:
        /// Calling the POST operation for DeleteRules to delete rules without giving a RuleId
        /// Input:
        /// List of three rules for delegation of the app org1/app3 and org1/app4 between for a single offeredby/coveredby combination resulting in no policy file beeing updated.
        /// Expected Result:
        /// No Rules are deleted and no rules are returned
        /// Success Criteria:
        /// DeleteRules returns status code 500 and no deletion is performed
        /// </summary>
        [Fact]
        public async Task Post_DeleteRules_InvalidInput_BadRequest()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/DeleteRules/ReadOrg1App3App4_50001337_20001337_NoRule.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/DeleteRules", content);

            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            List<Rule> actual = null;
            try
            {
                actual = (List<Rule>)JsonSerializer.Deserialize(responseContent, typeof(List<Rule>));
            }
            catch
            {
                // do nothing this is expected
            }

            Assert.Null(actual);
        }

        /// <summary>
        /// Test case: Calling the POST operation for DeleteRules to perform a deletion of org1/app4 and org1/app3 where the user performing the task is not defined
        /// Expected: DeleteRules returns status code 500 and no list of rules as one of the policies had no DeletedByUser set (0)
        /// 
        /// Scenario:
        /// Calling the POST operation for DeleteRules to delete rules without giving a DeletedByUserId
        /// Input:
        /// List of three rules for delegation of the app org1/app3 and org1/app4 between for a single offeredby/coveredby combination resulting in no policy file beeing updated.
        /// Expected Result:
        /// No Rules are deleted and no rules are returned
        /// Success Criteria:
        /// DeleteRules returns status code 500 and no deletion is performed
        /// </summary>
        [Fact]
        public async Task Post_DeleteRules_InvalidUserPerformingDeleteRule_BadRequest()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/DeleteRules/ReadOrg1App3App4_50001337_20001337_NoDeletedBy.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/DeleteRules", content);

            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            ValidationProblemDetails actual = JsonSerializer.Deserialize<ValidationProblemDetails>(responseContent, options);
            string errormessage = actual.Errors.Values.FirstOrDefault()[0];
            Assert.Equal("Not all RequestToDelete has a value for the user performing the delete", errormessage);
        }

        /// <summary>
        /// Test case: Calling the POST operation for DeleteRules to perform a deletion of org1/app4 and org1/app3 where the user performing the task is not defined
        /// Expected: DeleteRules returns status code 500 and no list of rules as one of the policies had no DeletedByUser set (0)
        ///
        /// Scenario:
        /// Calling the POST operation for DeleteRules to delete rules without giving a DeletedByUserId
        /// Input:
        /// List of three rules for delegation of the app org1/app3 and org1/app4 between for a single offeredby/coveredby combination resulting in no policy file beeing updated.
        /// Expected Result:
        /// No Rules are deleted and no rules are returned
        /// Success Criteria:
        /// DeleteRules returns status code 500 and no deletion is performed
        /// </summary>
        [Fact]
        public async Task Post_DeletePolicies_InvalidUserPerformingDeleteRule_BadRequest()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/DeletePolicies/ReadOrg1App3App4_50001337_20001337_NoDeletedBy.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/DeletePolicy", content);

            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            ValidationProblemDetails actual = JsonSerializer.Deserialize<ValidationProblemDetails>(responseContent, options);
            string errorMessage = actual.Errors.Values.FirstOrDefault()[0];
            Assert.Equal("Not all RequestToDelete has a value for the user performing the delete", errorMessage);
        }

        /// <summary>
        /// Test case: Calling the POST operation for DeleteRules to perform a deletion of org1/app4 and org1/app3 without rules defined
        /// Expected: DeleteRules returns status code 500 and no list of rules as one of the policies had no ruleids defined
        /// 
        /// Scenario:
        /// Calling the POST operation for DeleteRules to delete rules without giving a RuleId
        /// Input:
        /// List of three rules for delegation of the app org1/app3 and org1/app4 between for a single offeredby/coveredby combination resulting in no policy file beeing updated.
        /// Expected Result:
        /// No Rules are deleted and no rules are returned
        /// Success Criteria:
        /// DeleteRules returns status code 500 and no deletion is performed
        /// </summary>
        [Fact]
        public async Task Post_DeleteRules_ValidInputAllFails_BadRequest()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/DeleteRules/ReadOrg1App8-Errorpostgrewritechangefail_50001337_20001337_NoUpdates.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/DeleteRules", content);

            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            List<Rule> actual = null;
            try
            {
                actual = (List<Rule>)JsonSerializer.Deserialize(responseContent, typeof(List<Rule>));
            }
            catch
            {
                // do nothing this is expected
            }

            Assert.Null(actual);
        }

        /// <summary>
        /// Test case: Calling the POST operation for DeleteRules to perform a deletion of org1/app3 with difrent rules on same policy declared in two requests
        /// Expected: DeleteRules returns status code 500 and no list of rules as the same policy was tried to delete from twice
        /// 
        /// Scenario:
        /// Calling the POST operation for DeleteRules to delete rules giving the dame policy twice
        /// Input:
        /// List of two rules for deletion of the app org1/app3 for a single offeredby/coveredby combination resulting in no policy file beeing updated.
        /// Expected Result:
        /// No Rules are deleted and no rules are returned
        /// Success Criteria:
        /// DeleteRules returns status code 500 and no deletion is performed
        /// </summary>
        [Fact]
        public async Task Post_DeleteRules_DuplicatePolicy_BadRequest()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/DeleteRules/ReadOrg1App3_50001337_20001337_DuplicatePolicy.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/DeleteRules", content);

            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            ValidationProblemDetails actual = JsonSerializer.Deserialize<ValidationProblemDetails>(responseContent, options);
            string errorMessage = actual.Errors.Values.FirstOrDefault()[0];
            Assert.Equal("Input should not contain any duplicate policies", errorMessage);
        }

        /// <summary>
        /// Test case: Calling the POST operation for DeletePolicy to perform a valid deletion of org1/app3 org1/app4
        /// Expected: DeletePolicy returns status code 201 and list of rules created match expected
        /// 
        /// Scenario:
        /// Calling the POST operation for DeletePolicy to perform a valid deletion
        /// Input:
        /// List of 2 policy files of the app org1/app3 and org1/app4 between for a single offeredby/coveredby combination resulting in all rules in two policyfile beeing removed.
        /// Expected Result:
        /// Rules are deleted and returned with the CreatedSuccessfully flag set and rule ids but not all rules is retuned
        /// Success Criteria:
        /// DeleteRules returns status code 201 and list of rules deleted to match expected
        /// </summary>
        [Fact]
        public async Task Post_DeletePolicies_Sucess()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/DeletePolicies/ReadOrg1App3App4_50001337_20001337.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(20001336, 50001337, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(20001336, 50001337, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "write", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(20001336, 50001337, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", "org1", "app4", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(20001336, 50001337, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "write", "org1", "app4", createdSuccessfully: true),
            };

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/DeletePolicy", content);

            string responseContent = await response.Content.ReadAsStringAsync();
            List<Rule> actual = JsonSerializer.Deserialize<List<Rule>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(actual.TrueForAll(a => a.CreatedSuccessfully));
            Assert.True(actual.TrueForAll(a => !string.IsNullOrEmpty(a.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
        }

        /// <summary>
        /// Test case: Calling the POST operation for DeletePolicy to perform a valid deletion of resource 1 and resource2
        /// Expected: DeletePolicy returns status code 201 and list of rules created match expected
        /// 
        /// Scenario:
        /// Calling the POST operation for DeletePolicy to perform a valid deletion
        /// Input:
        /// List of 2 policy files of resource1 and resource2 between for a single offeredby/coveredby combination resulting in all rules in two policyfile beeing removed.
        /// Expected Result:
        /// Rules are deleted and returned with the CreatedSuccessfully flag set and rule ids but not all rules is retuned
        /// Success Criteria:
        /// DeleteRules returns status code 201 and list of rules deleted to match expected
        /// </summary>
        [Fact]
        public async Task Post_DeletePoliciesWithResourceRegistryId_Sucess()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/DeletePolicies/ReadResource1Resource2_50001337_20001337.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(20001336, 50001337, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", null, null, createdSuccessfully: true, resourceRegistryId: "resource1"),
                TestDataUtil.GetRuleModel(20001336, 50001337, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "write", null, null, createdSuccessfully: true, resourceRegistryId: "resource1"),
                TestDataUtil.GetRuleModel(20001336, 50001337, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", null, null, createdSuccessfully: true, resourceRegistryId: "resource2"),
                TestDataUtil.GetRuleModel(20001336, 50001337, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "write", null, null, createdSuccessfully: true, resourceRegistryId: "resource2"),
            };

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/DeletePolicy", content);

            string responseContent = await response.Content.ReadAsStringAsync();
            List<Rule> actual = JsonSerializer.Deserialize<List<Rule>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(actual.TrueForAll(a => a.CreatedSuccessfully));
            Assert.True(actual.TrueForAll(a => !string.IsNullOrEmpty(a.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
        }

        /// <summary>
        /// Test case: Calling the POST operation for DeletePolicy to perform an invalid deletion of a non-existing resource.
        /// Expected: DeletePolicy returns status code 400 and the error message "Unable to complete deletion".
        /// 
        /// Scenario:
        /// Calling the POST operation for DeletePolicy to perform an invalid deletion
        /// Input:
        /// Resource with a non existing resource registry id.
        /// Expected Result:
        /// Response contains error message.
        /// Success Criteria:
        /// DeleteRules returns status code 400
        /// </summary>
        [Fact]
        public async Task Post_DeletePoliciesWithNotExistingResourceRegistryId_Fail()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/DeletePolicies/ReadNotExistingResourceRegistryId.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            string expectedErrorMessage = "\"Unable to complete deletion\"";

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/DeletePolicy", content);

            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(responseContent, expectedErrorMessage);
        }

        /// <summary>
        /// Test case: Calling the POST operation for DeletePolicy to perform a valid deletion of org1/app3 with invalid Authorization token
        /// Expected: DeletePolicy returns status code 401
        /// 
        /// Scenario:
        /// Calling the POST operation for DeletePolicy to perform a valid deletion withot valid bearertoken
        /// Input:
        /// List of 2 policy files of the app org1/app3 and org1/app4 between for a single offeredby/coveredby combination resulting Http Unauthorized
        /// Expected Result:
        /// Nothing is performed and responce has UnAuthorized responcecode
        /// Success Criteria:
        /// DeleteRules returns status code 401
        /// </summary>
        [Fact]
        public async Task Post_DeletePolicies_InvalidBearerToken_Unauthorized()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/DeletePolicies/ReadOrg1App3App4_50001337_20001337.json");
            StreamContent content = new StreamContent(dataStream);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "ThisIsNotAValidToken");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/DeletePolicy", content);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        /// <summary>
        /// Test case: Calling the POST operation for DeletePolicy to perform a valid deletion of org1/app3 org1/app4 and one who does not exixt org1/app8
        /// Expected: DeletePolicy returns status code 206 and list of rules deleted match expected
        /// 
        /// Scenario:
        /// Calling the POST operation for DeletePolicy to perform a valid deletion
        /// Input:
        /// List of 3 policy files of the app org1/app3 and org1/app4 and org1/app8 between for a single offeredby/coveredby combination resulting in two policyfile beeing updated.
        /// Expected Result:
        /// Rules are deleted and returned with the CreatedSuccessfully flag set and rule in defoned policy files but not all policyfiles was touched so only rules from updated policyfiles is returned
        /// Success Criteria:
        /// DeletePolicy returns status code 206 and list of rules deleted to match expected
        /// </summary>
        [Fact]
        public async Task Post_DeletePolicies_OneMissingPolicyFile_PartialSucess()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/DeletePolicies/ReadOrg1App3App4App8_50001337_20001337.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(20001336, 50001337, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(20001336, 50001337, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "write", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(20001336, 50001337, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", "org1", "app4", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(20001336, 50001337, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "write", "org1", "app4", createdSuccessfully: true),
            };

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/DeletePolicy", content);

            string responseContent = await response.Content.ReadAsStringAsync();
            List<Rule> actual = JsonSerializer.Deserialize<List<Rule>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
            Assert.True(actual.TrueForAll(a => a.CreatedSuccessfully));
            Assert.True(actual.TrueForAll(a => !string.IsNullOrEmpty(a.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
        }

        /// <summary>
        /// Test case: Calling the POST operation for DeletePolicy to perform a valid deletion of org1/app8 error/postgrewritechangefail
        /// Expected: DeletePolicy returns status code 500
        /// 
        /// Scenario:
        /// Calling the POST operation for DeletePolicy to perform a valid deletion
        /// Input:
        /// List of four rules for deletion spread accross 2 policy files of the app org1/app8 and error/postgrewritechangefail between for a single offeredby/coveredby combination.
        /// Expected Result:
        /// Nothing are deleted and 500 status code is returned
        /// Success Criteria:
        /// postgrewritechangefail returns status code 500
        /// </summary>
        [Fact]
        public async Task Post_DeletePolicies_AllPoliciesFail_Fail()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/DeletePolicies/ReadOrg1App8-Errorpostgrewritechangefail_50001337_20001337_NoUpdates.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/DeletePolicy", content);

            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("\"Unable to complete deletion\"", responseContent);
        }

        /// <summary>
        /// Test case: Calling the POST operation for DeletePolicy to perform a invalid deletion of org1/app3 with the same policy defined twice
        /// Expected: DeletePolicy returns status code 500
        /// 
        /// Scenario:
        /// Calling the POST operation for DeletePolicy to perform a valid deletion
        /// Input:
        /// List of four rules for deletion spread accross 2 policy files of the app org1/app8 and error/postgrewritechangefail between for a single offeredby/coveredby combination.
        /// Expected Result:
        /// Nothing are deleted and 500 status code is returned
        /// Success Criteria:
        /// returns status code 500
        /// </summary>
        [Fact]
        public async Task Post_DeletePolicies_DuplicatePoliciesDefinedInput()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/DeletePolicies/ReadOrg1App3-DuplicatePolicyInRequest_50001337_20001337_NoUpdates.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/DeletePolicy", content);

            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            ValidationProblemDetails actual = JsonSerializer.Deserialize<ValidationProblemDetails>(responseContent, options);
            string errorMessage = actual.Errors.Values.FirstOrDefault()[0];
            Assert.Equal("Input should not contain any duplicate policies", errorMessage);
        }

        /// <summary>
        /// Test case: Calling the POST operation for DeletePolicy to perform a invalid deletion of org1/app3 with the same policy defined twice
        /// Expected: DeletePolicy returns status code 500
        /// 
        /// Scenario:
        /// Calling the POST operation for DeletePolicy to perform a valid deletion
        /// Input:
        /// List of four rules for deletion spread accross 2 policy files of the app org1/app8 and error/postgrewritechangefail between for a single offeredby/coveredby combination.
        /// Expected Result:
        /// Nothing are deleted and 500 status code is returned
        /// Success Criteria:
        /// returns status code 500
        /// </summary>
        [Fact]
        public async Task Post_DeletePolicies_EmptyInput()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/DeletePolicies/EmptyInput.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/DeletePolicy", content);

            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            ValidationProblemDetails actual = JsonSerializer.Deserialize<ValidationProblemDetails>(responseContent, options);
            string errorMessage = actual.Errors.Values.FirstOrDefault()[0];
            Assert.Equal("A non-empty request body is required.", errorMessage);
        }

        /// <summary>
        /// Scenario:
        /// Calling the POST operation for AddRules to without AccessToken
        /// Expected Result:
        /// Call should return Unauthorized
        /// Success Criteria:
        /// AddRules returns status code 401 Unauthorized
        /// </summary>
        [Fact]
        public async Task Post_AddRules_Unauthorized()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/AddRules/ReadWriteOrg1App1_50001337_20001336.json");
            StreamContent content = new StreamContent(dataStream);
            _client.DefaultRequestHeaders.Remove("Authorization");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/addrules", content);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        /// <summary>
        /// Scenario:
        /// Calling the POST operation for AddRules to without any rules specified in the body
        /// Expected Result:
        /// Call should return Badrequest
        /// Success Criteria:
        /// AddRules returns status code 400 Badrequest
        /// </summary>
        [Fact]
        public async Task Post_AddRules_Badrequest_NoRules()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/AddRules/EmptyRuleModel.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/addrules", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// Scenario:
        /// Calling the POST operation for AddRules to with invalid rule model
        /// Expected Result:
        /// Call should return Badrequest
        /// Success Criteria:
        /// AddRules returns status code 400 Badrequest
        /// </summary>
        [Fact]
        public async Task Post_AddRules_Badrequest_InvalidModel()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/AddRules/InvalidRuleModel.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/addrules", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// Scenario:
        /// Calling the POST operation for AddRules to perform a valid delegation
        /// Input:
        /// List of two rules for delegation of the app org1/app1 between for a single offeredby/coveredby combination resulting in a single delegation policy.
        /// Expected Result:
        /// Rules are created and returned with the CreatedSuccessfully flag set and rule ids
        /// Success Criteria:
        /// AddRules returns status code 201 and list of rules created match expected
        /// </summary>
        [Fact]
        public async Task Post_AddRules_Success()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/AddRules/ReadWriteOrg1App1_50001337_20001336.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(20001337, 50001337, "20001336", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", "org1", "app1", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(20001337, 50001337, "20001336", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "write", "org1", "app1", createdSuccessfully: true),
            };

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/addrules", content);

            string responseContent = await response.Content.ReadAsStringAsync();
            List<Rule> actual = JsonSerializer.Deserialize<List<Rule>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.True(actual.TrueForAll(a => a.CreatedSuccessfully));
            Assert.True(actual.TrueForAll(a => !string.IsNullOrEmpty(a.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
            foreach (Rule rule in actual)
            {
                Assert.True(Guid.TryParse(rule.RuleId, out _));
            }
        }

        /// <summary>
        /// Scenario:
        /// Calling the POST operation for AddRules to perform a valid delegation
        /// Input:
        /// List of one rule for delegation of the resourceregistry resource2 between for a single offeredby/coveredby combination resulting in a single delegation policy.
        /// Expected Result:
        /// Rules are created and returned with the CreatedSuccessfully flag set and rule ids
        /// Success Criteria:
        /// AddRules returns status code 201 and list of rules created match expected
        /// </summary>
        [Fact]
        public async Task Post_AddRules_DelegatedByParty_Success()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/AddRules/ScopeaccessResourceRegistryId_50001337_20001337.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(50001337, 50001337, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "scopeaccess", null, null, createdSuccessfully: true, resourceRegistryId: "resource2", delegatedByParty: true),
            };

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/addrules", content);

            string responseContent = await response.Content.ReadAsStringAsync();
            List<Rule> actual = JsonSerializer.Deserialize<List<Rule>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.True(actual.TrueForAll(a => a.CreatedSuccessfully));
            Assert.True(actual.TrueForAll(a => !string.IsNullOrEmpty(a.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
            foreach (Rule rule in actual)
            {
                Assert.True(Guid.TryParse(rule.RuleId, out _));
            }
        }

        /// <summary>
        /// Scenario:
        /// Calling the POST operation for AddRules to perform a valid delegation
        /// Input:
        /// List of two rules for delegation of the resource between for a single offeredby/coveredby combination resulting in a single delegation policy.
        /// Expected Result:
        /// Rules are created and returned with the CreatedSuccessfully flag set and rule ids
        /// Success Criteria:
        /// AddRules returns status code 201 and list of rules created match expected
        /// </summary>
        [Fact]
        public async Task Post_AddRules_With_ResourceRegistryId_Success()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/AddRules/ReadWriteResourceregistryId_50001337_20001336.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(20001337, 50001337, "20001336", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", null, null, createdSuccessfully: true, resourceRegistryId: "resource1"),
                TestDataUtil.GetRuleModel(20001337, 50001337, "20001336", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "write", null, null, createdSuccessfully: true, resourceRegistryId: "resource1"),
            };

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/addrules", content);

            string responseContent = await response.Content.ReadAsStringAsync();
            List<Rule> actual = JsonSerializer.Deserialize<List<Rule>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.True(actual.TrueForAll(a => a.CreatedSuccessfully));
            Assert.True(actual.TrueForAll(a => !string.IsNullOrEmpty(a.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
            foreach (Rule rule in actual)
            {
                Assert.True(Guid.TryParse(rule.RuleId, out _));
            }
        }

        /// <summary>
        /// Scenario:
        /// Calling the POST operation for AddRules to perform a valid delegation
        /// Input:
        /// List of two rules for delegation of the app org1/app3 between for a single offeredby/coveredby combination resulting in a single delegation policy.
        /// Expected Result:
        /// Rules are created and returned with the CreatedSuccessfully flag set and rule ids but since the delegation is already existing the RuleId is known before delegating as they are already existing in the Xacml file
        /// Success Criteria:
        /// AddRules returns status code 201 and list of rules created match expected
        /// </summary>
        [Fact]
        public async Task Post_AddRules_DuplicateSuccess()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/AddRules/ReadWriteOrg1App3_50001337_20001337.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            Rule rule1 = TestDataUtil.GetRuleModel(20001336, 50001337, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", "org1", "app3", createdSuccessfully: true);
            rule1.RuleId = "0d0c8570-64fb-49f9-9f7d-45c057fddf94";
            rule1.Type = RuleType.DirectlyDelegated;
            Rule rule2 = TestDataUtil.GetRuleModel(20001336, 50001337, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "write", "org1", "app3", createdSuccessfully: true);
            rule2.RuleId = "6f11dd0b-5e5d-4bd1-85f0-9796300dfded";
            rule2.Type = RuleType.DirectlyDelegated;

            List<Rule> expected = new List<Rule> { rule1, rule2 };

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/addrules", content);

            string responseContent = await response.Content.ReadAsStringAsync();
            List<Rule> actual = JsonSerializer.Deserialize<List<Rule>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.True(actual.TrueForAll(a => a.CreatedSuccessfully));
            Assert.True(actual.TrueForAll(a => !string.IsNullOrEmpty(a.RuleId)));
            AssertionUtil.AssertEqual(expected, actual, true);
        }

        /// <summary>
        /// Scenario:
        /// Calling the POST operation for AddRules to perform a valid delegation
        /// Input:
        /// List of two rules for delegation of the resource for a single offeredby/coveredby combination resulting in a single delegation policy.
        /// Expected Result:
        /// Rules are created and returned with the CreatedSuccessfully flag set and rule ids but since the delegation is already existing the RuleId is known before delegating as they are already existing in the Xacml file
        /// Success Criteria:
        /// AddRules returns status code 201 and list of rules created match expected
        /// </summary>
        [Fact]
        public async Task Post_AddRules_With_RegistryResource_DuplicateSuccess()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/AddRules/ReadWriteResourceregistryId_50001337_20001337.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            Rule rule1 = TestDataUtil.GetRuleModel(20001336, 50001337, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", null, null, createdSuccessfully: true, resourceRegistryId: "resource1");
            rule1.RuleId = "57b3ee85-f932-42c6-9ab0-941eb6c96eb0";
            rule1.Type = RuleType.DirectlyDelegated;
            Rule rule2 = TestDataUtil.GetRuleModel(20001336, 50001337, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "write", null, null, createdSuccessfully: true, resourceRegistryId: "resource1");
            rule2.RuleId = "99e5cced-3bcb-42b6-9089-63c834f89e77";
            rule2.Type = RuleType.DirectlyDelegated;

            List<Rule> expected = new List<Rule> { rule1, rule2 };

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/addrules", content);

            string responseContent = await response.Content.ReadAsStringAsync();
            List<Rule> actual = JsonSerializer.Deserialize<List<Rule>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.True(actual.TrueForAll(a => a.CreatedSuccessfully));
            Assert.True(actual.TrueForAll(a => !string.IsNullOrEmpty(a.RuleId)));
            AssertionUtil.AssertEqual(expected, actual, true);
        }

        /// <summary>
        /// Scenario:
        /// Calling the POST operation for AddRules to perform a valid delegation
        /// Input:
        /// List of 4 rules for delegation of from 4 different offeredBys to 4 different coveredBys for 4 different apps. Resulting in 4 different delegation policy files
        /// Expected Result:
        /// Rules are created and returned with the CreatedSuccessfully flag set and rule ids
        /// Success Criteria:
        /// AddRules returns status code 201 and list of rules created match expected
        /// </summary>
        [Fact]
        public async Task Post_AddRules_MultipleAppsOfferedBysAndCoveredBys_Success()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/AddRules/MultipleAppsOfferedBysAndCoveredBys.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(20001337, 50001337, "20001336", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", "org1", "app1", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(20001337, 50001337, "50001336", AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, "write", "org1", "app2", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(20001336, 50001336, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", "org2", "app1", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(20001336, 50001336, "50001337", AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, "write", "org2", "app2", createdSuccessfully: true),
            };

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/addrules", content);

            string responseContent = await response.Content.ReadAsStringAsync();
            List<Rule> actual = JsonSerializer.Deserialize<List<Rule>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.True(actual.TrueForAll(a => a.CreatedSuccessfully));
            Assert.True(actual.TrueForAll(a => !string.IsNullOrEmpty(a.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
        }

        /// <summary>
        /// Scenario:
        /// Calling the POST operation for AddRules to perform a partially valid delegation
        /// Input:
        /// List of 4 rules for delegation of from 4 different offeredBys to 4 different coveredBys for 4 different apps. Resulting in 4 different delegation policy files. 1 of the rules are for an app which does not exist
        /// Expected Result:
        /// 3 Rules are created and returned with the CreatedSuccessfully flag set and rule ids
        /// 1 Rule is not created and returned with the CreatedSuccessfully flag set to false and no rule id
        /// Success Criteria:
        /// AddRules returns status code 206 and list of rules created match expected
        /// </summary>
        [Fact]
        public async Task Post_AddRules_OneInvalidApp_PartialSuccess()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/AddRules/OneOutOfFourInvalidApp.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(20001337, 50001337, "20001336", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", "org1", "app1", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(20001337, 50001337, "50001336", AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, "write", "org1", "INVALIDAPPNAME", createdSuccessfully: false),
                TestDataUtil.GetRuleModel(20001336, 50001336, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", "org2", "app1", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(20001336, 50001336, "50001337", AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, "write", "org2", "app2", createdSuccessfully: true),
            };

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/addrules", content);

            string responseContent = await response.Content.ReadAsStringAsync();
            List<Rule> actual = JsonSerializer.Deserialize<List<Rule>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
            Assert.Equal(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                AssertionUtil.AssertEqual(expected[i], actual[i]);
            }
        }

        /// <summary>
        /// Scenario:
        /// Calling the POST operation for AddRules to perform a partially valid delegation
        /// Input:
        /// List of 4 rules for delegation of from 4 different offeredBys to 4 different coveredBys for 4 different apps. Resulting in 4 different delegation policy files. 1 of the rules are incomplete (missing org/app resource specification)
        /// Expected Result:
        /// 3 Rules are created and returned with the CreatedSuccessfully flag set and rule ids
        /// 1 Rule is not created and returned with the CreatedSuccessfully flag set to false and no rule id
        /// Success Criteria:
        /// AddRules returns status code 206 and list of rules created match expected
        /// </summary>
        [Fact]
        public async Task Post_AddRules_OneIncompleteInput_MissingOrgApp_PartialSuccess()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/AddRules/OneOutOfFourIncompleteApp.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            Rule invalidRule = TestDataUtil.GetRuleModel(20001337, 50001337, "50001336", AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, "write", null, null, createdSuccessfully: false);
            invalidRule.Resource = new List<AttributeMatch>();
            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(20001337, 50001337, "20001336", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", "org1", "app1", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(20001336, 50001336, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", "org2", "app1", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(20001336, 50001336, "50001337", AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, "write", "org2", "app2", createdSuccessfully: true),
                invalidRule,
            };

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/addrules", content);

            string responseContent = await response.Content.ReadAsStringAsync();
            List<Rule> actual = JsonSerializer.Deserialize<List<Rule>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
            Assert.Equal(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                AssertionUtil.AssertEqual(expected[i], actual[i]);
            }
        }

        /// <summary>
        /// Scenario:
        /// Calling the POST operation for AddRules to perform a valid delegation, but pushing the delegation event to the queue fails.
        /// Input:
        /// List with a rule for delegation of the app error/delegationeventfail between for a single offeredby/coveredby combination resulting in a single delegation policy.
        /// Expected Result:
        /// Internal exception cause pushing delegation event to fail, after delegation has been stored.
        /// Success Criteria:
        /// AddRules returns status code Created, but a Critical Error has been logged
        /// </summary>
        [Fact]
        public async Task Post_AddRules_DelegationEventQueue_Push_Exception()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/AddRules/DelegationEventError.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(20001337, 50001337, "20001336", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", "error", "delegationeventfail", createdSuccessfully: true)
            };

            // Act
            HttpResponseMessage response = await _client.PostAsync("accessmanagement/api/v1/delegations/addrules", content);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Rule> actual = JsonSerializer.Deserialize<List<Rule>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.True(actual.TrueForAll(a => a.CreatedSuccessfully));
            Assert.True(actual.TrueForAll(a => !string.IsNullOrEmpty(a.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
            foreach (Rule rule in actual)
            {
                Assert.True(Guid.TryParse(rule.RuleId, out _));
            }
        }

        /// <summary>
        /// Test case: GetRules returns a list of rules offeredby has given coveredby
        /// Expected: GetRules returns a list of rules offeredby has given coveredby
        /// </summary>
        [Fact]
        public async Task GetRules_RuleType_Is_DirectlyDelegated()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/GetRules/GetRules_SuccessRequest.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            List<Rule> expectedRules = GetExpectedRulesForUser();

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/delegations/getrules", content);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Rule> actualRules = JsonSerializer.Deserialize<List<Rule>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRules, actualRules, AssertionUtil.AssertRuleEqual);
        }

        /// <summary>
        /// Test case: GetRules returns a list of rules that have been inherited by the recipient via keyrole
        /// Expected: GetRules returns a list of rules offeredby's main unit has given to the recipient via keyrole
        /// </summary>
        [Fact]
        public async Task GetRules_RuleType_Is_InheritedViaKeyRole()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/GetRules/GetRules_RuleTypeInheritedViaKeyRoleRequest.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            List<Rule> expectedRules = new List<Rule>();
            expectedRules.Add(TestDataUtil.GetRuleModel(20001337, 50001337, "50001338", AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, "sign", "skd", "taxreport", ruleType: RuleType.InheritedViaKeyRole));

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/delegations/getrules", content);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Rule> actualRules = JsonSerializer.Deserialize<List<Rule>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRules, actualRules, AssertionUtil.AssertRuleEqual);
        }

        /// <summary>
        /// Test case: GetRules returns a list of rules that have been inherited by the subunit recipient via keyrole
        /// Expected: GetRules returns a list of rules offeredby's main unit has given to the subunit recipient via keyrole
        /// </summary>
        [Fact]
        public async Task GetRules_RuleType_Is_InheritedAsSubunitViaKeyrole()
        {
            // Arrange
            Assert.True(File.Exists("Data/Json/GetRules/GetRules_RuleTypeInheritedAsSubunitViaKeyroleRequest.json"));
            Stream dataStream = File.OpenRead("Data/Json/GetRules/GetRules_RuleTypeInheritedAsSubunitViaKeyroleRequest.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            List<Rule> expectedRules = new List<Rule>();
            expectedRules.Add(TestDataUtil.GetRuleModel(20001339, 50001335, "50001337", AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, "read", "skd", "taxreport", ruleType: RuleType.InheritedAsSubunitViaKeyrole));

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/delegations/getrules", content);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Rule> actualRules = JsonSerializer.Deserialize<List<Rule>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRules, actualRules, AssertionUtil.AssertRuleEqual);
        }

        /// <summary>
        /// Test case: GetRules returns rule that is inherited by a subunit from the main unit
        /// Expected: GetRules returns a list of rules the subunit has received from the main unit
        /// </summary>
        [Fact]
        public async Task GetRules_RuleType_Is_InheritedAsSubunit()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/GetRules/GetRules_RuleTypeInheritedAsSubunitRequest.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            List<Rule> expectedRules = new List<Rule>();
            expectedRules.Add(TestDataUtil.GetRuleModel(20001339, 50001338, "50001336", AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, "sign", "skd", "taxreport", ruleType: RuleType.InheritedAsSubunit));

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/delegations/getrules", content);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Rule> actualRules = JsonSerializer.Deserialize<List<Rule>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRules, actualRules, AssertionUtil.AssertRuleEqual);
        }

        /// <summary>
        /// Test case: GetRules with missing values in the request
        /// Expected: GetRules returns a BadRequest response
        /// </summary>
        [Fact]
        public async Task GetRules_MissingValuesInRequest()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/GetRules/GetRules_MissingValuesInRequestRequest.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/delegations/getrules", content);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// Test case: GetRules with missing values in the request
        /// Expected: GetRules returns a BadRequest response
        /// </summary>
        [Fact]
        public async Task GetRules_MissingOfferedByInRequest()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/GetRules/GetRules_MissingOfferedByInRequestRequest.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/delegations/getrules", content);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// Test case: GetRules for a coveredby that does not have any rules
        /// Expected: GetRules returns an empty list
        /// </summary>
        [Fact]
        public async Task GetRules_NoRulesRequest()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/GetRules/GetRules_NoRulesRequest.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            List<Rule> expectedRules = new List<Rule>();

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/delegations/getrules", content);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Rule> actualRules = JsonSerializer.Deserialize<List<Rule>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRules, actualRules, AssertionUtil.AssertRuleEqual);
        }

        /// <summary>
        /// Test case: GetRules returns a list of rules offeredby has given two coveredbys (a userid and partyid)
        /// Expected: GetRules returns a list of rules offeredby has given coveredby
        /// </summary>
        [Fact]
        public async Task GetRules_WithKeyRolePartyIdsSuccess()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Json/GetRules/GetRules_UsingkeyRolePartyIdsRequest.json");
            StreamContent content = new StreamContent(dataStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            List<Rule> expectedRules = GetExpectedRulesForUser();
            expectedRules.AddRange(GetExpectedRulesForParty());

            // Act
            HttpResponseMessage response = await _client.PostAsync($"accessmanagement/api/v1/delegations/getrules", content);
            string responseContent = await response.Content.ReadAsStringAsync();
            List<Rule> actualRules = JsonSerializer.Deserialize<List<Rule>>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedRules, actualRules, AssertionUtil.AssertRuleEqual);
        }

        /// <summary>
        /// Test case: GetMaskinportenSchemaDelegations returns a list of delegations between supplier and consumer for a given scope.
        ///            Token is authorized for admin scope and and can lookup delegations even when scope is not in the consumers owned scope-prefixes (consumer_prefix)
        /// Expected: GetMaskinportenSchemaDelegations returns a list of delegations offered by supplier to consumer for a given scope
        /// </summary>
        [Fact]
        public async Task GetMaskinportenSchemaDelegations_Admin_Valid()
        {
            // Arrange
            string token = PrincipalUtil.GetOrgToken("DIGDIR", "991825827", "altinn:maskinporten/delegations.admin");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            List<string> resourceIds = new List<string>
            {
                "nav_aa_distribution",
                "appid-123"
            };
            List<MPDelegationExternal> expectedDelegations = GetExpectedMaskinportenSchemaDelegations("810418672", "810418192", resourceIds);

            // Act
            int supplierOrg = 810418672;
            int consumerOrg = 810418192;
            string scope = "altinn:test/theworld.write";
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/admin/delegations/maskinportenschema/?supplierorg={supplierOrg}&consumerorg={consumerOrg}&scope={scope}");
            string responseContent = await response.Content.ReadAsStringAsync();
            List<MPDelegationExternal> actualDelegations = JsonSerializer.Deserialize<List<MPDelegationExternal>>(responseContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedDelegations, actualDelegations, AssertionUtil.AssertDelegationEqual);
        }

        /// <summary>
        /// Test case: GetMaskinportenSchemaDelegations returns a list of delegations between supplier and consumer for a given scope.
        ///            Token is authorized for admin scope and and can lookup delegations even when scope is not in the consumers owned scope-prefixes (consumer_prefix)
        /// Expected: GetMaskinportenSchemaDelegations returns a list of delegations offered by supplier to consumer for a given scope
        /// </summary>
        [Fact]
        public async Task GetMaskinportenSchemaDelegations_ServiceOwnerLookup_Valid()
        {
            // Arrange
            string token = PrincipalUtil.GetOrgToken("DIGDIR", "991825827", "altinn:maskinporten/delegations.admin");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            List<string> resourceIds = new List<string>
            {
                "nav_aa_distribution",
                "appid-123"
            };
            List<MPDelegationExternal> expectedDelegations = GetExpectedMaskinportenSchemaDelegations("810418672", "810418192", resourceIds);

            // Act
            int supplierOrg = 810418672;
            int consumerOrg = 810418192;
            string scope = "altinn:test/theworld.write";
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/admin/delegations/maskinportenschema/?supplierorg={supplierOrg}&consumerorg={consumerOrg}&scope={scope}");
            string responseContent = await response.Content.ReadAsStringAsync();
            List<MPDelegationExternal> actualDelegations = JsonSerializer.Deserialize<List<MPDelegationExternal>>(responseContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertionUtil.AssertCollections(expectedDelegations, actualDelegations, AssertionUtil.AssertDelegationEqual);
        }

        /// <summary>
        /// Test case: GetMaskinportenSchemaDelegations with a scope with altinn prefix, which the serviceowner skd is not authorized for
        /// Expected: GetMaskinportenSchemaDelegations returns forbidden
        /// </summary>
        [Fact]
        public async Task GetMaskinportenSchemaDelegations_ServiceOwnerLookup_UnauthorizedScope()
        {
            // Arrange
            string token = PrincipalUtil.GetOrgToken("SKD", "974761076", "altinn:maskinporten/delegations", new[] { "skd" });
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string expected = "Not authorized for lookup of delegations for the scope: altinn:instances.read";

            // Act
            int supplierOrg = 810418362;
            int consumerOrg = 810418532;
            string scope = "altinn:instances.read";
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/admin/delegations/maskinportenschema/?supplierorg={supplierOrg}&consumerorg={consumerOrg}&scope={scope}");
            string responseContent = await response.Content.ReadAsStringAsync();
            ProblemDetails errorResponse = JsonSerializer.Deserialize<ValidationProblemDetails>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(expected, errorResponse.Title);
        }

        /// <summary>
        /// Test case: GetMaskinportenSchemaDelegations for orgnummer that does not have any delegations
        /// Expected: GetMaskinportenSchemaDelegations returns ok, no delegations found
        /// </summary>
        [Fact]
        public async Task GetMaskinportenSchemaDelegations_Admin_Valid_DelegationsEmpty()
        {
            // Arrange
            string token = PrincipalUtil.GetOrgToken("DIGDIR", "991825827", "altinn:maskinporten/delegations.admin");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string expected = "[]";

            // Act
            int supplierOrg = 810418362;
            int consumerOrg = 810418532;
            string scope = "altinn:test/theworld.write";
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/admin/delegations/maskinportenschema/?supplierorg={supplierOrg}&consumerorg={consumerOrg}&scope={scope}");
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(expected, responseContent);
        }

        /// <summary>
        /// Test case: GetMaskinportenSchemaDelegations without sending scopes
        /// Expected: GetMaskinportenSchemaDelegations returns badrequest
        /// </summary>
        [Fact]
        public async Task GetMaskinportenSchemaDelegations_Admin_MissingScope()
        {
            // Arrange
            string token = PrincipalUtil.GetOrgToken("DIGDIR", "991825827", "altinn:maskinporten/delegations.admin");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string expected = "The scope field is required.";

            // Act
            int supplierOrg = 810418362;
            int consumerOrg = 810418532;
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/admin/delegations/maskinportenschema/?supplierorg={supplierOrg}&consumerorg={consumerOrg}&scope=");
            string responseContent = await response.Content.ReadAsStringAsync();
            ValidationProblemDetails errorResponse = JsonSerializer.Deserialize<ValidationProblemDetails>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(expected, errorResponse.Errors["scope"][0]);
        }

        /// <summary>
        /// Test case: GetMaskinportenSchemaDelegations for invalid supplier orgnummer
        /// Expected: GetMaskinportenSchemaDelegations returns badrequest
        /// </summary>
        [Fact]
        public async Task GetMaskinportenSchemaDelegations_Admin_InvalidSupplier()
        {
            // Arrange
            string token = PrincipalUtil.GetOrgToken("DIGDIR", "991825827", "altinn:maskinporten/delegations.admin");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string expected = "Supplierorg is not an valid organization number";

            // Act
            string supplierOrg = "12345";
            string consumerOrg = "810418532";
            string scope = "altinn:test/theworld.write";
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/admin/delegations/maskinportenschema/?supplierorg={supplierOrg}&consumerorg={consumerOrg}&scope={scope}");
            string responseContent = await response.Content.ReadAsStringAsync();
            ValidationProblemDetails errorResponse = JsonSerializer.Deserialize<ValidationProblemDetails>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(expected, errorResponse.Errors["supplierOrg"][0]);
        }

        /// <summary>
        /// Test case: GetMaskinportenSchemaDelegations for invalid consumer orgnummer
        /// Expected: GetMaskinportenSchemaDelegations returns badrequest
        /// </summary>
        [Fact]
        public async Task GetMaskinportenSchemaDelegations_Admin_InvalidConsumer()
        {
            // Arrange
            string token = PrincipalUtil.GetOrgToken("DIGDIR", "991825827", "altinn:maskinporten/delegations.admin");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string expected = "Consumerorg is not an valid organization number";

            // Act
            string supplierOrg = "810418362";
            string consumerOrg = "12345";
            string scope = "altinn:test/theworld.write";
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/admin/delegations/maskinportenschema/?supplierorg={supplierOrg}&consumerorg={consumerOrg}&scope={scope}");
            string responseContent = await response.Content.ReadAsStringAsync();
            ValidationProblemDetails errorResponse = JsonSerializer.Deserialize<ValidationProblemDetails>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(expected, errorResponse.Errors["consumerOrg"][0]);
        }

        /// <summary>
        /// Test case: GetMaskinportenSchemaDelegations for a scope which is not a registered reference on any resources
        /// Expected: GetMaskinportenSchemaDelegations returns ok, no delegations found
        /// </summary>
        [Fact]
        public async Task GetMaskinportenSchemaDelegations_Admin_ScopesNotRegisteredOnResource()
        {
            // Arrange
            string token = PrincipalUtil.GetOrgToken("DIGDIR", "991825827", "altinn:maskinporten/delegations.admin");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string expected = "[]";

            // Act
            int supplierOrg = 810418672;
            int consumerOrg = 810418192;
            string scope = "altinn:test/test";
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/admin/delegations/maskinportenschema/?supplierorg={supplierOrg}&consumerorg={consumerOrg}&scope={scope}");
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expected, responseContent.Replace('"', ' ').Trim());
        }

        /// <summary>
        /// Test case: GetMaskinportenSchemaDelegations for an invalid scope format
        /// Expected: GetMaskinportenSchemaDelegations returns badrequest
        /// </summary>
        [Fact]
        public async Task GetMaskinportenSchemaDelegations_Admin_InvalidScopeFormat()
        {
            // Arrange
            string token = PrincipalUtil.GetOrgToken("DIGDIR", "991825827", "altinn:maskinporten/delegations.admin");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string expected = "Is not well formatted: test invalid scope (Parameter 'scope')";

            // Act
            int supplierOrg = 810418672;
            int consumerOrg = 810418192;
            string scope = "test invalid scope";
            HttpResponseMessage response = await _client.GetAsync($"accessmanagement/api/v1/admin/delegations/maskinportenschema/?supplierorg={supplierOrg}&consumerorg={consumerOrg}&scope={scope}");
            string responseContent = await response.Content.ReadAsStringAsync();
            ValidationProblemDetails errorResponse = JsonSerializer.Deserialize<ValidationProblemDetails>(responseContent, options);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(expected, errorResponse.Errors["scope"][0]);
        }

        private static List<Rule> GetExpectedRulesForUser()
        {
            List<Rule> list = new List<Rule>();
            list.Add(TestDataUtil.GetRuleModel(20001337, 50001337, "20001336", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", "skd", "taxreport", ruleType: RuleType.DirectlyDelegated));
            list.Add(TestDataUtil.GetRuleModel(20001337, 50001337, "20001336", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "write", "skd", "taxreport", ruleType: RuleType.DirectlyDelegated));
            return list;
        }

        private static List<Rule> GetExpectedRulesForParty()
        {
            List<Rule> list = new List<Rule>();
            list.Add(TestDataUtil.GetRuleModel(20001337, 50001337, "50001336", AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, "sign", "skd", "taxreport", ruleType: RuleType.InheritedViaKeyRole));
            return list;
        }

        private static List<MPDelegationExternal> GetExpectedMaskinportenSchemaDelegations(string supplierOrg, string consumerOrg, List<string> resourceIds)
        {
            List<MPDelegationExternal> delegations = new List<MPDelegationExternal>();
            delegations = TestDataUtil.GetAdminDelegations(supplierOrg, consumerOrg, resourceIds);
            return delegations;
        }

        private HttpClient GetTestClient(IPDP pdpMock = null, IHttpContextAccessor httpContextAccessor = null, IDelegationMetadataRepository delegationMetadataRepositoryMock = null)
        {
            pdpMock ??= new PepWithPDPAuthorizationMock();
            httpContextAccessor ??= new HttpContextAccessor();
            delegationMetadataRepositoryMock ??= new DelegationMetadataRepositoryMock();

            HttpClient client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IPolicyRetrievalPoint, PolicyRetrievalPointMock>();
                    services.AddSingleton(delegationMetadataRepositoryMock);
                    services.AddSingleton<IPolicyFactory, PolicyFactoryMock>();
                    services.AddSingleton<IDelegationChangeEventQueue, DelegationChangeEventQueueMock>();
                    services.AddSingleton<IPostConfigureOptions<JwtCookieOptions>, JwtCookiePostConfigureOptionsStub>();
                    services.AddSingleton<IPartiesClient, PartiesClientMock>();
                    services.AddSingleton<IResourceRegistryClient, ResourceRegistryClientMock>();
                    services.AddSingleton(pdpMock);
                    services.AddSingleton(httpContextAccessor);
                    services.AddSingleton<IAltinnRolesClient, AltinnRolesClientMock>();
                    services.AddSingleton<IPublicSigningKeyProvider, SigningKeyResolverMock>();
                });
            }).CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            return client;
        }
    }
}
