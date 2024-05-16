using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Altinn.AccessManagement.Controllers;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Tests.Fixtures;
using Altinn.AccessManagement.Tests.Scenarios;
using Xunit;

namespace Altinn.AccessManagement.Tests.Controllers;

/// <summary>
/// <see cref="ResourceController"/>
/// </summary>
public class V2ResourceControllerTest(WebApplicationFixture fixture) : IClassFixture<WebApplicationFixture>
{
    private WebApplicationFixture Fixture { get; } = fixture;

    /// <summary>
    /// Asserts that resource exists in DB
    /// </summary>
    /// <param name="expected">excpected resource to exsist</param>
    /// <returns></returns>
    public static Action<AcceptanceCriteriaComposer> WithAssertResourceExistsInDb(AccessManagementResource expected) => test =>
    {
        test.ApiAssertions.Add(async host =>
        {
            await Task.CompletedTask;
        });
    };

    /// <summary>
    /// Test input
    /// </summary>
    private static readonly AccessManagementResource TestAltinnApp = new()
    {
        Created = DateTime.Today,
        Modified = DateTime.Today,
        ResourceId = 1,
        ResourceRegistryId = "test_id123",
        ResourceType = ResourceType.AltinnApp
    };

    /// <summary>
    /// Seeds for <see cref="SeedPostUpsertResource"/>
    /// </summary>
    /// <param name="acceptanceCriteria">Acceptance Criteria</param>
    /// <param name="actions">modifiers for <see cref="AcceptanceCriteriaComposer"/></param>
    public class SeedPostUpsertResource(string acceptanceCriteria, params Action<AcceptanceCriteriaComposer>[] actions) : AcceptanceCriteriaComposer(
            acceptanceCriteria,
            actions,
            WithRequestRoute("accessmanagement", "api", "v1", "internal", "resources"),
            WithRequestVerb(HttpMethod.Post))
    {
        /// <summary>
        /// Seeds
        /// </summary>
        public static TheoryData<SeedPostUpsertResource> Seeds() =>
        [
            new(
                /* Acceptance Criteria */ @"
                GIVEN a resource is upserted in resource registry
                WHEN resource registry forwards the resource
                THEN the resource should be stored",

                WithHttpRequestBodyJson<IEnumerable<AccessManagementResource>>([TestAltinnApp]),

                WithScenarios(TokenScenario.PlatformToken("platform", "resourceregistry")),

                WithAssertResponseStatusCodeSuccessful,
                WithAssertResourceExistsInDb(TestAltinnApp))
        ];
    }

    /// <summary>
    /// <see cref="ResourceController.Post(List{AccessManagementResource})"/>
    /// </summary>
    /// <param name="acceptanceCriteria">acceptance criteria</param>
    [Theory(DisplayName = "POST_UpsertResource")]
    [MemberData(nameof(SeedPostUpsertResource.Seeds), MemberType = typeof(SeedPostUpsertResource))]
    public async Task POST_UpsertResource(SeedPostUpsertResource acceptanceCriteria) => await acceptanceCriteria.Test(Fixture);
}