using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Altinn.AccessManagement.Tests.Fixtures;
using Altinn.AccessManagement.Tests.Scenarios;
using Xunit;

namespace Altinn.AccessManagement.Tests;

/// <summary>
/// Sets up tests and teardown tests for controller tests
/// </summary>
public abstract class AcceptanceCriteriaTest
{
    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="acceptanceCriteria">acceptance criteria</param>
    /// <param name="parent">list of functional object mutators provided to parent class</param>
    /// <param name="actions">list of functional object mutators provided from parent class</param>
    public AcceptanceCriteriaTest(string acceptanceCriteria, Action<AcceptanceCriteriaTest>[] parent, params Action<AcceptanceCriteriaTest>[] actions)
    {
        AcceptanceCriteria = acceptanceCriteria;
        foreach (var action in actions.Concat(parent))
        {
            action(this);
        }
    }

    /// <summary>
    /// List of Scenarios
    /// </summary>
    public List<Scenario> Scenarios { get; set; } = [];

    /// <summary>
    /// List of Assertions for HTTP assertions 
    /// </summary>
    public List<Func<HttpResponseMessage, Task>> ResponseAssertions { get; set; } = [];

    /// <summary>
    /// List of API assertions for mock context and DB
    /// </summary>
    public List<Func<WebApplicationFixture, Task>> ApiAssertions { get; set; } = [];

    /// <summary>
    /// Http request to be sent to the controller action
    /// </summary>
    public HttpRequestMessage Request { get; set; } = new();

    /// <summary>
    /// Acceptance criteria for the test
    /// </summary>
    protected string AcceptanceCriteria { get; }

    /// <summary>
    /// Asserts response given from API
    /// </summary>
    public void AssertResponse(HttpResponseMessage message)
    {
        foreach (var assert in ResponseAssertions)
        {
            assert(message);
        }
    }

    /// <summary>
    /// Asserts mock call and DB
    /// </summary>
    /// <param name="fixture">Web application fixture</param>
    public void AssertApi(WebApplicationFixture fixture)
    {
        Task.WaitAll([.. ApiAssertions.Select(async assertion => await assertion(fixture))]);
    }

    /// <summary>
    /// Asserts that response has given status code.
    /// </summary>
    /// <param name="code">HTTP status code</param>
    /// <returns></returns>
    public static Action<AcceptanceCriteriaTest> WithAssertResponseStatusCode(HttpStatusCode code) => test =>
    {
        test.ResponseAssertions.Add(response =>
        {
            Assert.Equal(code, response.StatusCode);
            return Task.CompletedTask;
        });
    };

    /// <summary>
    /// Asserts that response given from API is a successful status code.
    /// </summary>
    public static void WithAssertResponseStatusCodeSuccessful(AcceptanceCriteriaTest test)
    {
        test.ResponseAssertions.Add(response =>
        {
            Assert.True(response.IsSuccessStatusCode, $"expected successful status code, got status code {(int)response.StatusCode}: {response.StatusCode}");
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Seeds the DB and creates the mock context for the integrations.
    /// </summary>
    /// <param name="scenarios">list of scenarions</param>
    /// <returns></returns>
    public static Action<AcceptanceCriteriaTest> WithScenarios(params Scenario[] scenarios) => test =>
    {
        test.Scenarios.AddRange(scenarios);
    };

    /// <summary>
    /// Http request route
    /// </summary>
    /// <param name="segments">list of URL segments</param>
    public static Action<AcceptanceCriteriaTest> WithRequestRoute(params object[] segments) => test =>
    {
        var url = string.Join("/", segments.Select(segment => segment.ToString().Trim('/')));
        test.Request.RequestUri = new Uri("/" + url);
    };

    /// <summary>
    /// Sets the HTTP request method
    /// </summary>
    public static Action<AcceptanceCriteriaTest> WithRequestVerb(HttpMethod method) => test =>
    {
        test.Request.Method = method;
    };

    /// <summary>
    /// Deserialize the paylaod and sends the content as JSON and adds the 
    /// content type header 'application/json'
    /// </summary>
    /// <param name="body">object to deserialize</param>
    public static Action<AcceptanceCriteriaTest> WithHttpRequestBodyJson<T>(T body)
        where T : class => test =>
    {
        var content = JsonSerializer.Serialize(body);
        test.Request.Content = new StringContent(content, Encoding.UTF8, MediaTypeNames.Application.Json);
    };

    /// <summary>
    /// Runs tests
    /// </summary>
    /// <param name="fixture">web application fixture</param>
    public async Task Test(WebApplicationFixture fixture)
    {
        var api = await fixture.UseScenarios([.. Scenarios]);
        AssertResponse(await api.SendAsync(Request));
        AssertApi(fixture);
    }

    /// <summary>
    /// Return Acceptance Criteria
    /// </summary>
    public new string ToString()
    {
        return AcceptanceCriteria;
    }
}
