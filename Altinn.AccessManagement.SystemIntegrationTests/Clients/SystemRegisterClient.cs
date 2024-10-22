using Altinn.AccessManagement.SystemIntegrationTests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace Altinn.AccessManagement.SystemIntegrationTests.Clients;

/// <summary>
/// For specific requests needed for System Register tests or test data generation purposes
/// </summary>
public class SystemRegisterClient : PlatformAuthenticationClient
{
    private Helper Helper { get; set; }

    private ITestOutputHelper Output { get; set; }

    /// <summary>
    /// For specific Sytem Register Requests
    /// </summary>
    public SystemRegisterClient(ITestOutputHelper output)
    {
        Output = output;
        Helper = new Helper(Output);
    }

    /// <summary>
    /// Creates a new system in Systmeregister. Requires Bearer token from Maskinporten
    /// </summary>
    /// <param name="token">A maskinporten token</param>
    /// <param name="name">Name</param>
    /// <param name="vendorId">The vendor creating the system. Defaults to a user created in Selvbetjeningsportalen</param>
    /// <returns></returns>
    public async Task<HttpResponseMessage> CreateNewSystem(string token, string name, string vendorId = "312605031")
    {
        const string endpoint = "authentication/api/v1/systemregister/vendor";
        var testfile = await Helper.ReadFile("Resources/Testdata/Systemregister/CreateNewSystem.json");

        testfile = testfile
            .Replace("{vendorId}", vendorId)
            .Replace("{randomName}", name)
            .Replace("{clientId}", Guid.NewGuid().ToString());
        
        //Assert 
        //return await PostAsync(endpoint, testfile, token);
        var response = await PostAsync(endpoint, testfile, token);
        Assert.NotNull(response);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        return response;
    }
}