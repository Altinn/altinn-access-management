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

        /// <summary>
        /// Builds a claims principal used to generate an Altinn-token for an organization with the provided scopes, essentially mocking a Maskinporten-token exchanged to an Altinn-token.
        /// </summary>
        /// <param name="org">Org code</param>
        /// <param name="orgNumber">Organization number</param>
        /// <param name="scope">Scopes to add to token</param>
        /// <param name="consumerPrefix">If maskinporten token sets the scope prefixes the organization owns or authorized for</param>
        /// <returns>Claims principal</returns>
        public static ClaimsPrincipal GetClaimsPrincipal(string org, string orgNumber, string scope = null, string[] consumerPrefix = null)
        {
            string issuer = "https://platform.altinn.cloud/authentication/api/v1/openid/";

            List<Claim> claims = new List<Claim>();
            if (scope != null)
            {
                claims.Add(new Claim("scope", scope, ClaimValueTypes.String));
            }

            if (consumerPrefix != null)
            {
                foreach (string prefix in consumerPrefix)
                {
                    claims.Add(new Claim("consumer_prefix", prefix, ClaimValueTypes.String));
                }
            }

            if (!string.IsNullOrEmpty(org))
            {
                claims.Add(new Claim(AltinnCoreClaimTypes.Org, org, ClaimValueTypes.String, issuer));
            }

            claims.Add(new Claim("consumer", GetOrgNoObject(orgNumber)));
            claims.Add(new Claim(AltinnCoreClaimTypes.OrgNumber, orgNumber.ToString(), ClaimValueTypes.Integer32, issuer));
            claims.Add(new Claim(AltinnCoreClaimTypes.AuthenticateMethod, "IntegrationTestMock", ClaimValueTypes.String, issuer));
            claims.Add(new Claim(AltinnCoreClaimTypes.AuthenticationLevel, "3", ClaimValueTypes.Integer32, issuer));

            ClaimsIdentity identity = new ClaimsIdentity("mock-org");
            identity.AddClaims(claims);

            return new ClaimsPrincipal(identity);
        }

        /// <summary>
        /// Generates an Altinn-token for an organization with the provided scopes, essentially mocking a Maskinporten-token exchanged to an Altinn-token.
        /// </summary>
        /// <param name="org">Org code</param>
        /// <param name="orgNumber">Organization number</param>
        /// <param name="scope">Scopes to add to token</param>
        /// <param name="consumerPrefix">If maskinporten token sets the scope prefixes the organization owns or authorized for</param>
        /// <returns>Altinn org-token</returns>
        public static string GetOrgToken(string org, string orgNumber = "991825827", string scope = null, string[] consumerPrefix = null)
        {
            ClaimsPrincipal principal = GetClaimsPrincipal(org, orgNumber, scope, consumerPrefix);

            string token = JwtTokenMock.GenerateToken(principal, new TimeSpan(0, 30, 0));

            return token;
        }

        private static string GetOrgNoObject(string orgNo)
        {
            return $"{{ \"authority\":\"iso6523-actorid-upis\", \"ID\":\"0192:{orgNo}\"}}";
        }
    }
}
