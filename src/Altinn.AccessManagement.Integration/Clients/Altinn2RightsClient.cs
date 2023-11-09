using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.AccessManagement.Integration.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.AccessManagement.Integration.Clients
{
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

        /// <summary>
        /// Initializes a new instance of the <see cref="AltinnRolesClient"/> class
        /// </summary>
        /// <param name="httpClient">HttpClient from default httpclientfactory</param>
        /// <param name="sblBridgeSettings">the sbl bridge settings</param>
        /// <param name="logger">the logger</param>
        public Altinn2RightsClient(HttpClient httpClient, IOptions<SblBridgeSettings> sblBridgeSettings, ILogger<AltinnRolesClient> logger)
        {
            _sblBridgeSettings = sblBridgeSettings.Value;
            _logger = logger;
            _client = httpClient;
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _serializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        /// <inheritdoc />
        public async Task<DelegationCheckResponse> PostDelegationCheck(int userId, string partyId, string serviceCode, string serviceEditionCode)
        {
            UriBuilder uriBuilder = new UriBuilder($"{_sblBridgeSettings.BaseApiUrl}authorization/api/rights/delegation/userdelegationcheck?userId={userId}&partyId={partyId}&serviceCode={serviceCode}&serviceEditionCode={serviceEditionCode}");
            
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
                SblDelegationCheckError validationError = JsonSerializer.Deserialize<SblDelegationCheckError>(responseContent, _serializerOptions);
                foreach (KeyValuePair<string, List<string>> modelState in validationError.ModelState)
                {
                    delegationCheckResponse.Errors.Add(modelState.Key, string.Join(" | ", modelState.Value));
                }

                return delegationCheckResponse;
            }

            delegationCheckResponse.Errors.Add("SBLBridge", $"Unable to reach Altinn 2 for delegation check of Altinn 2 service. HttpStatusCode: {response.StatusCode}");
            return delegationCheckResponse;
        }
    }
}
