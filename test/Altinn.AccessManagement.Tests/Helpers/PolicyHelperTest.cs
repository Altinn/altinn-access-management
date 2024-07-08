using System.Collections.Generic;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Enums;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.AccessManagement.Tests.Utils;
using Altinn.Authorization.ABAC.Xacml;
using Xunit;

namespace Altinn.AccessManagement.Tests.Helpers
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
        public async Task GetRolesWithAccess()
        {
            // Arrange
            XacmlPolicy policy = await _policyRetrievalPointMock.GetPolicyAsync("resource1");

            List<string> expected = TestDataUtil.GetRolesWithAccess();

            // Act
            List<string> actual = PolicyHelper.GetRolesWithAccess(policy);

            // Assert
            Assert.Equal(actual, expected);
        }

        /// <summary>
        /// Scenario:
        /// Tests that the GetDelegationPolicyPath method throws the expected argument exception when org does not have a value
        /// Input:
        /// ResourceId is null because this is a org/app scenario but only app has been provided as part of the resource
        /// Expected Result:
        /// Argument exception thrown
        /// Success Criteria:
        /// Argument exception has the expected error message
        /// </summary>
        [Fact]
        public void GetDelegationPolicyPath_OrgEmpty()
        {
            // Arrange
            string expectedArgumentException = "Org was not defined";

            // Act
            string actual = string.Empty;
            try
            {
                PolicyHelper.GetDelegationPolicyPath(ResourceAttributeMatchType.AltinnAppId, null, null, "app", "50001337", 20001337, null, null, UuidType.NotSpecified);
            }
            catch (System.ArgumentException argEx)
            {
                actual = argEx.Message;
            }

            // Assert
            Assert.Equal(expectedArgumentException, actual);
        }

        /// <summary>
        /// Scenario:
        /// Tests that the GetDelegationPolicyPath method throws the expected argument exception when app does not have a value
        /// Input:
        /// ResourceId is null because this is a org/app scenario but only org has been provided as part of the resource
        /// Expected Result:
        /// Argument exception thrown
        /// Success Criteria:
        /// Argument exception has the expected error message
        /// </summary>
        [Fact]
        public void GetDelegationPolicyPath_AppEmpty()
        {
            // Arrange
            string expectedArgumentException = "App was not defined";

            // Act
            string actual = string.Empty;
            try
            {
                PolicyHelper.GetDelegationPolicyPath(ResourceAttributeMatchType.AltinnAppId, null, "org", string.Empty, "50001337", 20001337, null, null, UuidType.NotSpecified);
            }
            catch (System.ArgumentException argEx)
            {
                actual = argEx.Message;
            }

            // Assert
            Assert.Equal(expectedArgumentException, actual);
        }

        /// <summary>
        /// Scenario:
        /// Tests that the GetDelegationPolicyPath method throws the expected argument exception when resourceId does not have a value
        /// Input:
        /// Org and App is null because this is a resourceId scenario but resourceId has been provided as an empty string in the delegation
        /// Expected Result:
        /// Argument exception thrown
        /// Success Criteria:
        /// Argument exception has the expected error message
        /// </summary>
        [Fact]
        public void GetDelegationPolicyPath_ResourceIdEmpty()
        {
            // Arrange
            string expectedArgumentException = "ResourceRegistryId was not defined";

            // Act
            string actual = string.Empty;
            try
            {
                PolicyHelper.GetDelegationPolicyPath(ResourceAttributeMatchType.ResourceRegistry, string.Empty, null, null, "50001337", 20001337, null, null, UuidType.NotSpecified);
            }
            catch (System.ArgumentException argEx)
            {
                actual = argEx.Message;
            }

            // Assert
            Assert.Equal(expectedArgumentException, actual);
        }
    }
}
