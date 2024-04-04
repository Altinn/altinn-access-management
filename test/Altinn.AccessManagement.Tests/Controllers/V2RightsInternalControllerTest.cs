using System;
using System.Net.Http;
using System.Threading.Tasks;
using Altinn.AccessManagement.Controllers;
using Altinn.AccessManagement.Tests.Fixtures;
using Altinn.AccessManagement.Tests.Scenarios;
using Altinn.AccessManagement.Tests.Seeds;
using Xunit;

namespace Altinn.AccessManagement.Tests.Controllers;

public class V2RightsInternalControllerTest : IClassFixture<WebApplicationFixture>
{
    public V2RightsInternalControllerTest(WebApplicationFixture fixture)
    {
        Fixture = fixture;
    }

    private WebApplicationFixture Fixture { get; }

    /// <summary>
    /// Seeds for <see cref="GET_RightsDelegationsOffered"/>
    /// </summary>
    public class SeedGetRightsDelegationsOffered : AcceptanceCriteriaTest
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="acceptanceCriteria">Acceptance Criteria</param>
        /// <param name="partyId">partyId in URL</param>
        /// <param name="body">payload to be sent</param>
        /// <param name="actions">modifiers for <see cref="AcceptanceCriteriaTest"/></param>
        public SeedGetRightsDelegationsOffered(string acceptanceCriteria, int partyId, params Action<AcceptanceCriteriaTest>[] actions)
            : base(
                acceptanceCriteria,
                actions,
                WithRequestRoute("accessmanagement", "api", "v1", "internal", partyId, "rights", "delegation", "offered"),
                WithRequestVerb(HttpMethod.Get),
                WithAssertResponseStatusCodeSuccessful)
        {
        }

        /// <summary>
        /// Seeds
        /// </summary>
        /// <returns></returns>
        public static TheoryData<SeedGetRightsDelegationsOffered> Seeds() => new()
        {
            new(
                /* Acceptance Critieria */@"
                Given Organization Orstad Accounting having delegations to employee Paula
                When DAGL request offered delegations from Orstad 
                Then Paula should be in the list over active delegations",
                OrganizationSeeds.OrstadAccounting.PartyId,
                WithScenarios(
                    DelegationScenarios.WherePersonHasKeyRole(PersonSeeds.Orjan.Defaults, OrganizationSeeds.OrstadAccounting.Defaults),
                    DelegationScenarios.FromOrstadAccountingToPaula(),
                    TokenScenario.PersonToken(PersonSeeds.Orjan.Defaults))
                ),
            // new(
            //     /* Acceptance Critieria */@"
            //     Given user Orjan having delegations to user Paula
            //     When Orjan revoke last delegation to Paula
            //     Then last delegation should be set to revoked.",
            //     OrganizationSeeds.OrstadAccounting.PartyId,
            //     WithScenarios(DelegationScenarios.FromOrstadAccountingToPaula(), TokenScenario.PersonToken(PersonSeeds.Orjan.Defaults))),
            // new(
            //     /* Acceptance Critieria */@"
            //     Given user Orjan having delegations to user Paula
            //     When Orjan revoke last delegation to Paula
            //     Then last delegation should be set to revoked.",
            //     OrganizationSeeds.OrstadAccounting.PartyId,
            //     WithScenarios(DelegationScenarios.FromOrstadAccountingToPaula(), TokenScenario.PersonToken(PersonSeeds.Orjan.Defaults)))
        };
    }

    /// <summary>
    /// <see cref="RightsInternalController.RevokeOfferedDelegation(AccessManagement.Models.AuthorizedPartyInput, AccessManagement.Models.RevokeOfferedDelegationExternal, System.Threading.CancellationToken)"/>
    /// </summary>
    /// <param name="data">acceptance test</param>
    [Theory(DisplayName = nameof(RightsInternalController.RevokeOfferedDelegation))]
    [MemberData(nameof(SeedGetRightsDelegationsOffered.Seeds), MemberType = typeof(SeedGetRightsDelegationsOffered))]
    public async Task GET_RightsDelegationsOffered(SeedGetRightsDelegationsOffered data)
    {
        await data.RunTests(Fixture);
    }
}