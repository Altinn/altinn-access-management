using Aspire.Hosting;
using Aspire.Hosting.Azure;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var opt = new AzureAppConfigurationOptions();

builder.AddAzureAppConfiguration("", configureResource: dfd => { });


builder.AddProject<Projects.Altinn_AccessManagement>("api");

builder.Build().Run();
