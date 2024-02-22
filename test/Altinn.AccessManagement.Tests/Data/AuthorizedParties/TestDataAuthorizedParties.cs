using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Tests.Util;
using Altinn.AccessManagement.Tests.Utils;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Altinn.AccessManagement.Tests.Data;

/// <summary>
/// Test data builder for testing AuthorizedParties
/// </summary>
public static class TestDataAuthorizedParties
{
#pragma warning disable SA1600 // Elements should be documented
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    private static string OnlyAltinn3 => "OnlyAltinn3";

    private static string BothAltinn3AndAltinn2 => "BothAltinn3AndAltinn2";

    public static int PersonToPerson_FromUserId => 20100001;

    public static int PersonToPerson_FromPartyId => 50100001;

    public static int PersonToPerson_ToUserId => 20100002;

    public static int PersonToPerson_ToPartyId => 50100002;

    public static int PersonToOrg_FromUserId => 20100003;

    public static int PersonToOrg_FromPartyId => 50100003;

    public static int PersonToOrg_ToOrgPartyId => 50100004;

    public static int PersonToOrg_ToOrgDaglUserId => 20100005;

    public static int PersonToOrg_ToOrgDaglPartyId => 50100005;

    public static int MainUnit_PartyId => 50100006;

    public static int SubUnit_PartyId => 50100007;

    public static int MainUnitAndSubUnitToPerson_ToUserId => 20100008;

    public static int MainUnitAndSubUnitToPerson_ToPartyId => 50100008;

    public static int MainUnitAndSubUnitToOrg_ToOrgPartyId => 50100009;

    public static int MainUnitAndSubUnitToOrg_ToOrgDaglUserId => 20100010;

    public static int MainUnitAndSubUnitToOrg_ToOrgDaglPartyId => 50100010;

#pragma warning restore SA1600 // Elements should be documented

    /// <summary>
    /// Sets up a request without a valid token
    /// </summary>
    public static TheoryData<string> UnauthenticatedNoValidToken() => new()
    {
        {
            string.Empty
        }
    };

    /// <summary>
    /// Sets up a request with a valid token but mmissing a valid urn:altinn:userid claim
    /// </summary>
    public static TheoryData<string> UnauthenticatedValidTokenWithOutUserContext() => new()
    {
        {
            PrincipalUtil.GetToken(0, 0, 0)
        }
    };

    /// <summary>
    /// Sets up the authenticated user,
    /// getting authorized parties from only Altinn 3
    /// where the user has received delegations from a person,
    /// of both an Altinn App and a Resource
    /// </summary>
    public static TheoryData<string, bool, List<AuthorizedPartyExternal>> PersonToPerson() => new()
    {
        {
            PrincipalUtil.GetToken(PersonToPerson_ToUserId, PersonToPerson_ToPartyId, 3),
            false,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("PersonToPerson", OnlyAltinn3)
        }
    };

    /// <summary>
    /// Sets up the authenticated user,
    /// getting authorized parties from both Altinn 3 and Altinn 2
    /// where the user has received delegations from a person,
    /// of both an Altinn App, a Resource and a Role from Altinn 2
    /// </summary>
    public static TheoryData<string, bool, List<AuthorizedPartyExternal>> PersonToPersonInclA2() => new()
    {
        {
            PrincipalUtil.GetToken(PersonToPerson_ToUserId, PersonToPerson_ToPartyId, 3),
            true,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("PersonToPerson", BothAltinn3AndAltinn2)
        }
    };

    /// <summary>
    /// Sets up the authenticated user as DAGL of an organization,
    /// getting authorized parties from only Altinn 3
    /// where the user's organization has received delegations from a person,
    /// of both an Altinn App and a Resource
    /// </summary>
    public static TheoryData<string, bool, List<AuthorizedPartyExternal>> PersonToOrg() => new()
    {
        {
            PrincipalUtil.GetToken(PersonToOrg_ToOrgDaglUserId, PersonToOrg_ToOrgDaglPartyId, 3),
            false,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("PersonToOrg", OnlyAltinn3)
        }
    };

    /// <summary>
    /// Sets up the authenticated user as DAGL of an organization,
    /// getting authorized parties from both Altinn 3 and Altinn 2
    /// where the user's organization has received delegations from a person,
    /// of both an Altinn App, a Resource and a Role from Altinn 2
    /// </summary>
    public static TheoryData<string, bool, List<AuthorizedPartyExternal>> PersonToOrgInclA2() => new()
    {
        {
            PrincipalUtil.GetToken(PersonToOrg_ToOrgDaglUserId, PersonToOrg_ToOrgDaglPartyId, 3),
            true,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("PersonToOrg", BothAltinn3AndAltinn2)
        }
    };

    /// <summary>
    /// Sets up the authenticated user,
    /// getting authorized parties from only Altinn 3
    /// where the user has received the following delegations:
    ///     from mainunit:
    ///         the resource: devtest_gar_authparties-main-to-person
    ///     from subunit:
    ///         the altinn app: ttd/am-devtest-sub-to-person
    /// </summary>
    public static TheoryData<string, bool, List<AuthorizedPartyExternal>> MainUnitAndSubUnitToPerson() => new()
    {
        {
            PrincipalUtil.GetToken(MainUnitAndSubUnitToPerson_ToUserId, MainUnitAndSubUnitToPerson_ToPartyId, 3),
            false,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("MainUnitAndSubUnitToPerson", OnlyAltinn3)
        }
    };

