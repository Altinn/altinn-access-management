using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.Platform.Register.Models;
using Authorization.Platform.Authorization.Models;

namespace Altinn.AccessManagement.Core.Services.Interfaces
{
    /// <summary>
    /// Defines the interface for the context retrieval service defining operations for getting external context information for decision point requests
    /// </summary>
    public interface IContextRetrievalService
    {
        /// <summary>
        /// Integration point for retrieving a single resoure by it's resource id
        /// </summary>
        /// <param name="resourceRegistryId">The identifier of the resource in the Resource Registry</param>
        /// <returns>The resource if exists</returns>
        Task<ServiceResource> GetResource(string resourceRegistryId);

        /// <summary>
        /// Integration point for retrieving a list of resources by it's resource id
        /// </summary>
        /// <returns>The resource list if exists</returns>
        Task<List<ServiceResource>> GetResources();
    }
}
