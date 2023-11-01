using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Integration.Configuration;
using Authorization.Platform.Authorization.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Rest.Azure;

namespace Altinn.AccessManagement.Integration.Clients
{
    /// <summary>
    /// Client for getting Altinn roles from AltinnII SBL Bridge
    /// </summary>
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
            _serializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        /// <inheritdoc />
        public async Task<DelegationCheckResponse> PostDelegationCheck(int userId, string partyId, string serviceCode, string serviceEditionCode)
        {
            try
            {
                UriBuilder uriBuilder = new UriBuilder($"{_sblBridgeSettings.BaseApiUrl}authorization/api/rights/delegation/userdelegationcheck?userId={userId}&partyId={partyId}&serviceCode={serviceCode}&serviceEditionCode={serviceEditionCode}");
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await _client.GetAsync(uriBuilder.Uri);
                string rights = await response.Content.ReadAsStringAsync();

                DelegationCheckResponse delegationCheckResponse = new DelegationCheckResponse();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    delegationCheckResponse.RightDelegationCheckResults = JsonSerializer.Deserialize<List<RightDelegationCheckResult>>(rights, _serializerOptions);
                    return delegationCheckResponse;
                }

                SBLUserDelegationCheckError validationError = JsonSerializer.Deserialize<SBLUserDelegationCheckError>(rights, _serializerOptions);
                if (validationError.ModelState == null)
                {
                    delegationCheckResponse.Errors.Add("SBLBridge", "Unexpected error from AccessManagement // Altinn2RightsClient // PostDelegationCheck");
                    return delegationCheckResponse;
                }

                delegationCheckResponse.Errors.Add(validationError.ModelState.First().Key, validationError.ModelState.First().Value.First());

                _logger.LogError("AccessManagement // Altinn2RightsClient // PostDelegationCheck // Unexpected HttpStatusCode: {StatusCode}", response.StatusCode);
                return delegationCheckResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AccessManagement // Altinn2RightsClient // PostDelegationCheck // Exception");
                throw;
            }
        }
    }
}
