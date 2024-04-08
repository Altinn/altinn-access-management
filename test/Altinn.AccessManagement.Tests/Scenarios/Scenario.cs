using Altinn.AccessManagement.Tests.Contexts;
using Altinn.AccessManagement.Tests.Fixtures;

namespace Altinn.AccessManagement.Tests.Scenarios;

/// <summary>
/// Scenario function signature
/// </summary>
/// <param name="fixture">web application fixture</param>
/// <param name="mock">mock context object</param>
public delegate void Scenario(WebApplicationFixtureContext fixture, MockContext mock);
