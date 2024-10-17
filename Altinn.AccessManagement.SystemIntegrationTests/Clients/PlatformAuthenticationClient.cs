using System.Net.Http.Headers;

namespace Altinn.AccessManagement.SystemIntegrationTests.Clients;

public class PlatformAuthenticationClient(string platformUrl)
{
    public async Task<HttpResponseMessage> PostAsync(string endpoint, string body, string token)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        HttpContent content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"{platformUrl}/{endpoint}", content);
        return response;
    }

    public async Task<HttpResponseMessage> GetAsync(string endpoint, string token)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return await client.GetAsync($"{platformUrl}/{endpoint}");
    }
}