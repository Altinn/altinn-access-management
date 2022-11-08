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
    }
}
