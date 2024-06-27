using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Persistence.Configuration;
using Altinn.AccessManagement.Persistence.Extensions;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Altinn.AccessManagement.Persistence
{
    /// <summary>
    /// Storing Resource Rigistry metadata to Access management
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ResourceMetadataRepo : IResourceMetadataRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceMetadataRepo"/> class
        /// </summary>
        /// <param name="config">The postgreSQL configurations for AuthorizationDB</param>
        public ResourceMetadataRepo(IOptions<PostgreSQLSettings> config)
        {
            var bld = new NpgsqlConnectionStringBuilder(string.Format(config.Value.ConnectionString, config.Value.AuthorizationDbPwd));
            bld.AutoPrepareMinUsages = 2;
            bld.MaxAutoPrepare = 50;
            _connectionString = bld.ConnectionString;
        }

        /// <inheritdoc />
        public async Task<AccessManagementResource> InsertAccessManagementResource(AccessManagementResource resource, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

            var param = new Dictionary<string, object>();
            param.Add("resourceregistryid", resource.ResourceRegistryId);
            param.Add("resourcetype", resource.ResourceType.ToString().ToLower());

            string query =
            /*strpsql*/"""
            INSERT INTO accessmanagement.resource (resourceregistryid, resourcetype, created, modified)
            VALUES (@resourceregistryid, @resourcetype, now(), now())
            ON CONFLICT (resourceregistryid) DO UPDATE SET resourcetype = @resourcetype, modified = now()
            RETURNING resourceid, resourceregistryid, resourcetype, created, modified;
            """;

            try
            {
                await using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
                var res = await connection.QueryAsync<AccessManagementResource>(new CommandDefinition(query, param, cancellationToken: cancellationToken));
                return res.FirstOrDefault();
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
        }
    }
}
