using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Altinn.AccessManagement.Controllers;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Models;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.AccessManagement.Tests.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Altinn.AccessManagement.Tests.Controllers;

/// <summary>
/// Test class for <see cref="PolicyInformationPointController"></see>
/// </summary>
public class PolicyInformationPointControllerTest : IClassFixture<CustomWebApplicationFactory<PolicyInformationPointController>>
{
    private HttpClient _client;
    private readonly CustomWebApplicationFactory<PolicyInformationPointController> _factory;
    private readonly JsonSerializerOptions options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="PolicyInformationPointControllerTest"/> class.
    /// </summary>
    /// <param name="factory">CustomWebApplicationFactory</param>
    public PolicyInformationPointControllerTest(CustomWebApplicationFactory<PolicyInformationPointController> factory)
    {
        _factory = factory;
        _client = GetTestClient();
    }

    private HttpClient GetTestClient(IDelegationMetadataRepository delegationMetadataRepositoryMock = null)
    {
        delegationMetadataRepositoryMock ??= new DelegationMetadataRepositoryMock();

        HttpClient client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton(delegationMetadataRepositoryMock);
                services.AddSingleton<IPartiesClient, PartiesClientMock>();
            });
        }).CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        return client;
    }

    /// <summary>
    /// Sets up test scenarios for <see cref="PolicyInformationPointController.GetAllDelegationChanges(Core.Models.DelegationChangeInput, System.Threading.CancellationToken)"></see>
    /// </summary>
    public static TheoryData<string> Scenarios() => new()
    {
        { "app_toPerson" },
        { "resource_toPerson" },
        { "app_toSystemUser" },
        { "resource_toSystemUser" }
    };

    /// <summary>
    /// Test case: Tests if you can get all delegation changes for a resource
    /// Expected: Returns delegation changes for a resource
    /// </summary>
    [Theory]
    [MemberData(nameof(Scenarios))]
    public async Task GetDelegationChanges_ValidResponse(string scenario)
    {
        // Act
        HttpResponseMessage actualResponse = await _client.PostAsync($"accessmanagement/api/v1/policyinformation/getdelegationchanges", GetRequest(scenario));

        // Assert
        Assert.Equal(HttpStatusCode.OK, actualResponse.StatusCode);

        List<DelegationChangeExternal> actualDelegationChanges = JsonSerializer.Deserialize<List<DelegationChangeExternal>>(await actualResponse.Content.ReadAsStringAsync(), options);
        AssertionUtil.AssertEqual(GetExpected(scenario), actualDelegationChanges);
    }

    private static StreamContent GetRequest(string scenario)
    {
        Stream dataStream = File.OpenRead($"Data/PolicyInformationPoint/Requests/{scenario}.json");
        StreamContent content = new StreamContent(dataStream);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return content;
    }

    private List<DelegationChangeExternal> GetExpected(string scenario)
    {
        string expectedContent = File.ReadAllText($"Data/PolicyInformationPoint/Expected/{scenario}.json");
        return (List<DelegationChangeExternal>)JsonSerializer.Deserialize(expectedContent, typeof(List<DelegationChangeExternal>), options);
    }
}
