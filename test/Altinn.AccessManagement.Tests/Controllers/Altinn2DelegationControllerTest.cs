using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using Altinn.AccessManagement.Controllers;
using Altinn.AccessManagement.Core.Asserters;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
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
/// summary
/// </summary>
public class Altinn2DelegationControllerTest : IClassFixture<CustomWebApplicationFactory<Altinn2DelegationController>>
{
    private readonly CustomWebApplicationFactory<Altinn2DelegationController> _factory;

    private readonly string sblInternalToken = PrincipalUtil.GetAccessToken("sbl.authorization");

    private readonly JsonSerializerOptions options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Constructor setting up factory, test client and dependencies
    /// </summary>
    /// <param name="factory">CustomWebApplicationFactory</param>
    public Altinn2DelegationControllerTest(CustomWebApplicationFactory<Altinn2DelegationController> factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Test case: Get all delegations offered from person 22093229405
    /// </summary>
    [Theory]
    [MemberData(nameof(GetGivenDelegations_ReturnOk_Input))]
    public async Task GetGivenDelegations_ReturnOk(string header, string value, Action<HttpResponseMessage> assert)
    {
        var client = NewDefaultClient(WithHeader(header, value));

        var response = await client.GetAsync($"{GetUrlParameter(header, value)}/delegations/given");

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
            IdentifierUtil.OrganizationNumberHeader, "910459880", Assertions(
                AssertFromContains(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, "50005545"),
                AssertToContains(AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "20000095"),
                AssertStatusCode(StatusCodes.Status200OK))
        },
        {
            string.Empty, "50005545", response => Assertions(
                AssertFromContains(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, "50005545"),
                AssertToContains(AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "20000095"),
                AssertStatusCode(StatusCodes.Status200OK))
        },
        {
            IdentifierUtil.PersonHeader, "02056260016", Assertions(
                AssertFromContains(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, "50002203"),
                AssertToContains(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, "50005545"),
                AssertStatusCode(StatusCodes.Status200OK))
        },
        {
            IdentifierUtil.PersonHeader, "02056260016", Assertions(
                AssertFromContains(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, "50002203"),
                AssertToContains(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, "50005545"),
                AssertStatusCode(StatusCodes.Status200OK))
        },
        {
            string.Empty, "50002203",  Assertions(
                AssertFromContains(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, "50002203"),
                AssertToContains(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, "50005545"),
                AssertStatusCode(StatusCodes.Status200OK))
        }
    };

    /// <summary>
    /// Test 
    /// </summary>
    [Theory]
    [MemberData(nameof(GetReceviedDelegations_ReturnOk_Input))]
    public async Task GetReceviedDelegations_ReturnOk(string header, string value, Action<HttpResponseMessage> assert)
    {
        var client = NewDefaultClient(WithHeader(header, value));

        var response = await client.GetAsync($"{GetUrlParameter(header, value)}/delegations/received");

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
            IdentifierUtil.OrganizationNumberHeader, "910459880", Assertions(
                AssertFromContains(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, "50002203"),
                AssertToContains(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, "50005545"),
                AssertStatusCode(StatusCodes.Status200OK))
        },
        {
            string.Empty, "50005545", Assertions(
                AssertFromContains(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, "50002203"),
                AssertToContains(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, "50005545"),
                AssertStatusCode(StatusCodes.Status200OK))
        },
    };

    private static Action<HttpResponseMessage> Assertions(params Action<HttpResponseMessage>[] assertions) => response =>
    {
        foreach (var assertion in assertions)
        {
            assertion(response);
        }
    };

    private static Action<HttpResponseMessage> AssertStatusCode(int statuscode) => response =>
    {
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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

    private WebApplicationFactory<Altinn2DelegationController> NewServiceCollection(params Action<IServiceCollection>[] actions)
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
        NewClient(NewServiceCollection(WithServiceMoq), [WithClientToken(), WithClientRoute("accessmanagement/api/v1/altinn2/"), .. actions]);

    private static HttpClient NewClient(WebApplicationFactory<Altinn2DelegationController> factory, params Action<HttpClient>[] actions)
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
        services.AddSingleton<IPolicyRepository, PolicyRepositoryMock>();
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