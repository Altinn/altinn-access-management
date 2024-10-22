using System.Net;
using Altinn.AccessManagement.SystemIntegrationTests.Clients;
using Altinn.AccessManagement.SystemIntegrationTests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace Altinn.AccessManagement.SystemIntegrationTests.Tests;

/// <summary>
/// Tests relevant for "Systemregister": https://github.com/Altinn/altinn-authentication/issues/575
/// </summary>
public class SystemRegisterTests
{
    private readonly MaskinPortenTokenGenerator _maskinPortenTokenGenerator = new();
    private readonly ITestOutputHelper _outputHelper;
    private readonly Teststate _teststate;
    private readonly PlatformAuthenticationClient _platformAuthenticationClient;
    private readonly SystemRegisterClient _systemRegisterClient;

    /// <summary>
    /// Systemregister tests
    /// </summary>
    /// <param name="outputHelper">For test logging purposes</param>
    public SystemRegisterTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _systemRegisterClient = new SystemRegisterClient(_outputHelper);
        _teststate = new Teststate();
        _platformAuthenticationClient = new PlatformAuthenticationClient();
    }

    private async Task<HttpResponseMessage> CreateNewSystem(string token, string vendorId = "312605031")
    {
        const string endpoint = "authentication/api/v1/systemregister/vendor";
        _teststate.vendorId = vendorId;

        var randomName = Helper.GenerateRandomString(10);
        var testfile = await Helper.ReadFile("Resources/Testdata/Systemregister/CreateNewSystem.json");

        testfile = testfile
            .Replace("{vendorId}", vendorId)
            .Replace("{randomName}", randomName)
            .Replace("{clientId}", Guid.NewGuid().ToString());

        return await _platformAuthenticationClient.PostAsync(endpoint, testfile, token);
    }

    /// <summary>
    /// AK1 - Opprett system i systemregisteret
    /// </summary>
    [Fact]
    public async Task CreateNewSystemReturns200Ok()
    {
        var token = await _maskinPortenTokenGenerator.GetMaskinportenBearerToken();

        // Act
        var response = await CreateNewSystem(token);

        // Assert
        Assert.True(response.IsSuccessStatusCode, response.ReasonPhrase);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Test Get SystemRegister
    /// </summary>
    [Fact]
    public async Task GetSystemRegisterReturns200Ok()
    {
        var token = await _maskinPortenTokenGenerator.GetMaskinportenBearerToken();
        var altinnToken = await _platformAuthenticationClient.GetExchangeToken(token);

        // Act
        var response =
            await _platformAuthenticationClient.GetAsync("/authentication/api/v1/systemregister", altinnToken);

        // Assert
        Assert.True(response.IsSuccessStatusCode, response.ReasonPhrase);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ValidateRights()
    {
        // Prepare
        var token = await _maskinPortenTokenGenerator.GetMaskinportenBearerToken();
        const string vendorId = "312605031";
        var randomName = Helper.GenerateRandomString(15);
        await _systemRegisterClient.CreateNewSystem(token, randomName, vendorId);

        // Act
        var response =
            await _platformAuthenticationClient.GetAsync(
                $"/authentication/api/v1/systemregister/{vendorId}_{randomName}/rights", token);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(
            "[\n  {\n    \"resource\": [\n      {\n        \"id\": \"urn:altinn:resource\",\n        \"value\": \"kravogbetaling\"\n      }\n    ]\n  }\n]",
            responseContent);
    }

    /// <summary>
    /// Verify registered system gets deleted
    /// </summary>
    //[Fact] Todo: This currently fails
    public async Task DeleteRegisteredSystemReturns200Ok()
    {
        // Prepare
        var randomName = Helper.GenerateRandomString(20);
        const string vendorId = "312605031";
        var token = await _maskinPortenTokenGenerator.GetMaskinportenBearerToken();
        await _systemRegisterClient.CreateNewSystem(token, randomName);

        // Act
        var respons = await _platformAuthenticationClient.Delete(
            $"/authentication/api/v1/systemregister/vendor/{vendorId}_{randomName}",
            token);

        // Assert
        Assert.Equal(HttpStatusCode.OK, respons.StatusCode);
    }
}