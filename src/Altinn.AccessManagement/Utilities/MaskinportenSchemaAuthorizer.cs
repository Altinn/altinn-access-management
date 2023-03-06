using System.Security.Claims;
using Altinn.AccessManagement.Core.Constants;

namespace Altinn.AccessManagement.Utilities
{
    /// <summary>
    /// Authorization helper for custom authorization for maskinporten schema API operations
    /// </summary>
    public static class MaskinportenSchemaAuthorizer
    {
        /// <summary>
        /// Authorization of whether the provided claims is authorized for lookup of delegations of the given scope
        /// </summary>
        /// <param name="scope">The scope to authorize for delegation lookup</param>
        /// <param name="claims">The claims principal of the authenticated organization</param>
        /// <returns>bool</returns>
        public static bool IsAuthorizedDelegationLookupAccess(string scope, ClaimsPrincipal claims)
        {
            if (HasDelegationsAdminScope(claims))
            {
                return true;
            }

            return HasAuthorizedScopePrefixClaim(new[] { scope }, claims);
        }

        private static bool HasDelegationsAdminScope(ClaimsPrincipal claims)
        {
            return HasScope(claims, AuthzConstants.SCOPE_MASKINPORTEN_DELEGATIONS_ADMIN);
        }

        private static bool HasScope(ClaimsPrincipal claims, string scope)
        {
            Claim c = claims.Claims.FirstOrDefault(x => x.Type == AuthzConstants.CLAIM_MASKINPORTEN_SCOPE);
            if (c == null)
            {
                return false;
            }

            string[] scopes = c.Value.Split(' ');

            return scopes.Contains(scope);
        }

        private static bool HasAuthorizedScopePrefixClaim(IEnumerable<string> scopesToAuthorize, ClaimsPrincipal claims)
        {
            List<string> prefixes = claims.Claims.Where(x => x.Type == AuthzConstants.CLAIM_MASKINPORTEN_CONSUMER_PREFIX).Select(v => v.Value).ToList();
            return scopesToAuthorize.All(scopes => prefixes.Any(prefix => scopes.StartsWith(prefix + ':')));
        }
    }
}
