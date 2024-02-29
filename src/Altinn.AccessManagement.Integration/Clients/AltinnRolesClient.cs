using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Integration.Configuration;
using Authorization.Platform.Authorization.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.AccessManagement.Integration.Clients;

/// <summary>
/// Client for getting Altinn roles from AltinnII SBL Bridge
/// </summary>
[ExcludeFromCodeCoverage]
public class AltinnRolesClient : IAltinnRolesClient
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
    public AltinnRolesClient(HttpClient httpClient, IOptions<SblBridgeSettings> sblBridgeSettings, ILogger<AltinnRolesClient> logger)
    {
        _sblBridgeSettings = sblBridgeSettings.Value;
        _logger = logger;
        _client = httpClient;
    }

    /// <inheritdoc />
    public async Task<List<Role>> GetDecisionPointRolesForUser(int coveredByUserId, int offeredByPartyId, CancellationToken cancellationToken = default)
    {
        try
        {
            UriBuilder uriBuilder = new UriBuilder($"{_sblBridgeSettings.BaseApiUrl}authorization/api/roles?coveredByUserId={coveredByUserId}&offeredByPartyId={offeredByPartyId}");

            HttpResponseMessage response = await _client.GetAsync(uriBuilder.Uri, cancellationToken);
            string roleList = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonSerializer.Deserialize<List<Role>>(roleList, _serializerOptions);
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
    public async Task<List<Role>> GetRolesForDelegation(int coveredByUserId, int offeredByPartyId, CancellationToken cancellationToken = default)
    {
        try
        {
            UriBuilder uriBuilder = new UriBuilder($"{_sblBridgeSettings.BaseApiUrl}authorization/api/delegatableroles?coveredByUserId={coveredByUserId}&offeredByPartyId={offeredByPartyId}");

            HttpResponseMessage response = await _client.GetAsync(uriBuilder.Uri, cancellationToken);
            string roleList = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonSerializer.Deserialize<List<Role>>(roleList, _serializerOptions);
            }

            _logger.LogError("AccessManagement // AltinnRolesClient // GetRolesForDelegation // Unexpected HttpStatusCode: {StatusCode}", response.StatusCode);
            return new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AccessManagement // AltinnRolesClient // GetRolesForDelegation // Exception");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<AuthorizedParty>> GetAuthorizedPartiesWithRoles(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            UriBuilder uriBuilder = new UriBuilder($"{_sblBridgeSettings.BaseApiUrl}authorization/api/parties?userid={userId}");

            HttpResponseMessage response = await _client.GetAsync(uriBuilder.Uri, cancellationToken);
            string content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                List<SblAuthorizedParty> sblAuthorizedParties = JsonSerializer.Deserialize<List<SblAuthorizedParty>>(content, _serializerOptions);
                return sblAuthorizedParties.Select(sblAuthorizedParty => new AuthorizedParty(sblAuthorizedParty)).ToList();
            }

            _logger.LogError("AccessManagement // AltinnRolesClient // GetAuthorizedPartiesWithRoles // Unexpected HttpStatusCode: {StatusCode} Response: {Content}", response.StatusCode, content);
            return new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AccessManagement // AltinnRolesClient // GetAuthorizedPartiesWithRoles // Exception");
            throw;
        }
    }
}
