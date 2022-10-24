﻿using System.Text.Json;
using System.Web;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Services;
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
        private readonly PlatformSettings _generalSettings;
        private readonly ILogger _logger;
        private readonly HttpClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationRequestProxy"/> class
        /// </summary>
        /// <param name="httpClient">HttpClient from default httpclientfactory</param>
        /// <param name="generalSettings">the general settings</param>
        /// <param name="logger">the logger</param>
        public DelegationRequestProxy(HttpClient httpClient, IOptions<PlatformSettings> generalSettings, ILogger<DelegationRequestProxy> logger)
        {
            _generalSettings = generalSettings.Value;
            _logger = logger;
            _client = httpClient;
        }

        /// <inheritdoc/>
        public async Task<DelegationRequests> GetDelegationRequestsAsync(string who, string? serviceCode, int? serviceEditionCode, RestAuthorizationRequestDirection direction, List<RestAuthorizationRequestStatus>? status, string? continuation)
        {
            UriBuilder uriBuilder = new UriBuilder($"{_generalSettings.BridgeApiEndpoint}api/DelegationRequests");
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