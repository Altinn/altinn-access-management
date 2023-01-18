﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
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
            ServiceResource resource = null;
            string rolesPath = GetResourcePath(resourceId);
            if (File.Exists(rolesPath))
            {
                string content = File.ReadAllText(rolesPath);
                resource = (ServiceResource)JsonSerializer.Deserialize(content, typeof(ServiceResource), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            return await Task.FromResult(resource);
        }

        /// <inheritdoc/>
        public Task<List<ServiceResource>> GetResources()
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

            return Task.FromResult(resources);
        }

        private static string GetResourcePath(string resourceRegistryId)
        {
            string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(ResourceRegistryClientMock).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, "..", "..", "..", "Data", "ResourceRegistryResources", $"{resourceRegistryId}", "resource.json");
        }

        private static string GetDataPathForResources()
        {
            string? unitTestFolder = Path.GetDirectoryName(new Uri(typeof(ResourceRegistryClientMock).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, "Data", "Resources");
        }
    }
}
