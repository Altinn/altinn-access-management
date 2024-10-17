using System.Net;
using Altinn.AccessManagement.SystemIntegrationTests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace Altinn.AccessManagement.SystemIntegrationTests.Tests;

public class SystemRegisterTests
{
    private readonly MaskinPortenTokenGenerator _maskinPortenTokenGenerator = new();
    private readonly ITestOutputHelper _outputHelper;
    private readonly Helper _helper;

    public SystemRegisterTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _helper = new Helper(_outputHelper);
    }
    
    public async Task<HttpResponseMessage> CreateNewSystem(string token)
    {
        const string endpoint = "authentication/api/v1/systemregister/vendor";

        var randomName = Helper.GenerateRandomString(10);   
        var basePath = Directory.GetCurrentDirectory();
        var filePath = Path.Combine(basePath, "Resources/Testdata/Systemregister/TilgangslisteTest.json");
        
        var stringBody = await File.ReadAllTextAsync(filePath);
        
        stringBody = stringBody.Replace("{randomName}", randomName).Replace("{clientId}", Guid.NewGuid().ToString());
        
        return await _helper.PlatformAuthenticationClient.PostAsync(endpoint, stringBody, token);
    }

    /// <summary>
    /// AK1 - Opprett system i systemregisteret
    /// </summary>
    [Fact]
    public async Task CreateNewSystemReturns200Ok()
    {
        var token = await _maskinPortenTokenGenerator.GetMaskinportenBearerToken();
        var altinnToken = await _helper.GetExchangeToken(token);

        // Act
        var response = await CreateNewSystem(altinnToken);
        _outputHelper.WriteLine(await response.Content.ReadAsStringAsync());

        // Assert
        Assert.True(response.IsSuccessStatusCode, response.ReasonPhrase);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}