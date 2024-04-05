using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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

public class WebApplicationFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    public PostgresFixture Postgres { get; }

    public WebApplicationFixture()
    {
        Postgres = new();
    }

    /// <summary>
    /// ConfigureWebHost for setup of configuration and test services
    /// </summary>
    /// <param name="builder">IWebHostBuilder</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
           {
               config.AddConfiguration(new ConfigurationBuilder()
                   .AddJsonFile("appsettings.test.json")
                   .AddInMemoryCollection(new Dictionary<string, string>
                   {
                       ["PostgreSQLSettings:AdminConnectionString"] = Postgres.TestContainer.GetConnectionString(),
                       ["PostgreSQLSettings:ConnectionString"] = Postgres.TestContainer.GetConnectionString(),
                       ["PostgreSQLSettings:EnableDBConnection"] = "true",
                   })
                   .Build());
           });
    }

    /// <summary>
    /// Creates a specific mock context based on given scenarios.
    /// </summary>
    /// <param name="scenarios">list of scenarios</param>
    public HttpClient UseScenarios(params Scenario[] scenarios)
    {
        var mock = new MockContext();
        var client = GetClient(scenarios, mock);

        if (!string.IsNullOrEmpty(mock.JwtToken))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", mock.JwtToken);
        }

        Task.WaitAll([.. mock.DbSeeds.Select(seed => seed())]);

        return client;
    }

    private HttpClient GetClient(Scenario[] scenarios, MockContext mock)
    {
        return WithWebHostBuilder(builder =>
        {
            var context = new WebApplicationFixtureContext()
            {
                Host = builder,
                PostgresFixture = Postgres,
            };

            foreach (var scenario in scenarios)
            {
                scenario(context, mock);
            }

            mock.Parties = mock.Parties.DistinctBy(party => party.PartyId).ToList();
            mock.UserProfiles = mock.UserProfiles.DistinctBy(userProfile => userProfile.PartyId).ToList();
            mock.Resources = mock.Resources.DistinctBy(resource => resource.Identifier).ToList();

            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton(mock);
                AddMockClients(services);
            });
        }).CreateClient();
    }

    private static void AddMockClients(IServiceCollection services)
    {
        services.AddSingleton<IPolicyRetrievalPoint, PolicyRetrievalPointMock>();
        services.AddSingleton<IPolicyRepository, PolicyRepositoryMock>();
        services.AddSingleton<IPostConfigureOptions<JwtCookieOptions>, JwtCookiePostConfigureOptionsStub>();
        services.AddSingleton<IPublicSigningKeyProvider, SigningKeyResolverMock>();
        services.AddSingleton<IPartiesClient, Contexts.PartiesClientMock>();
        services.AddSingleton<IProfileClient, Contexts.ProfileClientMock>();
        services.AddSingleton<IResourceRegistryClient, ResourceRegistryClientMock>();
        services.AddSingleton<IAltinnRolesClient, AltinnRolesClientMock>();
        services.AddSingleton<IPDP, PdpPermitMock>();
        services.AddSingleton<IAltinn2RightsClient, Altinn2RightsClientMock>();
        services.AddSingleton<IDelegationChangeEventQueue>(new DelegationChangeEventQueueMock());
    }

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        await Postgres.InitializeAsync();
    }

    /// <inheritdoc/>
    public new async Task DisposeAsync()
    {
        await Postgres.DisposeAsync();
    }
}

/// <summary>
/// Wrapper object  
/// </summary>
public class WebApplicationFixtureContext
{
    /// <summary>
    /// Host builder 
    /// </summary>
    public IWebHostBuilder Host { get; set; }

    /// <summary>
    /// Postgres fixture
    /// </summary>
    public PostgresFixture PostgresFixture { get; set; }
}