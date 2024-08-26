using System.Collections.Concurrent;
using System.Reflection;
using Altinn.AccessManagement.Configuration;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Persistence;
using Altinn.AccessManagement.Persistence.Configuration;
using Altinn.AccessManagement.Tests.Seeds;
using Microsoft.Extensions.Options;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit.Sdk;
using Yuniql.Core;

namespace Altinn.AccessManagement.Tests.Fixtures;

/// <summary>
/// For running exclusively database tests. Before each tests, method <see cref="New"/> must be called in order
/// to create a new database that runs isolated for that tests. This should be used as an <see cref="IClassFixture{PostgresFixture}"/>
/// </summary>
public class PostgresFixture : IAsyncLifetime
{
    private ConsoleTraceService Tracer { get; } = new()
    {
        IsDebugEnabled = true
    };

    /// <summary>
    /// Creates a new database and runs the migrations
    /// </summary>
    /// <returns></returns>
    public PostgresDatabase New()
    {
        var db = PostgresServer.NewDatabase();
        var configuration = new Yuniql.AspNetCore.Configuration
        {
            Platform = SUPPORTED_DATABASES.POSTGRESQL,
            Workspace = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Migration"),
            ConnectionString = db.Admin.ToString(),
            IsAutoCreateDatabase = false,
        };

        var dataService = new Yuniql.PostgreSql.PostgreSqlDataService(Tracer);
        var bulkImportService = new Yuniql.PostgreSql.PostgreSqlBulkImportService(Tracer);
        var migrationServiceFactory = new MigrationServiceFactory(Tracer);
        var migrationService = migrationServiceFactory.Create(dataService, bulkImportService);
        ConfigurationHelper.Initialize(configuration);
        migrationService.Run();
        return db;
    }

