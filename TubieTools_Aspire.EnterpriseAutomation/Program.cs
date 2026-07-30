using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TubieTools_Aspire.EnterpriseAutomation.Security;
using TubieTools_Aspire.EnterpriseAutomation.Health;
using TubieTools_Aspire.EnterpriseAutomation.KubernetesGlobal;
using TubieTools_Aspire.EnterpriseAutomation.ServiceNow;
using TubieTools_Aspire.EnterpriseAutomation.Terraform;
using TubieTools_Aspire.EnterpriseAutomation.MCP;
using TubieTools_Aspire.EnterpriseAutomation.Azure;
using TubieTools_Aspire.EnterpriseAutomation.AzureDevOps;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults
builder.AddServiceDefaults();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Enterprise Automation Services
builder.Services.AddScoped<IAzureAutomationService, AzureAutomationService>();
builder.Services.AddScoped<IAzureDevOpsService, AzureDevOpsService>();
builder.Services.AddScoped<IServiceNowService, ServiceNowService>();
builder.Services.AddScoped<ITerraformService, TerraformService>();
builder.Services.AddScoped<IKubernetesService, KubernetesService>();
builder.Services.AddScoped<ISecurityService, SecurityService>();
builder.Services.AddScoped<IMcpProtocolHandler, McpProtocolHandler>();

// Add HTTP clients for external services
builder.Services.AddHttpClient<ServiceNowService>();

// Health checks
builder.Services.AddHealthChecks()
    .AddCheck<AzureHealthCheck>("azure")
    .AddCheck<ServiceNowHealthCheck>("servicenow")
    .AddCheck<KubernetesHealthCheck>("kubernetes");

var app = builder.Build();

// Configure HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Map endpoints
app.MapHealthChecks("/health");
app.MapControllers();
app.MapDefaultEndpoints();

await app.RunAsync();