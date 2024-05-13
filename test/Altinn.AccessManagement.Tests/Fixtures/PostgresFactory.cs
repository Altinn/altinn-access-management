using System;
using System.Threading;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Tests.Seeds;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit.Sdk;

namespace Altinn.AccessManagement.Tests.Fixtures;

/// <summary>
/// Postgres singleton that creates a npg sql server and creates a new database for each test 
/// </summary>
public static class PostgresFactory
{
    /// <summary>
    /// Database Password
    /// </summary>
    public static readonly string DbPassword = "Password";

    /// <summary>
    /// Database Username
    /// </summary>
    public static readonly string DbUserName = "platform_authorization";

    /// <summary>
    /// Database Admin
    /// </summary>
    public static readonly string DbAdminName = "platform_authorization_admin";

    /// <summary>
    /// Creates a new database and connection string
    /// </summary>
    public static async Task<PostgresServer> NewDbServer()
    {
        var database = new PostgreSqlBuilder()
                .WithCleanUp(true)
                .WithImage("docker.io/postgres:16.1-alpine")
                .Build();

        await database.StartAsync();

        var result = await database.ExecScriptAsync($@"
            CREATE USER {DbUserName} WITH PASSWORD '{DbPassword}';
            CREATE USER {DbAdminName} WITH PASSWORD '{DbPassword}';
            ALTER ROLE {DbUserName} LOGIN INHERIT;
            ALTER ROLE {DbAdminName} LOGIN SUPERUSER INHERIT;");

        if (result.ExitCode != 0)
        {
            throw new ArgumentException();
        }

        return new PostgresServer(database);
    }
}

/// <summary>
/// PostfgresServer
/// </summary>
public class PostgresServer(PostgreSqlContainer server) : IAsyncDisposable
{
    private PostgreSqlContainer Server { get; } = server;

    private int DatabaseNumber { get; set; } = 0;

    /// <summary>
    /// Creates a new postgres DB
    /// </summary>
    /// <exception cref="ArgumentException">if database couldn't be created</exception>
    public async Task<PostgresDatabase> CreateDb()
    {
        var dbname = $"test_{DatabaseNumber++}";
        var result = await Server.ExecScriptAsync($"CREATE DATABASE {dbname};");
        if (result.ExitCode != 0 || !string.IsNullOrEmpty(result.Stderr))
        {
            throw new XunitException($"Failed to create database {dbname}");
        }

        return new(dbname, Server.GetConnectionString());
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await Server.StopAsync();
    }
}

/// <summary>
/// Container for persisting connections string and database name
/// </summary>
/// <remarks>
/// ctor
/// </remarks>
public class PostgresDatabase(string dbname, string connectionString)
{
    /// <summary>
    /// Database name
    /// </summary>
    public string Dbname { get; } = dbname;

    /// <summary>
    /// Admin name
    /// </summary>
    public NpgsqlConnectionStringBuilder Admin { get; } = new NpgsqlConnectionStringBuilder(connectionString)
    {
        Database = dbname,
        Username = PostgresFactory.DbAdminName,
        Password = PostgresFactory.DbPassword,
    };

    /// <summary>
    /// User name
    /// </summary>
    public NpgsqlConnectionStringBuilder User { get; } = new NpgsqlConnectionStringBuilder(connectionString)
    {
        Database = dbname,
        Username = PostgresFactory.DbUserName,
        Password = PostgresFactory.DbPassword,
    };
}

/// <summary>
/// RepositoryContainer
/// </summary>
public class RepositoryContainer(IDelegationMetadataRepository delegationMetadataRepository, IResourceMetadataRepository resourceMetadataRepository)
{
    /// <summary>
    /// DelegationMetadataRepository
    /// </summary>
    public IDelegationMetadataRepository DelegationMetadataRepository { get; } = delegationMetadataRepository;

    /// <summary>
    /// ResourceMetadataRepository
    /// </summary>
    public IResourceMetadataRepository ResourceMetadataRepository { get; } = resourceMetadataRepository;

