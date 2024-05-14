using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Tests.Contexts;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.AccessManagement.Tests.Scenarios;
using Altinn.Common.AccessToken.Services;
using Altinn.Common.PEP.Interfaces;
using AltinnCore.Authentication.JwtCookie;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Altinn.AccessManagement.Tests.Fixtures;

/// <summary>
/// Test server for Access management API
/// </summary>
public class WebApplicationFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    /// <summary>
    /// Postgres test container
    /// </summary>
    public PostgresServer PostgresServer { get; set; }

    /// <summary>
    /// ConfigureWebHost for setup of configuration and test services
    /// </summary>
    /// <param name="builder">IWebHostBuilder</param>
    protected override async void ConfigureWebHost(IWebHostBuilder builder)
    {
        var db = await PostgresServer.CreateDb();
        builder.ConfigureAppConfiguration(config =>
           {
               config.AddConfiguration(new ConfigurationBuilder()
                   .AddJsonFile("appsettings.test.json")
                   .AddInMemoryCollection(new Dictionary<string, string>
                   {
                       ["PostgreSQLSettings:AdminConnectionString"] = db.Admin.ToString(),
                       ["PostgreSQLSettings:ConnectionString"] = db.User.ToString(),
                       ["PostgreSQLSettings:EnableDBConnection"] = "true",
                   })
                   .Build());
           });
    }

    /// <summary>
    /// Creates a specific mock context based on given scenarios.
    /// </summary>
    /// <param name="scenarios">list of scenarios</param>
    public async Task<Host> UseScenarios(params Scenario[] scenarios)
    {
        var mock = new MockContext();
        var host = CreateHost(scenarios, mock);

        var client = host.CreateClient();
        foreach (var header in mock.HttpHeaders)
        {
            client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        foreach (var seed in mock.DbSeeds)
        {
            await seed(host.Services.GetRequiredService<RepositoryContainer>());
        }

        return new Host(host, client);
    }

    private WebApplicationFactory<Program> CreateHost(Scenario[] scenarios, MockContext mock)
    {
        return WithWebHostBuilder(builder =>
        {
            foreach (var scenario in scenarios)
            {
                scenario(builder, mock);
            }

            mock.Parties = mock.Parties.DistinctBy(party => party.PartyId).ToList();
            mock.UserProfiles = mock.UserProfiles.DistinctBy(userProfile => userProfile.PartyId).ToList();
            mock.Resources = mock.Resources.DistinctBy(resource => resource.Identifier).ToList();

            builder.ConfigureServices(services =>
            {
                services.AddSingleton<RepositoryContainer>();
            });

            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton(mock);
                AddMockClients(services);
            });
        });
    }

    private static void AddMockClients(IServiceCollection services)
    {
        // services.AddSingleton<IResourceRegistryClient, ResourceRegistryClientMock>();
        services.AddSingleton<IPartiesClient, Contexts.PartiesClientMock>();
        services.AddSingleton<IProfileClient, Contexts.ProfileClientMock>();
        services.AddSingleton<IResourceRegistryClient, ResourceRegistryMock>();

        services.AddSingleton<IPolicyRetrievalPoint, PolicyRetrievalPointMock>();
        services.AddSingleton<IPolicyRepository, PolicyRepositoryMock>();
        services.AddSingleton<IPostConfigureOptions<JwtCookieOptions>, JwtCookiePostConfigureOptionsStub>();
        services.AddSingleton<IPublicSigningKeyProvider, SigningKeyResolverMock>();

        services.AddSingleton<IAltinnRolesClient, AltinnRolesClientMock>();
        services.AddSingleton<IPDP, PdpPermitMock>();
        services.AddSingleton<IAltinn2RightsClient, Altinn2RightsClientMock>();
        services.AddSingleton<IDelegationChangeEventQueue>(new DelegationChangeEventQueueMock());
    }

    /// <summary>
    /// Creates a new postgres server
    /// </summary>
    public async Task InitializeAsync()
    {
        PostgresServer = await PostgresFactory.NewDbServer();
    }

    /// <summary>
    /// Destroys Postgres DB server
    /// </summary>
    public new async Task DisposeAsync()
    {
        await PostgresServer.DisposeAsync();
    }
}

/// <summary>
/// Container for the test server API and HTTP Client for sending requests 
/// </summary>
public class Host(WebApplicationFactory<Program> api, HttpClient client)
{
    /// <summary>
    /// Test server
    /// </summary>
    public WebApplicationFactory<Program> Api { get; } = api;

    /// <summary>
    /// Http Client with predefined base route to the API
    /// </summary>
    public HttpClient Client { get; } = client;

    /// <summary>
    /// Repository Container
    /// </summary>
    public RepositoryContainer Repository => Api.Services.GetRequiredService<RepositoryContainer>();
}
