using System;
using System.Linq;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
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
    private static readonly Random Random = new(DateTime.UtcNow.Nanosecond);

    private static int RandPartyId => Random.Next(9000, 9999);

    private static int RandUserId => Random.Next(8000, 9000);

    /// <summary>
    /// Defaults setup
    /// 1. Add resources to dn
    /// 2. Add random delegation to delegationchange table.
    ///     - Uses random ID in range [9000, 99999]
    /// </summary>
    public static void Defaults(MockContext mock)
    {
        mock.Resources.AddRange([
            ResourceSeeds.AltinnApp.Defaults,
            ResourceSeeds.MaskinportenSchema.Defaults,
        ]);

        // Seed databases resources
        mock.DbSeeds.AddRange([
            async postgres => await postgres.ResourceMetadataRepository.InsertAccessManagementResource(ResourceSeeds.AltinnApp.Defaults.DbResource),
            async postgres => await postgres.ResourceMetadataRepository.InsertAccessManagementResource(ResourceSeeds.MaskinportenSchema.Defaults.DbResource),
        ]);

        foreach (var random in Enumerable.Range(0, Random.Next(20)))
        {
            mock.DbSeeds.AddRange([
                async postgres =>
                {
                    var delegation = DelegationChangeComposer.New(
                        DelegationChangeComposer.WithToParty(RandPartyId),
                        DelegationChangeComposer.WithFrom(RandPartyId),
                        DelegationChangeComposer.WithResource(ResourceSeeds.AltinnApp.Defaults));

                    await postgres.DelegationMetadataRepository.InsertDelegation(ResourceAttributeMatchType.AltinnAppId, delegation);
                },
                async postgres =>
                {
                    var delegation = DelegationChangeComposer.New(
                        DelegationChangeComposer.WithToUser(RandUserId),
                        DelegationChangeComposer.WithFrom(RandPartyId),
                        DelegationChangeComposer.WithResource(ResourceSeeds.MaskinportenSchema.Defaults));

                    await postgres.DelegationMetadataRepository.InsertDelegation(ResourceAttributeMatchType.ResourceRegistry, delegation);
                }
            ]);
        }
    }

    /// <summary>
    /// Ensures that given profile has a key role for given org i mock context
    /// </summary>
    /// <param name="profile">profile</param>
    /// <param name="organizations">organization</param>
    /// <returns></returns>
    public static Scenario WherePersonHasKeyRole(IUserProfile profile, params IParty[] organizations) => mock =>
    {
        var partyids = organizations.Select(organization => organization?.Party?.PartyId ?? 0);
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
    public static Scenario WhereUnitHasMainUnit(IParty subunit, IParty mainunit) => mock =>
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
    /// Removes a resource from mock context
    /// </summary>
    public static Scenario WithoutResource(IAccessManagementResource resource) => mock =>
    {
        mock.Resources.RemoveAt(mock.Resources.FindIndex(r => r.Identifier == resource.Resource.Identifier));
    };

    /// <summary>
    /// Add revoke delegation to db from given party to user
    /// </summary>
    /// <param name="organization">organization that revoking delegation</param>
    /// <param name="person">person that lose the delegation to the organization</param>
    /// <param name="resource">resource</param>
    /// <returns></returns>
    public static Scenario WithRevokedDelegationToUser(IParty organization, IUserProfile person, IAccessManagementResource resource = null) => mock =>
    {
        resource ??= ResourceSeeds.AltinnApp.Defaults;

        mock.DbSeeds.AddRange([
            async postgres => await postgres.DelegationMetadataRepository.InsertDelegation(
                ResourceAttributeMatchType.AltinnAppId,
                DelegationChangeComposer.New(
                    DelegationChangeComposer.WithFrom(organization),
                    DelegationChangeComposer.WithToUser(person),
                    DelegationChangeComposer.WithResource(resource),
                    DelegationChangeComposer.WithDelegationChangeRevokeLast))
        ]);
    };

    /// <summary>
    /// Adds mock context and db seeds. for given organization, person and resource
    /// </summary>
    /// <param name="from">from organization</param>
    /// <param name="to">to person</param>
    /// <param name="resource">resource</param>
    /// <returns></returns>
    public static Scenario FromOrganizationToPerson(IParty from, IUserProfile to, IAccessManagementResource resource = null) => mock =>
    {
        resource ??= ResourceSeeds.AltinnApp.Defaults;

        mock.Resources.Add(resource.Resource);
        mock.UserProfiles.Add(to.UserProfile);
        mock.Parties.AddRange([from.Party, to.UserProfile.Party]);

        mock.DbSeeds.AddRange([
            async postgres => await postgres.DelegationMetadataRepository.InsertDelegation(
                resource.Resource.ResourceType == ResourceType.AltinnApp ? ResourceAttributeMatchType.AltinnAppId : ResourceAttributeMatchType.ResourceRegistry,
                DelegationChangeComposer.New(
                    DelegationChangeComposer.WithFrom(from),
                    DelegationChangeComposer.WithToUser(to),
                    DelegationChangeComposer.WithResource(resource)))
        ]);
    };

    /// <summary>
    /// Adds mock context and db seeds. for given organization, person and resource
    /// </summary>
    /// <param name="from">from organization</param>
    /// <param name="to">to person</param>
    /// <param name="resource">resource</param>
    /// <returns></returns>
    public static Scenario FromOrganizationToOrganization(IParty from, IParty to, IAccessManagementResource resource = null) => mock =>
    {
        resource ??= ResourceSeeds.AltinnApp.Defaults;

        mock.Resources.Add(resource.Resource);
        mock.Parties.AddRange([from.Party, to.Party]);

        mock.DbSeeds.AddRange([
            async postgres => await postgres.DelegationMetadataRepository.InsertDelegation(
                resource.Resource.ResourceType == ResourceType.AltinnApp ? ResourceAttributeMatchType.AltinnAppId : ResourceAttributeMatchType.ResourceRegistry,
                DelegationChangeComposer.New(
                    DelegationChangeComposer.WithFrom(from),
                    DelegationChangeComposer.WithToParty(to),
                    DelegationChangeComposer.WithResource(resource)))
        ]);
    };
}