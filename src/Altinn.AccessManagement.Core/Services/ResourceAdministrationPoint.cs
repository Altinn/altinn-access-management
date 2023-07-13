using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Altinn.AccessManagement.Core.Services
{
    /// <inheritdoc />
    public class ResourceAdministrationPoint : IResourceAdministrationPoint
    {
        private readonly ILogger<IResourceAdministrationPoint> _logger;
        private readonly IContextRetrievalService _contextRetrievalService;
        private readonly IResourceMetadataRepository _resourceRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceAdministrationPoint"/> class.
        /// </summary>
        /// <param name="resourceRepository">The data layer to handle Resource related persistence</param>
        /// <param name="logger">Logger instance.</param>
        /// <param name="contextRetrievalService">the handler for resource registry client</param>
        public ResourceAdministrationPoint(
            IResourceMetadataRepository resourceRepository,
            ILogger<IResourceAdministrationPoint> logger,
            IContextRetrievalService contextRetrievalService)
        {
            _resourceRepository = resourceRepository;
            _logger = logger;
            _contextRetrievalService = contextRetrievalService;
        }

        /// <inheritdoc />
        public async Task<List<ServiceResource>> GetResources(ResourceType resourceType)
        {
            try
            {
                List<ServiceResource> resources = await _contextRetrievalService.GetResources();
                return resources.FindAll(r => r.ResourceType == resourceType);
            }
            catch (Exception ex)
            {
                _logger.LogError("//ResourceAdministrationPoint //GetResources by resourcetype failed to fetch resources", ex);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<List<ServiceResource>> GetResources(string scopes)
        {
            List<ServiceResource> filteredResources = new List<ServiceResource>();

            List<ServiceResource> resources = await _contextRetrievalService.GetResources();

            foreach (ServiceResource resource in resources)
            {
                foreach (ResourceReference reference in resource.ResourceReferences)
                {
                    if (reference != null && reference.Reference.Equals(scopes) && reference.ReferenceType == ReferenceType.MaskinportenScope)
                    {
                        filteredResources.Add(resource);
                    }
                }
            }

            return filteredResources;
        }

        /// <inheritdoc />
        public async Task<ServiceResource> GetResource(string resourceRegistryId)
        {
            return await _contextRetrievalService.GetResource(resourceRegistryId);
        }

        /// <inheritdoc />
        public async Task<List<AccessManagementResource>> TryWriteResourceFromResourceRegister(List<AccessManagementResource> resources)
        {
            List<AccessManagementResource> result = new List<AccessManagementResource>();

            foreach (AccessManagementResource resource in resources)
            {
                AccessManagementResource current = await _resourceRepository.InsertAccessManagementResource(resource);

                if (current != null)
                {
                    result.Add(current);
                }
            }

            return result;
        }
    }
}