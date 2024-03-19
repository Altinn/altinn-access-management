using System.Text.Json;
using System.Web;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Core.Telemetry;
using Altinn.AccessManagement.Integration.Configuration;
using Microsoft.Extensions.Options;

namespace Altinn.AccessManagement.Services
{
    /// <summary>
    /// Proxy implementation for delegation requests
    /// </summary>
    public class DelegationRequestProxy : IDelegationRequestsWrapper
    {
        private readonly SblBridgeSettings _sblBridgeSettings;
        private readonly HttpClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationRequestProxy"/> class
        /// </summary>
        /// <param name="httpClient">HttpClient from default httpclientfactory</param>
        /// <param name="sblBridgeSettings">the sbl bridge settings</param>
        public DelegationRequestProxy(HttpClient httpClient, IOptions<SblBridgeSettings> sblBridgeSettings)
        {
            _sblBridgeSettings = sblBridgeSettings.Value;
            _client = httpClient;
        }

        /// <inheritdoc/>
        public async Task<DelegationRequests> GetDelegationRequestsAsync(string who, string serviceCode, int? serviceEditionCode, RestAuthorizationRequestDirection direction, List<RestAuthorizationRequestStatus> status, string continuation)
        {
            using var activity = TelemetryConfig._activitySource.StartActivity();

            UriBuilder uriBuilder = new UriBuilder($"{_sblBridgeSettings.BaseApiUrl}authorization/api/DelegationRequests");
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["who"] = who;
            if (!string.IsNullOrEmpty(serviceCode))
            {
                query["serviceCode"] = serviceCode;
            }

            if (serviceEditionCode != null)
            {
                query["serviceEditionCode"] = serviceEditionCode.ToString();
            }

            query["direction"] = direction.ToString();

            if (status != null)
            {
                foreach (var statusItem in status)
                {
                    query.Add("status", statusItem.ToString());
                }
            }

            if (!string.IsNullOrEmpty(continuation))
            {
                query["continuation"] = continuation;
            }

            uriBuilder.Query = query.ToString();

            HttpResponseMessage response = await _client.GetAsync(uriBuilder.Uri);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return await JsonSerializer.DeserializeAsync<DelegationRequests>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }
            else
            {
                activity?.StopWithError(TelemetryEvents.UnexpectedHttpStatusCode(response));
                
                // Review: Original: _logger.LogError("Getting delegationg requsts from bridge failed with {StatusCode}", response.StatusCode); Fix: SBLBride.RequestFailed
            }

            return null;
        }
    }
}
