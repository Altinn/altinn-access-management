using System.Text.Json;
using Altinn.AccessManagement.SystemIntegrationTests.Domain;
using Altinn.AccessManagement.SystemIntegrationTests.Utils;
using Xunit.Abstractions;

namespace Altinn.AccessManagement.SystemIntegrationTests.Tests;

/// <summary>
/// Test that we're able to get a Machineporten token
/// </summary>
/// <param name="outputHelper">Needed for logging</param>
public class GetMaskinportenTokenTest(ITestOutputHelper outputHelper)
{
    private readonly MaskinPortenTokenGenerator _maskinPortenTokenGenerator = new();
    private readonly Helper _helper = new();

    private ITestOutputHelper OutputHelperutputHelperput { get; set; } = outputHelper;
    
    /// <summary>
    /// Test that we're able to read from jwks folder
    /// </summary>
    [Fact]
    public void ReadJwkFile()
    {
        var jsonString = File.ReadAllText("../../../Resources/JwksUnitTest/UnitTestJwks.json");

        var test =
            JsonSerializer.Deserialize<Jwk>(jsonString);

        Assert.Equal("RSA", test?.kty);
        Assert.Equal("_Pasldkalsødkasd", test?.p);
        Assert.Equal("ALKSdløaskdløasd", test?.q);
        Assert.Equal("h5u_77Q", test?.d);
        Assert.Equal("AQAB", test?.e);
        Assert.Equal("sig", test?.use);
        Assert.Equal("SystembrukerForSpesifikkOrgVegard", test?.kid);
        Assert.Equal("NxdzozNmkgIWIUFoRldlT1mVdE_H-8aJHdl4pUgI1J4iZanGhPgwGiOiFrHb3YLFQL0", test?.qi);
        Assert.Equal("2BJcrDuPJSL4kmi8epxNhRP-I0Kx78FwQWZ8", test?.dp);
        Assert.Equal("RS256", test?.alg);
        Assert.Equal("b_CE3QmIMsksEIVF178Ah2MqbJbPk", test?.dq);
        Assert.Equal("NzYiQSN_RNk-LSCqoMjPXCUv7g-Q", test?.n);
    }

    /// <summary>
    /// Test that we're able to get a Machineporten token on behalf of an organization
    /// </summary>
    [Fact]
    public async Task GetTokenAsOrganization()
    {
        var token = _maskinPortenTokenGenerator.GenerateJwt();
        Assert.NotEmpty(token);

        var maskinportenToken = await _maskinPortenTokenGenerator.RequestToken(token);
        Assert.Contains("systemregister.write", maskinportenToken);
        Assert.Contains("access_token", maskinportenToken);
    }

    /// <summary>
    /// Make sure we test that the ExchangeToken endpoint is up and running
    /// </summary>
    [Fact]
    public async Task GetExchangeToken()
    {
        var token = _maskinPortenTokenGenerator.GenerateJwt();
        Assert.NotEmpty(token);

        var maskinportenTokenResponse = await _maskinPortenTokenGenerator.RequestToken(token);
        var jsonDoc = JsonDocument.Parse(maskinportenTokenResponse);
        var root = jsonDoc.RootElement;

        var accessToken = root.GetProperty("access_token").GetString();
        Assert.NotNull(accessToken);

        var altinnToken = await _helper.GetExchangeToken(accessToken);
        Assert.NotEmpty(altinnToken);
    }

    [Fact]
    public async Task GetBearerToken()
    {
        var token = await _maskinPortenTokenGenerator.GetMaskinportenBearerToken();
        OutputHelperutputHelperput.WriteLine(token);
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }
}