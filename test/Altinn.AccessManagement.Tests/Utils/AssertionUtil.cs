using System;
using System.Collections.Generic;
using System.Linq;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Models;
using Altinn.Authorization.ABAC.Xacml;
using Altinn.Authorization.ABAC.Xacml.JsonProfile;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Altinn.AccessManagement.Tests.Utils
{
    /// <summary>
    /// Class with methods that can help with assertions of larger objects.
    /// </summary>
    public static class AssertionUtil
    {
        /// <summary>
        /// Asserts that two collections of objects have the same property values in the same positions.
        /// </summary>
        /// <typeparam name="T">The Type</typeparam>
        /// <param name="expected">A collection of expected instances</param>
        /// <param name="actual">The collection of actual instances</param>
        /// <param name="assertMethod">The assertion method to be used</param>
        public static void AssertCollections<T>(ICollection<T> expected, ICollection<T> actual, Action<T, T> assertMethod)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.Equal(expected.Count, actual.Count);

            Dictionary<int, T> expectedDict = new Dictionary<int, T>();
            Dictionary<int, T> actualDict = new Dictionary<int, T>();

            int i = 1;
            foreach (T ex in expected)
            {
                expectedDict.Add(i, ex);
                i++;
            }

            i = 1;
            foreach (T ac in actual)
            {
                actualDict.Add(i, ac);
                i++;
            }

            foreach (int key in expectedDict.Keys)
            {
                assertMethod(expectedDict[key], actualDict[key]);
            }
        }

        /// <summary>
        /// Assert that two <see cref="XacmlContextResponse"/> have the same property values.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertEqual(XacmlContextResponse expected, XacmlContextResponse actual)
        {
            Assert.NotNull(actual);
            Assert.NotNull(expected);
            Assert.Equal(expected.Results.Count, actual.Results.Count);

            if (expected.Results.Count > 0)
            {
                AssertEqual(expected.Results.First(), actual.Results.First());
            }
        }
        
        /// <summary>
        /// Assert that two <see cref="XacmlJsonResponse"/> have the same property values.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertEqual(XacmlJsonResponse expected, XacmlJsonResponse actual)
        {
            Assert.NotNull(actual);
            Assert.NotNull(expected);
            Assert.Equal(expected.Response.Count, actual.Response.Count);

            if (expected.Response.Count > 0)
            {
                for (int i = 0; i < expected.Response.Count; i++)
                {
                    AssertEqual(expected.Response[i], actual.Response[i]);
                }
            }
        }

        /// <summary>
        /// Assert that two dictionaries of <see cref="DelegationChange"/> have the same property values.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertEqual(Dictionary<string, List<DelegationChange>> expected, Dictionary<string, List<DelegationChange>> actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.Equal(expected.Count, actual.Count);
            foreach (KeyValuePair<string, List<DelegationChange>> expectedEntry in expected)
            {
                List<DelegationChange> actualValue = actual[expectedEntry.Key];
                Assert.NotNull(actualValue);
                AssertEqual(expectedEntry.Value, actualValue);
            }
        }
        
        /// <summary>
        /// Assert that two lists of <see cref="DelegationChange"/> have the same property values.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertEqual(List<DelegationChange> expected, List<DelegationChange> actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.Equal(expected.Count, actual.Count);
            foreach (DelegationChange expectedEntity in expected)
            {
                DelegationChange actualentity =
                    actual.FirstOrDefault(a => a.ResourceId == expectedEntity.ResourceId && 
                                               a.ResourceType == expectedEntity.ResourceType && 
                                               a.BlobStoragePolicyPath == expectedEntity.BlobStoragePolicyPath && 
                                               a.CoveredByPartyId == expectedEntity.CoveredByPartyId && 
                                               a.CoveredByUserId == expectedEntity.CoveredByUserId &&
                                               a.OfferedByPartyId == expectedEntity.OfferedByPartyId &&
                                               a.DelegationChangeType == expectedEntity.DelegationChangeType);
                Assert.NotNull(actualentity);
            }
        }
        
        /// <summary>
        /// Assert that two lists of <see cref="DelegationChange"/> have the same property values.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertEqual(List<DelegationChangeExternal> expected, List<DelegationChangeExternal> actual)
        {
            Assert.Equal(expected.Count, actual.Count);
            Assert.Equal(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                AssertEqual(expected[i], actual[i]);
            }
        }

        /// <summary>
        /// Assert that two <see cref="DelegationChangeEventList"/> have the same property values.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertEqual(DelegationChangeEventList expected, DelegationChangeEventList actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.Equal(expected.DelegationChangeEvents.Count, actual.DelegationChangeEvents.Count);
            for (int i = 0; i < expected.DelegationChangeEvents.Count; i++)
            {
                AssertEqual(expected.DelegationChangeEvents[i], actual.DelegationChangeEvents[i]);
            }
        }

        /// <summary>
        /// Assert that two <see cref="DelegationChangeEvent"/> have the same property values.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertEqual(DelegationChangeEvent expected, DelegationChangeEvent actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.Equal(expected.EventType, actual.EventType);
            AssertEqual(expected.DelegationChange, actual.DelegationChange);
        }

        /// <summary>
        /// Assert that two <see cref="SimpleDelegationChange"/> have the same property values.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertEqual(SimpleDelegationChange expected, SimpleDelegationChange actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.Equal(expected.DelegationChangeId, actual.DelegationChangeId);
            Assert.Equal(expected.AltinnAppId, actual.AltinnAppId);
            Assert.Equal(expected.OfferedByPartyId, actual.OfferedByPartyId);
            Assert.Equal(expected.CoveredByPartyId, actual.CoveredByPartyId);
            Assert.Equal(expected.CoveredByUserId, actual.CoveredByUserId);
            Assert.Equal(expected.PerformedByUserId, actual.PerformedByUserId);
            Assert.Equal(expected.Created, actual.Created);
        }

        /// <summary>
        /// Assert that two <see cref="XacmlContextRequest"/> have the same property values.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertEqual(XacmlContextRequest expected, XacmlContextRequest actual)
        {
            Assert.Equal(expected.Attributes.Count, actual.Attributes.Count);
            Assert.Equal(expected.GetResourceAttributes().Attributes.Count, actual.GetResourceAttributes().Attributes.Count);
            Assert.Equal(expected.GetSubjectAttributes().Attributes.Count, actual.GetSubjectAttributes().Attributes.Count);
            AssertEqual(expected.Attributes, actual.Attributes);
        }

        /// <summary>
        /// Assert that two Lists of <see cref="Rule"/> have the same number of rules and each rule have the same property values.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        /// <param name="assertOutputValues">Whether output only values should also be asserted</param>
        public static void AssertEqual(List<Rule> expected, List<Rule> actual, bool assertOutputValues = false)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.Equal(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                AssertEqual(expected[i], actual[i], assertOutputValues);
            }
        }

        /// <summary>
        /// Assert that two <see cref="Rule"/> have the same property values.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        /// <param name="assertOutputValues">Whether output only values should also be asserted</param>
        public static void AssertEqual(Rule expected, Rule actual, bool assertOutputValues = false)
        {
            Assert.NotNull(actual);
            Assert.NotNull(expected);

            if (assertOutputValues)
            {
                Assert.Equal(expected.RuleId, actual.RuleId);
                Assert.Equal(expected.Type, actual.Type);
            }

            Assert.Equal(expected.CreatedSuccessfully, actual.CreatedSuccessfully);
            Assert.Equal(expected.DelegatedByUserId, actual.DelegatedByUserId);
            Assert.Equal(expected.DelegatedByPartyId, actual.DelegatedByPartyId);
            Assert.Equal(expected.OfferedByPartyId, actual.OfferedByPartyId);
            AssertEqual(expected.CoveredBy, actual.CoveredBy);
            AssertEqual(expected.Resource, actual.Resource);
            AssertEqual(expected.Action, actual.Action);
        }

        /// <summary>
        /// Assert that two <see cref="MaskinportenSchemaDelegationExternal"/> have the same property in the same positions.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertMaskinportenSchemaDelegationExternalEqual(MaskinportenSchemaDelegationExternal expected, MaskinportenSchemaDelegationExternal actual)
        {
            Assert.NotNull(actual);
            Assert.NotNull(expected);

            Assert.Equal(expected.OfferedByPartyId, actual.OfferedByPartyId);
            Assert.Equal(expected.OfferedByName, actual.OfferedByName);
            Assert.Equal(expected.OfferedByOrganizationNumber, actual.OfferedByOrganizationNumber);
            Assert.Equal(expected.CoveredByPartyId, actual.CoveredByPartyId);
            Assert.Equal(expected.CoveredByName, actual.CoveredByName);
            Assert.Equal(expected.CoveredByOrganizationNumber, actual.CoveredByOrganizationNumber);
            ////Assert.Equal(expected.PerformedByUserId, actual.PerformedByUserId);
            Assert.Equal(expected.ResourceId, actual.ResourceId);
        }

        /// <summary>
        /// Assert that two <see cref="CompetentAuthorityExternal"/> have the same property in the same positions.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertCompetentAuthorityEqual(CompetentAuthorityExternal expected, CompetentAuthorityExternal actual)
        {
            Assert.NotNull(actual);
            Assert.NotNull(expected);

            Assert.Equal(expected?.Orgcode, actual.Orgcode);
            Assert.Equal(expected?.Organization, actual.Organization);
            Assert.Equal(expected?.Name, actual.Name);
        }

        /// <summary>
        /// Assert that two <see cref="MPDelegationExternal"/> have the same property in the same positions.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertDelegationEqual(MPDelegationExternal expected, MPDelegationExternal actual)
        {
            Assert.NotNull(actual);
            Assert.NotNull(expected);

            Assert.Equal(expected.SupplierOrg, actual.SupplierOrg);
            Assert.Equal(expected.ConsumerOrg, actual.ConsumerOrg);
            Assert.Equal(expected.Scopes, actual.Scopes);
            Assert.Equal(expected.DelegationSchemeId, actual.DelegationSchemeId);
            Assert.Equal(expected.Created, actual.Created);
        }

        /// <summary>
        /// Assert that two <see cref="PartyExternal"/> have the same property in the same positions.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertPartyEqual(PartyExternal expected, PartyExternal actual)
        {
            Assert.NotNull(actual);
            Assert.NotNull(expected);

            Assert.Equal(expected.PartyTypeName, actual.PartyTypeName);
            Assert.Equal(expected.OrgNumber, actual.OrgNumber);
            Assert.Equal(expected.Organization.Name, actual.Organization.Name);
            Assert.Equal(expected.Organization.OrgNumber, actual.Organization.OrgNumber);
            Assert.Equal(expected.Organization.UnitType, actual.Organization.UnitType);
            Assert.Equal(expected.PartyId, actual.PartyId);
            Assert.Equal(expected.UnitType, actual.UnitType);
            Assert.Equal(expected.Name, actual.Name);
        }

        /// <summary>
        /// Assert that two <see cref="Rule"/> have the same property in the same positions.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertRuleEqual(Rule expected, Rule actual)
        {
            Assert.NotNull(actual);
            Assert.NotNull(expected);

            Assert.Equal(expected.CreatedSuccessfully, actual.CreatedSuccessfully);
            Assert.Equal(expected.DelegatedByUserId, actual.DelegatedByUserId);
            Assert.Equal(expected.OfferedByPartyId, actual.OfferedByPartyId);
            Assert.Equal(expected.Type, actual.Type);
            AssertEqual(expected.CoveredBy, actual.CoveredBy);
            AssertEqual(expected.Resource, actual.Resource);
            AssertEqual(expected.Action, actual.Action);
        }

        /// <summary>
        /// Assert that two <see cref="RightExternal"/> have the same property in the same positions.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertRightExternalEqual(RightExternal expected, RightExternal actual)
        {
            Assert.NotNull(actual);
            Assert.NotNull(expected);

            Assert.Equal(expected.RightKey, actual.RightKey);
            AssertCollections(expected.Resource, actual.Resource, AssertAttributeMatchExternalEqual);
            Assert.Equal(expected.Action, actual.Action);
            Assert.Equal(expected.HasPermit, actual.HasPermit);
            Assert.Equal(expected.CanDelegate, actual.CanDelegate);
            AssertCollections(expected.RightSources, actual.RightSources, AssertRightSourceExternalEqual);
        }

        /// <summary>
        /// Assert that two <see cref="RightSourceExternal"/> have the same property in the same positions.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertRightSourceExternalEqual(RightSourceExternal expected, RightSourceExternal actual)
        {
            Assert.NotNull(actual);
            Assert.NotNull(expected);

            Assert.Equal(expected.RightSourceType, actual.RightSourceType);
            Assert.Equal(expected.PolicyId, actual.PolicyId);
            Assert.Equal(expected.PolicyVersion, actual.PolicyVersion);
            Assert.Equal(expected.RuleId, actual.RuleId);
            Assert.Equal(expected.HasPermit, actual.HasPermit);
            Assert.Equal(expected.CanDelegate, actual.CanDelegate);
            Assert.Equal(expected.OfferedByPartyId, actual.OfferedByPartyId);

            AssertCollections(expected.UserSubjects, actual.UserSubjects, AssertAttributeMatchExternalEqual);
            AssertCollections(expected.PolicySubjects, actual.PolicySubjects, AssertPolicySubjects);
        }

        /// <summary>
        /// Assert that two <see cref="BaseRightExternal"/> have the same property in the same positions.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertBaseRightExternalEqual(BaseRightExternal expected, BaseRightExternal actual)
        {
            Assert.NotNull(actual);
            Assert.NotNull(expected);

            AssertCollections(expected.Resource, actual.Resource, AssertAttributeMatchExternalEqual);
            Assert.Equal(expected.Action, actual.Action);
        }

        /// <summary>
        /// Assert that two <see cref="RightsDelegationResponseExternal"/> have the same property in the same positions.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertRightsDelegationResponseExternalEqual(RightsDelegationResponseExternal expected, RightsDelegationResponseExternal actual)
        {
            Assert.NotNull(actual);
            Assert.NotNull(expected);

            AssertCollections(expected.To, actual.To, AssertAttributeMatchExternalEqual);
            AssertCollections(expected.RightDelegationResults, actual.RightDelegationResults, AssertRightDelegationResultExternalEqual);
        }

        /// <summary>
        /// Assert that two <see cref="RightDelegationResultExternal"/> have the same property in the same positions.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertRightDelegationResultExternalEqual(RightDelegationResultExternal expected, RightDelegationResultExternal actual)
        {
            Assert.NotNull(actual);
            Assert.NotNull(expected);

            Assert.Equal(expected.RightKey, actual.RightKey);
            AssertCollections(expected.Resource, actual.Resource, AssertAttributeMatchExternalEqual);
            Assert.Equal(expected.Action, actual.Action);
            Assert.Equal(expected.Status, actual.Status);
            AssertCollections(expected.Details, actual.Details, AssertDetailExternalEqual);
        }

        /// <summary>
        /// Assert that two <see cref="ValidationProblemDetails"/> have the same property in the same positions.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertValidationProblemDetailsEqual(ValidationProblemDetails expected, ValidationProblemDetails actual)
        {
            Assert.NotNull(actual);
            Assert.NotNull(expected);

            Assert.Equal(expected.Type, actual.Type);
            Assert.Equal(expected.Title, actual.Title);
            Assert.Equal(expected.Status, actual.Status);

            Assert.Equal(expected.Errors.Keys.Count, actual.Errors.Keys.Count);
            Assert.True(expected.Errors.Keys.All(expectedKey => actual.Errors.ContainsKey(expectedKey)));
            foreach (string expectedKey in expected.Errors.Keys)
            {
                Assert.Equal(expected.Errors[expectedKey], actual.Errors[expectedKey]);
            }
        }

        /// <summary>
        /// Assert that two <see cref="RightDelegationCheckResultExternal"/> have the same property in the same positions.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertRightDelegationCheckExternalEqual(RightDelegationCheckResultExternal expected, RightDelegationCheckResultExternal actual)
        {
            Assert.NotNull(actual);
            Assert.NotNull(expected);

            Assert.Equal(expected.RightKey, actual.RightKey);
            AssertCollections(expected.Resource, actual.Resource, AssertAttributeMatchExternalEqual);
            Assert.Equal(expected.Action, actual.Action);
            Assert.Equal(expected.Status, actual.Status);
            AssertCollections(expected.Details, actual.Details, AssertDetailExternalEqual);
        }

        /// <summary>
        /// Assert that two <see cref="DetailExternal"/> have the same property in the same positions.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertDetailExternalEqual(DetailExternal expected, DetailExternal actual)
        {
            Assert.NotNull(actual);
            Assert.NotNull(expected);

            Assert.Equal(expected.Code, actual.Code);
            Assert.Equal(expected.Description, actual.Description);
            AssertDetailParametersExternalEqual(expected.Parameters, actual.Parameters);
        }

        /// <summary>
        /// Assert that two detail parameter dictionaries have the same property in the same positions.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertDetailParametersExternalEqual(Dictionary<string, List<AttributeMatchExternal>> expected, Dictionary<string, List<AttributeMatchExternal>> actual)
        {
            Assert.NotNull(actual);
            Assert.NotNull(expected);

            Assert.Equal(expected.Keys.Count, actual.Keys.Count);
            Assert.True(expected.Keys.All(expectedKey => actual.Keys.Contains(expectedKey)));
            foreach (string key in expected.Keys)
            {
                AssertCollections(expected[key], actual[key], AssertAttributeMatchExternalEqual);
            }
        }

        /// <summary>
        /// Assert that two <see cref="AuthorizedParty"/> have the same property in the same positions.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertAuthorizedPartyEqual(AuthorizedParty expected, AuthorizedParty actual)
        {
            Assert.NotNull(actual);
            Assert.NotNull(expected);

            Assert.Equal(expected.PartyId, actual.PartyId);
            Assert.Equal(expected.PartyUuid, actual.PartyUuid);
            Assert.Equal(expected.Type, actual.Type);
            Assert.Equal(expected.OrganizationNumber, actual.OrganizationNumber);
            Assert.Equal(expected.PersonId, actual.PersonId);
            Assert.Equal(expected.UnitType, actual.UnitType);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.IsDeleted, actual.IsDeleted);
            Assert.Equal(expected.OnlyHierarchyElementWithNoAccess, actual.OnlyHierarchyElementWithNoAccess);
            Assert.Equal(expected.AuthorizedResources, actual.AuthorizedResources);
            Assert.Equal(expected.AuthorizedRoles, actual.AuthorizedRoles);
            AssertCollections(expected.Subunits, actual.Subunits, AssertAuthorizedPartyEqual);
        }

        private static void AssertPolicySubjects(List<PolicyAttributeMatchExternal> expected, List<PolicyAttributeMatchExternal> actual)
        {
            AssertCollections(expected, actual, AssertPolicyAttributeMatchExternalEqual);
        }

        private static void AssertPolicyAttributeMatchExternalEqual(PolicyAttributeMatchExternal expected, PolicyAttributeMatchExternal actual)
        {
            Assert.NotNull(actual);
            Assert.NotNull(expected);

            Assert.Equal(expected.MatchFound, actual.MatchFound);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Value, actual.Value);
        }

        private static void AssertAttributeMatchExternalEqual(AttributeMatchExternal expected, AttributeMatchExternal actual)
        {
            Assert.NotNull(actual);
            Assert.NotNull(expected);

            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Value, actual.Value);
        }

        /// <summary>
        /// Compares two list contains the same data 
        /// </summary>
        /// <param name="expected">the expected list</param>
        /// <param name="actual">the actual list</param>
        public static void ListAccessManagementResourceAreEqual(List<AccessManagementResource> expected, List<AccessManagementResource> actual)
        {
            Assert.Equal(expected.Count, actual.Count);
            for (int i = 0; i < actual.Count; i++)
            {
                AccessManagementResource currentExpectedElement = expected[i];
                AccessManagementResource currentActualElement = actual[i];

                Assert.Equal(currentExpectedElement.Created, currentActualElement.Created);
                Assert.Equal(currentExpectedElement.Modified, currentActualElement.Modified);
                Assert.Equal(currentExpectedElement.ResourceRegistryId, currentActualElement.ResourceRegistryId);
                Assert.Equal(currentExpectedElement.ResourceId, currentActualElement.ResourceId);
                Assert.Equal(currentExpectedElement.ResourceType, currentActualElement.ResourceType);
            }
        }

        private static void AssertEqual(List<AttributeMatch> expected, List<AttributeMatch> actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.Equal(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                AssertEqual(expected[i], actual[i]);
            }
        }

        private static void AssertEqual(AttributeMatch expected, AttributeMatch actual)
        {
            Assert.NotNull(actual);
            Assert.NotNull(expected);

            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Value, actual.Value);
        }

        private static void AssertEqual(XacmlJsonResult expected, XacmlJsonResult actual)
        {
            Assert.Equal(expected.Decision, actual.Decision);
            Assert.Equal(expected.Status.StatusCode.Value, actual.Status.StatusCode.Value);
            AssertEqual(expected.Obligations, actual.Obligations);
            AssertEqual(expected.Category, actual.Category);
        }

        private static void AssertEqual(List<XacmlJsonObligationOrAdvice> expected, List<XacmlJsonObligationOrAdvice> actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.NotNull(actual);

            Assert.Equal(expected.Count, actual.Count);

            AssertEqual(expected.FirstOrDefault(), actual.FirstOrDefault());
        }

        private static void AssertEqual(List<XacmlJsonCategory> expected, List<XacmlJsonCategory> actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.NotNull(actual);

            Assert.Equal(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                AssertEqual(expected[i], actual[i]);
            }
        }

        private static void AssertEqual(XacmlJsonCategory expected, XacmlJsonCategory actual)
        {
            Assert.Equal(expected.CategoryId, actual.CategoryId);
            Assert.Equal(expected.Content, actual.Content);
            Assert.Equal(expected.Id, actual.Id);
            AssertEqual(expected.Attribute, actual.Attribute);
        }

        private static void AssertEqual(XacmlJsonObligationOrAdvice expected, XacmlJsonObligationOrAdvice actual)
        {
            Assert.Equal(expected.AttributeAssignment.Count, actual.AttributeAssignment.Count);

            AssertEqual(expected.AttributeAssignment.FirstOrDefault(), actual.AttributeAssignment.FirstOrDefault());
        }

        private static void AssertEqual(XacmlJsonAttributeAssignment expected, XacmlJsonAttributeAssignment actual)
        {
            Assert.Equal(expected.AttributeId, actual.AttributeId);
            Assert.Equal(expected.Category, actual.Category);
            Assert.Equal(expected.DataType, actual.DataType);
            Assert.Equal(expected.Issuer, actual.Issuer);
            Assert.Equal(expected.Value, actual.Value, true);
        }

        private static void AssertEqual(List<XacmlJsonAttribute> expected, List<XacmlJsonAttribute> actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.Equal(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                AssertEqual(expected[i], actual[i]);
            }
        }

        private static void AssertEqual(XacmlJsonAttribute expected, XacmlJsonAttribute actual)
        {
            Assert.Equal(expected.AttributeId, actual.AttributeId);
            Assert.Equal(expected.DataType, actual.DataType);
            Assert.Equal(expected.IncludeInResult, actual.IncludeInResult);
            Assert.Equal(expected.Issuer, actual.Issuer);
            Assert.Equal(expected.Value, actual.Value, true);
        }

        private static void AssertEqual(XacmlContextResult expected, XacmlContextResult actual)
        {
            Assert.NotNull(actual);
            Assert.NotNull(expected);

            Assert.Equal(expected.Decision, actual.Decision);

            AssertEqual(expected.Status, actual.Status);

            AssertEqual(expected.Attributes, actual.Attributes);

            Assert.Equal(expected.Obligations.Count, actual.Obligations.Count);

            if (expected.Obligations.Count > 0)
            {
                AssertEqual(expected.Obligations.First(), actual.Obligations.First());
            }
        }

        private static void AssertEqual(XacmlContextStatus expected, XacmlContextStatus actual)
        {
            if (expected != null)
            {
                Assert.NotNull(actual);
                Assert.Equal(expected.StatusCode.StatusCode, actual.StatusCode.StatusCode);
            }
        }

        private static void AssertEqual(XacmlObligation expected, XacmlObligation actual)
        {
            Assert.Equal(expected.FulfillOn, actual.FulfillOn);
            Assert.Equal(expected.ObligationId, actual.ObligationId);
            Assert.Equal(expected.AttributeAssignment.Count, expected.AttributeAssignment.Count);

            if (expected.AttributeAssignment.Count > 0)
            {
                AssertEqual(expected.AttributeAssignment.First(), actual.AttributeAssignment.First());
            }
        }

        private static void AssertEqual(ICollection<XacmlContextAttributes> expected, ICollection<XacmlContextAttributes> actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.NotNull(actual);
            Assert.Equal(expected.Count, actual.Count);

            List<XacmlContextAttributes> expectedList = expected.ToList();
            List<XacmlContextAttributes> actualList = actual.ToList();

            for (int i = 0; i < expected.Count; i++)
            {
                AssertEqual(expectedList[i], actualList[i]);
            }
        }

        private static void AssertEqual(XacmlContextAttributes expected, XacmlContextAttributes actual)
        {
            Assert.Equal(expected.Category.OriginalString, actual.Category.OriginalString);

            List<XacmlAttribute> expectedList = expected.Attributes.ToList();
            List<XacmlAttribute> actualList = actual.Attributes.ToList();

            for (int i = 0; i < expected.Attributes.Count; i++)
            {
                AssertEqual(expectedList[i], actualList[i]);
            }
        }

        private static void AssertEqual(XacmlAttribute expected, XacmlAttribute actual)
        {
            Assert.Equal(expected.AttributeId, actual.AttributeId);
            Assert.Equal(expected.IncludeInResult, actual.IncludeInResult);
            Assert.Equal(expected.Issuer, actual.Issuer);
            Assert.Equal(expected.AttributeValues.Count, actual.AttributeValues.Count);
            AssertEqual(expected.AttributeValues, actual.AttributeValues);
        }

        private static void AssertEqual(ICollection<XacmlAttributeValue> expected, ICollection<XacmlAttributeValue> actual)
        {
            List<XacmlAttributeValue> expectedList = expected.ToList();
            List<XacmlAttributeValue> actualList = actual.ToList();

            for (int i = 0; i < expected.Count; i++)
            {
                AssertEqual(expectedList[i], actualList[i]);
            }
        }

        private static void AssertEqual(XacmlAttributeValue expected, XacmlAttributeValue actual)
        {
            Assert.Equal(expected.DataType, actual.DataType);
            Assert.Equal(expected.Value, actual.Value, ignoreCase: true);
        }

        private static void AssertEqual(XacmlAttributeAssignment expected, XacmlAttributeAssignment actual)
        {
            Assert.Equal(expected.Value, actual.Value, ignoreCase: true);
            Assert.Equal(expected.Category, actual.Category);
            Assert.Equal(expected.AttributeId, actual.AttributeId);
            Assert.Equal(expected.DataType, actual.DataType);
        }

        private static void AssertResourcePolicyEqual(ResourcePolicy expected, ResourcePolicy actual)
        {
            Assert.Equal(expected.Title, actual.Title);
            AssertCollections(expected.Actions, actual.Actions, AssertResourceActionEqual);
            AssertCollections(expected.Resource, actual.Resource, AssertAttributeMatchEqual);
            if (expected.Description != null || actual.Description != null)
            {
                Assert.Equal(expected.Description, actual.Description);
            }
        }

        private static void AssertResourceActionEqual(ResourceAction expected, ResourceAction actual)
        {
            Assert.Equal(expected.Title, actual.Title);
            AssertAttributeMatchEqual(expected.Match, actual.Match);
            AssertCollections(expected.RoleGrants, actual.RoleGrants, AssertRoleGrantEqual);

            if (expected.Description != null && actual.Description != null)
            {
                Assert.Equal(expected.Description, actual.Description);
            }
        }

        private static void AssertRoleGrantEqual(RoleGrant expected, RoleGrant actual)
        {
            Assert.Equal(expected.IsDelegable, actual.IsDelegable);
            Assert.Equal(expected.RoleTypeCode, actual.RoleTypeCode);
        }

        private static void AssertAttributeMatchEqual(AttributeMatch expected, AttributeMatch actual)
        {
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Value, actual.Value);
        }

        private static void AssertEqual(Delegation expected, Delegation actual) 
        {
            Assert.Equal(expected.CoveredByPartyId, actual.CoveredByPartyId);
            Assert.Equal(expected.PerformedByUserId, actual.PerformedByUserId);
            Assert.Equal(expected.OfferedByPartyId, actual.OfferedByPartyId);
            Assert.Equal(expected.CoveredByName, actual.CoveredByName);
            Assert.Equal(expected.CoveredByOrganizationNumber, actual.CoveredByOrganizationNumber);
        }

        private static void AssertEqual(ServiceResource expected, ServiceResource actual)
        {
            Assert.Equal(expected.Identifier, actual.Identifier);
            Assert.Equal(expected.Title, actual.Title);
        }
        
        private static void AssertEqual(DelegationChangeExternal expected, DelegationChangeExternal actual)
        {
            Assert.Equal(expected.DelegationChangeId, actual.DelegationChangeId);
            Assert.Equal(expected.ResourceRegistryDelegationChangeId, actual.ResourceRegistryDelegationChangeId);
            Assert.Equal(expected.DelegationChangeType, actual.DelegationChangeType);
            Assert.Equal(expected.ResourceId, actual.ResourceId);
            Assert.Equal(expected.ResourceType, actual.ResourceType);
            Assert.Equal(expected.OfferedByPartyId, actual.OfferedByPartyId);
            Assert.Equal(expected.CoveredByPartyId, actual.CoveredByPartyId);
            Assert.Equal(expected.CoveredByUserId, actual.CoveredByUserId);
            Assert.Equal(expected.PerformedByUserId, actual.PerformedByUserId);
            Assert.Equal(expected.PerformedByPartyId, actual.PerformedByPartyId);
            Assert.Equal(expected.BlobStoragePolicyPath, actual.BlobStoragePolicyPath);
        }
    }
}
