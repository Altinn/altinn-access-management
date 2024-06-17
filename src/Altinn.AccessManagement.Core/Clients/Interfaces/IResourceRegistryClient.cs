using Altinn.AccessManagement.Core.Models;
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
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns>The resource if exists</returns>
        Task<ServiceResource> GetResource(string resourceId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Integration point for retrieving a list of resources
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns>The resource list if exists</returns>
        Task<List<ServiceResource>> GetResources(CancellationToken cancellationToken = default);

        /// <summary>
        /// Integration point for retrieving the full list of resources
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns>The resource full list of all resources if exists</returns>
        Task<List<ServiceResource>> GetResourceList(CancellationToken cancellationToken = default);

        /// <summary>
        /// Integration point for retrieving all resources having any of the request subjects in one or more resource policy rules
        /// </summary>
        /// <param name="subjects">Urn string representation of the subjects to lookup resources for</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns>Dictionary of all resources per subject, having policy rules with the subject</returns>
        Task<IDictionary<string, IEnumerable<BaseAttribute>>> GetSubjectResources(IEnumerable<string> subjects, CancellationToken cancellationToken = default);
    }
}
