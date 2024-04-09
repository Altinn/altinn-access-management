using System.Linq;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.AccessManagement.Tests.Contexts;
using Altinn.AccessManagement.Tests.Fixtures;
using Altinn.AccessManagement.Tests.Seeds;
using Microsoft.AspNetCore.Hosting;

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
    public static void Defaults(IWebHostBuilder host, PostgresFixture postgres, MockContext mock)
    {
        mock.Resources.AddRange([
            AltinnAppSeeds.AltinnApp.Defaults
        ]);

        mock.DbSeeds.AddRange([
            () => postgres.SeedDatabaseTXs(
                PostgresFixture.WithInsertResource(PostgresFixture.WithAccessManagementResource(AltinnAppSeeds.AltinnApp.Defaults)),
                PostgresFixture.WithInsertDelegationChangeNoise(AltinnAppSeeds.AltinnApp.Defaults),
                PostgresFixture.WithInsertDelegationChangeNoise(AltinnAppSeeds.AltinnApp.Defaults),
                PostgresFixture.WithInsertDelegationChangeNoise(AltinnAppSeeds.AltinnApp.Defaults),
                PostgresFixture.WithInsertDelegationChangeNoise(AltinnAppSeeds.AltinnApp.Defaults),
                PostgresFixture.WithInsertDelegationChangeNoise(AltinnAppSeeds.AltinnApp.Defaults))
        ]);
    }

    /// <summary>
    /// Ensures that given profile has a key role for given org i mock context
    /// </summary>
    /// <param name="profile">profile</param>
    /// <param name="organizations">organization</param>
    /// <returns></returns>
    public static Scenario WherePersonHasKeyRole(IUserProfile profile, params IParty[] organizations) => (builder, postgres, mock) =>
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
    public static Scenario WhereUnitHasMainUnit(IParty subunit, IParty mainunit) => (host, postgres, mock) =>
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
    public static Scenario WithRevokedDelegationToUser(IParty organization, IUserProfile person, IAccessManagementResource resource = null) => (host, postgres, mock) =>
    {
        mock.DbSeeds.AddRange([
            () => postgres.SeedDatabaseTXs(
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
    /// <param name="from">from organization</param>
    /// <param name="to">to person</param>
    /// <param name="resource">resource</param>
    /// <returns></returns>
    public static Scenario FromOrganizationToPerson(IParty from, IUserProfile to, IAccessManagementResource resource = null) => (host, postgres, mock) =>
    {
        resource ??= AltinnAppSeeds.AltinnApp.Defaults;

        mock.Resources.Add(resource.Resource);
        mock.UserProfiles.Add(to.UserProfile);
        mock.Parties.AddRange([from.Party, to.UserProfile.Party]);

        mock.DbSeeds.AddRange([
            () => postgres.SeedDatabaseTXs(
                PostgresFixture.WithInsertDelegationChange(
                    PostgresFixture.WithFrom(from),
                    PostgresFixture.WithToUser(to),
                    PostgresFixture.WithResource(resource)))
        ]);
    };

    /// <summary>
    /// Adds mock context and db seeds. for given organization, person and resource
    /// </summary>
    /// <param name="from">from organization</param>
    /// <param name="to">to organization</param>
    /// <param name="resource">resource</param>
    /// <returns></returns>
    public static Scenario FromOrganizationToOrganization(IParty from, IParty to, IAccessManagementResource resource = null) => (host, postgres, mock) =>
    {
        resource ??= AltinnAppSeeds.AltinnApp.Defaults;

        mock.Resources.Add(resource.Resource);
        mock.Parties.AddRange([from.Party, to.Party]);

        mock.DbSeeds.AddRange([
            () => postgres.SeedDatabaseTXs(
                PostgresFixture.WithInsertDelegationChange(
                    PostgresFixture.WithFrom(from),
                    PostgresFixture.WithToParty(to),
                    PostgresFixture.WithResource(resource)))
        ]);
    };
}