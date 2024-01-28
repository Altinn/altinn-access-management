using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Tests.Util;

/// <summary>
/// Test data builder for testing AuthorizedParties
/// </summary>
public static class TestDataAuthorizedPartiesExternal
{
    private static string OnlyAltinn3 => "OnlyAltinn3";

    private static string BothAltinn3AndAltinn2 => "BothAltinn3AndAltinn2";

    private static string PersonPaula => "Paula";

    private static int PersonPaulaUserId => 20000095;

    private static int PersonPaulaPartyId => 50002203;

    private static string PersonKasper => "Kasper";

    private static int PersonKasperUserId => 20000490;

    private static int PersonKasperPartyId => 50002598;

    /// <summary>
    /// Sets up the authenticated user as Kasper Børstad DAGL of ØRSTA OG HEGGEDAL REGNSKAP,
    /// getting only authorized parties from Altinn 3
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> KasperOnlyAltinn3AuthorizedParties() => [[
        PrincipalUtil.GetToken(PersonKasperUserId, PersonKasperPartyId, 3),
        false,
        GetExpectedAuthorizedParties(PersonKasper, OnlyAltinn3)
    ]];

    /// <summary>
    /// Sets up the authenticated user as Kasper Børstad DAGL of ØRSTA OG HEGGEDAL REGNSKAP,
    /// getting authorized parties from both Altinn 3 and Altinn 2
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> KasperBothAltinn3AndAltinn2AuthorizedParties() => [[
        PrincipalUtil.GetToken(PersonKasperUserId, PersonKasperPartyId, 3),
        true,
        GetExpectedAuthorizedParties(PersonKasper, BothAltinn3AndAltinn2)
    ]];

    /// <summary>
    /// Sets up the authenticated user as Paula Rimstad DAGL of KARLSTAD OG ULØYBUKT REGNSKAP
    /// getting only authorized parties from Altinn 3
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> PaulaOnlyAltinn3AuthorizedParties() => [[
        PrincipalUtil.GetToken(PersonPaulaUserId, PersonPaulaPartyId, 3),
        false,
        GetExpectedAuthorizedParties(PersonPaula, OnlyAltinn3)
    ]];

    /// <summary>
    /// Sets up the authenticated user as Paula Rimstad DAGL of KARLSTAD OG ULØYBUKT REGNSKAP
    /// getting authorized parties from both Altinn 3 and Altinn 2
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> PaulaBothAltinn3AndAltinn2AuthorizedParties() => [[
        PrincipalUtil.GetToken(PersonPaulaUserId, PersonPaulaPartyId, 3),
        true,
        GetExpectedAuthorizedParties(PersonPaula, BothAltinn3AndAltinn2)
    ]];

    private static List<AuthorizedParty> GetExpectedAuthorizedParties(string user, string scenario)
    {
        string content = File.ReadAllText($"Data/Json/AuthorizedParties/{user}/{scenario}.json");
        return (List<AuthorizedParty>)JsonSerializer.Deserialize(content, typeof(List<AuthorizedParty>), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}