using System.Linq;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.AccessManagement.Tests.Contexts;
using Altinn.AccessManagement.Tests.Fixtures;
using Altinn.AccessManagement.Tests.Seeds;
using Moq;

namespace Altinn.AccessManagement.Tests.Scenarios;

/// <summary>
/// Different scenarios that populates mock context and postgres db with data.
/// </summary>
public static class DelegationScenarios
{
    public static void Defaults(WebApplicationFixtureContext fixture, MockContext mock)
    {
        mock.Resources.AddRange([
            AltinnAppSeeds.AltinnApp.Defaults
        ]);

        mock.DbSeeds.AddRange([
            () => fixture.PostgresFixture.SeedDatabaseTXs(
                PostgresFixture.WithInsertResource(PostgresFixture.WithAccessManagementResource(AltinnAppSeeds.AltinnApp.Defaults)),
                PostgresFixture.WithInsertDelegationChangeNoise(AltinnAppSeeds.AltinnApp.Defaults),
                PostgresFixture.WithInsertDelegationChangeNoise(AltinnAppSeeds.AltinnApp.Defaults),
                PostgresFixture.WithInsertDelegationChangeNoise(AltinnAppSeeds.AltinnApp.Defaults),
                PostgresFixture.WithInsertDelegationChangeNoise(AltinnAppSeeds.AltinnApp.Defaults),
                PostgresFixture.WithInsertDelegationChangeNoise(AltinnAppSeeds.AltinnApp.Defaults)),
        ]);
    }

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

    public static Scenario WhereUnitHasMainUnit(IParty unit, IParty mainunit) => (builder, mock) =>
    {
        mock.MainUnits[unit.Party.PartyId] = new MainUnit
        {
            PartyId = mainunit.Party.PartyId,
            OrganizationName = mainunit?.Party?.Organization?.Name ?? "Unknown",
            OrganizationNumber = mainunit?.Party?.Organization?.OrgNumber ?? string.Empty,
            SubunitPartyId = unit.Party.PartyId,
        };
    };

    public static Scenario FromOrganizationToPerson(IParty organization, IUserProfile person, IAccessManagementResource resource = null) => (builder, mock) =>
    {
        resource ??= AltinnAppSeeds.AltinnApp.Defaults;

        mock.Resources.Add(AltinnAppSeeds.AltinnApp.Defaults);
        mock.UserProfiles.Add(person.UserProfile);
        mock.Parties.AddRange([organization.Party, person.UserProfile.Party]);

        mock.DbSeeds.AddRange([
            () => builder.PostgresFixture.SeedDatabaseTXs(
                PostgresFixture.WithInsertDelegationChange(
                    PostgresFixture.WithFrom(organization),
                    PostgresFixture.WithToUser(person),
                    PostgresFixture.WithResource(resource)))
        ]);
    };
}