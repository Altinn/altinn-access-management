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
using Xunit;

namespace Altinn.AccessManagement.Tests.Scenarios;

public abstract class AcceptanceCriteriaTest
{
    public AcceptanceCriteriaTest(string acceptanceCriteria, Action<AcceptanceCriteriaTest>[] parent, params Action<AcceptanceCriteriaTest>[] actions)
    {
        AcceptanceCriteria = acceptanceCriteria;
        foreach (var action in actions.Concat(parent))
        {
            action(this);
        }
    }

    public List<Scenario> Scenarios { get; set; } = [];

    public List<Action<HttpResponseMessage>> ResponseAssertions { get; set; } = [];

    public List<Action<WebApplicationFixture>> DbAssertions { get; set; } = [];

    public HttpRequestMessage Request { get; set; } = new();

    private string AcceptanceCriteria { get; }

    public void AssertResponse(HttpResponseMessage message)
    {
        foreach (var assert in ResponseAssertions)
        {
            assert(message);
        }
    }

    public void AssertApi(WebApplicationFixture fixture)
    {
        foreach (var assert in DbAssertions)
        {
            assert(fixture);
        }
    }

    public static Action<AcceptanceCriteriaTest> WithAssertResponseStatusCode(HttpStatusCode code) => test =>
    {
        test.ResponseAssertions.Add(response => Assert.Equal(code, response.StatusCode));
    };

    public static void WithAssertResponseStatusCodeSuccessful(AcceptanceCriteriaTest acceptanceCriteria)
    {
        acceptanceCriteria.ResponseAssertions.Add(response => Assert.True(response.IsSuccessStatusCode, $"expected successful status code, got status code {(int)response.StatusCode}: {response.StatusCode}"));
    }

    public static Action<AcceptanceCriteriaTest> WithScenarios(params Scenario[] scenarios) => test =>
    {
        test.Scenarios.AddRange(scenarios);
    };

    public static Action<AcceptanceCriteriaTest> WithRequestRoute(params object[] segments) => test =>
    {
        var url = string.Join("/", segments.Select(segment => segment.ToString().Trim('/')));
        test.Request.RequestUri = new Uri("/" + url);
    };

    public static Action<AcceptanceCriteriaTest> WithRequestVerb(HttpMethod method) => test =>
    {
        test.Request.Method = method;
    };

    public static Action<AcceptanceCriteriaTest> WithRequestBodyJson<T>(T body)
        where T : class => test =>
    {
        var content = JsonSerializer.Serialize(body);
        test.Request.Content = new StringContent(content, Encoding.UTF8, MediaTypeNames.Application.Json);
    };

    public async Task RunTests(WebApplicationFixture fixture)
    {
        AssertResponse(await fixture.UseScenarios([.. Scenarios]).SendAsync(Request));
        AssertApi(fixture);
    }

    /// <summary>
    /// Returns the Acceptance Criteria
    /// </summary>
    /// <returns></returns>
    public override sealed string ToString() => AcceptanceCriteria;
}