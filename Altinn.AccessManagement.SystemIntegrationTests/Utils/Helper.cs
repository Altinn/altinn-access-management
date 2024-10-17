using System.Net.Http.Headers;
using System.Text.Json;
using Altinn.AccessManagement.SystemIntegrationTests.Clients;
using Xunit;
using Xunit.Abstractions;

namespace Altinn.AccessManagement.SystemIntegrationTests.Utils;

public class Helper
{
    public string? PlatformUrl { get; }
    private PlatformEnvironment Environment { get; }
    public PlatformAuthenticationClient PlatformAuthenticationClient { get; }

    public Helper()
    {
        Environment = LoadEnvironment("../../../Resources/Environment/sample.at22.json") ??
                      throw new Exception("Unable to read environment file");
        PlatformUrl = Environment?.platformUrl ?? throw new InvalidOperationException("Platform URL not set");
        PlatformAuthenticationClient = new PlatformAuthenticationClient(PlatformUrl);
    }

    private static PlatformEnvironment LoadEnvironment(string filePath)
    {
        var environmentFile = File.ReadAllText(filePath);
        var env = JsonSerializer.Deserialize<PlatformEnvironment>(environmentFile);
        Assert.True(env != null, $"Environment file {filePath} not found");

        if (env.testCredentials == null || string.IsNullOrEmpty(env.testCredentials.username) ||
            string.IsNullOrEmpty(env.testCredentials.password))
        {
            throw new InvalidOperationException("TestCredentials, username or password is not set.");
        }

        return env;
    }

    public async Task<string> GetExchangeToken(string token)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.GetAsync(PlatformUrl + "/authentication/api/v1/exchange/maskinporten?test=true");

        if (response.IsSuccessStatusCode)
        {
            var accessToken = await response.Content.ReadAsStringAsync();
            return accessToken;
        }

        throw new Exception(
            $"Failed to retrieve exchange token: {response.StatusCode} {await response.Content.ReadAsStringAsync()}");
    }

    public static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        var result = new char[length];

        for (var i = 0; i < length; i++)
        {
            result[i] = chars[random.Next(chars.Length)];
        }

        return new string(result);
    }

    public async Task<string> GetAltinnToken(string partyId, string scopes, string pid, string userId, ITestOutputHelper helper, string organization)
    {
        var url =
            $"https://altinn-testtools-token-generator.azurewebsites.net/api/GetPersonalToken?env=at24" +
            $"&scopes={scopes}" +
            $"&pid={pid}" +
            $"&org={organization}" +
            //$"&userid={userId}" +
            //$"&partyid={partyId}" +
            $"&authLvl=3&ttl=3000";

        var username = Environment.testCredentials.username;
        var password = Environment.testCredentials.password;

        using var client = new HttpClient();
        try
        {
            var basicAuth = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{username}:{password}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);
            var response = await client.GetAsync(url);

            // Check the response status code
            if (response.IsSuccessStatusCode)
            {
                // Read the content of the response
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response Content: " + content);
                return content;
            }

            helper.WriteLine("Error: " + response.StatusCode + " " + await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception occurred: " + ex.Message);
        }

        throw new InvalidOperationException("Unable to get Altinn token");
    }
}