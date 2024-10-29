using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Enums;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.AccessManagement.Tests.Utils;
using Altinn.Authorization.ABAC.Xacml;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Altinn.AccessManagement.Tests.Helpers
{
    /// <summary>
    /// Test class for <see cref="DelegationHelper"></see>
    /// </summary>
    public class DelegationHelperTest
    {
        private PolicyRetrievalPointMock _prpMock = new PolicyRetrievalPointMock(new HttpContextAccessor(), new Mock<ILogger<PolicyRetrievalPointMock>>().Object);

        /// <summary>
        /// Scenario:
        /// Tests the SortRulesByDelegationPolicyPath function
        /// Input:
        /// List of un ordered rules for delegation of 3 different apps to/from the same set of OfferedBy/CoveredBy parties
        /// Expected Result:
        /// Dictionary with rules sorted by the path of the 3 delegation policy files
        /// Success Criteria:
        /// Dictionary with the expected keys (policy paths) and values (sorted rules for each file)
        /// </summary>
        [Fact]
        public void SortRulesByDelegationPolicyPath_ThreeAppsSameOfferedByAndCoveredBy_Success()
        {
            // Arrange
            int delegatedByUserId = 20001336;
            int offeredByPartyId = 50001337;
            string coveredBy = "20001337";
            string coveredByType = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute;

            List<Rule> unsortedRules = new List<Rule>
            {
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read", "org1", "app1"),
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read", "org2", "app1"),
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read", "org1", "App2"),
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "write", "org1", "app1", "task1"), // Should be sorted together with the first rule
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read", "org1", "app1", task: null, "event1") // Should be sorted together with the first rule
            };

            Dictionary<string, List<Rule>> expected = new Dictionary<string, List<Rule>>();
            expected.Add($"org1/app1/{offeredByPartyId}/u{coveredBy}/delegationpolicy.xml", new List<Rule>
            {
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read", "org1", "app1"),
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "write", "org1", "app1", "task1"),
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read", "org1", "app1",  task: null, "event1")
            });
            expected.Add($"org2/app1/{offeredByPartyId}/u{coveredBy}/delegationpolicy.xml", new List<Rule>
            {
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read", "org2", "app1")
            });
            expected.Add($"org1/App2/{offeredByPartyId}/u{coveredBy}/delegationpolicy.xml", new List<Rule>
            {
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read", "org1", "App2")
            });

            // Act
            Dictionary<string, List<Rule>> actual = DelegationHelper.SortRulesByDelegationPolicyPath(unsortedRules, out List<Rule> unsortables);

            // Assert
            Assert.NotNull(actual);
            Assert.Empty(unsortables);

            Assert.Equal(expected.Keys.Count, actual.Keys.Count);
            foreach (string expectedPathKey in expected.Keys)
            {
                Assert.True(actual.ContainsKey(expectedPathKey));
                Assert.Equal(expected[expectedPathKey].Count, actual[expectedPathKey].Count);
                AssertionUtil.AssertEqual(expected[expectedPathKey], actual[expectedPathKey]);
            }
        }

        /// <summary>
        /// Scenario:
        /// Tests the SortRulesByDelegationPolicyPath function
        /// Input:
        /// List of un ordered rules for delegation of the same apps from the same OfferedBy to two CoveredBy users, and one coveredBy organization/partyid
        /// Expected Result:
        /// Dictionary with rules sorted by the path of the 3 delegation policy files
        /// Success Criteria:
        /// Dictionary with the expected keys (policy paths) and values (sorted rules for each file)
        /// </summary>
        [Fact]
        public void SortRulesByDelegationPolicyPath_OneAppSameOfferedBy_ThreeCoveredBy_Success()
        {
            // Arrange
            int delegatedByUserId = 20001336;
            int offeredByPartyId = 50001337;

            List<Rule> unsortedRules = new List<Rule>
            {
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", "org1", "app1", "task1"),
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, "20001331", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", "org1", "app1", null, "event1"),
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, "50001333", AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, "read", "org1", "app1"),
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "write", "org1", "app1") // Should be sorted together with the first rule
            };

            Dictionary<string, List<Rule>> expected = new Dictionary<string, List<Rule>>();
            expected.Add($"org1/app1/{offeredByPartyId}/u20001337/delegationpolicy.xml", new List<Rule>
            {
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", "org1", "app1", "task1"),
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "write", "org1", "app1")
            });
            expected.Add($"org1/app1/{offeredByPartyId}/u20001331/delegationpolicy.xml", new List<Rule>
            {
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, "20001331", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", "org1", "app1", null, "event1"),
            });
            expected.Add($"org1/app1/{offeredByPartyId}/p50001333/delegationpolicy.xml", new List<Rule>
            {
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, "50001333", AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, "read", "org1", "app1"),
            });

            // Act
            Dictionary<string, List<Rule>> actual = DelegationHelper.SortRulesByDelegationPolicyPath(unsortedRules, out List<Rule> unsortables);

            // Assert
            Assert.NotNull(actual);
            Assert.Empty(unsortables);

            Assert.Equal(expected.Keys.Count, actual.Keys.Count);
            foreach (string expectedPathKey in expected.Keys)
            {
                Assert.True(actual.ContainsKey(expectedPathKey));
                Assert.Equal(expected[expectedPathKey].Count, actual[expectedPathKey].Count);
                AssertionUtil.AssertEqual(expected[expectedPathKey], actual[expectedPathKey]);
            }
        }

        /// <summary>
        /// Scenario:
        /// Tests the SortRulesByDelegationPolicyPath function
        /// Input:
        /// List of un ordered rules for delegation of the same apps from the same OfferedBy to two CoveredBy users, and one coveredBy organization/partyid
        /// Expected Result:
        /// Dictionary with rules sorted by the path of the 3 delegation policy files
        /// Success Criteria:
        /// Dictionary with the expected keys (policy paths) and values (sorted rules for each file)
        /// </summary>
        [Fact]
        public void SortRulesByDelegationPolicyPath_Unsortables_Success()
        {
            // Arrange
            int delegatedByUserId = 20001336;
            int offeredByPartyId = 50001337;

            List<Rule> unsortedRules = new List<Rule>
            {
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", "org1", "app1", "task1"),
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", null, "app1"),
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "write", "org1", "app1")
            };

            Dictionary<string, List<Rule>> expected = new Dictionary<string, List<Rule>>();
            expected.Add($"org1/app1/{offeredByPartyId}/u20001337/delegationpolicy.xml", new List<Rule>
            {
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", "org1", "app1", "task1"),
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "write", "org1", "app1")
            });

            List<Rule> expectedUnsortable = new List<Rule>
            {
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, "20001337", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", null, "app1")
            };

            // Act
            Dictionary<string, List<Rule>> actual = DelegationHelper.SortRulesByDelegationPolicyPath(unsortedRules, out List<Rule> unsortables);

            // Assert
            Assert.NotNull(actual);

            Assert.Equal(expectedUnsortable.Count, unsortables.Count);
            AssertionUtil.AssertEqual(expectedUnsortable, unsortables);

            Assert.Equal(expected.Keys.Count, actual.Keys.Count);
            foreach (string expectedPathKey in expected.Keys)
            {
                Assert.True(actual.ContainsKey(expectedPathKey));
                Assert.Equal(expected[expectedPathKey].Count, actual[expectedPathKey].Count);
                AssertionUtil.AssertEqual(expected[expectedPathKey], actual[expectedPathKey]);
            }
        }

        /// <summary>
        /// Scenario:
        /// Tests that the PolicyContainsMatchingRule function returns true when it finds a given API rule model as a XacmlRule in a XacmlPolicy
        /// Input:
        /// A XacmlPolicy containing read and write rules for org1/app1, and a API Rule model for write
        /// Expected Result:
        /// True
        /// Success Criteria:
        /// Rule is found and expected result is returned
        /// </summary>
        [Fact]
        public async Task PolicyContainsMatchingRule_PolicyContainsRule_True()
        {
            // Arrange
            Rule rule = TestDataUtil.GetRuleModel(20001337, 50001337, "20001336", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "write", "org1", "app1");
            XacmlPolicy policy = await _prpMock.GetPolicyAsync("org1", "app1");

            // Act
            bool actual = DelegationHelper.PolicyContainsMatchingRule(policy, rule);

            // Assert
            Assert.True(actual);
        }

        /// <summary>
        /// Scenario:
        /// Tests that the PolicyContainsMatchingRule function returns true when it finds a given API rule model as a XacmlRule in a XacmlPolicy
        /// Input:
        /// A XacmlPolicy containing read and write rules for org1/app1, and a API Rule model for read
        /// Expected Result:
        /// True
        /// Success Criteria:
        /// Rule is found and expected result is returned
        /// </summary>
        [Fact]
        public async Task PolicyContainsMatchingRule_PolicyContainsRule_PolicyResourcesOutOfOrder_True()
        {
            // Arrange
            Rule rule = TestDataUtil.GetRuleModel(20001337, 50001337, "20001336", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", "org1", "unorderedresources");
            XacmlPolicy policy = await _prpMock.GetPolicyAsync("org1", "unorderedresources");

            // Act
            bool actual = DelegationHelper.PolicyContainsMatchingRule(policy, rule);

            // Assert
            Assert.True(actual);
        }

        /// <summary>
        /// Scenario:
        /// Tests that the PolicyContainsMatchingRule function returns true when it finds a given API rule model as a XacmlRule in a XacmlPolicy
        /// Input:
        /// A XacmlPolicy containing a single rule for spanning multiple different resources and actions
        /// A rule for one of the last combinations for action and resource (eat, banana) 
        /// Expected Result:
        /// True
        /// Success Criteria:
        /// Rule is found and expected result is returned
        /// </summary>
        [Fact]
        public async Task PolicyContainsMatchingRule_PolicyContainsRule_SingleComplexRulePolicy_True()
        {
            // Arrange
            Rule rule = TestDataUtil.GetRuleModel(20001337, 50001337, "20001336", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "eat", "org1", "singlecomplexrule", appresource: "banana");
            XacmlPolicy policy = await _prpMock.GetPolicyAsync("org1", "singlecomplexrule");

            // Act
            bool actual = DelegationHelper.PolicyContainsMatchingRule(policy, rule);

            // Assert
            Assert.True(actual);
        }

        /// <summary>
        /// Scenario:
        /// Tests that the PolicyContainsMatchingRule function returns true when it finds a given API rule model as a XacmlRule in a XacmlPolicy
        /// Input:
        /// A XacmlPolicy containing XacmlRule for sign on task1 for org1/app1, and a API Rule model representation for the same rule
        /// Expected Result:
        /// True
        /// Success Criteria:
        /// Rule is found and expected result is returned
        /// </summary>
        [Fact]
        public async Task PolicyContainsMatchingRule_PolicyContainsRule_SignForTask_True()
        {
            // Arrange
            Rule rule = TestDataUtil.GetRuleModel(20001337, 50001337, "20001336", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "sign", "org1", "app1", task: "task1");
            XacmlPolicy policy = await _prpMock.GetPolicyAsync("org1", "app1");

            // Act
            bool actual = DelegationHelper.PolicyContainsMatchingRule(policy, rule);

            // Assert
            Assert.True(actual);
        }

        /// <summary>
        /// Scenario:
        /// Tests that the PolicyContainsMatchingRule function returns false when it does not find a given API rule model as a XacmlRule in a XacmlPolicy
        /// Input:
        /// A XacmlPolicy containing read and write rules for org1/app1, and a API Rule model for sign
        /// Expected Result:
        /// False
        /// Success Criteria:
        /// Rule is not found and expected result is returned
        /// </summary>
        [Fact]
        public async Task PolicyContainsMatchingRule_PolicyContainsRule_InvalidAction_False()
        {
            // Arrange
            Rule rule = TestDataUtil.GetRuleModel(20001337, 50001337, "20001336", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "sign", "org1", "app1");
            XacmlPolicy policy = await _prpMock.GetPolicyAsync("org1", "app1");

            // Act
            bool actual = DelegationHelper.PolicyContainsMatchingRule(policy, rule);

            // Assert
            Assert.False(actual);
        }

        /// <summary>
        /// Scenario:
        /// Tests that the PolicyContainsMatchingRule function returns false when it does not find a given API rule model as a XacmlRule in a XacmlPolicy
        /// Input:
        /// A XacmlPolicy containing read and write rules for org1/app1, and a API Rule model for read but for org2/app1
        /// Expected Result:
        /// False
        /// Success Criteria:
        /// Rule is not found and expected result is returned
        /// </summary>
        [Fact]
        public async Task PolicyContainsMatchingRule_PolicyContainsRule_InvalidOrg_False()
        {
            // Arrange
            Rule rule = TestDataUtil.GetRuleModel(20001337, 50001337, "20001336", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", "org2", "app1");
            XacmlPolicy policy = await _prpMock.GetPolicyAsync("org1", "app1");

            // Act
            bool actual = DelegationHelper.PolicyContainsMatchingRule(policy, rule);

            // Assert
            Assert.False(actual);
        }

        /// <summary>
        /// Scenario:
        /// Tests that the PolicyContainsMatchingRule function returns false when it does not find a given API rule model as a XacmlRule in a XacmlPolicy
        /// Input:
        /// A XacmlPolicy containing read and write rules for org1/app1, and a API Rule model for read but for org1/app2
        /// Expected Result:
        /// False
        /// Success Criteria:
        /// Rule is not found and expected result is returned
        /// </summary>
        [Fact]
        public async Task PolicyContainsMatchingRule_PolicyContainsRule_InvalidApp_False()
        {
            // Arrange
            Rule rule = TestDataUtil.GetRuleModel(20001337, 50001337, "20001336", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "read", "org2", "app1");
            XacmlPolicy policy = await _prpMock.GetPolicyAsync("org1", "app1");

            // Act
            bool actual = DelegationHelper.PolicyContainsMatchingRule(policy, rule);

            // Assert
            Assert.False(actual);
        }

        /// <summary>
        /// Scenario:
        /// Tests that the PolicyContainsMatchingRule function returns false when the App policy does not contain org/app level resource specification.
        /// Input:
        /// A XacmlPolicy containing no rules with resource specification on org/app level (all resources are more specific e.g incl task/appresource)
        /// A rule which match action but not a complete resource match
        /// Expected Result:
        /// False
        /// Success Criteria:
        /// Rule is not found and expected result is returned
        /// </summary>
        [Fact]
        public async Task PolicyContainsMatchingRule_PolicyContainsRule_PolicyWithoutAppLevelResource_False()
        {
            // Arrange
            Rule rule = TestDataUtil.GetRuleModel(20001337, 50001337, "20001336", AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "eat", "org1", "singlecomplexrule");
            XacmlPolicy policy = await _prpMock.GetPolicyAsync("org1", "singlecomplexrule");

            // Act
            bool actual = DelegationHelper.PolicyContainsMatchingRule(policy, rule);

            // Assert
            Assert.False(actual);
        }

        /// <summary>
        /// Scenario:
        /// Given an AttributeMatch list parse out the type and Identificator from the list
        /// Input:
        /// Attribute match list containing a SystemUser urn.
        /// Expected Result:
        /// True and Type set to UuidType.SystemUser and Id set to correct Uuid
        /// Success Criteria:
        /// Rule is not found and expected result is returned
        /// </summary>
        [Fact]
        public void ParseSystemUserTypeAndIdentifierFromUrn_Succsess()
        {
            string idString = "56224CB5-E8BF-4569-86EC-6CF104B63F74";
            List<AttributeMatch> input = new List<AttributeMatch>
            {
                new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.SystemUserUuid, Value = idString }
            };

            bool result = DelegationHelper.TryGetPerformerFromAttributeMatches(input, out string id, out UuidType type);
            Assert.True(result);
            Assert.Equal(UuidType.SystemUser, type);
            Assert.Equal(idString, id);
        }

        /// <summary>
        /// Scenario:
        /// Given an AttributeMatch list parse out the type and Identificator from the list
        /// Input:
        /// Attribute match list containing a Organization urn.
        /// Expected Result:
        /// returns True and output Type set to UuidType.Organization and Id set to correct Uuid
        /// </summary>
        [Fact]
        public void ParseOrganizationTypeAndIdentifierFromUrn_Succsess()
        {
            string idString = "9867756B-625E-4904-815E-889A5824C33C";
            List<AttributeMatch> input = new List<AttributeMatch>
            {
                new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationUuid, Value = idString }
            };

            bool result = DelegationHelper.TryGetPerformerFromAttributeMatches(input, out string id, out UuidType type);
            Assert.True(result);
            Assert.Equal(UuidType.Organization, type);
            Assert.Equal(idString, id);
        }

        /// <summary>
        /// Scenario:
        /// Given an AttributeMatch list parse out the type and Identificator from the list
        /// Input:
        /// Attribute match list containing a Person urn.
        /// Expected Result:
        /// returns True and output Type set to UuidType.Person and Id set to correct Uuid
        /// </summary>
        [Fact]
        public void ParsePersonTypeAndIdentifierFromUrn_Succsess()
        {
            string idString = "7514B58F-ABBC-42F7-98EB-11BCE123E757";
            List<AttributeMatch> input = new List<AttributeMatch>
            {
                new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.PersonUuid, Value = idString }
            };

            bool result = DelegationHelper.TryGetPerformerFromAttributeMatches(input, out string id, out UuidType type);
            Assert.True(result);
            Assert.Equal(UuidType.Person, type);
            Assert.Equal(idString, id);
        }

        /// <summary>
        /// Scenario:
        /// Given an AttributeMatch list parse out the type and Identificator from the list
        /// Input:
        /// Attribute match list containing a EnterpriseUser urn.
        /// Expected Result:
        /// returns True and output Type set to UuidType.EnterpriseUser and Id set to correct Uuid
        /// </summary>
        [Fact]
        public void ParseEnterpriseUserTypeAndIdentifierFromUrn_Succsess()
        {
            string idString = "1CF6DFC5-31BC-48F4-A5ED-48711DC0FF4B";
            List<AttributeMatch> input = new List<AttributeMatch>
            {
                new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.EnterpriseUserUuid, Value = idString }
            };

            bool result = DelegationHelper.TryGetPerformerFromAttributeMatches(input, out string id, out UuidType type);
            Assert.True(result);
            Assert.Equal(UuidType.EnterpriseUser, type);
            Assert.Equal(idString, id);
        }

        /// <summary>
        /// Scenario:
        /// Given an AttributeMatch list parse out the type and Identificator from the list
        /// Input:
        /// Attribute match list containing a EnterpriseUser urn.
        /// Expected Result:
        /// returns False and output Type set to UuidType.NotSpecified and Id set to NULL
        /// </summary>
        [Fact]
        public void ParseSsnTypeAndIdentifierFromUrn_Failure()
        {
            string idString = "01010149978";
            List<AttributeMatch> input = new List<AttributeMatch>
            {
                new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.PersonId, Value = idString }
            };

            bool result = DelegationHelper.TryGetPerformerFromAttributeMatches(input, out string id, out UuidType type);
            Assert.False(result);
            Assert.Equal(UuidType.NotSpecified, type);
            Assert.Null(id);
        }
    }
}
