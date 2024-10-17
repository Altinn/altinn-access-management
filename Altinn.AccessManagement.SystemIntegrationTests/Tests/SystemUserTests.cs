using System.Net;
using Altinn.AccessManagement.SystemIntegrationTests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace Altinn.AccessManagement.SystemIntegrationTests.Tests;

public class SystemUserTests
{
    private readonly MaskinPortenTokenGenerator _maskinPortenTokenGenerator = new();
    private readonly ITestOutputHelper _outputHelper;
    private readonly Helper _helper;

    public SystemUserTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _helper = new Helper();
    }

    [Fact]
    public async Task GetCreatedSystemUser()
    {
        const string party = "50651214";
        const string endpoint = "authentication/api/v1/systemuser/" + party;
        const string userId = "1";
        const string partyId = "50651214";
        const string scopes = "altinn:authentication/systemuser.request.read";
        const string pid = "04855195742";
        const string organization = "312397021";

        var token = await _helper.GetAltinnToken(partyId, scopes, pid, userId, _outputHelper, organization);
        _outputHelper.WriteLine(token);
        
        var respons = await _helper.PlatformAuthenticationClient.GetAsync(endpoint, token);
        _outputHelper.WriteLine(respons.StatusCode.ToString());
        _outputHelper.WriteLine(await respons.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, respons.StatusCode);
    }
}