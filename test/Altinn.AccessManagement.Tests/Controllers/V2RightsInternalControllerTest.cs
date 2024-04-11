using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Altinn.AccessManagement.Controllers;
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
public class V2RightsInternalControllerTest(WebApplicationFixture fixture) : ControllerTest(fixture)
{
    private static Action<AcceptanceCriteriaTest> WithAssertDbLastDelegationToUserIsRevoked(IParty from, IUserProfile to) => test =>
    {
        test.ApiAssertions.Add(async fixture =>
        {
            var delegations = await fixture.Postgres.ListDelegationsChanges(filter => filter.Where(delegation => from.Party.PartyId == delegation.OfferedByPartyId && to.UserProfile.UserId == delegation.CoveredByUserId));

            Assert.True(
                delegations.OrderBy(delegation => delegation.Created).First().DelegationChangeType == DelegationChangeType.RevokeLast,
                $"Last delegation from party '{from.Party.Name}' with party ID '{from.Party.PartyId}' to user '{to.UserProfile.Party.Name}' with user ID '{to.UserProfile.UserId}' is not of type '{DelegationChangeType.RevokeLast}'");
        });
    };

    private static void WithAssertDbDelegationsNotEmpty(AcceptanceCriteriaTest test)
    {
        test.ApiAssertions.Add(async api =>
        {
            var delegations = await api.Postgres.ListDelegationsChanges();
            Assert.NotEmpty(delegations);
        });
    }

    private static void WithAssertEmptyDelegationList(AcceptanceCriteriaTest test)
    {
        test.ResponseAssertions.Add(async response =>
        {
            var delegations = await response.Content.ReadFromJsonAsync<IEnumerable<RightDelegationExternal>>();
            Assert.Empty(delegations);
        });
    }

    private static Action<AcceptanceCriteriaTest> WithAssertResponseContainsDelegationToUserProfile(IParty from, IUserProfile to) => test =>
    {
        test.ResponseAssertions.Add(async response =>
        {
            var delegations = await response.Content.ReadFromJsonAsync<IEnumerable<RightDelegationExternal>>();
            var result = delegations.Any(delegation =>
                    delegation.To.Any(attribute => attribute.Value == from.Party.PartyId.ToString()) &&
                    delegation.From.Any(attribute => attribute.Value == to.UserProfile.UserId.ToString()));

            Assert.True(result, $"Response don't contain any delegations from '{from.Party.Name}' with party ID '{from.Party.PartyId}' to user profile '{to.UserProfile.Party.Name}' with user ID '{to.UserProfile.UserId}'");
        });
    };

    private static Action<AcceptanceCriteriaTest> WithAssertResponseContainsDelegationToParty(IParty from, IParty to) => test =>
    {
        test.ResponseAssertions.Add(async response =>
        {
            var delegations = await response.Content.ReadFromJsonAsync<IEnumerable<RightDelegationExternal>>();
            var result = delegations.Any(delegation =>
                delegation.To.Any(attribute => attribute.Value == from.Party.PartyId.ToString()) &&
                delegation.From.Any(attribute => attribute.Value == to.Party.PartyId.ToString()));

            Assert.True(result, $"Response don't contain any delegations from '{from.Party.Name}' with party ID '{from.Party.PartyId}' to party '{to.Party.Name}' with party ID '{to.Party.PartyId}'");
        });
    };

