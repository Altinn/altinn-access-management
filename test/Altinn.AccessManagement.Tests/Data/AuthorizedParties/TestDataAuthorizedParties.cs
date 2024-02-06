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
    private static string OnlyAltinn3 => "OnlyAltinn3";

    private static string BothAltinn3AndAltinn2 => "BothAltinn3AndAltinn2";

    private static int PersonToPerson_UserId => 20100001;

    private static int PersonToPerson_PartyId => 50100001;

    private static int PersonToOrg_DaglUserId => 20100002;

    private static int PersonToOrg_DaglPartyId => 50100002;

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
        PrincipalUtil.GetToken(PersonToPerson_UserId, PersonToPerson_PartyId, 3),
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
        PrincipalUtil.GetToken(PersonToPerson_UserId, PersonToPerson_PartyId, 3),
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
        PrincipalUtil.GetToken(PersonToOrg_DaglUserId, PersonToOrg_DaglPartyId, 3),
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
        PrincipalUtil.GetToken(PersonToOrg_DaglUserId, PersonToOrg_DaglPartyId, 3),
        false,
        GetExpectedAuthorizedParties("PersonToOrg", BothAltinn3AndAltinn2)
    ]];

    private static List<AuthorizedParty> GetExpectedAuthorizedParties(string delegationType, string retrievalType)
    {
        string content = File.ReadAllText($"Data/Json/AuthorizedParties/{delegationType}/{retrievalType}.json");
        return (List<AuthorizedParty>)JsonSerializer.Deserialize(content, typeof(List<AuthorizedParty>), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}