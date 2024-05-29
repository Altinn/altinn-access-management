using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Tests.Util;

namespace Altinn.AccessManagement.Tests.Mocks
{
    /// <summary>
    /// Mock class for <see cref="IAuthenticationClient"></see> interface
    /// </summary>
    public class AuthenticationMock : IAuthenticationClient
    {
        /// <inheritdoc/>
        public async Task<string> RefreshToken() => await Task.FromResult(PrincipalUtil.GetAccessToken("sbl-authorization"));
    }
}
