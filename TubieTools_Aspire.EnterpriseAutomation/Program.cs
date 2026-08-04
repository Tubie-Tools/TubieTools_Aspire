using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TubieTools_Aspire.EnterpriseAutomation.Security;
using TubieTools_Aspire.EnterpriseAutomation.Health;
using TubieTools_Aspire.EnterpriseAutomation.KubernetesGlobal;
using TubieTools_Aspire.EnterpriseAutomation.ServiceNow;
using TubieTools_Aspire.EnterpriseAutomation.ServiceNow.Tools;
using TubieTools_Aspire.EnterpriseAutomation.Terraform;
using TubieTools_Aspire.EnterpriseAutomation.MCP;
using TubieTools_Aspire.EnterpriseAutomation.Azure;
using TubieTools_Aspire.EnterpriseAutomation.AzureDevOps;
using TubieTools_Aspire.EnterpriseAutomation.AIAgent;
using TubieTools_Aspire.EnterpriseAutomation.MultiTenant;
using TubieTools_Aspire.Security.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults
builder.AddServiceDefaults();

// Add Entra ID authentication and authorization
builder.AddEntraIdAuthentication();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HTTP context accessor for services that need request context
builder.Services.AddHttpContextAccessor();

// Register Enterprise Automation Services
builder.Services.AddScoped<IAzureAutomationService, AzureAutomationService>();
builder.Services.AddScoped<IAzureDevOpsService, AzureDevOpsService>();
builder.Services.AddScoped<IServiceNowService, ServiceNowService>();
builder.Services.AddScoped<ITerraformService, TerraformService>();
builder.Services.AddScoped<IKubernetesService, KubernetesService>();
builder.Services.AddScoped<ISecurityService, SecurityService>();
builder.Services.AddScoped<IMcpProtocolHandler, McpProtocolHandler>();

// Register ServiceNow Tools
builder.Services.AddScoped<ICreateIncidentTool, CreateIncidentTool>();
builder.Services.AddScoped<ISearchIncidentTool, SearchIncidentTool>();
builder.Services.AddScoped<ICloseIncidentTool, CloseIncidentTool>();
builder.Services.AddScoped<IServiceNowToolsFactory, ServiceNowToolsFactory>();

// Register AI Agent Services
var chatGPTConfig = new ChatGPTAgentConfig
{
    ApiKey = builder.Configuration["ChatGPT:ApiKey"] ?? "",
    Model = builder.Configuration["ChatGPT:Model"] ?? "gpt-4",
    Temperature = decimal.TryParse(builder.Configuration["ChatGPT:Temperature"], out var temp) ? temp : 0.7m,
    MaxTokens = int.TryParse(builder.Configuration["ChatGPT:MaxTokens"], out var tokens) ? tokens : 2000
};

builder.Services.AddSingleton(chatGPTConfig);
builder.Services.AddScoped<IMCPClient, MCPClient>();
builder.Services.AddScoped<IAIAgent, ChatGPTAgent>();
builder.Services.AddScoped<IAgentOrchestrator, AgentOrchestrator>();
builder.Services.AddHttpClient<IAIAgent, ChatGPTAgent>();

// Load tenant sample data from JSON
var tenantConfigurationPath = Path.Combine(builder.Environment.ContentRootPath, "MultiTenant", "sample-tenants.json");
if (File.Exists(tenantConfigurationPath))
{
    builder.Configuration.AddJsonFile("MultiTenant/sample-tenants.json", optional: false, reloadOnChange: true);
}

// Register tenant configuration options
// Around line 64-65
builder.Services.Configure<TenantConfigurationOptions>(
    builder.Configuration); // Bind from root with case-insensitive JSON property names

// Register Multi-Tenant Services - Updated to accept options
builder.Services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ISubscriptionManager, SubscriptionManager>();
builder.Services.AddScoped<IMultiTenantAIAgent, MultiTenantAIAgent>();

// Add HTTP clients for external services
builder.Services.AddHttpClient<ServiceNowService>();

// Health checks
builder.Services.AddHealthChecks()
    .AddCheck<AzureHealthCheck>("azure")
    .AddCheck<ServiceNowHealthCheck>("servicenow")
    .AddCheck<KubernetesHealthCheck>("kubernetes")
    .AddCheck<TubieTools_Aspire.Security.Health.EntraIdHealthCheck>("entra-id");

var app = builder.Build();

// Add tenant resolver middleware
app.UseTenantResolver();

// Add Entra ID authentication middleware
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseEntraIdAuthentication();

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