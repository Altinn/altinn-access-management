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
/// <see cref="MaskinportenSchemaController"/>
/// </summary>
public class V2MaskinportenSchemaControllerTest(WebApplicationFixture fixture) : ControllerTest(fixture)
{
    public static Action<AcceptanceCriteriaTest> WithAssertDbContainsDelegations(int from, int to) => test =>
    {
        test.ApiAssertions.Add(async api =>
        {
            var actual = await api.Postgres.ListDelegationsChangesRR(delegations => delegations.Where(delegation => delegation.OfferedByPartyId == from && delegation.CoveredByPartyId == to));
            Assert.NotEmpty(actual);
        });
    };

    /// <summary>
    /// Assert response
    /// </summary>
    /// <param name="from">delegated from</param>
    /// <param name="to">delegation to</param>
    /// <returns></returns>
    public static Action<AcceptanceCriteriaTest> WithAssertResponseContainsDelegations(IParty from, IParty to) => test =>
    {
        test.ResponseAssertions.Add(async response =>
        {
            var body = await response.Content.ReadFromJsonAsync<IEnumerable<MaskinportenSchemaDelegationExternal>>();

            Assert.True(
                body.Any(delegation => delegation.OfferedByPartyId == from.Party.PartyId && delegation.CoveredByPartyId == to.Party.PartyId),
                $"Body did not contain a delegation from party with id {from.Party.PartyId} to party with id {to.Party.PartyId}");
        });
    };

    /// <summary>
    /// Seeds for <see cref="GET_GetOfferedMaskinportenSchemaDelegations"/>
    /// </summary>
    /// <param name="acceptanceCriteria">Acceptance Criteria</param>
    /// <param name="partyId">party id</param>
    /// <param name="actions">modifiers for <see cref="AcceptanceCriteriaTest"/></param>
    public class SeedGetOfferedMaskinportenSchemaDelegations(string acceptanceCriteria, int partyId, params Action<AcceptanceCriteriaTest>[] actions) : AcceptanceCriteriaTest(
            acceptanceCriteria,
            actions,

            WithRequestRoute("accessmanagement", "api", "v1", partyId, "maskinportenschema", "offered"),
            WithRequestVerb(HttpMethod.Get))
    {
        /// <summary>
        /// Seeds
        /// </summary>
        public static TheoryData<SeedGetOfferedMaskinportenSchemaDelegations> Seeds() =>
        [
            new(
                /* Acceptance Criteria */ @"
                GIVEN that organization Voss consulting has delegated a MaskinportenSchema resource to Voss Accounting
                WHEN DAGL Olav for voss consulting requests delegations that Voss consulting has offered
                THEN Voss accounting should be in the list of offered delegations",
                OrganizationSeeds.VossConsulting.PartyId,

                WithScenarios(
                    DelegationScenarios.Defaults,
                    DelegationScenarios.FromOrganizationToOrganization(
                        OrganizationSeeds.VossConsulting.Defaults,
                        OrganizationSeeds.VossAccounting.Defaults,
                        ResourceSeeds.MaskinportenSchema.Defaults)),

                WithAssertDbContainsDelegations(OrganizationSeeds.VossConsulting.PartyId, OrganizationSeeds.VossAccounting.PartyId),
                WithAssertResponseContainsDelegations(OrganizationSeeds.VossConsulting.Defaults, OrganizationSeeds.VossAccounting.Defaults),
                WithAssertResponseStatusCodeSuccessful)
        ];
    }

    /// <summary>
    /// <see cref="MaskinportenSchemaController.GetOfferedMaskinportenSchemaDelegations(string)"/>
    /// </summary>
    /// <param name="acceptanceCriteria">acceptance criteria</param>
    [Theory(DisplayName = nameof(MaskinportenSchemaController.GetOfferedMaskinportenSchemaDelegations))]
    [MemberData(nameof(SeedGetOfferedMaskinportenSchemaDelegations.Seeds), MemberType = typeof(SeedGetOfferedMaskinportenSchemaDelegations))]
    public async Task GET_GetOfferedMaskinportenSchemaDelegations(SeedGetOfferedMaskinportenSchemaDelegations acceptanceCriteria) => await acceptanceCriteria.Test(Fixture);
}