using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.AccessManagement.Tests.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Altinn.AccessManagement.Tests
{
    /// <summary>
    /// Test class for <see cref="PolicyAdministrationPoint"></see>
    /// </summary>
    [Collection("PolicyAdministrationPointTest")]
    public class PolicyAdministrationPointTest
    {
        private readonly IPolicyAdministrationPoint _pap;
        private readonly IPolicyFactory _prp;
        private readonly IDelegationChangeEventQueue _eventQueue;
        private readonly Mock<ILogger<IPolicyAdministrationPoint>> _logger;
        private DelegationMetadataRepositoryMock _delegationMetadataRepositoryMock;

        /// <summary>
        /// Constructor setting up dependencies
        /// </summary>
        public PolicyAdministrationPointTest()
        {
            ServiceCollection services = new ServiceCollection();
            services.AddMemoryCache();
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            IMemoryCache memoryCache = serviceProvider.GetService<IMemoryCache>();

            _logger = new Mock<ILogger<IPolicyAdministrationPoint>>();
            _delegationMetadataRepositoryMock = new DelegationMetadataRepositoryMock();
            _prp = new PolicyFactoryMock(new Mock<ILogger<PolicyRepositoryMock>>().Object);
            _eventQueue = new DelegationChangeEventQueueMock();
            _pap = new PolicyAdministrationPoint(
                new PolicyRetrievalPoint(_prp, memoryCache, Options.Create(new CacheConfig { PolicyCacheTimeout = 1 })),
                _prp,
                _delegationMetadataRepositoryMock,
                _eventQueue,
                _logger.Object);
        }

        /// <summary>
        /// Test case: Write to storage a file.
        /// Expected: WritePolicyAsync returns true.
        /// </summary>
        [Fact]
        public async Task WritePolicy_TC01()
        {
            // Arrange
            Stream dataStream = File.OpenRead("Data/Policies/policy.xml");

            // Act
            bool successfullyStored = await _pap.WritePolicyAsync("org", "app", dataStream);
            SetupUtils.DeleteAppBlobData("org", "app");

            // Assert
            Assert.True(successfullyStored);
        }

        /// <summary>
        /// Test case: Write a file to storage where the org parameter arguments is empty.
        /// Expected: WritePolicyAsync throws ArgumentException.
        /// </summary>
        [Fact]
        public async Task WritePolicy_TC02()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _pap.WritePolicyAsync(string.Empty, "app", new MemoryStream()));
        }

        /// <summary>
        /// Test case: Write a file to storage where the app parameter arguments is empty.
        /// Expected: WritePolicyAsync throws ArgumentException.
        /// </summary>
        [Fact]
        public async Task WritePolicy_TC03()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _pap.WritePolicyAsync("org", string.Empty, new MemoryStream()));
        }

        /// <summary>
        /// Test case: Write to storage a file that is null.
        /// Expected: WritePolicyAsync throws ArgumentException.
        /// </summary>
        [Fact]
        public async Task WritePolicy_TC04()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _pap.WritePolicyAsync("org", "app", null));
        }

        /// <summary>
        /// Test case: Write to storage a file that is null.
        /// Expected: WritePolicyAsync throws ArgumentException.
        /// </summary>
        [Fact]
        public async Task WritePolicy_TC05()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _pap.WritePolicyAsync("org", "app", null));
        }

        /// <summary>
        /// Scenario:
        /// Tests the TryWriteDelegationPolicyRules function, where all rules are deleted the db is updated with RevokeLast status
        /// Input:
        /// List of unordered rules for deletion multiple apps same OfferedBy to one CoveredBy user, and one coveredBy organization/partyid
        /// Expected Result:
        /// List of all rules actualy deleted.
        /// Success Criteria:
        /// All returned rules match expected and have success flag and rule id set
        /// </summary>
        [Fact]
        public async Task TryDeleteDelegationPolicyRules_Valid()
        {
            // Arrange
            int performedByUserId = 20001336;
            int offeredByPartyId = 50001337;
            int coveredBy = 20001337;
            string coveredByType = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute;
            _delegationMetadataRepositoryMock.MetadataChanges = new Dictionary<string, List<DelegationChange>>();

            List<RequestToDelete> inputRuleMatchess = new List<RequestToDelete>
            {
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org1", "app3", new List<string> { "0d0c8570-64fb-49f9-9f7d-45c057fddf94", "6f11dd0b-5e5d-4bd1-85f0-9796300dfded" }, coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org2", "app3", new List<string> { "244278c1-7c6b-4f6b-b6e9-2bd41f84812f" }, coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org1", "app4", new List<string> { "adfa64fa-5859-46e5-8d0d-62762082f3b9" }, coveredByUserId: coveredBy)
            };

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "write", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org2", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org1", "app4", createdSuccessfully: true)
            };

            Dictionary<string, List<DelegationChange>> expectedDbUpdates = new Dictionary<string, List<DelegationChange>>
            {
                { "org1/app3/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org1/app3", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.RevokeLast) } },
                { "org2/app3/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org2/app3", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.Revoke) } },
                { "org1/app4/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org1/app4", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.Revoke) } }
            };

            // Act
            List<Rule> actual = await _pap.TryDeleteDelegationPolicyRules(inputRuleMatchess);

            // Assert
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(actual.All(r => r.CreatedSuccessfully));
            Assert.True(actual.All(r => !string.IsNullOrEmpty(r.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
            AssertionUtil.AssertEqual(expectedDbUpdates, _delegationMetadataRepositoryMock.MetadataChanges);
        }

        /// <summary>
        /// Scenario:
        /// Tests the TryWriteDelegationPolicyRules function, where all rules are deleted the db is updated with RevokeLast status,
        /// but pushing RevokeLast event to DelegationChangeEventQueue fails which should trigger crittical error logging
        /// Input:
        /// List of unordered rules for deletion multiple apps same OfferedBy to one CoveredBy user, and one coveredBy organization/partyid
        /// Expected Result:
        /// List of all rules are deleted from policy and delegationchange stored in postgresql, critical error is logged
        /// Success Criteria:
        /// All returned rules match expected, and critical error has been logged
        /// </summary>
        [Fact]
        public async Task TryDeleteDelegationPolicyRules_Valid_DelegationEventQueue_Push_Exception()
        {
            // Arrange
            int performedByUserId = 20001337;
            int offeredByPartyId = 50001337;
            int coveredBy = 20001336;
            string coveredByType = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute;
            _delegationMetadataRepositoryMock.MetadataChanges = new Dictionary<string, List<DelegationChange>>();

            List<RequestToDelete> inputRuleMatchess = new List<RequestToDelete>
            {
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "error", "delegationeventfail", new List<string> { "c73079c1-ed67-4958-91e3-a388ee355097" }, coveredByUserId: coveredBy)
            };

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "error", "delegationeventfail", createdSuccessfully: true)
            };

            Dictionary<string, List<DelegationChange>> expectedDbUpdates = new Dictionary<string, List<DelegationChange>>
            {
                { "error/delegationeventfail/50001337/u20001336", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("error/delegationeventfail", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.Revoke) } }
            };

            // Act
            List<Rule> actual = await _pap.TryDeleteDelegationPolicyRules(inputRuleMatchess);

            // Assert
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(actual.All(r => !string.IsNullOrEmpty(r.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
            AssertionUtil.AssertEqual(expectedDbUpdates, _delegationMetadataRepositoryMock.MetadataChanges);
            _logger.Verify(
                x => x.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((@object, @type) => @object.ToString().StartsWith("DeleteRules could not push DelegationChangeEvent to DelegationChangeEventQueue. DelegationChangeEvent must be retried for successful sync with SBL Authorization. DelegationChange:") && @type.Name == "FormattedLogValues"),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        /// <summary>
        /// Scenario:
        /// Tests the TryWriteDelegationPolicyRules function, whether one policy is returned as already deleted the rules ok to delete is deleted the one already deleted ignored
        /// Input:
        /// List of unordered rules for deletion multiple apps same OfferedBy to one CoveredBy user, and one coveredBy organization/partyid
        /// Expected Result:
        /// List of deleted policy is ignored rest is deleted.
        /// Success Criteria:
        /// All returned rules match expected and have success flag and rule id set
        /// </summary>
        [Fact]
        public async Task TryDeleteDelegationPolicyRules_PolicyAlredyDeleted()
        {
            // Arrange
            int performedByUserId = 20001336;
            int offeredByPartyId = 50001337;
            int coveredBy = 20001337;
            string coveredByType = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute;
            _delegationMetadataRepositoryMock.MetadataChanges = new Dictionary<string, List<DelegationChange>>();

            List<RequestToDelete> inputRuleMatchess = new List<RequestToDelete>
            {
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org1", "app5", new List<string> { "0d0c8570-64fb-49f9-9f7d-45c057fddf94", "6f11dd0b-5e5d-4bd1-85f0-9796300dfded" }, coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org2", "app3", new List<string> { "244278c1-7c6b-4f6b-b6e9-2bd41f84812f" }, coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org1", "app4", new List<string> { "adfa64fa-5859-46e5-8d0d-62762082f3b9" }, coveredByUserId: coveredBy)
            };

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org2", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org1", "app4", createdSuccessfully: true)
            };

            Dictionary<string, List<DelegationChange>> expectedDbUpdates = new Dictionary<string, List<DelegationChange>>
            {
                { "org2/app3/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org2/app3", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.Revoke) } },
                { "org1/app4/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org1/app4", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.Revoke) } }
            };

            // Act
            List<Rule> actual = await _pap.TryDeleteDelegationPolicyRules(inputRuleMatchess);

            // Assert
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(actual.All(r => r.CreatedSuccessfully));
            Assert.True(actual.All(r => !string.IsNullOrEmpty(r.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
            AssertionUtil.AssertEqual(expectedDbUpdates, _delegationMetadataRepositoryMock.MetadataChanges);
            _logger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == "The policy is already deleted for: org1/app5 CoveredBy: 20001337 OfferedBy: 50001337"),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        /// <summary>
        /// Scenario:
        /// Tests the TryWriteDelegationPolicyRules function, whether all rules are returned as successfully deleated whera all rules are deleted the db is also updated with isDeleted status
        /// Input:
        /// List of unordered rules for deletion multiple apps same OfferedBy to one CoveredBy user, and one coveredBy organization/partyid
        /// Expected Result:
        /// List of all rules actualy deleted.
        /// Success Criteria:
        /// All returned rules match expected and have success flag and rule id set
        /// </summary>
        [Fact]
        public async Task TryDeleteDelegationPolicyRules_ForOrganizationValid()
        {
            // Arrange
            int performedByUserId = 20001336;
            int offeredByPartyId = 50001337;
            int coveredBy = 50001336;
            string coveredByType = AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute;
            _delegationMetadataRepositoryMock.MetadataChanges = new Dictionary<string, List<DelegationChange>>();

            List<RequestToDelete> inputRuleMatchess = new List<RequestToDelete>
            {
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org1", "app1", new List<string> { "57b3ee85-f932-42c6-9ab0-941eb6c96eb0",  "78e5cced-3bcb-42b6-9089-63c834f89e73" }, coveredByPartyId: coveredBy)
            };

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org1", "app1", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "write", "org1", "app1", "task1", createdSuccessfully: true),
            };

            Dictionary<string, List<DelegationChange>> expectedDbUpdates = new Dictionary<string, List<DelegationChange>>
            {
                { "org1/app1/50001337/p50001336", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org1/app1", offeredByPartyId, performedByUserId: performedByUserId, coveredByPartyId: coveredBy, changeType: DelegationChangeType.Revoke) } },
            };

            // Act
            List<Rule> actual = await _pap.TryDeleteDelegationPolicyRules(inputRuleMatchess);

            // Assert
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(actual.All(r => r.CreatedSuccessfully));
            Assert.True(actual.All(r => !string.IsNullOrEmpty(r.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
            AssertionUtil.AssertEqual(expectedDbUpdates, _delegationMetadataRepositoryMock.MetadataChanges);
        }

        /// <summary>
        /// Scenario:
        /// Tests the TryWriteDelegationPolicyRules function, whether one rules are returned as not successfully deleated due to error retriving data from db whera all rules are deleted the db is also updated with isDeleted status
        /// Input:
        /// List of unordered rules for deletion multiple apps same OfferedBy to one CoveredBy user, and one coveredBy organization/partyid
        /// Expected Result:
        /// List of all rules actualy deleted.
        /// Success Criteria:
        /// All returned rules match expected and have success flag and rule id set
        /// </summary>
        [Fact]
        public async Task TryDeleteDelegationPolicyRules_DBFetchFail()
        {
            // Arrange
            int performedByUserId = 20001336;
            int offeredByPartyId = 50001337;
            int coveredBy = 20001337;
            string coveredByType = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute;
            _delegationMetadataRepositoryMock.MetadataChanges = new Dictionary<string, List<DelegationChange>>();

            List<RequestToDelete> inputRuleMatchess = new List<RequestToDelete>
            {
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org1", "app3", new List<string> { "0d0c8570-64fb-49f9-9f7d-45c057fddf94", "6f11dd0b-5e5d-4bd1-85f0-9796300dfded" }, coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org2", "app3", new List<string> { "244278c1-7c6b-4f6b-b6e9-2bd41f84812f" }, coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "error", "postgregetcurrentfail", new List<string> { "ade3b138-7fa4-4c83-9306-8ec4a72c2daa" }, coveredByUserId: coveredBy)
            };

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "write", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org2", "app3", createdSuccessfully: true),
            };

            Dictionary<string, List<DelegationChange>> expectedDbUpdates = new Dictionary<string, List<DelegationChange>>
            {
                { "org1/app3/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org1/app3", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.RevokeLast) } },
                { "org2/app3/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org2/app3", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.Revoke) } },
            };

            // Act
            List<Rule> actual = await _pap.TryDeleteDelegationPolicyRules(inputRuleMatchess);

            // Assert
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(actual.All(r => r.CreatedSuccessfully));
            Assert.True(actual.All(r => !string.IsNullOrEmpty(r.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
            AssertionUtil.AssertEqual(expectedDbUpdates, _delegationMetadataRepositoryMock.MetadataChanges);
        }

        /// <summary>
        /// Scenario:
        /// Tests the TryWriteDelegationPolicyRules function, whether one rules are returned as not successfully deleted due to error locking data for update in blob storage where all rules are deleted the db is also updated with isDeleted status
        /// Input:
        /// List of unordered rules for deletion multiple apps same OfferedBy to one CoveredBy user, and one coveredBy organization/partyid
        /// Expected Result:
        /// List of all rules actualy deleted.
        /// Success Criteria:
        /// All returned rules match expected and have success flag and rule id set
        /// </summary>
        [Fact]
        public async Task TryDeleteDelegationPolicyRules_DataStorageLeaseFail()
        {
            // Arrange
            int performedByUserId = 20001336;
            int offeredByPartyId = 50001337;
            int coveredBy = 20001337;
            string coveredByType = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute;
            _delegationMetadataRepositoryMock.MetadataChanges = new Dictionary<string, List<DelegationChange>>();

            List<RequestToDelete> inputRuleMatchess = new List<RequestToDelete>
            {
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org1", "app3", new List<string> { "0d0c8570-64fb-49f9-9f7d-45c057fddf94", "6f11dd0b-5e5d-4bd1-85f0-9796300dfded" }, coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org2", "app3", new List<string> { "244278c1-7c6b-4f6b-b6e9-2bd41f84812f" }, coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "error", "blobstoragegetleaselockfail", new List<string> { "ade3b138-7fa4-4c83-9306-8ec4a72c2daa" }, coveredByUserId: coveredBy)
            };

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "write", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org2", "app3", createdSuccessfully: true),
            };

            Dictionary<string, List<DelegationChange>> expectedDbUpdates = new Dictionary<string, List<DelegationChange>>
            {
                { "org1/app3/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org1/app3", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.RevokeLast) } },
                { "org2/app3/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org2/app3", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.Revoke) } },
            };

            // Act
            List<Rule> actual = await _pap.TryDeleteDelegationPolicyRules(inputRuleMatchess);

            // Assert
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(actual.All(r => r.CreatedSuccessfully));
            Assert.True(actual.All(r => !string.IsNullOrEmpty(r.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
            AssertionUtil.AssertEqual(expectedDbUpdates, _delegationMetadataRepositoryMock.MetadataChanges);
            _logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == "Could not acquire blob lease lock on delegation policy at path: error/blobstoragegetleaselockfail/50001337/u20001337/delegationpolicy.xml" && @type.Name == "FormattedLogValues"),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        /// <summary>
        /// Scenario:
        /// Tests the TryWriteDelegationPolicyRules function, whether one rules are returned as not successfully deleted due to error writing data for update in blob storage.
        /// Input:
        /// List of unordered rules for deletion multiple apps same OfferedBy to one CoveredBy user, and one coveredBy organization/partyid
        /// Expected Result:
        /// List of all rules actualy deleted.
        /// Success Criteria:
        /// All returned rules match expected and have success flag and rule id set
        /// </summary>
        [Fact]
        public async Task TryDeleteDelegationPolicyRules_DataWriteFail()
        {
            // Arrange
            int performedByUserId = 20001336;
            int offeredByPartyId = 50001337;
            int coveredBy = 20001337;
            string coveredByType = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute;
            _delegationMetadataRepositoryMock.MetadataChanges = new Dictionary<string, List<DelegationChange>>();

            List<RequestToDelete> inputRuleMatchess = new List<RequestToDelete>
            {
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org1", "app3", new List<string> { "0d0c8570-64fb-49f9-9f7d-45c057fddf94", "6f11dd0b-5e5d-4bd1-85f0-9796300dfded" }, coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org2", "app3", new List<string> { "244278c1-7c6b-4f6b-b6e9-2bd41f84812f" }, coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "error", "blobstorageleaselockwritefail", new List<string> { "ade3b138-7fa4-4c83-9306-8ec4a72c2daa" }, coveredByUserId: coveredBy)
            };

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "write", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org2", "app3", createdSuccessfully: true),
            };

            Dictionary<string, List<DelegationChange>> expectedDbUpdates = new Dictionary<string, List<DelegationChange>>
            {
                { "org1/app3/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org1/app3", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.RevokeLast) } },
                { "org2/app3/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org2/app3", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.Revoke) } },
            };

            // Act
            List<Rule> actual = await _pap.TryDeleteDelegationPolicyRules(inputRuleMatchess);

            // Assert
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(actual.All(r => r.CreatedSuccessfully));
            Assert.True(actual.All(r => !string.IsNullOrEmpty(r.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
            AssertionUtil.AssertEqual(expectedDbUpdates, _delegationMetadataRepositoryMock.MetadataChanges);
            _logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == "Writing of delegation policy at path: error/blobstorageleaselockwritefail/50001337/u20001337/delegationpolicy.xml failed. Is delegation blob storage account alive and well?" && @type.Name == "FormattedLogValues"),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        /// <summary>
        /// Scenario:
        /// Tests the TryWriteDelegationPolicyRules function, whether one rules are returned as not successfully deleted due to undefined resource
        /// Input:
        /// List of unordered rules for deletion multiple apps same OfferedBy to one CoveredBy user, and one coveredBy organization/partyid
        /// Expected Result:
        /// List of all rules actualy deleted.
        /// Success Criteria:
        /// All returned rules match expected and have success flag and rule id set
        /// </summary>
        [Fact]
        public async Task TryDeleteDelegationPolicyRules_PolicyPathInvalid()
        {
            // Arrange
            int performedByUserId = 20001336;
            int offeredByPartyId = 50001337;
            int coveredBy = 20001337;
            string coveredByType = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute;
            _delegationMetadataRepositoryMock.MetadataChanges = new Dictionary<string, List<DelegationChange>>();

            List<RequestToDelete> inputRuleMatchess = new List<RequestToDelete>
            {
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org1", "app3", new List<string> { "0d0c8570-64fb-49f9-9f7d-45c057fddf94", "6f11dd0b-5e5d-4bd1-85f0-9796300dfded" }, coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org2", "app3", new List<string> { "244278c1-7c6b-4f6b-b6e9-2bd41f84812f" }, coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org1", "app4", new List<string> { "ade3b138-7fa4-4c83-9306-8ec4a72c2daa" }, coveredByUserId: 0)
            };

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "write", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org2", "app3", createdSuccessfully: true),
            };

            Dictionary<string, List<DelegationChange>> expectedDbUpdates = new Dictionary<string, List<DelegationChange>>
            {
                { "org1/app3/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org1/app3", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.RevokeLast) } },
                { "org2/app3/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org2/app3", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.Revoke) } },
            };

            // Act
            List<Rule> actual = await _pap.TryDeleteDelegationPolicyRules(inputRuleMatchess);

            // Assert
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(actual.All(r => r.CreatedSuccessfully));
            Assert.True(actual.All(r => !string.IsNullOrEmpty(r.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
            AssertionUtil.AssertEqual(expectedDbUpdates, _delegationMetadataRepositoryMock.MetadataChanges);
            _logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == "Not possible to build policy path for: org1/app4 CoveredBy: (null) OfferedBy: 50001337 RuleIds: ade3b138-7fa4-4c83-9306-8ec4a72c2daa" && @type.Name == "FormattedLogValues"),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        /// <summary>
        /// Scenario:
        /// Tests the TryWriteDelegationPolicyRules function, whether one policy are returned as not successfully deleted due to error update changelog.
        /// Input:
        /// List of unordered rules for deletion multiple apps same OfferedBy to one CoveredBy user, and one coveredBy organization/partyid
        /// Expected Result:
        /// List of all rules actualy deleted.
        /// Success Criteria:
        /// All returned rules match expected and have success flag and rule id set
        /// </summary>
        [Fact]
        public async Task TryDeleteDelegationPolicyRules_PostgreeUpdateFail()
        {
            // Arrange
            int performedByUserId = 20001336;
            int offeredByPartyId = 50001337;
            int coveredBy = 20001337;
            string coveredByType = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute;
            _delegationMetadataRepositoryMock.MetadataChanges = new Dictionary<string, List<DelegationChange>>();

            List<RequestToDelete> inputRuleMatchess = new List<RequestToDelete>
            {
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org1", "app3", new List<string> { "0d0c8570-64fb-49f9-9f7d-45c057fddf94", "6f11dd0b-5e5d-4bd1-85f0-9796300dfded" }, coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org2", "app3", new List<string> { "244278c1-7c6b-4f6b-b6e9-2bd41f84812f" }, coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "error", "postgrewritechangefail", new List<string> { "ade3b138-7fa4-4c83-9306-8ec4a72c2daa" }, coveredByUserId: coveredBy)
            };

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "write", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org2", "app3", createdSuccessfully: true),
            };

            Dictionary<string, List<DelegationChange>> expectedDbUpdates = new Dictionary<string, List<DelegationChange>>
            {
                { "org1/app3/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org1/app3", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.RevokeLast) } },
                { "org2/app3/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org2/app3", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.Revoke) } },
                { "error/postgrewritechangefail/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("error/postgrewritechangefail", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.RevokeLast) } },
            };

            // Act
            List<Rule> actual = await _pap.TryDeleteDelegationPolicyRules(inputRuleMatchess);

            // Assert
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(actual.All(r => r.CreatedSuccessfully));
            Assert.True(actual.All(r => !string.IsNullOrEmpty(r.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
            AssertionUtil.AssertEqual(expectedDbUpdates, _delegationMetadataRepositoryMock.MetadataChanges);
            _logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == "Writing of delegation change to authorization postgresql database failed for changes to delegation policy at path: error/postgrewritechangefail/50001337/u20001337/delegationpolicy.xml. is authorization postgresql database alive and well?" && @type.Name == "FormattedLogValues"),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        /// <summary>
        /// Scenario:
        /// Tests the TryWriteDelegationPolicyRules function, whether one rules are returned as not successfully deleated due to error finding the file on blob storage where all rules are deleted the db is also updated with isDeleted status
        /// Input:
        /// List of unordered rules for deletion multiple apps same OfferedBy to one CoveredBy user, and one coveredBy organization/partyid
        /// Expected Result:
        /// List of all rules actualy deleted.
        /// Success Criteria:
        /// All returned rules match expected and have success flag and rule id set
        /// </summary>
        [Fact]
        public async Task TryDeleteDelegationPolicyRules_PolicyPathDoesNotExist()
        {
            // Arrange
            int performedByUserId = 20001336;
            int offeredByPartyId = 50001337;
            int coveredBy = 20001337;
            string coveredByType = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute;
            _delegationMetadataRepositoryMock.MetadataChanges = new Dictionary<string, List<DelegationChange>>();

            List<RequestToDelete> inputRuleMatchess = new List<RequestToDelete>
            {
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org1", "app8", new List<string> { "0d0c8570-64fb-49f9-9f7d-45c057fddf94" }, coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org2", "app3", new List<string> { "244278c1-7c6b-4f6b-b6e9-2bd41f84812f" }, coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org1", "app4", new List<string> { "adfa64fa-5859-46e5-8d0d-62762082f3b9" }, coveredByUserId: coveredBy)
            };

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org2", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org1", "app4", createdSuccessfully: true)
            };

            Dictionary<string, List<DelegationChange>> expectedDbUpdates = new Dictionary<string, List<DelegationChange>>
            {
                { "org2/app3/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org2/app3", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.Revoke) } },
                { "org1/app4/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org1/app4", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.Revoke) } }
            };

            // Act
            List<Rule> actual = await _pap.TryDeleteDelegationPolicyRules(inputRuleMatchess);

            // Assert
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(actual.All(r => r.CreatedSuccessfully));
            Assert.True(actual.All(r => !string.IsNullOrEmpty(r.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
            AssertionUtil.AssertEqual(expectedDbUpdates, _delegationMetadataRepositoryMock.MetadataChanges);
        }

        /// <summary>
        /// Scenario:
        /// Tests the TryWriteDelegationPolicies function, all rules are returned as successfully created
        /// Input:
        /// List of unordered rules for deletion of the same apps from the same OfferedBy to one CoveredBy user
        /// Expected Result:
        /// List of all rules deleted returned.
        /// Success Criteria:
        /// All returned rules match expected and have success flag and rule id set
        /// </summary>
        [Fact]
        public async Task TryDeleteDelegationPolicies_Valid()
        {
            // Arrange
            int performedByUserId = 20001336;
            int offeredByPartyId = 50001337;
            int coveredBy = 20001337;
            string coveredByType = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute;
            _delegationMetadataRepositoryMock.MetadataChanges = new Dictionary<string, List<DelegationChange>>();

            List<RequestToDelete> inputRuleMatchess = new List<RequestToDelete>
            {
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org1", "app3", coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org2", "app3", coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org1", "app4", coveredByUserId: coveredBy)
            };

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "write", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org2", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "write", "org2", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org1", "app4", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "write", "org1", "app4", createdSuccessfully: true),
            };

            Dictionary<string, List<DelegationChange>> expectedDbUpdates = new Dictionary<string, List<DelegationChange>>
            {
                { "org1/app3/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org1/app3", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.RevokeLast) } },
                { "org2/app3/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org2/app3", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.RevokeLast) } },
                { "org1/app4/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org1/app4", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.RevokeLast) } }
            };

            // Act
            List<Rule> actual = await _pap.TryDeleteDelegationPolicies(inputRuleMatchess);

            // Assert
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(actual.All(r => r.CreatedSuccessfully));
            Assert.True(actual.All(r => !string.IsNullOrEmpty(r.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
            AssertionUtil.AssertEqual(expectedDbUpdates, _delegationMetadataRepositoryMock.MetadataChanges);
        }

        /// <summary>
        /// Scenario:
        /// Tests the TryDeleteDelegationPolicies operation, where all rules in a given delegation policy are deleted and stored in delegationchange database with RevokeLast status,
        /// but pushing RevokeLast event to DelegationChangeEventQueue fails which should trigger crittical error logging
        /// Input:
        /// List of RequestToDelete models identifying the delegation policies to be deleted
        /// Expected Result:
        /// List of all rules are deleted from policy and delegationchange stored in postgresql, critical error is logged
        /// Success Criteria:
        /// All returned rules match expected, and critical error has been logged
        /// </summary>
        [Fact]
        public async Task TryDeleteDelegationPolicies_Valid_DelegationEventQueue_Push_Exception()
        {
            // Arrange
            int performedByUserId = 20001337;
            int offeredByPartyId = 50001337;
            int coveredBy = 20001336;
            string coveredByType = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute;
            _delegationMetadataRepositoryMock.MetadataChanges = new Dictionary<string, List<DelegationChange>>();

            List<RequestToDelete> inputRuleMatchess = new List<RequestToDelete>
            {
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "error", "delegationeventfail", coveredByUserId: coveredBy)
            };

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "error", "delegationeventfail", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "write", "error", "delegationeventfail", createdSuccessfully: true)
            };

            Dictionary<string, List<DelegationChange>> expectedDbUpdates = new Dictionary<string, List<DelegationChange>>
            {
                { "error/delegationeventfail/50001337/u20001336", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("error/delegationeventfail", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.RevokeLast) } }
            };

            // Act
            List<Rule> actual = await _pap.TryDeleteDelegationPolicies(inputRuleMatchess);

            // Assert
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(actual.All(r => !string.IsNullOrEmpty(r.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
            AssertionUtil.AssertEqual(expectedDbUpdates, _delegationMetadataRepositoryMock.MetadataChanges);
            _logger.Verify(
                x => x.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((@object, @type) => @object.ToString().StartsWith("DeletePolicy could not push DelegationChangeEvent to DelegationChangeEventQueue. DelegationChangeEvent must be retried for successful sync with SBL Authorization. DelegationChange:") && @type.Name == "FormattedLogValues"),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        /// <summary>
        /// Scenario:
        /// Tests the TryWriteDelegationPolicies function, one rule are returned as failed due to error locking the data for update
        /// Input:
        /// List of unordered rules for deletion of the same apps from the same OfferedBy to one CoveredBy user
        /// Expected Result:
        /// List of all rules deleted returned.
        /// Success Criteria:
        /// All returned rules match expected and have success flag and rule id set
        /// </summary>
        [Fact]
        public async Task TryDeleteDelegationPolicies_StorageLeaseFail()
        {
            // Arrange
            int performedByUserId = 20001336;
            int offeredByPartyId = 50001337;
            int coveredBy = 20001337;
            string coveredByType = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute;
            _delegationMetadataRepositoryMock.MetadataChanges = new Dictionary<string, List<DelegationChange>>();

            List<RequestToDelete> inputRuleMatchess = new List<RequestToDelete>
            {
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org1", "app3", coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org2", "app3", coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "error", "blobstoragegetleaselockfail", coveredByUserId: coveredBy)
            };

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "write", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org2", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "write", "org2", "app3", createdSuccessfully: true),
            };

            Dictionary<string, List<DelegationChange>> expectedDbUpdates = new Dictionary<string, List<DelegationChange>>
            {
                { "org1/app3/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org1/app3", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.RevokeLast) } },
                { "org2/app3/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org2/app3", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.RevokeLast) } },
            };

            // Act
            List<Rule> actual = await _pap.TryDeleteDelegationPolicies(inputRuleMatchess);

            // Assert
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(actual.All(r => r.CreatedSuccessfully));
            Assert.True(actual.All(r => !string.IsNullOrEmpty(r.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
            AssertionUtil.AssertEqual(expectedDbUpdates, _delegationMetadataRepositoryMock.MetadataChanges);
            _logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == "Could not acquire blob lease on delegation policy at path: error/blobstoragegetleaselockfail/50001337/u20001337/delegationpolicy.xml" && @type.Name == "FormattedLogValues"),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        /// <summary>
        /// Scenario:
        /// Tests the TryWriteDelegationPolicies function, one rule are returned as failed due to error fetching data from DB
        /// Input:
        /// List of unordered rules for deletion of the same apps from the same OfferedBy to one CoveredBy user
        /// Expected Result:
        /// List of all rules deleted returned.
        /// Success Criteria:
        /// All returned rules match expected and have success flag and rule id set
        /// </summary>
        [Fact]
        public async Task TryDeleteDelegationPolicies_DBFetchFail()
        {
            // Arrange
            int performedByUserId = 20001336;
            int offeredByPartyId = 50001337;
            int coveredBy = 20001337;
            string coveredByType = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute;
            _delegationMetadataRepositoryMock.MetadataChanges = new Dictionary<string, List<DelegationChange>>();

            List<RequestToDelete> inputRuleMatchess = new List<RequestToDelete>
            {
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org1", "app3", coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org2", "app3", coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "error", "postgregetcurrentfail", coveredByUserId: coveredBy)
            };

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "write", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org2", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "write", "org2", "app3", createdSuccessfully: true),
            };

            Dictionary<string, List<DelegationChange>> expectedDbUpdates = new Dictionary<string, List<DelegationChange>>
            {
                { "org1/app3/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org1/app3", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.RevokeLast) } },
                { "org2/app3/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org2/app3", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.RevokeLast) } },
            };

            // Act
            List<Rule> actual = await _pap.TryDeleteDelegationPolicies(inputRuleMatchess);

            // Assert
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(actual.All(r => r.CreatedSuccessfully));
            Assert.True(actual.All(r => !string.IsNullOrEmpty(r.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
            AssertionUtil.AssertEqual(expectedDbUpdates, _delegationMetadataRepositoryMock.MetadataChanges);
        }

        /// <summary>
        /// Scenario:
        /// Tests the TryWriteDelegationPolicies function, one rule are returned as failed due to error finding data on blob storage
        /// Input:
        /// List of unordered rules for deletion of the same apps from the same OfferedBy to one CoveredBy user
        /// Expected Result:
        /// List of all rules deleted returned.
        /// Success Criteria:
        /// All returned rules match expected and have success flag and rule id set
        /// </summary>
        [Fact]
        public async Task TryDeleteDelegationPolicies_PolicyPathDoesNotExist()
        {
            // Arrange
            int performedByUserId = 20001336;
            int offeredByPartyId = 50001337;
            int coveredBy = 20001337;
            string coveredByType = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute;
            _delegationMetadataRepositoryMock.MetadataChanges = new Dictionary<string, List<DelegationChange>>();

            List<RequestToDelete> inputRuleMatchess = new List<RequestToDelete>
            {
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org1", "app3", coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org2", "app3", coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org1", "app8", coveredByUserId: coveredBy)
            };

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "write", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org2", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "write", "org2", "app3", createdSuccessfully: true),
            };

            Dictionary<string, List<DelegationChange>> expectedDbUpdates = new Dictionary<string, List<DelegationChange>>
            {
                { "org1/app3/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org1/app3", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.RevokeLast) } },
                { "org2/app3/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org2/app3", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.RevokeLast) } },
            };

            // Act
            List<Rule> actual = await _pap.TryDeleteDelegationPolicies(inputRuleMatchess);

            // Assert
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(actual.All(r => r.CreatedSuccessfully));
            Assert.True(actual.All(r => !string.IsNullOrEmpty(r.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
            AssertionUtil.AssertEqual(expectedDbUpdates, _delegationMetadataRepositoryMock.MetadataChanges);
        }

        /// <summary>
        /// Scenario:
        /// Tests the TryWriteDelegationPolicies function, one rule are returned as failed due to error updating data in DB
        /// Input:
        /// List of unordered rules for deletion of the same apps from the same OfferedBy to one CoveredBy user
        /// Expected Result:
        /// List of all rules deleted returned.
        /// Success Criteria:
        /// All returned rules match expected and have success flag and rule id set
        /// </summary>
        [Fact]
        public async Task TryDeleteDelegationPolicies_DBUpdateFails()
        {
            // Arrange
            int performedByUserId = 20001336;
            int offeredByPartyId = 50001337;
            int coveredBy = 20001337;
            string coveredByType = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute;
            _delegationMetadataRepositoryMock.MetadataChanges = new Dictionary<string, List<DelegationChange>>();

            List<RequestToDelete> inputRuleMatchess = new List<RequestToDelete>
            {
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org1", "app3", coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org2", "app3", coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "error", "postgrewritechangefail", coveredByUserId: coveredBy)
            };

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "write", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org2", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "write", "org2", "app3", createdSuccessfully: true),
            };

            Dictionary<string, List<DelegationChange>> expectedDbUpdates = new Dictionary<string, List<DelegationChange>>
            {
                { "org1/app3/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org1/app3", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.RevokeLast) } },
                { "org2/app3/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org2/app3", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.RevokeLast) } },
                { "error/postgrewritechangefail/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("error/postgrewritechangefail", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.RevokeLast) } }
            };

            // Act
            List<Rule> actual = await _pap.TryDeleteDelegationPolicies(inputRuleMatchess);

            // Assert
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(actual.All(r => r.CreatedSuccessfully));
            Assert.True(actual.All(r => !string.IsNullOrEmpty(r.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
            AssertionUtil.AssertEqual(expectedDbUpdates, _delegationMetadataRepositoryMock.MetadataChanges);
            _logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == "Writing of delegation change to authorization postgresql database failed for changes to delegation policy at path: error/postgrewritechangefail/50001337/u20001337/delegationpolicy.xml. is authorization postgresql database alive and well?" && @type.Name == "FormattedLogValues"),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        /// <summary>
        /// Scenario:
        /// Tests the TryWriteDelegationPolicies function, one rule are returned as failed due to error in resource to delete
        /// Input:
        /// List of unordered rules for deletion of the same apps from the same OfferedBy to one CoveredBy user
        /// Expected Result:
        /// List of all rules deleted returned.
        /// Success Criteria:
        /// All returned rules match expected and have success flag and rule id set
        /// </summary>
        [Fact]
        public async Task TryDeleteDelegationPolicies_UndefinedResource()
        {
            // Arrange
            int performedByUserId = 20001336;
            int offeredByPartyId = 50001337;
            int coveredBy = 20001337;
            string coveredByType = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute;
            _delegationMetadataRepositoryMock.MetadataChanges = new Dictionary<string, List<DelegationChange>>();

            List<RequestToDelete> inputRuleMatchess = new List<RequestToDelete>
            {
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org1", "app3", coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org2", "app3", coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org1", string.Empty, coveredByUserId: coveredBy)
            };

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "write", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org2", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "write", "org2", "app3", createdSuccessfully: true),
            };

            Dictionary<string, List<DelegationChange>> expectedDbUpdates = new Dictionary<string, List<DelegationChange>>
            {
                { "org1/app3/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org1/app3", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.RevokeLast) } },
                { "org2/app3/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org2/app3", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.RevokeLast) } },
            };

            // Act
            List<Rule> actual = await _pap.TryDeleteDelegationPolicies(inputRuleMatchess);

            // Assert
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(actual.All(r => r.CreatedSuccessfully));
            Assert.True(actual.All(r => !string.IsNullOrEmpty(r.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
            AssertionUtil.AssertEqual(expectedDbUpdates, _delegationMetadataRepositoryMock.MetadataChanges);
            _logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == "Not possible to build policy path for: org1/ CoveredBy: 20001337 OfferedBy: 50001337" && @type.Name == "FormattedLogValues"),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        /// <summary>
        /// Scenario:
        /// Tests the TryWriteDelegationPolicies function, one rule are returned as failed due to policy already deleted
        /// Input:
        /// List of unordered rules for deletion of the same apps from the same OfferedBy to one CoveredBy user
        /// Expected Result:
        /// List of all rules deleted returned.
        /// Success Criteria:
        /// All returned rules match expected and have success flag and rule id set
        /// </summary>
        [Fact]
        public async Task TryDeleteDelegationPolicies_PolicyAlreadyDeleted()
        {
            // Arrange
            int performedByUserId = 20001336;
            int offeredByPartyId = 50001337;
            int coveredBy = 20001337;
            string coveredByType = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute;
            _delegationMetadataRepositoryMock.MetadataChanges = new Dictionary<string, List<DelegationChange>>();

            List<RequestToDelete> inputRuleMatchess = new List<RequestToDelete>
            {
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org1", "app3", coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org2", "app3", coveredByUserId: coveredBy),
                TestDataUtil.GetRequestToDeleteModel(performedByUserId, offeredByPartyId, "org1", "app5", coveredByUserId: coveredBy)
            };

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "write", "org1", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "read", "org2", "app3", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(performedByUserId, offeredByPartyId, coveredBy.ToString(), coveredByType, "write", "org2", "app3", createdSuccessfully: true),
            };

            Dictionary<string, List<DelegationChange>> expectedDbUpdates = new Dictionary<string, List<DelegationChange>>
            {
                { "org1/app3/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org1/app3", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.RevokeLast) } },
                { "org2/app3/50001337/u20001337", new List<DelegationChange> { TestDataUtil.GetAltinnAppDelegationChange("org2/app3", offeredByPartyId, performedByUserId: performedByUserId, coveredByUserId: coveredBy, changeType: DelegationChangeType.RevokeLast) } }
            };

            // Act
            List<Rule> actual = await _pap.TryDeleteDelegationPolicies(inputRuleMatchess);

            // Assert
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(actual.All(r => r.CreatedSuccessfully));
            Assert.True(actual.All(r => !string.IsNullOrEmpty(r.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
            AssertionUtil.AssertEqual(expectedDbUpdates, _delegationMetadataRepositoryMock.MetadataChanges);
            _logger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == "The policy is already deleted for: org1/app5 CoveredBy: 20001337 OfferedBy: 50001337" && @type.Name == "FormattedLogValues"),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        /// <summary>
        /// Tests the TryWriteDelegationPolicyRules function, whether all rules are returned as successfully created
        /// Input:
        /// List of unordered rules for delegation of the same apps from the same OfferedBy to two CoveredBy users, and one coveredBy organization/partyid
        /// Expected Result:
        /// List of all rules (now in sorted order of the resulting 3 delegation policy files) with success flag and rule id set.
        /// Success Criteria:
        /// All returned rules match expected and have success flag and rule id set
        /// </summary>
        [Fact]
        public async Task TryWriteDelegationPolicyRules_Valid()
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
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read", "org1", "app2"),
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "sign", "org1", "app1", "task1"), // Should be sorted together with the first rule
            };

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read", "org1", "app1", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "sign", "org1", "app1", "task1", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read", "org2", "app1", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read", "org1", "app2", createdSuccessfully: true)
            };

            // Act
            List<Rule> actual = await _pap.TryWriteDelegationPolicyRules(unsortedRules);

            // Assert
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(actual.All(r => r.CreatedSuccessfully));
            Assert.True(actual.All(r => !string.IsNullOrEmpty(r.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
        }

        /// <summary>
        /// Scenario:
        /// Tests the TryWriteDelegationPolicyRules function, when only partial set of the rules are returned as successfully created.
        /// Input:
        /// List of unordered rules for delegation from the same OfferedBy to two CoveredBy users, and one coveredBy organization/partyid
        /// One of the rules are for an app where no app policy exists and should fail.
        /// Expected Result:
        /// List of all rules (now in sorted order of the resulting 3 delegation policy files).
        /// Only stored rules should have success flag and rule id set.
        /// Success Criteria:
        /// All returned rules match expected
        /// </summary>
        [Fact]
        public async Task TryWriteDelegationPolicyRules_UnknownApp_PartialSuccess()
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
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read", "unknownorg", "unknownapp"), // Should fail as there is no App Policy for this app
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "sign", "org1", "app1", "task1"), // Should be sorted together with the first rule
            };

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read", "org1", "app1", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "sign", "org1", "app1", "task1", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read", "org2", "app1", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read", "unknownorg", "unknownapp", createdSuccessfully: false)
            };

            // Act
            List<Rule> actual = await _pap.TryWriteDelegationPolicyRules(unsortedRules);

            // Assert
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(actual.Where(r => r.CreatedSuccessfully).All(r => !string.IsNullOrEmpty(r.RuleId)));
            Assert.True(actual.Where(r => !r.CreatedSuccessfully).All(r => string.IsNullOrEmpty(r.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
            _logger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == "No valid App policy found for delegation policy path: unknownorg/unknownapp/50001337/u20001337/delegationpolicy.xml" && @type.Name == "FormattedLogValues"),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        /// <summary>
        /// Scenario:
        /// Tests the TryWriteDelegationPolicyRules function, when only partial set of the rules are returned as successfully created.
        /// Input:
        /// List of unordered rules for delegation from the same OfferedBy to two CoveredBy users, and one coveredBy organization/partyid
        /// One of the rules for org1, app1 contains a non-existing resource and should fail all rules for that app.
        /// Expected Result:
        /// List of all rules (now in sorted order of the resulting 3 delegation policy files).
        /// Only stored rules should have success flag and rule id set.
        /// Success Criteria:
        /// All returned rules match expected
        /// </summary>
        [Fact]
        public async Task TryWriteDelegationPolicyRules_UnknownResource_PartialSuccess()
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
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "sign", "org1", "app1", "task1"),
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read", "org1", "app1", task: null, "event1") // This should fail all rules for org1, app1 as there is no event1 resource in the App Policy for this app
            };

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read", "org1", "app1", createdSuccessfully: false),
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "sign", "org1", "app1", "task1", createdSuccessfully: false),
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read", "org1", "app1", task: null, "event1", createdSuccessfully: false),
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read", "org2", "app1", createdSuccessfully: true)
            };

            // Act
            List<Rule> actual = await _pap.TryWriteDelegationPolicyRules(unsortedRules);

            // Assert
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(actual.Where(r => r.CreatedSuccessfully).All(r => !string.IsNullOrEmpty(r.RuleId)));
            Assert.True(actual.Where(r => !r.CreatedSuccessfully).All(r => string.IsNullOrEmpty(r.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
            _logger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((@object, @type) => @object.ToString().StartsWith("Matching rule not found in app policy. Action might not exist for Resource, or Resource itself might not exist. Delegation policy path: org1/app1/50001337/u20001337/delegationpolicy.xml") && @type.Name == "FormattedLogValues"),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        /// <summary>
        /// Scenario:
        /// Tests the TryWriteDelegationPolicyRules function, when only partial set of the rules are returned as successfully created.
        /// Input:
        /// List of unordered rules for delegation from the same OfferedBy to two CoveredBy users, and one coveredBy organization/partyid
        /// One of the rules for org1, app1 is for an action which does not exist for that resource in the app policy and should fail all rules for that app.
        /// Expected Result:
        /// List of all rules (now in sorted order of the resulting 3 delegation policy files).
        /// Only stored rules should have success flag and rule id set.
        /// Success Criteria:
        /// All returned rules match expected
        /// </summary>
        [Fact]
        public async Task TryWriteDelegationPolicyRules_UnknownActionForResource_PartialSuccess()
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
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read", "org1", "app1", "task1"), // This should fail all rules for org1, app1 as there is no read action for the resource in the App Policy for this app
            };

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read", "org1", "app1", createdSuccessfully: false),
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read", "org1", "app1", "task1", createdSuccessfully: false),
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read", "org2", "app1", createdSuccessfully: true),
            };

            // Act
            List<Rule> actual = await _pap.TryWriteDelegationPolicyRules(unsortedRules);

            // Assert
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(actual.Where(r => r.CreatedSuccessfully).All(r => !string.IsNullOrEmpty(r.RuleId)));
            Assert.True(actual.Where(r => !r.CreatedSuccessfully).All(r => string.IsNullOrEmpty(r.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
            _logger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((@object, @type) => @object.ToString().StartsWith("Matching rule not found in app policy. Action might not exist for Resource, or Resource itself might not exist. Delegation policy path: org1/app1/50001337/u20001337/delegationpolicy.xml") && @type.Name == "FormattedLogValues"),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        /// <summary>
        /// Scenario:
        /// Tests the TryWriteDelegationPolicyRules function, when only partial set of the rules are returned as successfully created, caused by one of the rules not having a complete model for sorting to a delegation policy filepath
        /// Input:
        /// List of unordered rules for delegation of the same apps from the same OfferedBy to two CoveredBy users, and one coveredBy organization/partyid
        /// Expected Result:
        /// List of all rules (now in sorted order of the resulting 3 delegation policy files) with success flag and rule id set.
        /// Success Criteria:
        /// All returned rules match expected and have success flag and rule id set
        /// </summary>
        [Fact]
        public async Task TryWriteDelegationPolicyRules_PartialSuccess_UnsortableRule()
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
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read", null, null), // Should fail as the rule model is not complete (missing org/app)
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "sign", "org1", "app1", "task1"), // Should be sorted together with the first rule
            };

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read", "org1", "app1", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "sign", "org1", "app1", "task1", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read", "org2", "app1", createdSuccessfully: true),
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read",  null, null, createdSuccessfully: false)
            };

            // Act
            List<Rule> actual = await _pap.TryWriteDelegationPolicyRules(unsortedRules);

            // Assert
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(actual.Where(r => r.CreatedSuccessfully).All(r => !string.IsNullOrEmpty(r.RuleId)));
            Assert.True(actual.Where(r => !r.CreatedSuccessfully).All(r => string.IsNullOrEmpty(r.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
            _logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((@object, @type) => @object.ToString().StartsWith("One or more rules could not be processed because of incomplete input:") && @type.Name == "FormattedLogValues"),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        /// <summary>
        /// Scenario:
        /// Tests the TryWriteDelegationPolicyRules function, when blobLeaseClient.AcquireAsync throws exception when trying to get lease lock on delegation policy blob
        /// Input:
        /// Single rule
        /// Expected Result:
        /// The blob storage throws exception when aqcuiring lease lock
        /// Success Criteria:
        /// The blob storage exception is handled and logged. The rule is returned as not created.
        /// </summary>
        [Fact]
        public async Task TryWriteDelegationPolicyRules_Error_BlobStorageAqcuireLeaseLockException()
        {
            // Arrange
            int delegatedByUserId = 20001336;
            int offeredByPartyId = 50001337;
            string coveredBy = "20001337";
            string coveredByType = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute;

            List<Rule> unsortedRules = new List<Rule>
            {
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "error", "error", "blobstoragegetleaselockfail"),
            };

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "error", "error", "blobstoragegetleaselockfail", createdSuccessfully: false),
            };

            // Act
            List<Rule> actual = await _pap.TryWriteDelegationPolicyRules(unsortedRules);

            // Assert
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(actual.All(r => string.IsNullOrEmpty(r.RuleId)));
            Assert.True(actual.All(r => !r.CreatedSuccessfully));
            AssertionUtil.AssertEqual(expected, actual);
            _logger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == "Could not acquire blob lease lock on delegation policy at path: error/blobstoragegetleaselockfail/50001337/u20001337/delegationpolicy.xml" && @type.Name == "FormattedLogValues"),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        /// <summary>
        /// Scenario:
        /// Tests the TryWriteDelegationPolicyRules function, when blob storage write throws exception caused by lease locking
        /// Input:
        /// Single rule
        /// Expected Result:
        /// The blob storage write throws exception
        /// Success Criteria:
        /// The blob storage exception is handled and logged. The rule is returned as not created.
        /// </summary>
        [Fact]
        public async Task TryWriteDelegationPolicyRules_Error_BlobStorageLeaseLockWriteException()
        {
            // Arrange
            int delegatedByUserId = 20001336;
            int offeredByPartyId = 50001337;
            string coveredBy = "20001337";
            string coveredByType = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute;

            List<Rule> unsortedRules = new List<Rule>
            {
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "error", "error", "blobstorageleaselockwritefail"),
            };

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "error", "error", "blobstorageleaselockwritefail", createdSuccessfully: false),
            };

            // Act
            List<Rule> actual = await _pap.TryWriteDelegationPolicyRules(unsortedRules);

            // Assert
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(actual.All(r => string.IsNullOrEmpty(r.RuleId)));
            Assert.True(actual.All(r => !r.CreatedSuccessfully));
            AssertionUtil.AssertEqual(expected, actual);
            _logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == "An exception occured while processing authorization rules for delegation on delegation policy path: error/blobstorageleaselockwritefail/50001337/u20001337/delegationpolicy.xml" && @type.Name == "FormattedLogValues"),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        /// <summary>
        /// Scenario:
        /// Tests the TryWriteDelegationPolicyRules function, when getting current delegation change from postgre fails
        /// Input:
        /// Single rule
        /// Expected Result:
        /// The postgre integration throws exception when getting the current change from the database
        /// Success Criteria:
        /// The postgre exception is handled and logged. The rule is returned as not created.
        /// </summary>
        [Fact]
        public async Task TryWriteDelegationPolicyRules_Error_PostgreGetCurrentException()
        {
            // Arrange
            int delegatedByUserId = 20001336;
            int offeredByPartyId = 50001337;
            string coveredBy = "20001337";
            string coveredByType = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute;

            List<Rule> unsortedRules = new List<Rule>
            {
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "error", "error", "postgregetcurrentfail"),
            };

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "error", "error", "postgregetcurrentfail", createdSuccessfully: false),
            };

            // Act
            List<Rule> actual = await _pap.TryWriteDelegationPolicyRules(unsortedRules);

            // Assert
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(actual.All(r => string.IsNullOrEmpty(r.RuleId)));
            Assert.True(actual.All(r => !r.CreatedSuccessfully));
            AssertionUtil.AssertEqual(expected, actual);
            _logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == "An exception occured while processing authorization rules for delegation on delegation policy path: error/postgregetcurrentfail/50001337/u20001337/delegationpolicy.xml" && @type.Name == "FormattedLogValues"),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        /// <summary>
        /// Scenario:
        /// Tests the TryWriteDelegationPolicyRules function, when writing the new current delegation change to postgre fails
        /// Input:
        /// Single rule
        /// Expected Result:
        /// The postgre integration throws exception when writing the new delegation change to the database
        /// Success Criteria:
        /// The postgre exception is handled and logged. The rule is returned as not created.
        /// </summary>
        [Fact]
        public async Task TryWriteDelegationPolicyRules_Error_PostgreWriteDelegationChangeException()
        {
            // Arrange
            int delegatedByUserId = 20001336;
            int offeredByPartyId = 50001337;
            string coveredBy = "20001337";
            string coveredByType = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute;

            List<Rule> unsortedRules = new List<Rule>
            {
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "error", "error", "postgrewritechangefail"),
            };

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "error", "error", "postgrewritechangefail", createdSuccessfully: false),
            };

            // Act
            List<Rule> actual = await _pap.TryWriteDelegationPolicyRules(unsortedRules);

            // Assert
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(actual.All(r => string.IsNullOrEmpty(r.RuleId)));
            Assert.True(actual.All(r => !r.CreatedSuccessfully));
            AssertionUtil.AssertEqual(expected, actual);
            _logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == "Writing of delegation change to authorization postgresql database failed for changes to delegation policy at path: error/postgrewritechangefail/50001337/u20001337/delegationpolicy.xml" && @type.Name == "FormattedLogValues"),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        /// <summary>
        /// Scenario:
        /// Tests the TryWriteDelegationPolicyRules function, but pushing the delegation event to the queue fails.
        /// Input:
        /// List with a rule for delegation of the app error/delegationeventfail between for a single offeredby/coveredby combination resulting in a single delegation policy.
        /// Expected Result:
        /// Internal exception cause pushing delegation event to fail, after delegation has been stored.
        /// Success Criteria:
        /// TryWriteDelegationPolicyRules returns rules as created, but a Critical Error has been logged
        /// </summary>
        [Fact]
        public async Task TryWriteDelegationPolicyRules_DelegationEventQueue_Push_Exception()
        {
            // Arrange
            int delegatedByUserId = 20001337;
            int offeredByPartyId = 50001337;
            string coveredBy = "20001336";
            string coveredByType = AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute;

            List<Rule> unsortedRules = new List<Rule>
            {
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read", "error", "delegationeventfail"),
            };

            List<Rule> expected = new List<Rule>
            {
                TestDataUtil.GetRuleModel(delegatedByUserId, offeredByPartyId, coveredBy, coveredByType, "read", "error", "delegationeventfail", createdSuccessfully: true),
            };

            // Act
            List<Rule> actual = await _pap.TryWriteDelegationPolicyRules(unsortedRules);

            // Assert
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(actual.All(r => r.CreatedSuccessfully));
            Assert.True(actual.All(r => !string.IsNullOrEmpty(r.RuleId)));
            AssertionUtil.AssertEqual(expected, actual);
            _logger.Verify(
                x => x.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((@object, @type) => @object.ToString().StartsWith("AddRules could not push DelegationChangeEvent to DelegationChangeEventQueue. DelegationChangeEvent must be retried for successful sync with SBL Authorization. DelegationChange:") && @type.Name == "FormattedLogValues"),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}
