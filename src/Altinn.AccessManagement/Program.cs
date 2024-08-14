using Altinn.AccessManagement;
using Microsoft.IdentityModel.Logging;

WebApplication app = AccessManagementHost.Create(args);

app.AddDefaultAltinnMiddleware(errorHandlingPath: "/accessmanagement/api/v1/error");

if (app.Environment.IsDevelopment())
{
    // Enable higher level of detail in exceptions related to JWT validation
    IdentityModelEventSource.ShowPII = true;

    // Enable Swagger
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultAltinnEndpoints();
app.MapControllers();

await app.RunAsync();

/// <summary>
/// Startup class.
/// </summary>
public sealed partial class Program
{
    private Program()
    {
    }
}