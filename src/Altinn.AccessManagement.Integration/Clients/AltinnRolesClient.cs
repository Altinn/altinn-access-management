﻿using System.Text.Json;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Integration.Configuration;
using Authorization.Platform.Authorization.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.AccessManagement.Integration.Clients
{
    /// <summary>
    /// Client for getting Altinn roles from AltinnII SBL Bridge
    /// </summary>
    public class AltinnRolesClient : IAltinnRolesClient
    {
        private readonly SblBridgeSettings _sblBridgeSettings;
        private readonly HttpClient _client;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AltinnRolesClient"/> class
        /// </summary>
        /// <param name="httpClient">HttpClient from default httpclientfactory</param>
        /// <param name="sblBridgeSettings">the sbl bridge settings</param>
        /// <param name="logger">the logger</param>
        public AltinnRolesClient(HttpClient httpClient, IOptions<SblBridgeSettings> sblBridgeSettings, ILogger<AltinnRolesClient> logger)
        {
            _sblBridgeSettings = sblBridgeSettings.Value;
            _logger = logger;
            _client = httpClient;
        }

        /// <inheritdoc />
        public async Task<List<Role>> GetDecisionPointRolesForUser(int coveredByUserId, int offeredByPartyId)
        {
            try
            {
                UriBuilder uriBuilder = new UriBuilder($"{_sblBridgeSettings.BaseApiUrl}authorization/api/roles?coveredByUserId={coveredByUserId}&offeredByPartyId={offeredByPartyId}");

                HttpResponseMessage response = await _client.GetAsync(uriBuilder.Uri);
                string roleList = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonSerializer.Deserialize<List<Role>>(roleList);
                }

                _logger.LogError("AccessManagement // AltinnRolesClient // GetDecisionPointRolesForUser // Unexpected HttpStatusCode: {StatusCode}", response.StatusCode);
                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AccessManagement // AltinnRolesClient // GetDecisionPointRolesForUser // Exception");
                throw;
            }
        }
    }
}