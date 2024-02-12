using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Clients.Interfaces;

namespace Altinn.AccessManagement.Tests.Mocks
{
    /// <summary>
    /// Mock class for <see cref="IAuthenticationClient"></see> interface
    /// </summary>
    public class AuthenticationNullRefreshMock : IAuthenticationClient
    {
        /// <inheritdoc/>
        public async Task<string> RefreshToken() =>
            await Task.FromResult(string.Empty);
    }
}
