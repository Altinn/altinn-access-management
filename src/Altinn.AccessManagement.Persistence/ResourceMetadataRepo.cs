using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Altinn.AccessManagement.Persistence
{
    /// <summary>
    /// Storing Resource Rigistry metadata to Access management
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ResourceMetadataRepo : IResourceMetadataRepository
    {
        private readonly IDbConnection _connection;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceMetadataRepo"/> class
        /// </summary>
        /// <param name="dbConnection">Database connection for AuthorizationDb</param>
        /// <param name="logger">logger</param>
        public ResourceMetadataRepo(IDbConnection dbConnection, ILogger<ResourceMetadataRepo> logger) 
        {
            _connection = dbConnection;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<AccessManagementResource> InsertAccessManagementResource(AccessManagementResource resource, CancellationToken cancellationToken = default)
        {
            var param = new Dictionary<string, object>();
            param.Add("resourceregistryid", resource.ResourceRegistryId);
            param.Add("resourcetype", resource.ResourceType.ToString().ToLower());

            string query = $"INSERT INTO accessmanagement.resource (resourceregistryid, resourcetype, created, modified)" +
                $"VALUES (@resourceregistryid, @resourcetype, now(), now())" +
                $"ON CONFLICT (resourceregistryid) DO UPDATE SET resourcetype = @resourcetype, modified = now();" +
                $"SELECT r.resourceid, r.resourceregistryid, r.resourcetype, r.created, r.modified" +
                $"FROM accessmanagement.resource r" +
                $"WHERE r.resourceregistryid = @resourceregistryid";

            try
            {
                _connection.Open();
                var res = await _connection.QueryAsync<AccessManagementResource>(new CommandDefinition(query, param, cancellationToken: cancellationToken));
                return res.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authorization // DelegationMetadataRepository // GetCurrentAppDelegation // Exception");
                throw;
            }
            finally
            {
                _connection.Close();
            }
        }
    }
}
