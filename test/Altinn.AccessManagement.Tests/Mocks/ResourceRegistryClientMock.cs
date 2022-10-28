using System.Collections.Generic;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Integration.Clients;
using Altinn.AccessManagement.Tests.Utils;

namespace Altinn.AccessManagement.Tests.Mocks
{
    /// <summary>
    /// Mock class for <see cref="IResourceRegistryClient"></see> interface
    /// </summary>
    public class ResourceRegistryClientMock : IResourceRegistryClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceRegistryClient"/> class
        /// </summary>
        public ResourceRegistryClientMock()
        {
        }

        /// <inheritdoc/>
        public async Task<ServiceResource> GetResource(string resourceId)
        {
            string resourceTitle = string.Empty;
            if (resourceId == "nav_aa_distribution")
            {
                resourceTitle = "NAV aa distribution";
                return await Task.FromResult(TestDataUtil.GetResource(resourceId, resourceTitle));
            }
            else if (resourceId == "skd_1")
            {
                resourceTitle = "SKD 1";
                return await Task.FromResult(TestDataUtil.GetResource(resourceId, resourceTitle));
            }
            else if (resourceId == "resource1")
            {
                resourceTitle = "resource 1";
                return await Task.FromResult(TestDataUtil.GetResource(resourceId, resourceTitle));
            }
            else if (resourceId == "resource2")
            {
                resourceTitle = "resource 2";
                return await Task.FromResult(TestDataUtil.GetResource(resourceId, resourceTitle));
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<List<ServiceResource>> GetResources(List<string> resourceIds)
        {
            List<ServiceResource> resources = new List<ServiceResource>();
            foreach (string id in resourceIds)
            {
                ServiceResource resource = null;

                resource = await GetResource(id);
                
                if (resource == null)
                {
                    ServiceResource unavailableResource = new ServiceResource
                    {
                        Identifier = id,
                        Title = new Dictionary<string, string>
                        {
                            { "en", "Not Available" },
                            { "nb-no", "ikke tilgjengelig" },
                            { "nn-no", "ikkje tilgjengelig" }
                        }
                    };
                    resources.Add(unavailableResource);
                }
                else
                {
                    resources.Add(resource);
                }
            }

            return resources;
        }
    }
}
