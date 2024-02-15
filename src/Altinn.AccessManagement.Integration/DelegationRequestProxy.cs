using System.Text.Json;
using System.Web;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Integration.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.AccessManagement.Services
{
    /// <summary>
    /// Proxy implementation for delegation requests
    /// </summary>
    public class DelegationRequestProxy : IDelegationRequestsWrapper
    {
        private readonly SblBridgeSettings _sblBridgeSettings;
        private readonly ILogger _logger;
        private readonly HttpClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationRequestProxy"/> class
        /// </summary>
        /// <param name="httpClient">HttpClient from default httpclientfactory</param>
        /// <param name="sblBridgeSettings">the sbl bridge settings</param>
        /// <param name="logger">the logger</param>
        public DelegationRequestProxy(HttpClient httpClient, IOptions<SblBridgeSettings> sblBridgeSettings, ILogger<DelegationRequestProxy> logger)
        {
            _sblBridgeSettings = sblBridgeSettings.Value;
            _logger = logger;
            _client = httpClient;
        }

        /// <inheritdoc/>
        public async Task<DelegationRequests> GetDelegationRequestsAsync(string who, string serviceCode, int? serviceEditionCode, RestAuthorizationRequestDirection direction, List<RestAuthorizationRequestStatus> status, string continuation)
        {
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
                _logger.LogError("Getting delegationg requsts from bridge failed with {StatusCode}", response.StatusCode);
            }

            return null;
        }
    }
}
