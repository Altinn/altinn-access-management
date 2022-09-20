using Altinn.AuthorizationAdmin.Core.Services.Implementation;
using Altinn.AuthorizationAdmin.Integration.Configuration;
using Altinn.AuthorizationAdmin.Core.Helpers;
using Altinn.AuthorizationAdmin.Models;
using Altinn.AuthorizationAdmin.Services;
using Npgsql.Logging;
using Altinn.Platform.Authorization.Configuration;
using Yuniql.AspNetCore;
using Yuniql.PostgreSql;
using Altinn.AuthorizationAdmin.Services.Implementation;
using Altinn.AuthorizationAdmin.Services.Interface;
using Altinn.AuthorizationAdmin.Persistance;
using Altinn.AuthorizationAdmin.Core.Repositories.Interface;
using Altinn.AuthorizationAdmin.Core.Services;
using Altinn.AuthorizationAdmin.Core.Services.Interface;

ILogger logger;

var builder = WebApplication.CreateBuilder(args);

string frontendProdFolder = AppEnvironment.GetVariable("FRONTEND_PROD_FOLDER", "wwwroot/AuthorizationAdmin/");
builder.Configuration.AddJsonFile(frontendProdFolder + "manifest.json", optional: true, reloadOnChange: true);

ConfigureSetupLogging();
NpgsqlLogManager.Provider = new ConsoleLoggingProvider(NpgsqlLogLevel.Trace, true, true);

// Add services to the container.
ConfigureServices(builder.Services, builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://devenv.altinn.no");
        });
});


var app = builder.Build();
ConfigurePostgreSql();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();

app.Run();




void ConfigureServices(IServiceCollection services, IConfiguration config)
{
    logger.LogInformation("Startup // ConfigureServices");
    services.AddControllersWithViews();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();

    services.AddSwaggerGen();
    services.AddMvc();
    services.Configure<PlatformSettings>(config.GetSection("PlatformSettings"));
    services.AddHttpClient<IDelegationRequestsWrapper, DelegationRequestProxy>();
    services.AddTransient<IDelegationRequests, DelegationRequestService>();
    services.AddSingleton<IPolicyRetrievalPoint, PolicyRetrievalPoint>();
    services.AddSingleton<IPolicyInformationPoint, PolicyInformationPoint>();
    services.AddSingleton<IPolicyAdministrationPoint, PolicyAdministrationPoint>();
    services.AddSingleton<IPolicyRepository, PolicyRepository>();
    services.AddSingleton<IDelegationMetadataRepository, DelegationMetadataRepository>();
    services.AddSingleton<IDelegationChangeEventQueue, DelegationChangeEventQueue>();
    services.AddSingleton<IEventMapperService, EventMapperService>();
    services.AddOptions<FrontEndEntryPointOptions>()
        .BindConfiguration(FrontEndEntryPointOptions.SectionName);
}


void ConfigureSetupLogging()
{
    // Setup logging for the web host creation
    var logFactory = LoggerFactory.Create(builder =>
    {
        builder
            .AddFilter("Microsoft", LogLevel.Warning)
            .AddFilter("System", LogLevel.Warning)
            .AddFilter("Altinn.Platform.AuthorizationAdmin.Program", LogLevel.Debug)
            .AddConsole();
    });

    logger = logFactory.CreateLogger<Program>();
}

void ConfigurePostgreSql()
{
    if (builder.Configuration.GetValue<bool>("PostgreSQLSettings:EnableDBConnection"))
    {
        ConsoleTraceService traceService = new ConsoleTraceService { IsDebugEnabled = true };

        string connectionString = string.Format(
            builder.Configuration.GetValue<string>("PostgreSQLSettings:AdminConnectionString"),
            builder.Configuration.GetValue<string>("PostgreSQLSettings:authorizationDbAdminPwd"));

        app.UseYuniql(
            new PostgreSqlDataService(traceService),
            new PostgreSqlBulkImportService(traceService),
            traceService,
            new Yuniql.AspNetCore.Configuration
            {
                Workspace = Path.Combine(Environment.CurrentDirectory, builder.Configuration.GetValue<string>("PostgreSQLSettings:WorkspacePath")),
                ConnectionString = connectionString,
                IsAutoCreateDatabase = false,
                IsDebug = true,
            });
    }
}
