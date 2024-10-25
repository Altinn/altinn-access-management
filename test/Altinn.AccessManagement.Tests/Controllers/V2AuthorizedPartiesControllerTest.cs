using System.Net.Http.Json;
using Altinn.AccessManagement.Controllers;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Tests.Fixtures;
using Altinn.AccessManagement.Tests.Scenarios;
using Altinn.AccessManagement.Tests.Seeds;

namespace Altinn.AccessManagement.Tests.Controllers;

/// <summary>
/// <see cref="AuthorizedPartiesController"/>
/// </summary>
public class V2AuthorizedPartiesControllerTest(WebApplicationFixture fixture) : IClassFixture<WebApplicationFixture>
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

    private static Action<AcceptanceCriteriaComposer> WithAssertResponseContainsInstance(string resourceId, string instanceId) => test =>
    {
        test.ResponseAssertions.Add(async response =>
        {
            var delegations = await response.Content.ReadFromJsonAsync<List<AuthorizedPartyExternal>>();
            var result = delegations.Any(delegation => delegation.AuthorizedInstances.Any(instance => instance.ResourceId == resourceId && instance.InstanceId == instanceId));
            Assert.True(result, $"Response don't contains instance delegations with resource Id {resourceId} and instance id {instanceId}");
        });
    };

    /// <summary>
    /// Seeds for <see cref="GET_AuthorizedParties"/>
    /// </summary>
    /// <param name="acceptanceCriteria">Acceptance Criteria</param>
    /// <param name="actions">modifiers for <see cref="AcceptanceCriteriaComposer"/></param>
    public class GetAuthorizedParties(string acceptanceCriteria, params Action<AcceptanceCriteriaComposer>[] actions) : AcceptanceCriteriaComposer(
            acceptanceCriteria,
            actions,
            WithRequestRoute("accessmanagement", "api", "v1", "authorizedparties"),
            WithRequestVerb(HttpMethod.Get))
    {
        /// <summary>
        /// Seeds
        /// </summary>
        public static TheoryData<GetAuthorizedParties> Seeds() => [
            new(
                /* Acceptance Critieria */ @"
                GIVEN that organization Voss has shared an instance with DAGL Olav for Orstad Accounting
                WHEN DAGL Olav for Orstad Accounting requests authorized parties
                THEN Organization should be in the list of authorized parties
                AND the instance and resource id should be included in list containing instances",

                WithScenarios(
                    DelegationScenarios.Defaults,
                    DelegationScenarios.WithInstanceDelegation(OrganizationSeeds.VossAccounting.Defaults, PersonSeeds.Paula.Defaults, ResourceSeeds.ChalkboardResource.Defaults, "1337"),
                    TokenScenario.PersonToken(PersonSeeds.Olav.Defaults)),

                WithAssertResponseContainsInstance(ResourceSeeds.ChalkboardResource.Identifier, "1337"),
                WithAssertResponseStatusCodeSuccessful),
        ];
    }

    /// <summary>
    /// <see cref="AuthorizedPartiesController.GetAuthorizedParties(bool, CancellationToken)"/>
    /// </summary>
    /// <param name="acceptanceCriteria">acceptance test</param>
    [Theory]
    [MemberData(nameof(GetAuthorizedParties.Seeds), MemberType = typeof(GetAuthorizedParties))]
    public async Task GET_AuthorizedParties(GetAuthorizedParties acceptanceCriteria) => await acceptanceCriteria.Test(Fixture);
}