    /// <summary>
    /// Sets up the authenticated user,
    /// getting authorized parties from only Altinn 3
    /// where the user has received the following delegations:
    ///     from mainunit:
    ///         the resource: devtest_gar_authparties-main-to-person
    ///         the role: REGNA
    ///     from subunit:
    ///         the altinn app: ttd/am-devtest-sub-to-person
    ///         the role: SISKD
    /// </summary>
    public static TheoryData<string, bool, List<AuthorizedPartyExternal>> MainUnitAndSubUnitToPersonInclA2() => new()
    {
        {
            PrincipalUtil.GetToken(MainUnitAndSubUnitToPerson_ToUserId, MainUnitAndSubUnitToPerson_ToPartyId, 3),
            true,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("MainUnitAndSubUnitToPerson", BothAltinn3AndAltinn2)
        }
    };

    /// <summary>
    /// Sets up the authenticated user as DAGL of an organization,
    /// getting authorized parties from only Altinn 3
    /// where the user's organization has received the following delegations:
    ///     from mainunit:
    ///         the resource: devtest_gar_authparties-main-to-org
    ///     from subunit:
    ///         the altinn app: ttd/am-devtest-sub-to-org
    /// </summary>
    public static TheoryData<string, bool, List<AuthorizedPartyExternal>> MainUnitAndSubUnitToOrg() => new()
    {
        {
            PrincipalUtil.GetToken(MainUnitAndSubUnitToOrg_ToOrgDaglUserId, MainUnitAndSubUnitToOrg_ToOrgDaglPartyId, 3),
            false,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("MainUnitAndSubUnitToOrg", OnlyAltinn3)
        }
    };

    /// <summary>
    /// Sets up the authenticated user as DAGL of an organization,
    /// getting authorized parties from only Altinn 3
    /// where the user's organization has received the following delegations:
    ///     from mainunit:
    ///         the resource: devtest_gar_authparties-main-to-org
    ///         the role: UTINN
    ///     from subunit:
    ///         the altinn app: ttd/am-devtest-sub-to-org
    ///         the role: APIADM
    /// </summary>
    public static TheoryData<string, bool, List<AuthorizedPartyExternal>> MainUnitAndSubUnitToOrgInclA2() => new()
    {
        {
            PrincipalUtil.GetToken(MainUnitAndSubUnitToOrg_ToOrgDaglUserId, MainUnitAndSubUnitToOrg_ToOrgDaglPartyId, 3),
            true,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("MainUnitAndSubUnitToOrg", BothAltinn3AndAltinn2)
        }
    };

    /// <summary>
    /// Sets up the authenticated user,
    /// getting it's own authorized party
    /// without including Altinn 2 authorized parties
    /// 
    /// Expected result: empty response since Altinn 2 is required in order to get self through PRIV role
    /// </summary>
    public static TheoryData<string, int, bool, ValidationProblemDetails> PersonGettingSelf_BadRequest() => new()
    {
        {
            PrincipalUtil.GetToken(PersonToPerson_ToUserId, PersonToPerson_ToPartyId, 3),
            PersonToPerson_ToPartyId,
            false,
            GetExpectedResponse<ValidationProblemDetails>("PersonGettingSelf", OnlyAltinn3)
        }
    };

    /// <summary>
    /// Sets up the authenticated user,
    /// getting it's own authorized party
    /// including Altinn 2 authorized parties
    /// 
    /// Expected result: success, own authorized party is found including the PRIV authorized role
    /// </summary>
    public static TheoryData<string, int, bool, AuthorizedPartyExternal> PersonGettingSelfInclA2_Success() => new()
    {
        {
            PrincipalUtil.GetToken(PersonToPerson_ToUserId, PersonToPerson_ToPartyId, 3),
            PersonToPerson_ToPartyId,
            true,
            GetExpectedResponse<AuthorizedPartyExternal>("PersonGettingSelf", BothAltinn3AndAltinn2)
        }
    };

    /// <summary>
    /// Test case:  GET AuthorizedParty/{A3DelegatorPartyId} for authenticated user, without including Altinn 2 authorized parties
    /// Expected:   - Should return 200 OK
    ///             - Should include expected authorized party model of the authenticated user
    /// Reason:     Since Altinn 3 delegation exists A2 authorized parties is not needed to find Authorized Party with an authorized resource connection
    /// </summary>
    public static TheoryData<string, int, bool, AuthorizedPartyExternal> PersonGettingA3Delegator_Success() => new()
    {
        {
            PrincipalUtil.GetToken(PersonToPerson_ToUserId, PersonToPerson_ToPartyId, 3),
            PersonToPerson_ToPartyId,
            true,
            GetExpectedResponse<AuthorizedPartyExternal>("PersonGettingSelf", BothAltinn3AndAltinn2)
        }
    };

