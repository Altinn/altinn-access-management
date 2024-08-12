using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models;
using Authorization.Platform.Authorization.Models;

namespace Altinn.AccessManagement.Tests.Mocks;

/// <summary>
/// Mock class for <see cref="IAltinnRolesClient"></see> interface
/// </summary>
public class AltinnRolesClientMock : IAltinnRolesClient
{
    private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Initializes a new instance of the <see cref="AltinnRolesClientMock"/> class
    /// </summary>
    public AltinnRolesClientMock()
    {
    }

    /// <inheritdoc/>
    public async Task<List<Role>> GetDecisionPointRolesForUser(int coveredByUserId, int offeredByPartyId, CancellationToken cancellationToken = default)
    {
        List<Role> roles = new List<Role>();
        string rolesPath = GetRolesPath(coveredByUserId, offeredByPartyId);
        if (File.Exists(rolesPath))
        {
            string content = await File.ReadAllTextAsync(rolesPath, cancellationToken);
            roles = (List<Role>)JsonSerializer.Deserialize(content, typeof(List<Role>), jsonOptions);
        }

        return roles;
    }

    /// <inheritdoc/>
    public async Task<List<Role>> GetRolesForDelegation(int coveredByUserId, int offeredByPartyId, CancellationToken cancellationToken = default)
    {
        List<Role> roles = new List<Role>();
        string rolesPath = GetRolesForDelegationPath(coveredByUserId, offeredByPartyId);
        if (File.Exists(rolesPath))
        {
            string content = await File.ReadAllTextAsync(rolesPath, cancellationToken);
            roles = (List<Role>)JsonSerializer.Deserialize(content, typeof(List<Role>), jsonOptions);
        }

        return roles;
    }

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedPartiesWithRoles(int userId, CancellationToken cancellationToken = default)
    {
        string authorizedPartiesPath = GetAltinn2AuthorizedPartiesWithRolesPath(userId);
        if (File.Exists(authorizedPartiesPath))
        {
            string content = await File.ReadAllTextAsync(authorizedPartiesPath, cancellationToken);
            List<SblAuthorizedParty> bridgeAuthParties = (List<SblAuthorizedParty>)JsonSerializer.Deserialize(content, typeof(List<SblAuthorizedParty>), jsonOptions);
            return bridgeAuthParties.Select(sblAuthorizedParty => new AuthorizedParty(sblAuthorizedParty)).ToList();
        }

        return new();
    }

    private static string GetRolesPath(int coveredByUserId, int offeredByPartyId)
    {
        string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(AltinnRolesClientMock).Assembly.Location).LocalPath);
        return Path.Combine(unitTestFolder, "..", "..", "..", "Data", "Roles", $"user_{coveredByUserId}", $"party_{offeredByPartyId}", "roles.json");
    }

    private static string GetRolesForDelegationPath(int coveredByUserId, int offeredByPartyId)
    {
        string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(AltinnRolesClientMock).Assembly.Location).LocalPath);
        return Path.Combine(unitTestFolder, "..", "..", "..", "Data", "RolesForDelegation", $"user_{coveredByUserId}", $"party_{offeredByPartyId}", "roles.json");
    }

    private static string GetAltinn2AuthorizedPartiesWithRolesPath(int userId)
    {
        string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(AltinnRolesClientMock).Assembly.Location).LocalPath);
        return Path.Combine(unitTestFolder, "..", "..", "..", "Data", "AuthorizedParties", "SBLBridge", $"authorizedparties_u{userId}.json");
    }
}
