using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Clients.Interfaces;
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
                return await Task.FromResult(TestDataUtil.GetResource(resourceId, resourceTitle, ResourceType.MaskinportenSchema));
            }
            else if (resourceId == "skd_1")
            {
                resourceTitle = "SKD 1";
                return await Task.FromResult(TestDataUtil.GetResource(resourceId, resourceTitle, ResourceType.MaskinportenSchema));
            }
            else if (resourceId == "resource1")
            {
                resourceTitle = "resource 1";
                return await Task.FromResult(TestDataUtil.GetResource(resourceId, resourceTitle, ResourceType.MaskinportenSchema));
            }
            else if (resourceId == "resource2")
            {
                resourceTitle = "resource 2";
                return await Task.FromResult(TestDataUtil.GetResource(resourceId, resourceTitle, ResourceType.MaskinportenSchema));
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

        /// <inheritdoc/>
        public async Task<List<ServiceResource>> GetResources(ResourceType resourceType)
        {
            List<ServiceResource> resources = new List<ServiceResource>();
            resources.Add(TestDataUtil.GetResource("acn_1", "ACN 1", ResourceType.MaskinportenSchema, "Acn service 1", Convert.ToDateTime("2022-09-20T06:46:07.598"), Convert.ToDateTime("2022-12-24T23:59:59"), "Active"));
            resources.Add(TestDataUtil.GetResource("nav_aa_distribution", "nav_aa_distribution", ResourceType.MaskinportenSchema, "nav aa distribution", Convert.ToDateTime("2022-09-20T06:46:07.598"), Convert.ToDateTime("2022-12-24T23:59:59"), "Active"));
            resources.Add(TestDataUtil.GetResource("ssb_1", "Statistikk informasjon", ResourceType.MaskinportenSchema, "Statistikk informasjon", Convert.ToDateTime("2022-09-20T06:46:07.598"), Convert.ToDateTime("2022-12-24T23:59:59"), "Active"));
            resources.Add(TestDataUtil.GetResource("ttd_1", "TTD 1", ResourceType.MaskinportenSchema, "Test department service 1", Convert.ToDateTime("2022-09-20T06:46:07.598"), Convert.ToDateTime("2022-12-24T23:59:59"), "Active"));
            resources.Add(TestDataUtil.GetResource("appid-123", "appid 123", ResourceType.MaskinportenSchema, "appid 123", Convert.ToDateTime("2020-03-06T09:41:15.817"), Convert.ToDateTime("9999-12-31T23:59:59.997"), "Active"));

            return resources;
        }
    }
}
