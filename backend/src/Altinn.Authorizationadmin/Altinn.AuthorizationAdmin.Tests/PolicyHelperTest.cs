using System.Collections.Generic;
using Altinn.Authorization.ABAC.Xacml;
using Altinn.AuthorizationAdmin.Core.Helpers;
using Altinn.AuthorizationAdmin.Tests.Mocks;
using Altinn.AuthorizationAdmin.Tests.Utils;
using Xunit;

namespace Altinn.AuthorizationAdmin.Tests
{
    /// <summary>
    /// Test class for <see cref="PolicyHelper"></see>
    /// </summary>
    public class PolicyHelperTest
    {
        private readonly PolicyRetrievalPointMock _policyRetrievalPointMock;

        /// <summary>
        /// Constructor setting up dependencies
        /// </summary>
        public PolicyHelperTest()
        {
            _policyRetrievalPointMock = new PolicyRetrievalPointMock();
        }

        /// <summary>
        /// Scenario:
        /// Tests that the GetAltinnAppsPolicyPath method returns a correct path based on the input parameters.
        /// Input:
        /// Org and app from the AltinnAppId.
        /// Expected Result:
        /// True
        /// Success Criteria:
        /// Rule is found and expected result is returned
        /// </summary>
        [Fact]
        public void GetAltinnAppsPolicyPath()
        {
            // Arrange
            string expected = $"org1/app1/policy.xml";

            // Act
            string actual = PolicyHelper.GetAltinnAppsPolicyPath("org1", "app1");

            // Assert
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Scenario:
        /// Tests that the GetAltinnAppsPolicyPath method returns a correct path based on the input parameters.
        /// Input:
        /// Org and app from the AltinnAppId.
        /// Expected Result:
        /// True
        /// Success Criteria:
        /// Rule is found and expected result is returned
        /// </summary>
        [Fact]
        public async void GetRolesWithAccess()
        {
            // Arrange
            XacmlPolicy policy = await _policyRetrievalPointMock.GetPolicyAsync("resource1");
            
            List<string> expected = TestDataUtil.GetRolesWithAccess();

            // Act
            List<string> actual = PolicyHelper.GetRolesWithAccess(policy);

            // Assert
            Assert.Equal(actual, expected);
        }
    }
}
