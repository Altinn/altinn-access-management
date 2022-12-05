using Altinn.AccessManagement.Core.Models.ResourceRegistry;

namespace Altinn.AccessManagement.Core.Clients.Interfaces
{
    /// <summary>
    /// Interface for client integration with the Resource Registry
    /// </summary>
    public interface IResourceRegistryClient
    {
        /// <summary>
        /// Integration point for retrieving a single resoure by it's resource id
        /// </summary>
        /// <param name="resourceId">The identifier of the resource in the Resource Registry</param>
        /// <returns>The resource if exists</returns>
        Task<ServiceResource> GetResource(string resourceId);

        /// <summary>
        /// Integration point for retrieving a list of resources by it's resource id
        /// </summary>
        /// <param name="resourceIds">The identifier of the resource in the Resource Registry</param>
        /// <returns>The resource list if exists</returns>
        Task<List<ServiceResource>> GetResources(List<string> resourceIds);

        /// <summary>
        /// Integration point for retrieving a list of resources by resourceType
        /// </summary>
        /// <param name="resourceType">resource type</param>
        /// <returns>The resource list if exists</returns>
        Task<List<ServiceResource>> GetResources(ResourceType resourceType);
    }
}
