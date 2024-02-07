using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Tests.Util;

/// <summary>
/// Test data builder for testing AuthorizedParties
/// </summary>
public static class TestDataAuthorizedParties
{
#pragma warning disable SA1600 // Elements should be documented
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
    /// <returns></returns>
    public static IEnumerable<object[]> UnauthenticatedNoValidToken() => [[
        string.Empty
    ]];

    /// <summary>
    /// Sets up a request with a valid token but mmissing a valid urn:altinn:userid claim
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> UnauthenticatedValidTokenWithOutUserContext() => [[
        PrincipalUtil.GetToken(0, 0, 0)
    ]];

    /// <summary>
    /// Sets up the authenticated user,
    /// getting authorized parties from only Altinn 3
    /// where the user has received delegations from a person,
    /// of both an Altinn App and a Resource
    /// </summary>
    public static IEnumerable<object[]> PersonToPerson() => [[
        PrincipalUtil.GetToken(PersonToPerson_ToUserId, PersonToPerson_ToPartyId, 3),
        false,
        GetExpectedAuthorizedParties("PersonToPerson", OnlyAltinn3)
    ]];

    /// <summary>
    /// Sets up the authenticated user,
    /// getting authorized parties from both Altinn 3 and Altinn 2
    /// where the user has received delegations from a person,
    /// of both an Altinn App, a Resource and a Role from Altinn 2
    /// </summary>
    public static IEnumerable<object[]> PersonToPersonInclA2() => [[
        PrincipalUtil.GetToken(PersonToPerson_ToUserId, PersonToPerson_ToPartyId, 3),
        true,
        GetExpectedAuthorizedParties("PersonToPerson", BothAltinn3AndAltinn2)
    ]];

    /// <summary>
    /// Sets up the authenticated user as DAGL of an organization,
    /// getting authorized parties from only Altinn 3
    /// where the user's organization has received delegations from a person,
    /// of both an Altinn App and a Resource
    /// </summary>
    public static IEnumerable<object[]> PersonToOrg() => [[
        PrincipalUtil.GetToken(PersonToOrg_ToOrgDaglUserId, PersonToOrg_ToOrgDaglPartyId, 3),
        false,
        GetExpectedAuthorizedParties("PersonToOrg", OnlyAltinn3)
    ]];

    /// <summary>
    /// Sets up the authenticated user as DAGL of an organization,
    /// getting authorized parties from both Altinn 3 and Altinn 2
    /// where the user's organization has received delegations from a person,
    /// of both an Altinn App, a Resource and a Role from Altinn 2
    /// </summary>
    public static IEnumerable<object[]> PersonToOrgInclA2() => [[
        PrincipalUtil.GetToken(PersonToOrg_ToOrgDaglUserId, PersonToOrg_ToOrgDaglPartyId, 3),
        true,
        GetExpectedAuthorizedParties("PersonToOrg", BothAltinn3AndAltinn2)
    ]];

    /// <summary>
    /// Sets up the authenticated user,
    /// getting authorized parties from only Altinn 3
    /// where the user has received the following delegations:
    ///     from mainunit:
    ///         the resource: devtest_gar_authparties-main-to-person
    ///     from subunit:
    ///         the altinn app: ttd/am-devtest-sub-to-person
    /// </summary>
    public static IEnumerable<object[]> MainUnitAndSubUnitToPerson() => [[
        PrincipalUtil.GetToken(MainUnitAndSubUnitToPerson_ToUserId, MainUnitAndSubUnitToPerson_ToPartyId, 3),
        false,
        GetExpectedAuthorizedParties("MainUnitAndSubUnitToPerson", OnlyAltinn3)
    ]];

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
    public static IEnumerable<object[]> MainUnitAndSubUnitToPersonInclA2() => [[
        PrincipalUtil.GetToken(MainUnitAndSubUnitToPerson_ToUserId, MainUnitAndSubUnitToPerson_ToPartyId, 3),
        true,
        GetExpectedAuthorizedParties("MainUnitAndSubUnitToPerson", BothAltinn3AndAltinn2)
    ]];

    /// <summary>
    /// Sets up the authenticated user as DAGL of an organization,
    /// getting authorized parties from only Altinn 3
    /// where the user's organization has received the following delegations:
    ///     from mainunit:
    ///         the resource: devtest_gar_authparties-main-to-org
    ///     from subunit:
    ///         the altinn app: ttd/am-devtest-sub-to-org
    public static IEnumerable<object[]> MainUnitAndSubUnitToOrg() => [[
        PrincipalUtil.GetToken(MainUnitAndSubUnitToOrg_ToOrgDaglUserId, MainUnitAndSubUnitToOrg_ToOrgDaglPartyId, 3),
        false,
        GetExpectedAuthorizedParties("MainUnitAndSubUnitToOrg", OnlyAltinn3)
    ]];

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
    public static IEnumerable<object[]> MainUnitAndSubUnitToOrgInclA2() => [[
        PrincipalUtil.GetToken(MainUnitAndSubUnitToOrg_ToOrgDaglUserId, MainUnitAndSubUnitToOrg_ToOrgDaglPartyId, 3),
        true,
        GetExpectedAuthorizedParties("MainUnitAndSubUnitToOrg", BothAltinn3AndAltinn2)
    ]];

    private static List<AuthorizedParty> GetExpectedAuthorizedParties(string delegationType, string retrievalType)
    {
        string content = File.ReadAllText($"Data/Json/AuthorizedParties/{delegationType}/{retrievalType}.json");
        return (List<AuthorizedParty>)JsonSerializer.Deserialize(content, typeof(List<AuthorizedParty>), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}