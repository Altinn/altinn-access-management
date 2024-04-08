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
    /// <summary>
    /// Defaults setup
    /// 1. Add resources to dn
    /// 2. Add random delegation to delegationchange table.
    ///     - Uses random ID in range [9000, 99999]
    /// </summary>
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

    /// <summary>
    /// Ensures that given profile has a key role for given org i mock context
    /// </summary>
    /// <param name="profile">profile</param>
    /// <param name="organizations">organization</param>
    /// <returns></returns>
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

    /// <summary>
    /// Add subunit as a mainunit in mock context 
    /// </summary>
    /// <param name="subunit">subunit</param>
    /// <param name="mainunit">mainunit</param>
    /// <returns></returns>
    public static Scenario WhereUnitHasMainUnit(IParty subunit, IParty mainunit) => (builder, mock) =>
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
    /// Add revoke delegation to db from given party to user
    /// </summary>
    /// <param name="organization">organization that revoking delegation</param>
    /// <param name="person">person that lose the delegation to the organization</param>
    /// <param name="resource">resource</param>
    /// <returns></returns>
    public static Scenario WithRevokedDelegationToUser(IParty organization, IUserProfile person, IAccessManagementResource resource = null) => (builder, mock) =>
    {
        mock.DbSeeds.AddRange([
            () => builder.PostgresFixture.SeedDatabaseTXs(
                PostgresFixture.WithInsertDelegationChange(
                    PostgresFixture.WithFrom(organization),
                    PostgresFixture.WithToUser(person),
                    PostgresFixture.WithResource(resource ?? AltinnAppSeeds.AltinnApp.Defaults),
                    PostgresFixture.WithDelegationChangeRevokeLast))
        ]);
    };

    /// <summary>
    /// Adds mock context and db seeds. for given organization, person and resource
    /// </summary>
    /// <param name="organization">organization that being delegated from</param>
    /// <param name="person">organization that being delegated to</param>
    /// <param name="resource">resource</param>
    /// <returns></returns>
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