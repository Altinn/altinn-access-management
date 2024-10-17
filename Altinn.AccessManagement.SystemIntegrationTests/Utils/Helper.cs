using System.Net.Http.Headers;
using System.Text.Json;
using Altinn.AccessManagement.SystemIntegrationTests.Clients;
using Xunit;
using Xunit.Abstractions;

namespace Altinn.AccessManagement.SystemIntegrationTests.Utils;

public class Helper
{
    private string? PlatformUrl { get; }

    private PlatformEnvironment Environment { get; }

    public PlatformAuthenticationClient PlatformAuthenticationClient { get; }
    
    public ITestOutputHelper Output { get; }

    public Helper(ITestOutputHelper output)
    {
        Output = output;
        Environment = LoadEnvironment("../../../Resources/Environment/at22.json") ??
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

    public async Task<string> GetAltinnEnterpriseToken(string scopes, string orgNo)
    {
        var url =
            $"https://altinn-testtools-token-generator.azurewebsites.net/api/GetEnterpriseToken?env=at22" +
            $"&scopes={scopes}" +
            $"&org=umake%hemmelighetsfull%katt%restaurant" +
            $"&orgNo=310193380";
        
        //310193380

        //Må angis: env: environment,
        // org: idKey.org,
        // orgNo: idKey.orgno,
        // scopes: idKey.scopes,
        // ttl: ttl
        var token = GetAltinnToken(url, Output);
        return await token;
    }

    public async Task<string> GetAltinnPersonalToken(string partyId, string scopes, string pid, string userId, ITestOutputHelper helper, string organization)
    {
        // scopes = "altinn:instances:read";
        var url =
            $"https://altinn-testtools-token-generator.azurewebsites.net/api/GetPersonalToken?env=at22" +
            $"&scopes={scopes}" +
            $"&pid={pid}" +
            $"&userid={userId}" +
            $"&partyid={partyId}" +
            $"&authLvl=3&ttl=3000";

        var token = GetAltinnToken(url, helper);
        return await token;
    }

    /// <summary>
    /// Add header values to Http client needed for altinn test api
    /// </summary>
    /// <returns></returns>
    private async Task<string> GetAltinnToken(string url, ITestOutputHelper helper)
    {
        var client = new HttpClient();
        var username = Environment.testCredentials.username;
        var password = Environment.testCredentials.password;
        var basicAuth = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{username}:{password}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);
        try
        {
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