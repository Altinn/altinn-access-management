using Altinn.AuthorizationAdmin.Core.Services.Implementation;
using Altinn.AuthorizationAdmin.Integration.Configuration;
using Altinn.AuthorizationAdmin.Services;

ILogger logger;

var builder = WebApplication.CreateBuilder(args);

ConfigureSetupLogging();

// Add services to the container.
ConfigureServices(builder.Services, builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("http://localhost:3000");
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