using Altinn.AccessManagement.Core.Constants;
using Microsoft.AspNetCore.Http;

namespace Altinn.AccessManagement.Core.Helpers
{
    /// <summary>
    /// helper class for authentication
    /// </summary>
    public static class AuthenticationHelper
    {
        /// <summary>
        /// Gets the users id
        /// </summary>
        /// <param name="context">the http context</param>
        /// <returns>the logged in users id</returns>
        public static int GetUserId(HttpContext context)
        {
            var claim = context.User?.Claims.FirstOrDefault(c => c.Type.Equals(AltinnCoreClaimTypes.UserId));
            if (claim != null && int.TryParse(claim.Value, out int userId))
            {
                return userId;
            }

            return 0;
        }

        /// <summary>
        /// Gets the authenticated user's party id
        /// </summary>
        /// <param name="context">the http context</param>
        /// <returns>the logged in users party id</returns>
        public static int GetPartyId(HttpContext context)
        {
            var claim = context.User?.Claims.FirstOrDefault(c => c.Type.Equals(AltinnCoreClaimTypes.PartyID));
            if (claim != null && int.TryParse(claim.Value, out int partyId))
            {
                return partyId;
            }

            return 0;
        }

        /// <summary>
        /// Gets the users authentication level
        /// </summary>
        /// <param name="context">the http context</param>
        /// <returns>the logged in users authentication level</returns>
        public static int GetUserAuthenticationLevel(HttpContext context)
        {
            var claim = context.User?.Claims.FirstOrDefault(c => c.Type.Equals(AltinnCoreClaimTypes.AuthenticationLevel));
            if (claim != null && int.TryParse(claim.Value, out int authenticationLevel))
            {
                return authenticationLevel;
            }

            return 0;
        }
    }
}
