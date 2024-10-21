using System.Net;
using Altinn.AccessManagement.SystemIntegrationTests.Clients;
using Altinn.AccessManagement.SystemIntegrationTests.Domain;
using Altinn.AccessManagement.SystemIntegrationTests.Utils;
using Altinn.AccessManagement.SystemIntegrationTests.Utils.TestUsers;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Altinn.AccessManagement.SystemIntegrationTests.Tests;

/// <summary>
/// Class containing system user tests
/// </summary>
public class SystemUserTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly SystemRegisterClient _systemRegisterClient;
    private readonly PlatformAuthenticationClient _platformAuthenticationClient;
    private readonly MaskinPortenTokenGenerator _maskinPortenTokenGenerator;

    /// <summary>
    /// Testing System user endpoints
    /// </summary>
    public SystemUserTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _systemRegisterClient = new SystemRegisterClient(_outputHelper);
        _platformAuthenticationClient = new PlatformAuthenticationClient();
        _maskinPortenTokenGenerator = new MaskinPortenTokenGenerator();
    }

    /// <summary>
    /// Verify that system is created
    /// </summary>
    [Fact]
    public async Task CreateSystemUser()
    {
        var maskinportenBearerToken = await _maskinPortenTokenGenerator.GetMaskinportenBearerToken();
        await _systemRegisterClient.CreateNewSystem(maskinportenBearerToken);

        // Todo - bug? Doesn't verify scopes
        const string scopes = "altinn:authentication/systemuser.request.read";
        const string userId = "20012772";
        const string partyId = "51670464";
        const string pid = "64837001585";

        // Create system user for this party??
        const string party = "50692553";

        var manager = new AltinnUser
            { userId = userId, partyId = partyId, pid = pid };

        var token = await _platformAuthenticationClient.GetPersonalAltinnToken(manager.partyId, scopes, manager.pid, manager.userId);
        var requestBody = await Helper.ReadFile("Resources/Testdata/SystemUser/RequestSystemUser.json");
        const string endpoint = "authentication/api/v1/systemuser/" + party;

        var respons = await _platformAuthenticationClient.PostAsync(endpoint, requestBody, token);

        Assert.Equal(HttpStatusCode.Created, respons.StatusCode);
    }

    /// <summary>
    /// Test Get endpoint for System User
    /// </summary>
    [Fact]
    public async Task GetCreatedSystemUser()
    {
        const string party = "50692553";
        const string endpoint = "authentication/api/v1/systemuser/" + party;
        const string userId = "20012772";
        const string scopes = "altinn:authentication/systemuser.request.read";
        const string pid = "04855195742";

        const string partyId = "50822874";
        var token = await _platformAuthenticationClient.GetPersonalAltinnToken(partyId, scopes, pid, userId);

        var respons = await _platformAuthenticationClient.GetAsync(endpoint, token);
        Assert.Equal(HttpStatusCode.OK, respons.StatusCode);
    }
}