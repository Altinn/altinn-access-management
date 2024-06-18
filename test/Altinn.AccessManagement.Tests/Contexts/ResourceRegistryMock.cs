using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;

namespace Altinn.AccessManagement.Tests.Contexts;

/// <summary>
/// ResourceRegistryMock
/// </summary>
public class ResourceRegistryMock(MockContext context) : IResourceRegistryClient
{
    private MockContext Context { get; } = context;

    /// <inheritdoc/>
    public Task<ServiceResource> GetResource(string resourceId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Context.Resources.FirstOrDefault(resource => resource.Identifier == resourceId));

    /// <inheritdoc/>
    public Task<List<ServiceResource>> GetResourceList(CancellationToken cancellationToken = default) =>
        Task.FromResult(Context.Resources);

    /// <inheritdoc/>
    public Task<List<ServiceResource>> GetResources(CancellationToken cancellationToken = default) =>
        Task.FromResult(Context.Resources);

    /// <inheritdoc/>
    public Task<IDictionary<string, IEnumerable<BaseAttribute>>> GetSubjectResources(IEnumerable<string> subjects, CancellationToken cancellationToken = default) =>
        Task.FromResult(Context.SubjectResources);
}