    /// <summary>
    /// Test case:  GET AuthorizedParty/{A3DelegatorPartyId} for authenticated user, including Altinn 2 authorized parties
    /// Expected:   - Should return 200 OK
    ///             - Should include expected authorized party model of the authenticated user
    /// Reason:     Since Altinn 3 delegation exists Altinn 2 authorized parties is not needed to find Authorized Party with an authorized resource connection,
    ///             but will enrich the Authorized Party with any authorized roles from Altinn 2
    /// </summary>
    public static TheoryData<string, int, bool, AuthorizedPartyExternal> PersonGettingA3DelegatorInclA2_Success() => new()
    {
        {
            PrincipalUtil.GetToken(PersonToPerson_ToUserId, PersonToPerson_ToPartyId, 3),
            PersonToPerson_ToPartyId,
            true,
            GetExpectedResponse<AuthorizedPartyExternal>("PersonGettingSelf", BothAltinn3AndAltinn2)
        }
    };

    /// <summary>
    /// Test case:  GET {party}/authorizedparties?includeAltinn2={includeAltinn2}
    ///             with the authenticated user and getting the authorized party list for it's own {party}
    /// Expected:   - Should return 200 OK
    ///             - Should include expected authorized party list of the authenticated user
    /// Reason:     Authenticated users are authorized for getting own authorized party list
    /// </summary>
    public static TheoryData<string, int, bool, List<AuthorizedPartyExternal>> PersonGettingOwnList_Success() => new()
    {
        {
            PrincipalUtil.GetToken(PersonToPerson_ToUserId, PersonToPerson_ToPartyId, 3),
            PersonToPerson_ToPartyId,
            true,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("PersonGettingOwnList", BothAltinn3AndAltinn2)
        }
    };

    /// <summary>
    /// Test case:  GET {party}/authorizedparties?includeAltinn2={includeAltinn2}
    ///             with the authenticated user being an authorized Access Manager for the {party} organization
    /// Expected:   - Should return 200 OK
    ///             - Should include expected authorized party list of the organization
    /// Reason:     Authenticated users who are authorized as Access Manager for organizations should be allowed to get the organizations authorized party list
    /// </summary>
    public static TheoryData<string, int, bool, List<AuthorizedPartyExternal>> AccessManagerGettingOrgList_Success() => new()
    {
        {
            PrincipalUtil.GetToken(MainUnitAndSubUnitToOrg_ToOrgDaglUserId, MainUnitAndSubUnitToOrg_ToOrgDaglPartyId, 3),
            MainUnitAndSubUnitToOrg_ToOrgPartyId,
            true,
            GetExpectedResponse<List<AuthorizedPartyExternal>>("AccessManagerGettingOrgList", BothAltinn3AndAltinn2)
        }
    };

    /// <summary>
    /// Test case:  GET {party}/authorizedparties?includeAltinn2={includeAltinn2}
    ///             with the authenticated user being an authorized Access Manager for the {party} person
    /// Expected:   - Should return 403 Forbidden
    ///             - Should include expected authorized party list of the organization
    /// Reason:     Authenticated users who are authorized as Access Manager for organizations should be allowed to get the organizations authorized party list
    /// </summary>
    public static TheoryData<string, int, bool> AccessManagerGettingPersonList_Forbidden() => new()
    {
        {
            PrincipalUtil.GetToken(PersonToPerson_ToUserId, PersonToPerson_ToPartyId, 3),
            PersonToPerson_FromPartyId,
            true
        }
    };

    private static T GetExpectedResponse<T>(string delegationType, string retrievalType)
    {
        string content = File.ReadAllText($"Data/Json/AuthorizedParties/{delegationType}/{retrievalType}.json");
        return (T)JsonSerializer.Deserialize(content, typeof(T), JsonOptions);
    }

    /// <summary>
    /// Assert that two <see cref="AuthorizedParty"/> have the same property in the same positions.
    /// </summary>
    /// <param name="expected">An instance with the expected values.</param>
    /// <param name="actual">The instance to verify.</param>
    public static void AssertAuthorizedPartyExternalEqual(AuthorizedPartyExternal expected, AuthorizedPartyExternal actual)
    {
        Assert.NotNull(actual);
        Assert.NotNull(expected);

        Assert.Equal(expected.PartyId, actual.PartyId);
        Assert.Equal(expected.PartyUuid, actual.PartyUuid);
        Assert.Equal(expected.PartyType, actual.PartyType);
        Assert.Equal(expected.OrgNumber, actual.OrgNumber);
        Assert.Equal(expected.SSN, actual.SSN);
        Assert.Equal(expected.UnitType, actual.UnitType);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.IsDeleted, actual.IsDeleted);
        Assert.Equal(expected.OnlyHierarchyElementWithNoAccess, actual.OnlyHierarchyElementWithNoAccess);
        Assert.Equal(expected.AuthorizedResources, actual.AuthorizedResources);
        Assert.Equal(expected.AuthorizedRoles, actual.AuthorizedRoles);
        AssertionUtil.AssertCollections(expected.ChildParties, actual.ChildParties, AssertAuthorizedPartyExternalEqual);
    }
}