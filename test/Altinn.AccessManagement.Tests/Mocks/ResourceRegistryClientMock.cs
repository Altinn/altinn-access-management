﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Integration.Clients;
using Altinn.AccessManagement.Tests.Utils;
using Altinn.Platform.Register.Models;

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
        public async Task<List<ServiceResource>> GetResources()
        {
            List<ServiceResource> resources = new List<ServiceResource>();

            string path = GetDataPathForResources();
            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };
                foreach (string file in files)
                {
                    if (file.Contains("resources"))
                    {
                        string content = File.ReadAllText(Path.Combine(path, file));
                        resources = JsonSerializer.Deserialize<List<ServiceResource>>(content, options);
                    }
                }
            }

            return resources;
        }

        private static string GetDataPathForResources()
        {
            string? unitTestFolder = Path.GetDirectoryName(new Uri(typeof(ResourceRegistryClientMock).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, "Data", "Resources");
        }
    }
}
