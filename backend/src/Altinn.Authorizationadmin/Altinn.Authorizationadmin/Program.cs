using Altinn.AuthorizationAdmin.Core.Services.Implementation;
using Altinn.AuthorizationAdmin.Integration.Configuration;
using Altinn.AuthorizationAdmin.Core.Helpers;
using Altinn.AuthorizationAdmin.Models;
using Altinn.AuthorizationAdmin.Services;
using Altinn.AuthorizationAdmin.Core.Services;

ILogger logger;

var builder = WebApplication.CreateBuilder(args);

string frontendProdFolder = AppEnvironment.GetVariable("FRONTEND_PROD_FOLDER", "wwwroot/AuthorizationAdmin/");
builder.Configuration.AddJsonFile(frontendProdFolder + "manifest.json", optional: true, reloadOnChange: true);

ConfigureSetupLogging();

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
