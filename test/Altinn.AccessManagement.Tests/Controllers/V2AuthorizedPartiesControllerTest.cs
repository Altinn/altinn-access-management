using System.Net.Http.Json;
using Altinn.AccessManagement.Controllers;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Tests.Fixtures;
using Altinn.AccessManagement.Tests.Scenarios;
using Altinn.AccessManagement.Tests.Seeds;
using Microsoft.VisualBasic;

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

    private static Action<AcceptanceCriteriaComposer> WithAssertSuccessfulResponse(params Func<List<AuthorizedPartyExternal>, string>[] asserts) => test =>
    {
        test.ResponseAssertions.Add(async response =>
        {
            if (response.IsSuccessStatusCode)
            {
                var authorizedParties = await response.Content.ReadFromJsonAsync<List<AuthorizedPartyExternal>>();
                var result = new List<string>();
                foreach (var assert in asserts)
                {
                    if (assert(authorizedParties) is var msg && !string.IsNullOrEmpty(msg))
                    {
                        result.Add(msg);
                    }
                }

                Assert.True(result.Count == 0, string.Join("\n", result));
            }
        });
    };

    private static Func<List<AuthorizedPartyExternal>, string> WithAssertResponseContainsPartyWithInstance(string resourceId, string instanceId) => response =>
    {
        var result = response.Exists(authorizedParty => authorizedParty.AuthorizedInstances.Exists(instance => instance.ResourceId == resourceId && instance.InstanceId == instanceId));
        if (result)
        {
            return null;
        }

        return $"Response don't contain authorized party with instance delegations with resource Id {resourceId} and instance id {instanceId}";
    };

    private static Func<List<AuthorizedPartyExternal>, string> WithAssertResponseContainsSubUnitWithInstance(string resourceId, string instanceId) => response =>
    {
        var result = response.Exists(authorizedParty => authorizedParty.Subunits.Exists(authorizedSubUnit => authorizedSubUnit.AuthorizedInstances.Exists(instance => instance.ResourceId == resourceId && instance.InstanceId == instanceId)));
        if (result)
        {
            return null;
        }

        return $"Response don't contain authorized party with subunit having instance delegations with resource Id {resourceId} and instance id {instanceId}";
    };

    private static Func<List<AuthorizedPartyExternal>, string> WithAssertResponseContainsParty(IParty party) => response =>
    {
        var result = response.Exists(authorizedParty => authorizedParty.PartyId == party.Party.PartyId);
        if (result)
        {
            return null;
        }

        return $"Response don't contain any reportees with party ID {party.Party.PartyId} ({party.Party.Name})";
    };

    private static Func<List<AuthorizedPartyExternal>, string> WithAssertResponseContainsNotParty(IParty party) => response =>
    {
        var result = response.Exists(authorizedParty => authorizedParty.PartyId == party.Party.PartyId);
        if (result)
        {
            return $"Response should not contain reportees with party ID {party.Party.PartyId} ({party.Party.Name})";
        }

        return null;
    };

    private static Func<List<AuthorizedPartyExternal>, string> WithAssertResponseContainsNotInstance(string resourceId, string instanceId) => response =>
    {
        var result = response.Exists(authorizedParty => authorizedParty.AuthorizedInstances.Any(instance => instance.ResourceId == resourceId && instance.InstanceId == instanceId));
        if (result)
        {
            return $"Response should not contain any instance delegations with resource Id {resourceId} and instance id {instanceId}";
        }

        return null;
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
                /* Acceptance Critieria: User Receiving Instance Access from an Individual */ @"
                GIVEN a user who has received instance delegation directly from an individual
                WHEN the user's list of authorized parties is retrieved
                THEN the access list should contain the individual who delegated the instance access
                AND the individual's AuthorizedInstances should include an identifier specifying the resourceId and instanceId for the instance delegation",
                WithScenarios(
                    DelegationScenarios.Defaults,
                    DelegationScenarios.WithInstanceDelegation(PersonSeeds.Kasper.Defaults, PersonSeeds.Paula.Defaults, ResourceSeeds.ChalkboardResource.Defaults, "chalk"),
                    DelegationScenarios.WithInstanceDelegation(PersonSeeds.Paula.Defaults, PersonSeeds.Kasper.Defaults, ResourceSeeds.ChalkboardResource.Defaults, "should_not_be_in_result_list_1"),
                    DelegationScenarios.WithInstanceDelegation(PersonSeeds.Olav.Defaults, PersonSeeds.Kasper.Defaults, ResourceSeeds.ChalkboardResource.Defaults, "should_not_be_in_result_list_2"),
                    TokenScenario.PersonToken(PersonSeeds.Paula.Defaults)
                ),
                WithAssertResponseStatusCodeSuccessful,
                WithAssertSuccessfulResponse(
                    WithAssertResponseContainsNotParty(PersonSeeds.Paula.Defaults),
                    WithAssertResponseContainsPartyWithInstance(ResourceSeeds.ChalkboardResource.Identifier, "chalk"),
                    WithAssertResponseContainsNotInstance(ResourceSeeds.ChalkboardResource.Identifier, "should_not_be_in_result_list_2"),
                    WithAssertResponseContainsParty(PersonSeeds.Kasper.Defaults),
                    WithAssertResponseContainsNotParty(PersonSeeds.Olav.Defaults),
                    WithAssertResponseContainsNotInstance(ResourceSeeds.ChalkboardResource.Identifier, "should_not_be_in_result_list_1")
                )
            ),
            new(
                /* Acceptance Critieria: User Receiving Instance Access from a Primary Unit */ @"
                GIVEN a user who has received instance delegation from a primary unit
                WHEN the user's list of authorized parties is retrieved
                THEN the access list should contain the primary unit from which the instance access is delegated
                AND the primary unit's AuthorizedInstances should include an identifier specifying the resourceId and instanceId for the instance delegation
                AND if the user has no other permissions for the primary unit or its subunits, no other subunits should be present in the list of authorized parties",
                WithScenarios(
                    DelegationScenarios.Defaults,
                    DelegationScenarios.WhereUnitHasMainUnit(OrganizationSeeds.VossAccounting.Defaults, OrganizationSeeds.VossConsulting.Defaults),
                    DelegationScenarios.WithInstanceDelegation(OrganizationSeeds.VossConsulting.Defaults, PersonSeeds.Paula.Defaults, ResourceSeeds.ChalkboardResource.Defaults, "sponge"),
                    DelegationScenarios.WithInstanceDelegation(PersonSeeds.Paula.Defaults, OrganizationSeeds.VossAccounting.Defaults, ResourceSeeds.ChalkboardResource.Defaults, "should_not_be_in_result_list_1"),
                    DelegationScenarios.WithInstanceDelegation(PersonSeeds.Paula.Defaults, OrganizationSeeds.VossConsulting.Defaults, ResourceSeeds.ChalkboardResource.Defaults, "should_not_be_in_result_list_2"),
                    DelegationScenarios.WithInstanceDelegation(OrganizationSeeds.VossConsulting.Defaults, PersonSeeds.Olav.Defaults, ResourceSeeds.ChalkboardResource.Defaults, "should_not_be_in_result_list_3"),
                    TokenScenario.PersonToken(PersonSeeds.Paula.Defaults)
                ),
                WithAssertResponseStatusCodeSuccessful,
                WithAssertSuccessfulResponse(
                    WithAssertResponseContainsParty(OrganizationSeeds.VossConsulting.Defaults),
                    WithAssertResponseContainsNotParty(PersonSeeds.Paula.Defaults),
                    WithAssertResponseContainsNotParty(PersonSeeds.Olav.Defaults),
                    WithAssertResponseContainsNotParty(OrganizationSeeds.VossAccounting.Defaults),
                    WithAssertResponseContainsPartyWithInstance(ResourceSeeds.ChalkboardResource.Identifier, "sponge"),
                    WithAssertResponseContainsNotInstance(ResourceSeeds.ChalkboardResource.Identifier, "should_not_be_in_result_list_1"),
                    WithAssertResponseContainsNotInstance(ResourceSeeds.ChalkboardResource.Identifier, "should_not_be_in_result_list_2"),
                    WithAssertResponseContainsNotInstance(ResourceSeeds.ChalkboardResource.Identifier, "should_not_be_in_result_list_3")
                )
            ),
            new(
                /* Acceptance Critieria: User Receiving Instance Access from a Subunit */ @"
                GIVEN a user who has received instance delegation from a subunit
                WHEN the user's list of authorized parties is retrieved
                THEN the list of authorized parties should include the subunit from which the instance access is delegated
                AND the subunit's AuthorizedInstances should include an identifier specifying the resourceId and instanceId for the instance delegation
                AND the list of authorized parties should also include the primary unit of the subunit
                AND with no additional authorized roles/resources/instances and the flag 'onlyHierarchyElementWithNoAccess': true",
                WithScenarios(
                    DelegationScenarios.Defaults,
                    DelegationScenarios.WhereUnitHasMainUnit(OrganizationSeeds.VossAccounting.Defaults, OrganizationSeeds.VossConsulting.Defaults),
                    DelegationScenarios.WithInstanceDelegation(OrganizationSeeds.VossAccounting.Defaults, PersonSeeds.Paula.Defaults, ResourceSeeds.ChalkboardResource.Defaults, "water"),
                    DelegationScenarios.WithInstanceDelegation(PersonSeeds.Paula.Defaults, OrganizationSeeds.VossAccounting.Defaults, ResourceSeeds.ChalkboardResource.Defaults, "should_not_be_in_result_list_1"),
                    DelegationScenarios.WithInstanceDelegation(PersonSeeds.Paula.Defaults, OrganizationSeeds.VossConsulting.Defaults, ResourceSeeds.ChalkboardResource.Defaults, "should_not_be_in_result_list_2"),
                    DelegationScenarios.WithInstanceDelegation(OrganizationSeeds.VossConsulting.Defaults, PersonSeeds.Olav.Defaults, ResourceSeeds.ChalkboardResource.Defaults, "should_not_be_in_result_list_3"),
                    TokenScenario.PersonToken(PersonSeeds.Paula.Defaults)
                ),
                WithAssertResponseStatusCodeSuccessful,
                WithAssertSuccessfulResponse(
                    WithAssertResponseContainsParty(OrganizationSeeds.VossConsulting.Defaults),
                    WithAssertResponseContainsNotParty(OrganizationSeeds.VossAccounting.Defaults),
                    WithAssertResponseContainsNotParty(PersonSeeds.Paula.Defaults),
                    WithAssertResponseContainsNotParty(PersonSeeds.Olav.Defaults),
                    WithAssertResponseContainsSubUnitWithInstance(ResourceSeeds.ChalkboardResource.Identifier, "water"),
                    WithAssertResponseContainsNotInstance(ResourceSeeds.ChalkboardResource.Identifier, "should_not_be_in_result_list_1"),
                    WithAssertResponseContainsNotInstance(ResourceSeeds.ChalkboardResource.Identifier, "should_not_be_in_result_list_2"),
                    WithAssertResponseContainsNotInstance(ResourceSeeds.ChalkboardResource.Identifier, "should_not_be_in_result_list_3")
                )
            ),
        ];
    }

    /// <summary>
    /// <see cref="AuthorizedPartiesController.GetAuthorizedParties(bool, CancellationToken)"/>
    /// </summary>
    /// <param name="acceptanceCriteria">acceptance test</param>
    [Theory]
    [MemberData(nameof(GetAuthorizedParties.Seeds), MemberType = typeof(GetAuthorizedParties))]
    public async Task GET_AuthorizedParties(GetAuthorizedParties acceptanceCriteria) => await acceptanceCriteria.Test(Fixture);

    /// <summary>
    /// Seeds for <see cref="GET_AuthorizedPartiesAsAccessManager"/>
    /// </summary>
    /// <param name="acceptanceCriteria">Acceptance Criteria</param>
    /// <param name="partyId">partyId</param>
    /// <param name="actions">modifiers for <see cref="AcceptanceCriteriaComposer"/></param>
    public class GetAuthorizedPartiesAsAccessManager(string acceptanceCriteria, int partyId, params Action<AcceptanceCriteriaComposer>[] actions) : AcceptanceCriteriaComposer(
            acceptanceCriteria,
            actions,
            WithRequestRoute("accessmanagement", "api", "v1", partyId, "authorizedparties"),
            WithRequestVerb(HttpMethod.Get))
    {
        /// <summary>
        /// Seeds
        /// </summary>
        public static TheoryData<GetAuthorizedPartiesAsAccessManager> Seeds() => [
            new(
                /* Acceptance Critieria: Organization Receiving Instance Access from an Individual */ @"
                GIVEN a user holding a key role (e.g., DAGL/ECKeyRole) for an organization that has received instance delegation from an individual
                WHEN the user's list of authorized parties is retrieved
                THEN the list of authorized parties should include the individual from whom the instance access is delegated
                AND the individual's AuthorizedInstances should include an identifier specifying the resourceId and instanceId for the instance delegation",
                OrganizationSeeds.VossConsulting.PartyId,
                WithScenarios(
                    DelegationScenarios.Defaults,
                    TokenScenario.PersonToken(PersonSeeds.Olav.Defaults),
                    DelegationScenarios.WherePersonHasKeyRole(PersonSeeds.Olav.Defaults, OrganizationSeeds.VossConsulting.Defaults),
                    DelegationScenarios.WithInstanceDelegation(PersonSeeds.Paula.Defaults, OrganizationSeeds.VossConsulting.Defaults, ResourceSeeds.ChalkboardResource.Defaults, "frame_1"),
                    DelegationScenarios.WithInstanceDelegation(PersonSeeds.Kasper.Defaults, OrganizationSeeds.VossConsulting.Defaults, ResourceSeeds.ChalkboardResource.Defaults, "frame_2"),
                    DelegationScenarios.WithInstanceDelegation(OrganizationSeeds.VossAccounting.Defaults, PersonSeeds.Paula.Defaults, ResourceSeeds.ChalkboardResource.Defaults, "should_not_be_in_result_list_1"),
                    DelegationScenarios.WithInstanceDelegation(OrganizationSeeds.VossConsulting.Defaults, PersonSeeds.Olav.Defaults, ResourceSeeds.ChalkboardResource.Defaults, "should_not_be_in_result_list_2"),
                    TokenScenario.PersonToken(PersonSeeds.Olav.Defaults)),

                WithAssertResponseStatusCodeSuccessful,
                WithAssertSuccessfulResponse(
                    WithAssertResponseContainsParty(PersonSeeds.Paula.Defaults),
                    WithAssertResponseContainsParty(PersonSeeds.Kasper.Defaults),
                    WithAssertResponseContainsNotParty(OrganizationSeeds.VossAccounting.Defaults),
                    WithAssertResponseContainsNotParty(OrganizationSeeds.VossAccounting.Defaults),
                    WithAssertResponseContainsPartyWithInstance(ResourceSeeds.ChalkboardResource.Identifier, "frame_1"),
                    WithAssertResponseContainsPartyWithInstance(ResourceSeeds.ChalkboardResource.Identifier, "frame_2"),
                    WithAssertResponseContainsNotInstance(ResourceSeeds.ChalkboardResource.Identifier, "should_not_be_in_result_list_1"),
                    WithAssertResponseContainsNotInstance(ResourceSeeds.ChalkboardResource.Identifier, "should_not_be_in_result_list_2")
                )
            ),
            new(
                /* Acceptance Critieria: User Receiving Instance Access from a Subunit */ @"
                GIVEN an organization that has received instance delegation from an individual
                WHEN the organization's list of authorized parties is retrieved
                THEN the list of authorized parties should contain the individual from whom the instance access is delegated
                AND the individual's AuthorizedInstances should include an identifier specifying the resourceId and instanceId for the instance delegation",
                OrganizationSeeds.VossConsulting.PartyId,
                WithScenarios(
                    DelegationScenarios.Defaults,
                    TokenScenario.PersonToken(PersonSeeds.Olav.Defaults),
                    DelegationScenarios.WherePersonHasKeyRole(PersonSeeds.Olav.Defaults, OrganizationSeeds.VossConsulting.Defaults),
                    DelegationScenarios.WithInstanceDelegation(PersonSeeds.Paula.Defaults, OrganizationSeeds.VossConsulting.Defaults, ResourceSeeds.ChalkboardResource.Defaults, "frame_1"),
                    DelegationScenarios.WithInstanceDelegation(PersonSeeds.Kasper.Defaults, OrganizationSeeds.VossConsulting.Defaults, ResourceSeeds.ChalkboardResource.Defaults, "frame_2"),
                    DelegationScenarios.WithInstanceDelegation(OrganizationSeeds.VossAccounting.Defaults, PersonSeeds.Paula.Defaults, ResourceSeeds.ChalkboardResource.Defaults, "should_not_be_in_result_list_1"),
                    DelegationScenarios.WithInstanceDelegation(OrganizationSeeds.VossConsulting.Defaults, PersonSeeds.Olav.Defaults, ResourceSeeds.ChalkboardResource.Defaults, "should_not_be_in_result_list_2"),
                    TokenScenario.PersonToken(PersonSeeds.Olav.Defaults)),

                WithAssertResponseStatusCodeSuccessful,
                WithAssertSuccessfulResponse(
                    WithAssertResponseContainsParty(PersonSeeds.Paula.Defaults),
                    WithAssertResponseContainsParty(PersonSeeds.Kasper.Defaults),
                    WithAssertResponseContainsNotParty(OrganizationSeeds.VossAccounting.Defaults),
                    WithAssertResponseContainsNotParty(OrganizationSeeds.VossAccounting.Defaults),
                    WithAssertResponseContainsPartyWithInstance(ResourceSeeds.ChalkboardResource.Identifier, "frame_1"),
                    WithAssertResponseContainsPartyWithInstance(ResourceSeeds.ChalkboardResource.Identifier, "frame_2"),
                    WithAssertResponseContainsNotInstance(ResourceSeeds.ChalkboardResource.Identifier, "should_not_be_in_result_list_1"),
                    WithAssertResponseContainsNotInstance(ResourceSeeds.ChalkboardResource.Identifier, "should_not_be_in_result_list_2")
                )
            ),
            new(
                /* Acceptance Critieria: Organization Receiving Instance Access from a Primary Unit */ @"
                GIVEN an organization that has received instance delegation from a primary unit
                WHEN the organization's list of authorized parties is retrieved
                THEN the list of authorized parties should contain the primary unit from which the instance access is delegated
                AND the primary unit's AuthorizedInstances should include an identifier specifying the resourceId and instanceId for the instance delegation
                AND if the organization has no other permissions for the primary unit or its subunits, no other subunits should be present in the list of authorized parties",
                OrganizationSeeds.VossConsulting.PartyId,
                WithScenarios(
                    DelegationScenarios.Defaults,
                    TokenScenario.PersonToken(PersonSeeds.Olav.Defaults),
                    DelegationScenarios.WherePersonHasKeyRole(PersonSeeds.Olav.Defaults, OrganizationSeeds.VossConsulting.Defaults),
                    DelegationScenarios.WhereUnitHasMainUnit(OrganizationSeeds.VossConsulting.Defaults, OrganizationSeeds.VossAccounting.Defaults),
                    DelegationScenarios.WithInstanceDelegation(OrganizationSeeds.VossAccounting.Defaults, OrganizationSeeds.VossConsulting.Defaults, ResourceSeeds.ChalkboardResource.Defaults, "color_1"),
                    DelegationScenarios.WithInstanceDelegation(OrganizationSeeds.VossAccounting.Defaults, PersonSeeds.Paula.Defaults, ResourceSeeds.ChalkboardResource.Defaults, "should_not_be_in_result_list_1"),
                    DelegationScenarios.WithInstanceDelegation(OrganizationSeeds.VossConsulting.Defaults, PersonSeeds.Olav.Defaults, ResourceSeeds.ChalkboardResource.Defaults, "should_not_be_in_result_list_2"),
                    TokenScenario.PersonToken(PersonSeeds.Olav.Defaults)),

                WithAssertResponseStatusCodeSuccessful,
                WithAssertSuccessfulResponse(
                    WithAssertResponseContainsParty(OrganizationSeeds.VossAccounting.Defaults),
                    WithAssertResponseContainsNotParty(PersonSeeds.Paula.Defaults),
                    WithAssertResponseContainsNotParty(PersonSeeds.Olav.Defaults),
                    WithAssertResponseContainsPartyWithInstance(ResourceSeeds.ChalkboardResource.Identifier, "color_1"),
                    WithAssertResponseContainsNotInstance(ResourceSeeds.ChalkboardResource.Identifier, "should_not_be_in_result_list_1"),
                    WithAssertResponseContainsNotInstance(ResourceSeeds.ChalkboardResource.Identifier, "should_not_be_in_result_list_2")
                )
            )
        ];
    }

    /// <summary>
    /// <see cref="AuthorizedPartiesController.GetAuthorizedPartiesAsAccessManager(int, bool, CancellationToken)"/>
    /// </summary>
    /// <param name="acceptanceCriteria">acceptance test</param>
    [Theory]
    [MemberData(nameof(GetAuthorizedPartiesAsAccessManager.Seeds), MemberType = typeof(GetAuthorizedPartiesAsAccessManager))]
    public async Task GET_AuthorizedPartiesAsAccessManager(GetAuthorizedPartiesAsAccessManager acceptanceCriteria) => await acceptanceCriteria.Test(Fixture);
}