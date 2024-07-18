using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models.Authentication;

namespace Altinn.AccessManagement.Tests.Mocks
{
    /// <summary>
    /// Mock class for <see cref="IAuthenticationClient"></see> interface
    /// </summary>
    public class AuthenticationNullRefreshMock : IAuthenticationClient
    {
        /// <inheritdoc/>
        public async Task<string> RefreshToken(CancellationToken cancellationToken = default) => await Task.FromResult(string.Empty);

        /// <inheritdoc/>
        public async Task<SystemUser> GetSystemUser(int partyId, string systemUserId, CancellationToken cancellationToken = default) => await Task.FromResult((SystemUser)null);

        /// <inheritdoc/>
        public async Task<List<DefaultRight>> GetDefaultRightsForRegisteredSystem(string productId, CancellationToken cancellationToken = default) => await Task.FromResult((List<DefaultRight>)null);
    }
}
