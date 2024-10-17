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
        _helper = new Helper(_outputHelper);
    }

    [Fact]
    public async Task CreateSystemUser()
    {
        
    }

    [Fact]
    public async Task GetCreatedSystemUser()
    {
        const string party = "50692553";
        const string endpoint = "authentication/api/v1/systemuser/" + party;
        const string userId = "20012772";
        const string partyId = "50822874";
        const string scopes = "altinn:authentication/systemuser.request.read";
        const string pid = "04855195742";
        const string orgNo = "314279794";

        var token = await _helper.GetAltinnPersonalToken(partyId, scopes, pid, userId, _outputHelper, orgNo);
        // var token = await _helper.GetAltinnEnterpriseToken(scopes, orgNo);
        // _outputHelper.WriteLine(token);
        
        var respons = await _helper.PlatformAuthenticationClient.GetAsync(endpoint, token);
        _outputHelper.WriteLine(respons.StatusCode.ToString());
        _outputHelper.WriteLine(await respons.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, respons.StatusCode);
    }
}