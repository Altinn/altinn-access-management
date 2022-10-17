using Altinn.AuthorizationAdmin.Core.Enums;
using Altinn.AuthorizationAdmin.Core.Models;
using Altinn.Platform.Register.Models;

namespace Altinn.AuthorizationAdmin.Core.Clients
{
    /// <summary>
    /// Interface for a client wrapper for integration with SBL bridge delegation request API
    /// </summary>
    public interface IPartiesClient
    {
        /// <summary>
        /// Returns a list of parties
        /// </summary>
        /// <returns>List of parties</returns>
        Task<List<Party>> GetPartiesAsync(List<int> parties);
    }
}