    /// <summary>
    /// Creates a new delegation
    /// </summary>
    public DelegationChange NewDelegationChange(params Action<DelegationChange>[] actions)
    {
        var delegation = new DelegationChange()
        {
            DelegationChangeType = DelegationChangeType.Grant
        };

        foreach (var action in actions)
        {
            action(delegation);
        }

        if (delegation.PerformedByUserId == null && delegation.CoveredByUserId != null)
        {
            delegation.PerformedByUserId = delegation.CoveredByUserId;
        }

        if (delegation.PerformedByPartyId == null && delegation.CoveredByPartyId != null)
        {
            delegation.PerformedByPartyId = delegation.CoveredByPartyId;
        }

        if (string.IsNullOrEmpty(delegation.BlobStoragePolicyPath))
        {
            delegation.BlobStoragePolicyPath = $"undefined";
        }

        if (string.IsNullOrEmpty(delegation.BlobStorageVersionId))
        {
            delegation.BlobStorageVersionId = "v1";
        }

        return delegation;
    }

    /// <summary>
    /// sets the field <see cref="DelegationChange.DelegationChangeType"/> to given delegation
    /// </summary>
    /// <param name="delegation">delegation</param>
    public void WithDelegationChangeRevokeLast(DelegationChange delegation)
    {
        delegation.DelegationChangeType = DelegationChangeType.RevokeLast;
    }

    /// <summary>
    /// Sets the field <see cref="DelegationChange.ResourceId"/> to given "resource"
    /// </summary>
    /// <param name="resource">resource</param>
    public Action<DelegationChange> WithResource(IAccessManagementResource resource) => delegation =>
    {
        delegation.ResourceId = resource.Resource.Identifier;
        delegation.ResourceType = resource.Resource.ResourceType.ToString();
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.CoveredByUserId"/> to given "profile"
    /// </summary>
    /// <param name="profile">user profile</param>
    /// <returns></returns>
    public Action<DelegationChange> WithToUser(IUserProfile profile) => delegation =>
    {
        delegation.CoveredByUserId = profile.UserProfile.UserId;
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.CoveredByUserId"/> to given "profile"
    /// </summary>
    /// <param name="userId">manual set user ID</param>
    /// <returns></returns>
    public Action<DelegationChange> WithToUser(int userId) => delegation =>
    {
        delegation.CoveredByUserId = userId;
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.CoveredByPartyId"/> to given party
    /// </summary>
    /// <param name="party">party</param>
    public Action<DelegationChange> WithToParty(IParty party) => delegation =>
    {
        delegation.CoveredByPartyId = party.Party.PartyId;
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.CoveredByPartyId"/> to given party
    /// </summary>
    /// <param name="partyId">manually sets Party ID</param>
    public Action<DelegationChange> WithToParty(int partyId) => delegation =>
    {
        delegation.CoveredByPartyId = partyId;
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.OfferedByPartyId"/> to given party 
    /// </summary>
    public Action<DelegationChange> WithFrom(IParty party) => delegation =>
    {
        delegation.OfferedByPartyId = party.Party.PartyId;
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.OfferedByPartyId"/> to given party 
    /// </summary>
    public Action<DelegationChange> WithFrom(int partyId) => delegation =>
    {
        delegation.OfferedByPartyId = partyId;
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.PerformedByUserId"/> to given userId
    /// </summary>
    /// <param name="profile">User profile</param>
    public Action<DelegationChange> WithPerformedByUserProfile(IUserProfile profile) => delegation =>
    {
        delegation.PerformedByUserId = profile.UserProfile.UserId;
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.PerformedByPartyId"/> to given party
    /// </summary>
    /// <param name="party">party</param>
    public Action<DelegationChange> WithPerformedByParty(IParty party) => delegation =>
    {
        delegation.PerformedByPartyId = party.Party.PartyId;
    };
}