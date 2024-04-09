using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.AccessManagement.Integration.Configuration;
using Altinn.AccessManagement.Integration.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.AccessManagement.Integration.Clients;

/// <summary>
/// Client for getting Altinn roles from AltinnII SBL Bridge
/// </summary>
[ExcludeFromCodeCoverage]
public class Altinn2RightsClient : IAltinn2RightsClient
{
    private readonly SblBridgeSettings _sblBridgeSettings;
    private readonly HttpClient _client;
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    private readonly IPlatformAuthorizationTokenProvider _sblTokenProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="AltinnRolesClient"/> class
    /// </summary>
    /// <param name="httpClient">HttpClient from default httpclientfactory</param>
    /// <param name="sblBridgeSettings">the sbl bridge settings</param>
    /// <param name="logger">the logger</param>
    /// <param name="sblTokenProvider">instance of authorization platform token provider</param>
    public Altinn2RightsClient(HttpClient httpClient, IOptions<SblBridgeSettings> sblBridgeSettings, ILogger<AltinnRolesClient> logger, IPlatformAuthorizationTokenProvider sblTokenProvider)
    {
        _sblBridgeSettings = sblBridgeSettings.Value;
        _logger = logger;
        _client = httpClient;
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _serializerOptions.Converters.Add(new JsonStringEnumConverter());
        _sblTokenProvider = sblTokenProvider;
    }

    /// <inheritdoc />
    public async Task<DelegationCheckResponse> PostDelegationCheck(int authenticatedUserId, int reporteePartyId, string serviceCode, string serviceEditionCode)
    {
        UriBuilder uriBuilder = new UriBuilder($"{_sblBridgeSettings.BaseApiUrl}authorization/api/rights/delegation/userdelegationcheck?userId={authenticatedUserId}&partyId={reporteePartyId}&serviceCode={serviceCode}&serviceEditionCode={serviceEditionCode}");
            
        HttpResponseMessage response = await _client.GetAsync(uriBuilder.Uri);
        string responseContent = await response.Content.ReadAsStringAsync();

        DelegationCheckResponse delegationCheckResponse = new DelegationCheckResponse();

        if (response.StatusCode == HttpStatusCode.OK)
        {
            delegationCheckResponse.RightDelegationCheckResults = JsonSerializer.Deserialize<List<RightDelegationCheckResult>>(responseContent, _serializerOptions);
            return delegationCheckResponse;
        }

        _logger.LogError("AccessManagement // Altinn2RightsClient // PostDelegationCheck // Unexpected HttpStatusCode: {StatusCode}\n {responseContent}", response.StatusCode, responseContent);
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            SblValidationProblemResponse validationError = JsonSerializer.Deserialize<SblValidationProblemResponse>(responseContent, _serializerOptions);
            foreach (KeyValuePair<string, List<string>> modelState in validationError.ModelState)
            {
                delegationCheckResponse.Errors.Add(modelState.Key, string.Join(" | ", modelState.Value));
            }

            return delegationCheckResponse;
        }

        delegationCheckResponse.Errors.Add("SBLBridge", $"Unable to reach Altinn 2 for delegation check of Altinn 2 service. HttpStatusCode: {response.StatusCode}");
        return delegationCheckResponse;
    }

    /// <inheritdoc />
    public async Task<DelegationActionResult> PostDelegation(int authenticatedUserId, int reporteePartyId, SblRightDelegationRequest delegationRequest)
    {
        DelegationActionResult delegationResult = new DelegationActionResult();
        UriBuilder uriBuilder = new UriBuilder($"{_sblBridgeSettings.BaseApiUrl}authorization/api/rights/delegation/userdelegation?authenticatedUserId={authenticatedUserId}&partyId={reporteePartyId}");

        string token = await _sblTokenProvider.GetAccessToken();

        StringContent requestBody = new StringContent(JsonSerializer.Serialize(delegationRequest), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _client.PostAsync(token, uriBuilder.Uri.ToString(), requestBody);
        string responseContent = await response.Content.ReadAsStringAsync();

        if (response.StatusCode == HttpStatusCode.OK)
        {
            delegationResult.Rights = JsonSerializer.Deserialize<List<RightDelegationResult>>(responseContent, _serializerOptions);
            return delegationResult;
        }

        _logger.LogError("AccessManagement // Altinn2RightsClient // PostDelegation // Unexpected HttpStatusCode: {StatusCode}\n {responseContent}", response.StatusCode, responseContent);
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            SblValidationProblemResponse validationError = JsonSerializer.Deserialize<SblValidationProblemResponse>(responseContent, _serializerOptions);
            foreach (KeyValuePair<string, List<string>> modelState in validationError.ModelState)
            {
                delegationResult.Errors.Add(modelState.Key, string.Join(" | ", modelState.Value));
            }

            return delegationResult;
        }

        delegationResult.Errors.Add("SBLBridge", $"Unable to reach Altinn 2 for delegation of Altinn 2 service. HttpStatusCode: {response.StatusCode}");
        return delegationResult;
    }

    /// <inheritdoc />
    public async Task<HttpResponseMessage> ClearReporteeRights(int fromPartyId, int toPartyId, int toUserId = 0, CancellationToken cancellationToken = default)
    {
        UriBuilder endpoint = new UriBuilder($"{_sblBridgeSettings.BaseApiUrl}cache/api/clearreporteerights?reporteePartyId={fromPartyId}&coveredByPartyId={toPartyId}&coveredByUserId={toUserId}");
        return await _client.PutAsync(endpoint.Uri.ToString(), null, cancellationToken);
    }
}