    /// <inheritdoc/>
    public Task DisposeAsync()
    {
        PostgresServer.StopUsing(this);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task InitializeAsync()
    {
        PostgresServer.StartUsing(this);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Postgres singleton that creates a npg sql server and creates a new database for each test 
/// </summary>
public static class PostgresServer
{
    private static PostgreSqlContainer Server { get; } = new PostgreSqlBuilder()
        .WithCleanUp(true)
        .WithImage("docker.io/postgres:16.1-alpine")
        .Build();

    private static Mutex Mutex { get; } = new();

    private static ConcurrentDictionary<object, int> Consumers { get; } = new();

    private static int DatabaseInstance { get; set; } = 0;

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
    /// Must be called before getting creating databases
    /// </summary>
    /// <param name="consumer">this</param>
    public static void StartUsing(object consumer)
    {
        Mutex.WaitOne();
        try
        {
            Consumers.AddOrUpdate(consumer, 1, (consumer, current) =>
            {
                return ++current;
            });

            if (Consumers.Values.Sum() == 1)
            {
                Server.StartAsync().Wait();
                var result = Server.ExecScriptAsync($@"
                DO
                $$
                BEGIN
                	IF NOT EXISTS (SELECT * FROM pg_user WHERE usename IN ('{DbUserName}', '{DbAdminName}')) THEN
                		CREATE USER {DbUserName} WITH PASSWORD '{DbPassword}';
                		CREATE USER {DbAdminName} WITH PASSWORD '{DbPassword}';
                		ALTER ROLE {DbUserName} LOGIN INHERIT;
                		ALTER ROLE {DbAdminName} LOGIN SUPERUSER INHERIT;
                	END IF;
                END
                $$;").Result;

                if (result.ExitCode != 0 || !string.IsNullOrEmpty(result.Stderr))
                {
                    throw new XunitException($"Failed to create users. Exitcode {result.ExitCode}, Error Message ${result.Stderr}");
                }
            }
        }
        finally
        {
            Mutex.ReleaseMutex();
        }
    }

    /// <summary>
    /// Should be called after tests are executed
    /// </summary>
    public static void StopUsing(object consumer)
    {
        Mutex.WaitOne();
        try
        {
            Consumers.AddOrUpdate(consumer, 1, (consumer, current) =>
            {
                if (current > 0)
                {
                    return --current;
                }

                return 0;
            });

            if (Consumers.Values.Sum() == 0)
            {
                Server.StopAsync().Wait();
            }
        }
        finally
        {
            Mutex.ReleaseMutex();
        }
    }

    /// <summary>
    /// Creates a new database and connection string
    /// </summary>
    public static PostgresDatabase NewDatabase()
    {
        Mutex.WaitOne();
        try
        {
            var dbname = $"test_{DatabaseInstance++}";
            Server.ExecScriptAsync($"CREATE DATABASE {dbname};").Wait();
            return new(dbname, Server.GetConnectionString());
        }
        finally
        {
            Mutex.ReleaseMutex();
        }
    }
}

/// <summary>
/// Container for persisting connections string and database name
/// </summary>
public class PostgresDatabase(string dbname, string connectionString) : IOptions<PostgreSQLSettings>
{
    /// <summary>
    /// Database name
    /// </summary>
    public string Dbname { get; } = dbname;

    /// <summary>
    /// Creates <see cref="DelegationMetadataRepository"/>
    /// </summary>
    public IDelegationMetadataRepository DelegationMetadata =>
        new DelegationMetadataRepository(new NpgsqlDataSourceBuilder(User.ToString()).Build());

    /// <summary>
    /// Creates <see cref="ResourceMetadata"/>
    /// </summary>
    public IResourceMetadataRepository ResourceMetadata => new ResourceMetadataRepository(this);

    /// <summary>
    /// Admin name
    /// </summary>
    public NpgsqlConnectionStringBuilder Admin { get; } = new NpgsqlConnectionStringBuilder(connectionString)
    {
        Timeout = 3,
        Database = dbname,
        Username = PostgresServer.DbAdminName,
        Password = PostgresServer.DbPassword,
        IncludeErrorDetail = true,
    };

    /// <summary>
    /// User name
    /// </summary>
    public NpgsqlConnectionStringBuilder User { get; } = new NpgsqlConnectionStringBuilder(connectionString)
    {
        Timeout = 3,
        Database = dbname,
        Username = PostgresServer.DbUserName,
        Password = PostgresServer.DbPassword,
        IncludeErrorDetail = true,
    };

    /// <summary>
    /// Implements <see cref="IOptions{PostgreSQLSettings}"/> 
    /// </summary>
    public PostgreSQLSettings Value => new()
    {
        ConnectionString = User.ToString(),
        AuthorizationDbPwd = User.Password
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
}

/// <summary>
/// Build a Delegation Change
/// </summary>
public static class DelegationChangeComposer
{
    /// <summary>
    /// Creates a new delegation
    /// </summary>
    public static DelegationChange New(params Action<DelegationChange>[] actions)
    {
        var delegation = new DelegationChange()
        {
            DelegationChangeType = DelegationChangeType.Grant
        };

        foreach (var action in actions)
        {
            action(delegation);
        }

        if (delegation.PerformedByUserId == null)
        {
            delegation.PerformedByUserId = delegation.CoveredByUserId == null ? 1 : PersonSeeds.Paula.PartyId;
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
    public static void WithDelegationChangeRevokeLast(DelegationChange delegation)
    {
        delegation.DelegationChangeType = DelegationChangeType.RevokeLast;
    }

    /// <summary>
    /// Sets the field <see cref="DelegationChange.ResourceId"/> to given "resource"
    /// </summary>
    /// <param name="resource">resource</param>
    public static Action<DelegationChange> WithResource(IAccessManagementResource resource) => delegation =>
    {
        if (resource.Resource.ResourceType == ResourceType.AltinnApp)
        {
            var org = resource.Resource.AuthorizationReference.FirstOrDefault(attr => attr.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute);
            var app = resource.Resource.AuthorizationReference.FirstOrDefault(attr => attr.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute);
            delegation.ResourceId = $"{org.Value}/{app.Value}";
        }
        else
        {
            delegation.ResourceId = resource.Resource.Identifier;
        }

        delegation.ResourceType = resource.Resource.ResourceType.ToString();
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.CoveredByUserId"/> to given "profile"
    /// </summary>
    /// <param name="profile">user profile</param>
    /// <returns></returns>
    public static Action<DelegationChange> WithToUser(IUserProfile profile) => delegation =>
    {
        delegation.CoveredByUserId = profile.UserProfile.UserId;
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.CoveredByUserId"/> to given "profile"
    /// </summary>
    /// <param name="userId">manual set user ID</param>
    /// <returns></returns>
    public static Action<DelegationChange> WithToUser(int userId) => delegation =>
    {
        delegation.CoveredByUserId = userId;
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.CoveredByPartyId"/> to given party
    /// </summary>
    /// <param name="party">party</param>
    public static Action<DelegationChange> WithToParty(IParty party) => delegation =>
    {
        delegation.CoveredByPartyId = party.Party.PartyId;
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.CoveredByPartyId"/> to given party
    /// </summary>
    /// <param name="partyId">manually sets Party ID</param>
    public static Action<DelegationChange> WithToParty(int partyId) => delegation =>
    {
        delegation.CoveredByPartyId = partyId;
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.OfferedByPartyId"/> to given party 
    /// </summary>
    public static Action<DelegationChange> WithFrom(IParty party) => delegation =>
    {
        delegation.OfferedByPartyId = party.Party.PartyId;
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.OfferedByPartyId"/> to given party 
    /// </summary>
    public static Action<DelegationChange> WithFrom(int partyId) => delegation =>
    {
        delegation.OfferedByPartyId = partyId;
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.PerformedByUserId"/> to given userId
    /// </summary>
    /// <param name="profile">User profile</param>
    public static Action<DelegationChange> WithPerformedByUserProfile(IUserProfile profile) => delegation =>
    {
        delegation.PerformedByUserId = profile.UserProfile.UserId;
    };

    /// <summary>
    /// Sets the field <see cref="DelegationChange.PerformedByPartyId"/> to given party
    /// </summary>
    /// <param name="party">party</param>
    public static Action<DelegationChange> WithPerformedByParty(IParty party) => delegation =>
    {
        delegation.PerformedByPartyId = party.Party.PartyId;
    };
}