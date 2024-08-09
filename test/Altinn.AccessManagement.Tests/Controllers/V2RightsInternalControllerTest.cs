using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Altinn.AccessManagement.Controllers;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Models;
using Altinn.AccessManagement.Tests.Fixtures;
using Altinn.AccessManagement.Tests.Scenarios;
using Altinn.AccessManagement.Tests.Seeds;
using Xunit;

namespace Altinn.AccessManagement.Tests.Controllers;

/// <summary>
/// <see cref="RightsInternalController"/>
/// </summary>
public class V2RightsInternalControllerTest(WebApplicationFixture fixture) : IClassFixture<WebApplicationFixture>
{
    private WebApplicationFixture Fixture { get; } = fixture;

    private static Action<AcceptanceCriteriaComposer> WithAssertDbContainsDelegations(IParty from, IAccessManagementResource resource) => test =>
    {
        test.ApiAssertions.Add(async host =>
        {
            var delegations = await host.Repository.DelegationMetadataRepository.GetAllCurrentAppDelegationChanges(from.Party.PartyId.SingleToList(), resource.DbResource.ResourceRegistryId.SingleToList());
            Assert.True(
                delegations.Count > 0,
                $"Couldn't find any delegations from {from.Party.PartyId} to app {resource.DbResource.ResourceRegistryId}");
        });
    };

    private static Action<AcceptanceCriteriaComposer> WithAssertResponseContainsDelegationToUserProfile(IParty from, IUserProfile to) => test =>
    {
        test.ResponseAssertions.Add(async response =>
        {
            var delegations = await response.Content.ReadFromJsonAsync<IEnumerable<RightDelegationExternal>>();
            var result = delegations
                .Where(delegation => delegation.To.Any(attribute => attribute.Value == to.UserProfile.UserId.ToString()))
                .Where(delegation => delegation.From.Any(attribute => attribute.Value == from.Party.PartyId.ToString()));

            Assert.True(result.Any(), $"Response don't contain any delegations from '{from.Party.Name}' with party ID '{from.Party.PartyId}' to user profile '{to.UserProfile.Party.Name}' with user ID '{to.UserProfile.UserId}'");
        });
    };

    private static Action<AcceptanceCriteriaComposer> WithAssertResponseContainsDelegationToParty(IParty from, IParty to) => test =>
    {
        test.ResponseAssertions.Add(async response =>
        {
            var delegations = await response.Content.ReadFromJsonAsync<IEnumerable<RightDelegationExternal>>();
            var result = delegations
                .Where(delegation => delegation.To.Any(attribute => attribute.Value == to.Party.PartyId.ToString()))
                .Where(delegation => delegation.From.Any(attribute => attribute.Value == from.Party.PartyId.ToString()));

            Assert.True(result.Any(), $"Response don't contain any delegations from '{from.Party.Name}' with party ID '{from.Party.PartyId}' to party '{to.Party.Name}' with party ID '{to.Party.PartyId}'");
        });
    };

    /// <summary>
    /// Seeds for <see cref="GET_RightsDelegationsOffered"/>
    /// </summary>
    /// <param name="acceptanceCriteria">Acceptance Criteria</param>
    /// <param name="partyId">partyId in URL</param>
    /// <param name="actions">modifiers for <see cref="AcceptanceCriteriaComposer"/></param>
    public class SeedGetRightsDelegationsOffered(string acceptanceCriteria, int partyId, params Action<AcceptanceCriteriaComposer>[] actions) : AcceptanceCriteriaComposer(
            acceptanceCriteria,
            actions,
            WithRequestRoute("accessmanagement", "api", "v1", "internal", partyId, "rights", "delegation", "offered"),
            WithRequestVerb(HttpMethod.Get))
    {
        /// <summary>
        /// Seeds
        /// </summary>
        public static TheoryData<SeedGetRightsDelegationsOffered> Seeds() => [
            new(
                /* Acceptance Critieria */ @"
                GIVEN that organization Voss Accounting has an active delegation to employee Paula
                WHEN DAGL Olav for Orstad Accounting requests offered delegations from Orstad Accounting
                THEN Paula should be included in the list of offered delegations",
                OrganizationSeeds.VossAccounting.PartyId,

                WithScenarios(
                    DelegationScenarios.Defaults,
                    DelegationScenarios.FromOrganizationToPerson(OrganizationSeeds.VossAccounting.Defaults, PersonSeeds.Paula.Defaults),
                    DelegationScenarios.WherePersonHasKeyRole(PersonSeeds.Olav.Defaults, OrganizationSeeds.VossAccounting.Defaults),
                    TokenScenario.PersonToken(PersonSeeds.Olav.Defaults)),

                WithAssertResponseStatusCodeSuccessful,
                WithAssertResponseContainsDelegationToUserProfile(OrganizationSeeds.VossAccounting.Defaults, PersonSeeds.Paula.Defaults)),

            new(
                /* Acceptance Critieria */ @"
                GIVEN that organization Voss Consulting has delegations to organization Voss Accounting
                WHEN DAGL Paula for Voss Consulting requests offered delegations from Voss Consulting
                THEN Voss Accounting should be included in the list of offered delegations",
                OrganizationSeeds.VossConsulting.PartyId,

                WithScenarios(
                    DelegationScenarios.Defaults,
                    DelegationScenarios.WherePersonHasKeyRole(PersonSeeds.Paula.Defaults, OrganizationSeeds.VossConsulting.Defaults),
                    DelegationScenarios.FromOrganizationToOrganization(OrganizationSeeds.VossConsulting.Defaults, OrganizationSeeds.VossAccounting.Defaults, ResourceSeeds.AltinnApp.Defaults),
                    TokenScenario.PersonToken(PersonSeeds.Paula.Defaults)),

                WithAssertResponseStatusCodeSuccessful,
                WithAssertResponseContainsDelegationToParty(OrganizationSeeds.VossConsulting.Defaults, OrganizationSeeds.VossAccounting.Defaults)),
            new(
                /* Acceptance Critieria */ @"
                GIVEN that organization Voss Consulting has active app delegations to organization Voss Accounting
                AND app is deleted 
                WHEN DAGL Paula for Voss Consulting requests offered delegations from Voss Consulting
                THEN Voss Accounting should be included in the list of offered delegations
                AND organization name and partyid of deleted app should be included in the list of offered delegations",
                OrganizationSeeds.VossConsulting.PartyId,

                WithScenarios(
                    DelegationScenarios.Defaults,
                    DelegationScenarios.FromOrganizationToOrganization(OrganizationSeeds.VossConsulting.Defaults, OrganizationSeeds.VossAccounting.Defaults, ResourceSeeds.AltinnApp.Defaults),
                    DelegationScenarios.WithoutResource(ResourceSeeds.AltinnApp.Defaults)),

                WithAssertResponseStatusCodeSuccessful,
                WithAssertResponseContainsDelegationToParty(OrganizationSeeds.VossConsulting.Defaults, OrganizationSeeds.VossAccounting.Defaults)),
        ];
    }

    /// <summary>
    /// <see cref="RightsInternalController.GetOfferedRights(int, CancellationToken)"/>
    /// </summary>
    /// <param name="acceptanceCriteria">acceptance test</param>
    [Theory]
    [MemberData(nameof(SeedGetRightsDelegationsOffered.Seeds), MemberType = typeof(SeedGetRightsDelegationsOffered))]
    public async Task GET_RightsDelegationsOffered(SeedGetRightsDelegationsOffered acceptanceCriteria) => await acceptanceCriteria.Test(Fixture);
}