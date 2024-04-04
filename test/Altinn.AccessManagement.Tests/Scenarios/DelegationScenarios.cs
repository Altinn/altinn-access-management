using System.Linq;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.AccessManagement.Tests.Fixtures;
using Altinn.AccessManagement.Tests.Seeds;

namespace Altinn.AccessManagement.Tests.Scenarios;

/// <summary>
/// Different scenarios that populates mock context and postgres db with data.
/// </summary>
public static class DelegationScenarios
{
    public static Scenario WherePersonHasKeyRole(IUserProfile profile, params IParty[] organizations) => (builder, mock) =>
    {
        var partyids = organizations.Select(organization => organization.Party.PartyId);
        if (mock.KeyRoles.TryGetValue(profile.UserProfile.UserId, out var value))
        {
            value.AddRange(partyids);
        }
        else
        {
            mock.KeyRoles.Add(profile.UserProfile.UserId, partyids.ToList());
        }
    };

    public static Scenario WhereSubunitHasMainUnit(IParty subunit, IParty mainunit) => (builder, mock) =>
    {
        mock.MainUnits[subunit.Party.PartyId] = new MainUnit
        {
            PartyId = mainunit.Party.PartyId,
            OrganizationName = mainunit?.Party?.Organization?.Name ?? "Unknown",
            OrganizationNumber = mainunit?.Party?.Organization?.OrgNumber ?? string.Empty,
            SubunitPartyId = subunit.Party.PartyId,
        };
    };

    /// <summary>
    /// Populates mock context and postgres DB with following seeds 
    /// 
    /// From: <see cref="PersonSeeds.Orjan"/>
    /// To: <see cref="PersonSeeds.Paula"/>
    /// AltinnApp: <see cref="AltinnAppSeeds.AltinnApp"/>
    /// </summary>
    /// <param name="modifiers">For add or changing default setup for the scenario</param>
    /// <returns>Scenario</returns>
    public static Scenario FromOrjanToPaula(params Scenario[] modifiers) => (builder, mock) =>
    {
        mock.Resources.Add(AltinnAppSeeds.AltinnApp.Defaults);
        mock.UserProfiles.AddRange([PersonSeeds.Orjan.Defaults, PersonSeeds.Paula.Defaults]);
        mock.Parties.AddRange([PersonSeeds.Orjan.Defaults.UserProfile.Party, PersonSeeds.Paula.Defaults.UserProfile.Party]);

        foreach (var action in modifiers)
        {
            action(builder, mock);
        }

        mock.DbSeeds.AddRange([
            () => builder.PostgresFixture.SeedDatabaseTXs(
                PostgresFixture.WithInsertResource(
                    PostgresFixture.WithAccessManagementResource(AltinnAppSeeds.AltinnApp.Defaults))),

            () => builder.PostgresFixture.SeedDatabaseTXs(
                PostgresFixture.WithInsertDelegationChange(
                    PostgresFixture.WithTupleUserAndParty(PersonSeeds.Orjan.Defaults, PersonSeeds.Paula.Defaults),
                    PostgresFixture.WithResource(AltinnAppSeeds.AltinnApp.Defaults)),
                PostgresFixture.WithInsertDelegationChange(
                    PostgresFixture.WithTupleUserAndParty(PersonSeeds.Orjan.Defaults, PersonSeeds.Kasper.Defaults),
                    PostgresFixture.WithResource(AltinnAppSeeds.AltinnApp.Defaults))),
            ]);
    };

    /// <summary>
    /// Populates mock context and postgres DB with following seeds 
    /// 
    /// From: <see cref="PersonSeeds.Orjan"/>
    /// To: <see cref="PersonSeeds.Paula"/>
    /// AltinnApp: <see cref="AltinnAppSeeds.AltinnApp"/>
    /// </summary>
    /// <param name="modifiers">For add or changing default setup for the scenario</param>
    /// <returns>Scenario</returns>
    public static Scenario FromOrstadAccountingToPaula(params Scenario[] modifiers) => (builder, mock) =>
    {
        mock.Resources.Add(AltinnAppSeeds.AltinnApp.Defaults);
        mock.UserProfiles.Add(PersonSeeds.Paula.Defaults);
        mock.Parties.AddRange([OrganizationSeeds.OrstadAccounting.Defaults, PersonSeeds.Paula.Defaults.UserProfile.Party]);

        foreach (var action in modifiers)
        {
            action(builder, mock);
        }

        mock.DbSeeds.AddRange([
            () => builder.PostgresFixture.SeedDatabaseTXs(
                PostgresFixture.WithInsertResource(
                    PostgresFixture.WithAccessManagementResource(AltinnAppSeeds.AltinnApp.Defaults))),

            () => builder.PostgresFixture.SeedDatabaseTXs(
                PostgresFixture.WithInsertDelegationChange(
                    PostgresFixture.WithTupleParties(OrganizationSeeds.OrstadAccounting.Defaults, PersonSeeds.Paula.Defaults),
                    PostgresFixture.WithResource(AltinnAppSeeds.AltinnApp.Defaults))),
            ]);
    };
}