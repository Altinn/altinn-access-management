using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Altinn.AccessManagement.Controllers;
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
        private static void WithAssertDbDelegationsNotEmpty(AcceptanceCriteriaTest test)
        {
            test.ApiAssertions.Add(async api =>
            {
                var delegations = await api.Postgres.ListDelegationsChanges();
                Assert.NotEmpty(delegations);
            });
        }

        private static Action<AcceptanceCriteriaTest> WithAssertResponseContainsDelegationToOrganization(IParty party) => test =>
        {
            test.ResponseAssertions.Add(async response =>
            {
                var delegations = await response.Content.ReadFromJsonAsync<IEnumerable<RightDelegationExternal>>();
                Assert.True(
                    delegations.Any(delegation => delegation.To.Any(to => to.Value == party.Party.PartyId.ToString())),
                    $"Response don't contain any delegation to organization {party.Party.Organization.Name} with partyid {party.Party.PartyId}");
            });
        };

        private static Action<AcceptanceCriteriaTest> WithAssertResponseContainsDelegationToPerson(IUserProfile profile) => test =>
        {
            test.ResponseAssertions.Add(async response =>
            {
                var delegations = await response.Content.ReadFromJsonAsync<IEnumerable<RightDelegationExternal>>();
                var result = delegations.Any(delegation => delegation.To.Any(to => to.Value == profile.UserProfile.UserId.ToString()));
                Assert.True(result, $"Response don't contain any delegation to organization {profile.UserProfile.Party.Name} with userid {profile.UserProfile.UserId}");
            });
        };

        /// <summary>
        /// Seeds
        /// </summary>
        public static TheoryData<SeedGetRightsDelegationsOffered> Seeds() => new()
        {
            new(
                /* Acceptance Critieria */@"
                GIVEN that organization Voss Accounting has active delegations to employee Paula
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
                WithAssertResponseContainsDelegationToPerson(PersonSeeds.Paula.Defaults)),
            new(
                /* Acceptance Critieria */@"
                GIVEN that organization Voss has active delegations to employee Paula
                AND Voss Accounting has active delegations to employee Kasper
                AND Voss accounting is a subunit of the organization Voss
                WHEN DAGL Olav for Voss requests offered delegations from Voss
                THEN Paula should be included in the list of offered delegations
                AND Kasper should be included in the list of offered delegations",
                OrganizationSeeds.Voss.PartyId,

                WithScenarios(
                    DelegationScenarios.Defaults,
                    DelegationScenarios.FromOrganizationToPerson(OrganizationSeeds.Voss.Defaults, PersonSeeds.Paula.Defaults),
                    DelegationScenarios.FromOrganizationToPerson(OrganizationSeeds.VossAccounting.Defaults, PersonSeeds.Kasper.Defaults),
                    DelegationScenarios.WhereUnitHasMainUnit(OrganizationSeeds.VossAccounting.Defaults, OrganizationSeeds.Voss.Defaults),
                    DelegationScenarios.WherePersonHasKeyRole(PersonSeeds.Olav.Defaults, OrganizationSeeds.Voss.Defaults),
                    TokenScenario.PersonToken(PersonSeeds.Olav.Defaults)),

                WithAssertDbDelegationsNotEmpty,
                WithAssertResponseStatusCodeSuccessful,
                WithAssertResponseContainsDelegationToPerson(PersonSeeds.Paula.Defaults),
                WithAssertResponseContainsDelegationToPerson(PersonSeeds.Kasper.Defaults))
        };

        /// <summary>
        /// Returns the Acceptance Criteria
        /// </summary>
        public override sealed string ToString() => AcceptanceCriteria;
    }

    /// <summary>
    /// <see cref="RightsInternalController.RevokeOfferedDelegation(AuthorizedPartyInput, RevokeOfferedDelegationExternal, System.Threading.CancellationToken)"/>
    /// </summary>
    /// <param name="data">acceptance test</param>
    [Theory(DisplayName = nameof(RightsInternalController.RevokeOfferedDelegation))]
    [MemberData(nameof(SeedGetRightsDelegationsOffered.Seeds), MemberType = typeof(SeedGetRightsDelegationsOffered))]
    public async Task GET_RightsDelegationsOffered(SeedGetRightsDelegationsOffered data)
    {
        await data.RunTests(Fixture);
    }
}