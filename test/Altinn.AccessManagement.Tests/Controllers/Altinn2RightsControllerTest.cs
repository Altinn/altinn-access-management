using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Altinn.AccessManagement.Controllers;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Resolvers;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Models;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.AccessManagement.Tests.Util;
using Altinn.AccessManagement.Utilities;
using Altinn.Common.AccessToken.Services;
using Altinn.Common.PEP.Interfaces;
using AltinnCore.Authentication.JwtCookie;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Altinn.AccessManagement.Tests.Controllers;

/// <summary>
/// Controller test for <see cref="RightsInternalController"/>
/// </summary>
public class Altinn2RightsControllerTest : IClassFixture<CustomWebApplicationFactory<RightsInternalController>>
{
    private readonly CustomWebApplicationFactory<RightsInternalController> _factory;

    private readonly string sblInternalToken = PrincipalUtil.GetAccessToken("sbl.authorization");

    private readonly JsonSerializerOptions options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Constructor setting up factory, test client and dependencies
    /// </summary>
    /// <param name="factory">CustomWebApplicationFactory</param>
    public Altinn2RightsControllerTest(CustomWebApplicationFactory<RightsInternalController> factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Tests <see cref="RightsInternalController.GetOfferedRights(int, System.Threading.CancellationToken)"/>
    /// </summary>
    [Theory]
    [MemberData(nameof(GetGivenDelegations_ReturnOk_Input))]
    public async Task GetGivenDelegations_ReturnOk(string header, string value, Action<HttpResponseMessage> assert)
    {
        var client = NewDefaultClient(WithHeader(header, value));

        var response = await client.GetAsync($"internal/{GetUrlParameter(header, value)}/rights/delegation/offered");

        assert(response);
    }

    /// <summary>
    /// Case 1. List delegations from an organization to person using their orgnumber
    /// Case 2. List delegations from an organization to person using their partyid
    /// Case 3. List delegations from a person to org with no keyroles using their ssn
    /// Case 4. List delegations from a person to org with keyroles using their ssn
    /// Case 5. List delegations from a person to org with no keyroles using their partyid
    /// </summary>
    public static TheoryData<string, string, Action<HttpResponseMessage>> GetGivenDelegations_ReturnOk_Input() => new()
    {
        {
            string.Empty, "50005545", response => Assertions(
                AssertFromContains(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, "50005545"),
                AssertToContains(AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "20000095"),
                AssertStatusCode(HttpStatusCode.OK))
        },
        {
            string.Empty, "50002203",  Assertions(
                AssertFromContains(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, "50002203"),
                AssertToContains(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, "50005545"),
                AssertStatusCode(HttpStatusCode.OK))
        }
    };

    /// <summary>
    /// Tests <see cref="RightsInternalController.GetReceivedRights(int, System.Threading.CancellationToken)"/>
    /// </summary>
    [Theory]
    [MemberData(nameof(GetReceviedDelegations_ReturnOk_Input))]
    public async Task GetReceviedDelegations_ReturnOk(string header, string value, Action<HttpResponseMessage> assert)
    {
        var client = NewDefaultClient(WithHeader(header, value));

        var response = await client.GetAsync($"internal/{GetUrlParameter(header, value)}/rights/delegation/received");

        assert(response);
    }

    /// <summary>
    /// Case 1. List all delegations to an organization using orgnumber ""
    /// Case 2. List all delegations to an organization using party ""
    /// Case 3. List all delegations to a person "" with no keyroles using ssn
    /// Case 4. List all delegations to a person "" with keyroles using ssn
    /// Case 5. List all delegations to a person with no keyroles using partyid
    /// </summary>
    public static TheoryData<string, string, Action<HttpResponseMessage>> GetReceviedDelegations_ReturnOk_Input() => new()
    {
        {
            string.Empty, "50005545", Assertions(
                AssertFromContains(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, "50002203"),
                AssertToContains(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, "50005545"),
                AssertStatusCode(HttpStatusCode.OK))
        },
    };

    /// <summary>
    /// Tests <see cref="RightsInternalController.ClearAccessCache(int, BaseAttributeExternal, System.Threading.CancellationToken)"/>
    /// </summary>
    [Theory]
    [MemberData(nameof(ClearAccessCache_ReturnOk_input))]
    public async Task ClearAccessCache_ReturnOk(string authnUserToken, int party, BaseAttributeExternal toAttribute, Action<HttpResponseMessage> assert)
    {
        var client = NewClient(NewServiceCollection(WithServiceMoq), WithClientRoute("accessmanagement/api/v1/"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authnUserToken);

        HttpResponseMessage response = await client.PutAsync($"internal/{party}/accesscache/clear", new StringContent(JsonSerializer.Serialize(toAttribute), Encoding.UTF8, MediaTypeNames.Application.Json));

        assert(response);
    }

    /// <summary>
    /// Test case:  PUT internal/{party}/accesscache/clear
    ///             with the authenticated user being an authorized Administrator for the {party}
    /// Expected:   - Should return 200 OK
    /// Reason:     Authenticated users which authorized as Administrator/Main Administrator for the {party} should be allowed to clear access cache for recipient
    /// </summary>
    public static TheoryData<string, int, BaseAttributeExternal, Action<HttpResponseMessage>> ClearAccessCache_ReturnOk_input() => new()
    {
        {
            PrincipalUtil.GetToken(20000490, 50002598, 3), // Kasper B�rstad
            50002598, // From Kasper
            new BaseAttributeExternal { Type = Urn.Altinn.Person.Uuid, Value = "00000000-0000-0000-0005-000000003899" }, // To �rjan Ravn�s
            AssertStatusCode(HttpStatusCode.OK)
        },
        {
            PrincipalUtil.GetToken(20000490, 50002598, 3), // Kasper B�rstad
            50002598, // From Kasper
            new BaseAttributeExternal { Type = Urn.Altinn.Organization.Uuid, Value = "00000000-0000-0000-0005-000000004222" }, // To KARLSTAD OG ULOYBUKT
            AssertStatusCode(HttpStatusCode.OK)
        },
        {
            PrincipalUtil.GetToken(20000490, 50002598, 3), // Kasper B�rstad
            50005545, // From �rsta
            new BaseAttributeExternal { Type = Urn.Altinn.EnterpriseUser.Uuid, Value = "00000000-0000-0000-0002-000000010727" }, // To OrstaECUser
            AssertStatusCode(HttpStatusCode.OK)
        }
    };

    /// <summary>
    /// Tests <see cref="RightsInternalController.ClearAccessCache(int, BaseAttributeExternal, System.Threading.CancellationToken)"/>
    /// </summary>
    [Theory]
    [MemberData(nameof(ClearAccessCache_ReturnBadRequest_input))]
    public async Task ClearAccessCache_ReturnBadRequest(string authnUserToken, int party, BaseAttributeExternal toAttribute, Action<HttpResponseMessage> assert)
    {
        var client = NewClient(NewServiceCollection(WithServiceMoq), WithClientRoute("accessmanagement/api/v1/"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authnUserToken);

        HttpResponseMessage response = await client.PutAsync($"internal/{party}/accesscache/clear", new StringContent(JsonSerializer.Serialize(toAttribute), Encoding.UTF8, MediaTypeNames.Application.Json));

        assert(response);
    }

    /// <summary>
    /// Test case:  PUT internal/{party}/accesscache/clear
    ///             with the authenticated user being an Administrator for the {party}
    ///             where input attribute does not contain a well-formatted uuid
    /// Expected:   - Should return 400 BadRequest
    /// </summary>
    public static TheoryData<string, int, BaseAttributeExternal, Action<HttpResponseMessage>> ClearAccessCache_ReturnBadRequest_input() => new()
    {
        {
            PrincipalUtil.GetToken(20000490, 50002598, 3), // Kasper B�rstad
            50002598, // From Kasper
            new BaseAttributeExternal { Type = Urn.Altinn.Person.Uuid, Value = "asdf" }, // To not a well-formated uuid
            AssertStatusCode(HttpStatusCode.BadRequest)
        },
        {
            PrincipalUtil.GetToken(20000490, 50002598, 3), // Kasper B�rstad
            50005545, // From �rsta
            new BaseAttributeExternal { Type = Urn.Altinn.Organization.Uuid, Value = "123" }, // To not a well-formated uuid
            AssertStatusCode(HttpStatusCode.BadRequest)
        }
    };

    private static Action<HttpResponseMessage> Assertions(params Action<HttpResponseMessage>[] assertions) => response =>
    {
        foreach (var assertion in assertions)
        {
            assertion(response);
        }
    };

    private static Action<HttpResponseMessage> AssertStatusCode(HttpStatusCode expected) => response =>
    {
        Assert.Equal(expected, response.StatusCode);
    };

    private static Action<HttpResponseMessage> AssertToContains(string type, string value) => response =>
    {
        var content = response.Content.ReadAsStringAsync().Result;
        var models = JsonSerializer.Deserialize<IEnumerable<RightDelegationExternal>>(content);

        foreach (var model in models)
        {
            foreach (var attribute in model.To)
            {
                if (attribute.Id.Equals(type, StringComparison.InvariantCultureIgnoreCase) && attribute.Value.Equals(value))
                {
                    return;
                }
            }
        }

        Assert.Fail($"Failed to find any attributes in the field 'To' with type '{type}' and value '{value}'");
    };

    private static Action<HttpResponseMessage> AssertFromContains(string type, string value) => response =>
    {
        var content = response.Content.ReadAsStringAsync().Result;
        var models = JsonSerializer.Deserialize<IEnumerable<RightDelegationExternal>>(content);

        foreach (var model in models)
        {
            foreach (var attribute in model.From)
            {
                if (attribute.Id.Equals(type, StringComparison.InvariantCultureIgnoreCase) && attribute.Value.Equals(value))
                {
                    return;
                }
            }
        }

        Assert.Fail($"Failed to find any attributes in the field 'From' with type '{type}' and value '{value}'");
    };

    private WebApplicationFactory<RightsInternalController> NewServiceCollection(params Action<IServiceCollection>[] actions)
    {
        return _factory.WithWebHostBuilder(builder =>
       {
           builder.ConfigureTestServices(services =>
           {
               foreach (var action in actions)
               {
                   action(services);
               }
           });
       });
    }

    private HttpClient NewDefaultClient(params Action<HttpClient>[] actions) =>
        NewClient(NewServiceCollection(WithServiceMoq), [WithClientToken(), WithClientRoute("accessmanagement/api/v1/"), .. actions]);

    private static HttpClient NewClient(WebApplicationFactory<RightsInternalController> factory, params Action<HttpClient>[] actions)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        foreach (var action in actions)
        {
            action(client);
        }

        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }

    private static void WithServiceMoq(IServiceCollection services)
    {
        services.AddSingleton<IPolicyRetrievalPoint, PolicyRetrievalPointMock>();
        services.AddSingleton<IDelegationMetadataRepository, DelegationMetadataRepositoryMock>();
        services.AddSingleton<IPolicyFactory, PolicyFactoryMock>();
        services.AddSingleton<IPostConfigureOptions<JwtCookieOptions>, JwtCookiePostConfigureOptionsStub>();
        services.AddSingleton<IPublicSigningKeyProvider, SigningKeyResolverMock>();
        services.AddSingleton<IPartiesClient, PartiesClientMock>();
        services.AddSingleton<IProfileClient, ProfileClientMock>();
        services.AddSingleton<IResourceRegistryClient, ResourceRegistryClientMock>();
        services.AddSingleton<IAltinnRolesClient, AltinnRolesClientMock>();
        services.AddSingleton<IPDP, PdpPermitMock>();
        services.AddSingleton<IAltinn2RightsClient, Altinn2RightsClientMock>();
        services.AddSingleton<IDelegationChangeEventQueue>(new DelegationChangeEventQueueMock());
    }

    private static string GetUrlParameter(string header, object value) => header switch
    {
        IdentifierUtil.OrganizationNumberHeader => "organization",
        IdentifierUtil.PersonHeader => "person",
        _ => value.ToString(),
    };

    private static Action<HttpClient> WithHeader(string header, object value) => client =>
    {
        if (!string.IsNullOrEmpty(header))
        {
            client.DefaultRequestHeaders.Add(header, value.ToString());
        }
    };

    private static Action<HttpClient> WithClientRoute(string route) => client =>
    {
        client.BaseAddress = new Uri(client.BaseAddress, route);
    };

    private static void WithClientAcceptContentTypeJson(HttpClient client) =>
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

    private static Action<HttpClient> WithClientToken(int userId = 20001337, int partyId = 50002203, int authenticationLevel = 3) => client =>
    {
        var token = PrincipalUtil.GetToken(userId, partyId, authenticationLevel);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    };
}