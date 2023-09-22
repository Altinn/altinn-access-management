using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using static System.Formats.Asn1.AsnWriter;

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
        public async Task<List<ServiceResource>> GetResources(string scope)
        {
            List<ServiceResource> filteredResources = new List<ServiceResource>();

            List<ServiceResource> resources = await _contextRetrievalService.GetResources();

            foreach (ServiceResource resource in resources.Where(r => r.ResourceType == ResourceType.MaskinportenSchema))
            {
                foreach (ResourceReference reference in resource.ResourceReferences?.Where(rf => rf.ReferenceType == ReferenceType.MaskinportenScope && rf.Reference.Equals(scope)))
                {
                    filteredResources.Add(resource);
                }
            }

            return filteredResources;
        }

        /// <inheritdoc />
        public async Task<List<ServiceResource>> GetResources(List<Tuple<string, string>> resourceIds)
        {
            List<ServiceResource> filteredResources = new List<ServiceResource>();

            try
            {
                foreach (Tuple<string, string> id in resourceIds)
                {
                    ServiceResource resource = null;

                    resource = await GetResource(id.Item1);

                    if (resource == null)
                    {
                        ServiceResource unavailableResource = new ServiceResource
                        {
                            Identifier = id.Item1,
                            Title = new Dictionary<string, string>
                        {
                            { "en", "Not Available" },
                            { "nb-no", "ikke tilgjengelig" },
                            { "nn-no", "ikkje tilgjengelig" }
                        },
                            ResourceType = Enum.TryParse<ResourceType>(id.Item2, out ResourceType resourceType) ? resourceType : ResourceType.Default
                        };
                        filteredResources.Add(unavailableResource);
                    }
                    else
                    {
                        filteredResources.Add(resource);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("//ResourceAdministrationPoint //GetResources by resource id failed to fetch resources", ex);
                throw;
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