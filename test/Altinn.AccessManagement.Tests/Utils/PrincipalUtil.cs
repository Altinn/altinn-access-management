using System;
using System.Collections.Generic;
using System.Security.Claims;
using Altinn.AccessManagement.Tests.Utils;
using Altinn.Common.AccessTokenClient.Constants;
using AltinnCore.Authentication.Constants;

namespace Altinn.AccessManagement.Tests.Util
{
    /// <summary>
    /// Utility class for usefull common operations for setup of authentication tokens for integration tests
    /// </summary>
    public static class PrincipalUtil
    {
        /// <summary>
        /// Gets a user token
        /// </summary>
        /// <param name="userId">The user id</param>
        /// <param name="partyId">The users party id</param>
        /// <param name="authenticationLevel">The users authentication level</param>
        /// <returns>jwt token string</returns>
        public static string GetToken(int userId, int partyId, int authenticationLevel = 2)
        {
            List<Claim> claims = new List<Claim>();
            string issuer = "www.altinn.no";
            claims.Add(new Claim(AltinnCoreClaimTypes.UserId, userId.ToString(), ClaimValueTypes.String, issuer));
            claims.Add(new Claim(AltinnCoreClaimTypes.UserName, "UserOne", ClaimValueTypes.String, issuer));
            claims.Add(new Claim(AltinnCoreClaimTypes.PartyID, partyId.ToString(), ClaimValueTypes.Integer32, issuer));
            claims.Add(new Claim(AltinnCoreClaimTypes.AuthenticateMethod, "Mock", ClaimValueTypes.String, issuer));
            claims.Add(new Claim(AltinnCoreClaimTypes.AuthenticationLevel, authenticationLevel.ToString(), ClaimValueTypes.Integer32, issuer));

            ClaimsIdentity identity = new ClaimsIdentity("mock");
            identity.AddClaims(claims);
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            string token = JwtTokenMock.GenerateToken(principal, new TimeSpan(1, 1, 1));

            return token;
        }

        /// <summary>
        /// Get access token for issuer
        /// </summary>
        /// <returns></returns>
        public static string GetAccessToken(string issuer, string app)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(AccessTokenClaimTypes.App, app, ClaimValueTypes.String, issuer)
            };

            ClaimsIdentity identity = new ClaimsIdentity("mock");
            identity.AddClaims(claims);
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            string token = JwtTokenMock.GenerateToken(principal, new TimeSpan(0, 1, 5), issuer);

            return token;
        }

        /// <summary>
        /// Gets an access token for an app
        /// </summary>
        /// <param name="appId">The app to add as claim</param>
        /// <returns></returns>
        public static string GetAccessToken(string appId)
        {
            List<Claim> claims = new List<Claim>();
            string issuer = "www.altinn.no";
            if (!string.IsNullOrEmpty(appId))
            {
                claims.Add(new Claim("urn:altinn:app", appId, ClaimValueTypes.String, issuer));
            }

            ClaimsIdentity identity = new ClaimsIdentity("mock-org");
            identity.AddClaims(claims);
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            string token = JwtTokenMock.GenerateToken(principal, new TimeSpan(1, 1, 1), issuer);

            return token;
        }
    }
}