    /// <summary>
    /// Seeds for <see cref="GET_RightsDelegationsOffered"/>
    /// </summary>
    /// <param name="acceptanceCriteria">Acceptance Criteria</param>
    /// <param name="partyId">partyId in URL</param>
    /// <param name="actions">modifiers for <see cref="AcceptanceCriteriaTest"/></param>
    public class SeedGetRightsDelegationsOffered(string acceptanceCriteria, int partyId, params Action<AcceptanceCriteriaTest>[] actions) : AcceptanceCriteriaTest(
            acceptanceCriteria,
            actions,
            WithRequestRoute("accessmanagement", "api", "v1", "internal", partyId, "rights", "delegation", "offered"),
            WithRequestVerb(HttpMethod.Get))
    {
        /// <summary>
        /// Seeds
        /// </summary>
        public static TheoryData<SeedGetRightsDelegationsOffered> Seeds() => new()
        {
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

                WithAssertDbDelegationsNotEmpty,
                WithAssertResponseStatusCodeSuccessful,
                WithAssertResponseContainsDelegationToUserProfile(OrganizationSeeds.VossAccounting.Defaults, PersonSeeds.Paula.Defaults)),
            new(
                /* Acceptance Critieria */ @"
                GIVEN that organization Voss has an active delegation to employee Paula
                AND Voss Accounting has an active delegation to employee Kasper
                AND Voss Accounting is a subunit of the organization Voss
                WHEN DAGL Olav for Voss requests offered delegations from Voss Accounting
                THEN Paula should be included in the list of offered delegations
                AND Kasper should be included in the list of offered delegations",
                OrganizationSeeds.VossAccounting.PartyId,

                WithScenarios(
                    DelegationScenarios.Defaults,
                    DelegationScenarios.FromOrganizationToPerson(OrganizationSeeds.Voss.Defaults, PersonSeeds.Paula.Defaults),
                    DelegationScenarios.FromOrganizationToPerson(OrganizationSeeds.VossAccounting.Defaults, PersonSeeds.Kasper.Defaults),
                    DelegationScenarios.WhereUnitHasMainUnit(OrganizationSeeds.VossAccounting.Defaults, OrganizationSeeds.Voss.Defaults),
                    DelegationScenarios.WherePersonHasKeyRole(PersonSeeds.Olav.Defaults, OrganizationSeeds.Voss.Defaults),
                    TokenScenario.PersonToken(PersonSeeds.Olav.Defaults)),

                WithAssertDbDelegationsNotEmpty,
                WithAssertResponseStatusCodeSuccessful,
                WithAssertResponseContainsDelegationToUserProfile(OrganizationSeeds.VossAccounting.Defaults, PersonSeeds.Kasper.Defaults),
                WithAssertResponseContainsDelegationToUserProfile(OrganizationSeeds.Voss.Defaults, PersonSeeds.Paula.Defaults)),
            new(
                /* Acceptance Critieria */ @"
                GIVEN that organization Voss Consulting has delegations to employee Paula
                AND that the last delegation given to Paula is revoked
                WHEN DAGL Olav for Voss requests offered delegations from Voss
                THEN the list of delegation should be empty",
                OrganizationSeeds.VossConsulting.PartyId,

                WithScenarios(
                    DelegationScenarios.Defaults,
                    DelegationScenarios.FromOrganizationToPerson(OrganizationSeeds.VossConsulting.Defaults, PersonSeeds.Paula.Defaults),
                    DelegationScenarios.WithRevokedDelegationToUser(OrganizationSeeds.VossConsulting.Defaults, PersonSeeds.Paula.Defaults),
                    DelegationScenarios.WherePersonHasKeyRole(PersonSeeds.Olav.Defaults, OrganizationSeeds.Voss.Defaults),
                    TokenScenario.PersonToken(PersonSeeds.Olav.Defaults)),

                WithAssertDbDelegationsNotEmpty,
                WithAssertResponseStatusCodeSuccessful,
                WithAssertEmptyDelegationList),
            new(
                /* Acceptance Critieria */ @"
                GIVEN that organization Voss Consulting has delegations to organization Voss Accounting
                WHEN DAGL Paula for Voss Consulting requests offered delegations from Voss Consulting
                THEN Voss Accounting should be included in the list of offered delegations",
                OrganizationSeeds.VossConsulting.PartyId,

                WithScenarios(
                    DelegationScenarios.Defaults,
                    DelegationScenarios.FromOrganizationToOrganization(OrganizationSeeds.VossConsulting.Defaults, OrganizationSeeds.VossAccounting.Defaults),
                    DelegationScenarios.WherePersonHasKeyRole(PersonSeeds.Paula.Defaults, OrganizationSeeds.VossConsulting.Defaults),
                    TokenScenario.PersonToken(PersonSeeds.Paula.Defaults)),

                WithAssertDbDelegationsNotEmpty,
                WithAssertResponseStatusCodeSuccessful,
                WithAssertResponseContainsDelegationToParty(OrganizationSeeds.VossConsulting.Defaults, OrganizationSeeds.VossAccounting.Defaults))
        };
    }

    /// <summary>
    /// <see cref="RightsInternalController.RevokeOfferedDelegation(AuthorizedPartyInput, RevokeOfferedDelegationExternal, CancellationToken)"/>
    /// </summary>
    /// <param name="data">acceptance test</param>
    [Theory(DisplayName = nameof(RightsInternalController.RevokeOfferedDelegation))]
    [MemberData(nameof(SeedGetRightsDelegationsOffered.Seeds), MemberType = typeof(SeedGetRightsDelegationsOffered))]
    public async Task GET_RightsDelegationsOffered(SeedGetRightsDelegationsOffered data) => await data.RunTests(Fixture);
}