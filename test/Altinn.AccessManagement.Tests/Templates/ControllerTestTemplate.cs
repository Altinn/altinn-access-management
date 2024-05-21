using System;
using System.Threading.Tasks;
using Altinn.AccessManagement.Tests.Fixtures;
using Altinn.AccessManagement.Tests.Scenarios;
using Altinn.AccessManagement.Tests.Seeds;
using Xunit;

namespace Altinn.AccessManagement.Tests;

/// <summary>
/// Controller Name, use XML tag see
/// </summary>
public class ControllerTestTemplate(WebApplicationFixture fixture) : IClassFixture<WebApplicationFixture>
{
    private WebApplicationFixture Fixture { get; } = fixture;

    /*
    Test Collection asserters
    
    WithAssertResponse
    WithAssertApi
    
    */

    /// <summary>
    /// Seeds for <see cref="SeedNameOfEndpoint"/>
    /// </summary>
    /// <param name="acceptanceCriteria">Acceptance Criteria</param>
    /// <param name="partyId">parameter to api route</param>
    /// <param name="actions">modifiers for <see cref="AcceptanceCriteriaComposer"/></param>
    public class SeedNameOfEndpoint(string acceptanceCriteria, int partyId, params Action<AcceptanceCriteriaComposer>[] actions) : AcceptanceCriteriaComposer(
            acceptanceCriteria,
            actions,
            WithRequestRoute("route", "to", "endpoint", partyId))
    {
        /// <summary>
        /// List of test scenarios
        /// </summary>
        public static TheoryData<SeedNameOfEndpoint> Seeds() =>
        [

            // new(
            //     /* Acceptance Criteria */ @"
            //     GIVEN that organization Voss consulting has delegated a MaskinportenSchema resource to Voss Accounting
            //     WHEN DAGL Olav for voss consulting requests delegations that Voss consulting has offered
            //     THEN Voss accounting should be in the list of offered delegations",
            //
            //     /* Additional arguments */
            //     OrganizationSeeds.VossConsulting.PartyId,
            //
            //     /* Scenario */
            //     WithScenarios(
            //         DelegationScenarios.Defaults,
            //         DelegationScenarios.FromOrganizationToOrganization(OrganizationSeeds.VossConsulting.Defaults, OrganizationSeeds.VossAccounting.Defaults, ResourceSeeds.MaskinportenSchema.Defaults)),
            //
            //     /* Assertions */ 
            //     WithAssertResponseStatusCodeSuccessful)
        ];
    }

    /// <summary>
    /// Controller Action Name and prefix method, use XML tag see
    /// </summary>
    /// <param name="acceptanceCriteria">acceptance criteria</param>
    // [Theory]
    // [MemberData(nameof(SeedNameOfEndpoint.Seeds), MemberType = typeof(SeedNameOfEndpoint))]
    public async Task POST_SeedNameOfEndpoint(SeedNameOfEndpoint acceptanceCriteria) => await acceptanceCriteria.Test(Fixture);
}