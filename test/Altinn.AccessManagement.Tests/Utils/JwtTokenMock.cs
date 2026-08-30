using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Tests.Models;
using Microsoft.IdentityModel.Tokens;

namespace Altinn.AccessManagement.Tests.Utils
{
    /// <summary>
    /// Represents a mechanism for creating JSON Web tokens for use in integration tests.
    /// </summary>
    public static class JwtTokenMock
    {
        /// <summary>
        /// Generates a token with a self signed certificate included in the integration test project.
        /// </summary>
        /// <param name="principal">The claims principal to include in the token.</param>
        /// <param name="tokenExpiry">How long the token should be valid for.</param>
        /// <param name="issuer">The URL of the token issuer</param>
        /// <returns>A new token.</returns>
        public static string GenerateToken(ClaimsPrincipal principal, TimeSpan tokenExpiry, string issuer = "UnitTest")
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(principal.Identity),
                Expires = DateTime.UtcNow.AddSeconds(tokenExpiry.TotalSeconds),
                SigningCredentials = GetSigningCredentials(issuer),
                Audience = "altinn.no",
                Issuer = issuer,
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            string serializedToken = tokenHandler.WriteToken(token);

            return serializedToken;
        }

        /// <summary>
        /// Generates a system user test token for unit tests. In production Maskinporten will be the entity that creates these tokens.
        /// </summary>
        /// <returns></returns>
        public static string GenerateSystemUserToken(string systemUserId, string systemUserOrg, string systemId, string consumer,  TimeSpan tokenExpiry, string issuer = "UnitTest")
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            SystemUserClaim systemUserClaims = new SystemUserClaim
            {
                Systemuser_id = [systemUserId],
                Systemuser_org = new OrgClaim()
                {
                    ID = systemUserOrg
                },
                System_id = systemId
            };
            List<SystemUserClaim> systemUserList = [systemUserClaims];

            OrgClaim consumerOrg = new OrgClaim()
            {
                ID = consumer
            };

            JsonElement systemUserClaimsJson = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(systemUserList));
            JsonElement consumerOrgJson = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(consumerOrg));

            Dictionary<string, object> claims = new Dictionary<string, object>()
            {
                { "authorization_details", systemUserClaimsJson },
                { "consumer", consumerOrgJson }
            };

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddSeconds(tokenExpiry.TotalSeconds),
                SigningCredentials = GetSigningCredentials(issuer),
                Audience = "altinn.no",
                Issuer = issuer,
                Claims = claims
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            string serializedToken = tokenHandler.WriteToken(token);

            return serializedToken;
        }

        private static SigningCredentials GetSigningCredentials(string issuer)
        {
            string certPath = "selfSignedTestCertificate.pfx";
            if (!issuer.Equals("sbl.authorization") && !issuer.Equals("www.altinn.no") && !issuer.Equals("UnitTest"))
            {
                certPath = $"{issuer}-org.pfx";

                X509Certificate2 certIssuer = new X509Certificate2(certPath);
                return new X509SigningCredentials(certIssuer, SecurityAlgorithms.RsaSha256);
            }

            X509Certificate2 cert = new X509Certificate2(certPath, "qwer1234");
            return new X509SigningCredentials(cert, SecurityAlgorithms.RsaSha256);
        }
    }
}
