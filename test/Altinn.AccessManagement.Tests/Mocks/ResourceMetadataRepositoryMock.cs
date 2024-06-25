using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Repositories.Interfaces;

namespace Altinn.AccessManagement.Tests.Mocks
{
    /// <summary>
    /// Mock for ResourceMetadataRepository
    /// </summary>
    public class ResourceMetadataRepositoryMock : IResourceMetadataRepository
    {
        /// <summary>
        /// Mock
        /// </summary>
        /// <param name="resource">the resource to store in AccessManagment</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>the inserted/updated resource</returns>
        public Task<AccessManagementResource> InsertAccessManagementResource(AccessManagementResource resource, CancellationToken cancellationToken = default)
        {
            AccessManagementResource result = null
                ;
            if (resource.ResourceRegistryId == "string3")
            {
                return Task.FromResult(result);
            }

            Stream dataStream = File.OpenRead($"Data/Json/InsertAccessManagementResource/InsertData_{resource.ResourceRegistryId}.json");
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            result = JsonSerializer.Deserialize<AccessManagementResource>(dataStream, options);
            return Task.FromResult(result);
        }
    }
}
