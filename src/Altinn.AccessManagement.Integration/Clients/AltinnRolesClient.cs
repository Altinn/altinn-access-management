using System.Text.Json;
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

        /// <inheritdoc />
        public async Task<List<Role>> GetRolesForDelegation(int coveredByUserId, int offeredByPartyId)
        {
            try
            {
                return JsonSerializer.Deserialize<List<Role>>("[{\"Type\": \"http://schemas.altinn.no/rest/2019/05/identity/claims/role\", \"Value\": \"LOPER\"}, {\"Type\": \"http://schemas.altinn.no/rest/2019/05/identity/claims/role\", \"Value\": \"ADMAI\"}, {\"Type\": \"http://schemas.altinn.no/rest/2019/05/identity/claims/role\", \"Value\": \"REGNA\"}, {\"Type\": \"http://schemas.altinn.no/rest/2019/05/identity/claims/role\", \"Value\": \"SISKD\"}, {\"Type\": \"http://schemas.altinn.no/rest/2019/05/identity/claims/role\", \"Value\": \"UILUF\"}, {\"Type\": \"http://schemas.altinn.no/rest/2019/05/identity/claims/role\", \"Value\": \"UTINN\"}, {\"Type\": \"http://schemas.altinn.no/rest/2019/05/identity/claims/role\", \"Value\": \"UTOMR\"}, {\"Type\": \"http://schemas.altinn.no/rest/2019/05/identity/claims/role\", \"Value\": \"KLADM\"}, {\"Type\": \"http://schemas.altinn.no/rest/2019/05/identity/claims/role\", \"Value\": \"ATTST\"}, {\"Type\": \"http://schemas.altinn.no/rest/2019/05/identity/claims/role\", \"Value\": \"HVASK\"}, {\"Type\": \"http://schemas.altinn.no/rest/2019/05/identity/claims/role\", \"Value\": \"PAVAD\"}, {\"Type\": \"http://schemas.altinn.no/rest/2019/05/identity/claims/role\", \"Value\": \"SIGNE\"}, {\"Type\": \"http://schemas.altinn.no/rest/2019/05/identity/claims/role\", \"Value\": \"UIHTL\"}, {\"Type\": \"http://schemas.altinn.no/rest/2019/05/identity/claims/role\", \"Value\": \"KOMAB\"}, {\"Type\": \"http://schemas.altinn.no/rest/2019/05/identity/claims/role\", \"Value\": \"LEDE\"}, {\"Type\": \"http://schemas.altinn.no/rest/2019/05/identity/claims/role\", \"Value\": \"DAGL\"}, {\"Type\": \"http://schemas.altinn.no/rest/2019/05/identity/claims/role\", \"Value\": \"ECKEYROLE\"}, {\"Type\": \"http://schemas.altinn.no/rest/2019/05/identity/claims/role\", \"Value\": \"HADM\"}, {\"Type\": \"http://schemas.altinn.no/rest/2019/05/identity/claims/role\", \"Value\": \"PASIG\"}, {\"Type\": \"http://schemas.altinn.no/rest/2019/05/identity/claims/role\", \"Value\": \"A0278\"}, {\"Type\": \"http://schemas.altinn.no/rest/2019/05/identity/claims/role\", \"Value\": \"A0236\"}, {\"Type\": \"http://schemas.altinn.no/rest/2019/05/identity/claims/role\", \"Value\": \"A0212\"}, {\"Type\": \"http://schemas.altinn.no/rest/2019/05/identity/claims/role\", \"Value\": \"A0293\"}, {\"Type\": \"http://schemas.altinn.no/rest/2019/05/identity/claims/role\", \"Value\": \"A0294\"}, {\"Type\": \"http://schemas.altinn.no/rest/2019/05/identity/claims/role\", \"Value\": \"EKTJ\"}, {\"Type\": \"http://schemas.altinn.no/rest/2019/05/identity/claims/role\", \"Value\": \"A0286\"}]");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AccessManagement // AltinnRolesClient // GetRolesForDelegation // Exception");
                throw;
            }
        }
    }
}
