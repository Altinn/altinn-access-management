using System.Threading.Tasks;
using Altinn.AccessManagement.Tests.Fixtures;
using Xunit;

namespace Altinn.AccessManagement.Tests.Controllers;

/// <summary>
///  Base class for executing controller tests
/// </summary>
public class ControllerTest(WebApplicationFixture fixture) : IClassFixture<WebApplicationFixture>, IAsyncLifetime
{
    /// <summary>
    /// Web application fixture
    /// </summary>
    protected WebApplicationFixture Fixture { get; } = fixture;

    /// <inheritdoc/>
    public async Task DisposeAsync()
    {
        await Fixture.Postgres.DropDb();
    }

    /// <inheritdoc/>
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }
}