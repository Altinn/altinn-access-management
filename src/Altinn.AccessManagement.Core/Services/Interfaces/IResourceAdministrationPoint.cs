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
        /// <returns></returns>
        Task<List<ServiceResource>> GetResources(ResourceType resourceType);
    }
}