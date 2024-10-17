using System.Text.Json;
using Altinn.AccessManagement.SystemIntegrationTests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace Altinn.AccessManagement.SystemIntegrationTests.Tests;

public class GetMaskinportenTokenTest(ITestOutputHelper outputHelper)
{
    private readonly MaskinPortenTokenGenerator _maskinPortenTokenGenerator = new();
    private readonly Helper _helper = new();
    private ITestOutputHelper OutputHelperutputHelperput { get; set; } = outputHelper;
    
    [Fact]
    public void ReadJwkFile()
    {
        var jsonString = File.ReadAllText("../../../Resources/Jwks/UnitTestJwks.json");

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

    [Fact]
    public async Task GetTokenAsOrganization()
    {
        var token = _maskinPortenTokenGenerator.GenerateJwt();
        Assert.NotEmpty(token);

        var maskinportenToken = await _maskinPortenTokenGenerator.RequestToken(token);
        Assert.Contains("systemregister.write", maskinportenToken);
        Assert.Contains("access_token", maskinportenToken);
        
        OutputHelperutputHelperput.WriteLine(maskinportenToken);
    }

    [Fact]
    public async Task GetExchangeToken()
    {
        var token = _maskinPortenTokenGenerator.GenerateJwt();
        Assert.NotEmpty(token);

        var maskinportenTokenResponse = await _maskinPortenTokenGenerator.RequestToken(token);
        var jsonDoc = JsonDocument.Parse(maskinportenTokenResponse);
        var root = jsonDoc.RootElement;

        // Extract the access token
        var accessToken = root.GetProperty("access_token").GetString();
        var altinnToken = await _helper.GetExchangeToken(accessToken);
        Assert.NotNull(altinnToken);
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