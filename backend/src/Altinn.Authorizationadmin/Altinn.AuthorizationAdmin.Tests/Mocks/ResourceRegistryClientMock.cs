using System.Threading.Tasks;
using Altinn.AuthorizationAdmin.Core;
using Altinn.AuthorizationAdmin.Core.Models.ResourceRegistry;
using Altinn.AuthorizationAdmin.Tests.Utils;

namespace Altinn.AuthorizationAdmin.Tests.Mocks
{
    /// <summary>
    /// Mock class for <see cref="IResourceRegistryClient"></see> interface
    /// </summary>
    internal class ResourceRegistryClientMock : IResourceRegistryClient
    {
        /// <inheritdoc/>
        public async Task<ServiceResource> GetResource(string resourceId)
        {
            return await Task.FromResult(TestDataUtil.GetResource(resourceId));
        }
    }
}
