using System.Threading.Tasks;
using Altinn.AccessManagement.Tests.Fixtures;
using Xunit;

namespace Altinn.AccessManagement.Tests.Templates;

/// <summary>
/// Template
/// </summary>
public class DatabaseTestTemplate(PostgresFixture fixture) : IClassFixture<PostgresFixture>
{
    private PostgresDatabase Database { get; } = fixture.New();

    /// <summary>
    /// Template
    /// </summary>
    // [Fact]
    public async Task Test_DatabaseTestTemplate()
    {
        var result = await Database.ResourceMetadata.InsertAccessManagementResource(new()
        {
            ResourceRegistryId = "1",
            ResourceType = Core.Models.ResourceRegistry.ResourceType.Systemresource
        });

        Assert.NotNull(result.ResourceId);
        Assert.Equal("1", result.ResourceRegistryId);
    }
}