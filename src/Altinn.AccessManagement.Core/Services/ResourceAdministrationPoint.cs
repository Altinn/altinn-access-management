using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Altinn.AccessManagement.Core.Services
{
    /// <inheritdoc />
    public class ResourceAdministrationPoint : IResourceAdministrationPoint
    {
        private readonly IResourceMetadataRepository _resourceRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceAdministrationPoint"/> class.
        /// </summary>
        /// <param name="resourceRepository">The policy repository (blob storage).</param>
        /// <param name="logger">Logger instance.</param>
        public ResourceAdministrationPoint(IResourceMetadataRepository resourceRepository, ILogger<IResourceAdministrationPoint> logger)
        {
            _resourceRepository = resourceRepository;
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
