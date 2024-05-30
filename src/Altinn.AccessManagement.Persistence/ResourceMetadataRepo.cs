using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Persistence.Configuration;
using Altinn.AccessManagement.Persistence.Extensions;
using Dapper;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceMetadataRepo"/> class
        /// </summary>
        /// <param name="dbConnection">Database connection for AuthorizationDb</param>
        public ResourceMetadataRepo(NpgsqlDataSource dbConnection)
        {
            var bld = new NpgsqlConnectionStringBuilder(dbConnection.ConnectionString);
            bld.AutoPrepareMinUsages = 2;
            bld.MaxAutoPrepare = 50;
            _connection = new Npgsql.NpgsqlConnection(bld.ConnectionString);
        }

        /// <inheritdoc />
        public async Task<AccessManagementResource> InsertAccessManagementResource(AccessManagementResource resource, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

            var param = new Dictionary<string, object>();
            param.Add("resourceregistryid", resource.ResourceRegistryId);
            param.Add("resourcetype", resource.ResourceType.ToString().ToLower());

            string query = $"INSERT INTO accessmanagement.resource (resourceregistryid, resourcetype, created, modified)" +
                $"VALUES (@resourceregistryid, @resourcetype, now(), now())" +
                $"ON CONFLICT (resourceregistryid) DO UPDATE SET resourcetype = @resourcetype, modified = now();" +
                $"SELECT r.resourceid, r.resourceregistryid, r.resourcetype, r.created, r.modified" +
                $" FROM accessmanagement.resource r" +
                $" WHERE r.resourceregistryid = @resourceregistryid";

            try
            {
                _connection.Open();
                var res = await _connection.QueryAsync<AccessManagementResource>(new CommandDefinition(query, param, cancellationToken: cancellationToken));
                return res.FirstOrDefault();
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
            finally
            {
                _connection.Close();
            }
        }
    }
}
