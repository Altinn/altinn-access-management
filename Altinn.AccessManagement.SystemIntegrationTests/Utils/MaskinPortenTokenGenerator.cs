using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Altinn.AccessManagement.SystemIntegrationTests.Utils;

public class MaskinPortenTokenGenerator
{
    private static string ToStandardBase64(string? base64Url)
    {
        Assert.True(null != base64Url,"Base64 url should not be null");
        var base64 = base64Url.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2:
                base64 += "==";
                break;
            case 3:
                base64 += "=";
                break;
        }

        return base64;
    }

    public string GenerateJwt()
    {
        const string audience = "https://test.maskinporten.no/token";
        //string audience = "https://at22.altinn.cloud/maskinporten-api/";
        const string iss = "89708189-bb7f-475b-b0ac-0219f3271318"; // Replace with your client ID
        const string scope = "altinn:authentication/systemregister.write"; 
        
        var jwksJson = File.ReadAllText("../../../Resources/Jwks/jwks.json");
        var jwks = 
            JsonSerializer.Deserialize<Jwk>(jwksJson);

        // Set the current time and expiration time for the token
        var now = DateTimeOffset.UtcNow;
        var exp = now.AddMinutes(1).ToUnixTimeSeconds(); // Token valid for 10 minutes
        var iat = now.ToUnixTimeSeconds();
        var jti = Guid.NewGuid().ToString(); // Unique ID for the JWT   

        // Create RSA key from your JSON key parameters
        var rsa = new RSACryptoServiceProvider();
        rsa.ImportParameters(new RSAParameters
        {
            Modulus = Convert.FromBase64String(ToStandardBase64(
                jwks?.n)), // n
            Exponent = Convert.FromBase64String("AQAB"), // e
            D = Convert.FromBase64String(ToStandardBase64(
                jwks?.d)), // d
            P = Convert.FromBase64String(ToStandardBase64(
                jwks?.p)), // p
            Q = Convert.FromBase64String(ToStandardBase64(
                jwks?.q)), // q
            DP = Convert.FromBase64String(ToStandardBase64(
                jwks?.dp)), // dp
            DQ = Convert.FromBase64String(ToStandardBase64(
                jwks?.dq)), // dq
            InverseQ = Convert.FromBase64String(ToStandardBase64(
                jwks?.qi)) // qi
        });

        var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Aud, audience),
            new Claim(JwtRegisteredClaimNames.Iss, iss),
            new Claim("scope", scope),
            new Claim(JwtRegisteredClaimNames.Exp, exp.ToString(), ClaimValueTypes.Integer64),
            new Claim(JwtRegisteredClaimNames.Iat, iat.ToString(), ClaimValueTypes.Integer64),
            new Claim(JwtRegisteredClaimNames.Jti, jti)
        };

        var header = new JwtHeader(signingCredentials)
        {
            { "kid", "SystembrukerForSpesifikkOrgVegard" }  // Ensure 'kid' is added here
        };

        var payload = new JwtPayload(claims);

        // Create the JWT token manually with the custom header
        var token = new JwtSecurityToken(header, payload);
        var tokenHandler = new JwtSecurityTokenHandler();

        // Write and return the JWT
        return tokenHandler.WriteToken(token);
    }

    public async Task<string> RequestToken(string jwt)
    {
        using var client = new HttpClient();
        // Create the content for the request, application/x-www-form-urlencoded
        var requestContent = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"),
            new KeyValuePair<string, string>("assertion", jwt)
        ]);
        
        var response = await client.PostAsync("https://test.maskinporten.no/token", requestContent);

        if (response.IsSuccessStatusCode)
        {
            // Read the response body
            var responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }

        throw new Exception($"Failed to retrieve token: {response.StatusCode} {await response.Content.ReadAsStringAsync()}");
    }
    
    public async Task<string?> GetMaskinportenBearerToken()
    {
        var jwt = GenerateJwt();
        var maskinportenTokenResponse = await RequestToken(jwt);
        var jsonDoc = JsonDocument.Parse(maskinportenTokenResponse);
        var root = jsonDoc.RootElement;
        return root.GetProperty("access_token").GetString();
    }


}