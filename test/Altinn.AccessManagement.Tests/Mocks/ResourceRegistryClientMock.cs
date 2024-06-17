using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Integration.Clients;

namespace Altinn.AccessManagement.Tests.Mocks
{
    /// <summary>
    /// Mock class for <see cref="IResourceRegistryClient"></see> interface
    /// </summary>
    public class ResourceRegistryClientMock : IResourceRegistryClient
    {
        private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceRegistryClient"/> class
        /// </summary>
        public ResourceRegistryClientMock()
        {
        }

        /// <inheritdoc/>
        public async Task<ServiceResource> GetResource(string resourceId, CancellationToken cancellationToken = default)
        {
            ServiceResource resource = null;
            string rolesPath = GetResourcePath(resourceId);
            if (File.Exists(rolesPath))
            {
                string content = File.ReadAllText(rolesPath);
                resource = (ServiceResource)JsonSerializer.Deserialize(content, typeof(ServiceResource), _serializerOptions);
            }

            return await Task.FromResult(resource);
        }

        /// <inheritdoc/>
        public Task<List<ServiceResource>> GetResources(CancellationToken cancellationToken = default)
        {
            List<ServiceResource> resources = new List<ServiceResource>();

            string path = GetDataPathForResources();
            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path);
                foreach (string file in files)
                {
                    if (file.Contains("resources"))
                    {
                        string content = File.ReadAllText(Path.Combine(path, file));
                        resources = JsonSerializer.Deserialize<List<ServiceResource>>(content, _serializerOptions);
                    }
                }
            }

            return Task.FromResult(resources);
        }

        /// <inheritdoc/>
        public Task<List<ServiceResource>> GetResourceList(CancellationToken cancellationToken = default)
        {
            string content = File.ReadAllText($"Data/Resources/resourceList.json");
            List<ServiceResource> resources = (List<ServiceResource>)JsonSerializer.Deserialize(content, typeof(List<ServiceResource>), _serializerOptions);

            return Task.FromResult(resources);
        }

        /// <inheritdoc/>
        public Task<IDictionary<string, IEnumerable<BaseAttribute>>> GetSubjectResources(IEnumerable<string> subjects, CancellationToken cancellationToken = default)
        {
            string content = File.ReadAllText($"Data/Resources/subjectResources.json");
            PaginatedResult<SubjectResources> allSubjectResources = (PaginatedResult<SubjectResources>)JsonSerializer.Deserialize(content, typeof(PaginatedResult<SubjectResources>), _serializerOptions);

            IDictionary<string, IEnumerable<BaseAttribute>> result = new Dictionary<string, IEnumerable<BaseAttribute>>();
            if (allSubjectResources != null && allSubjectResources.Items != null)
            {
                foreach (SubjectResources resultItem in allSubjectResources.Items.Where(sr => subjects.Contains(sr.Subject.Urn)))
                {
                    result.Add(resultItem.Subject.Urn, resultItem.Resources);
                }
            }

            return Task.FromResult(result);
        }

        private static string GetResourcePath(string resourceRegistryId)
        {
            string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(ResourceRegistryClientMock).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, "..", "..", "..", "Data", "ResourceRegistryResources", $"{resourceRegistryId}", "resource.json");
        }

        private static string GetDataPathForResources()
        {
            string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(ResourceRegistryClientMock).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, "Data", "Resources");
        }
    }
}
