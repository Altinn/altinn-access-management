using Altinn.AccessManagement.Core.Models.ResourceRegistry;

namespace Altinn.AccessManagement.Core.Services.Interfaces
{
    /// <summary>
    /// Service for creating and updating Resources in AccessMAnagment existing in the ResourceRegister
    /// </summary>
    public interface IResourceAdministrationPoint
    {
        /// <summary>
        /// Gets a list of Resources from ResourceRegister
        /// </summary>
        /// <param name="resourceType">The type of resource to be filtered</param>
        /// <returns>resource list based on resource type</returns>
        Task<List<ServiceResource>> GetResources(ResourceType resourceType);

        /// <summary>
        /// Gets a list of Resources from ResourceRegister
        /// </summary>
        /// <param name="scopes">The scope of the resource</param>
        /// <returns>resource list based on given scope</returns>
        Task<List<ServiceResource>> GetResources(string scopes);

        /// <summary>
        /// Gets a list of Resources from ResourceRegister
        /// </summary>
        /// <param name="resourceIds">The list of resource ids</param>
        /// <returns>resource list based on given resource ids</returns>
        Task<List<ServiceResource>> GetResources(List<Tuple<string, string>> resourceIds);

        /// <summary>
        /// Integration point for retrieving a single resoure by it's resource id
        /// </summary>
        /// <param name="resourceId">The identifier of the resource in the Resource Registry</param>
        /// <returns>The resource if exists</returns>
        Task<ServiceResource> GetResource(string resourceId);
    }
}