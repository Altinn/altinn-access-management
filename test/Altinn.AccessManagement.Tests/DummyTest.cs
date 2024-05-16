using System.Threading.Tasks;
using Altinn.AccessManagement.Tests.Fixtures;
using Xunit;

namespace Altinn.AccessManagement.Tests
{
    public class DummyTest(PostgresFixture fixture) : IClassFixture<PostgresFixture>
    {
        public readonly PostgresDatabase Db = fixture.New();

        [Fact]
        public async Task TestNameAsync()
        {
            var result = await Db.ResourceMetadata.InsertAccessManagementResource(new()
            {
                ResourceId = 1,
                ResourceRegistryId = "1",
                ResourceType = Core.Models.ResourceRegistry.ResourceType.AltinnApp
            });

            Assert.Equal(result.ResourceId, 1);
        }
    }
}