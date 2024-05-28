using Altinn.AccessManagement.Tests.Contexts;
using Altinn.AccessManagement.Tests.Fixtures;
using Microsoft.AspNetCore.Hosting;

namespace Altinn.AccessManagement.Tests.Scenarios;

/// <summary>
/// Scenario function signature
/// </summary>
/// <param name="host">host builder</param>
/// <param name="mock">mock context object</param>
public delegate void Scenario(IWebHostBuilder host, MockContext mock);
