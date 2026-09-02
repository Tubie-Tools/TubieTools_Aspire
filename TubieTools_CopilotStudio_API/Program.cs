using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using DataAccessLayer.Data.Contexts; 
using TubieTools_CopilotStudio_API.Services;
using DataAccessLayer.Data.Repositories;
using DataAccessLayer.Repositories;

// Initialize builder
var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "TubieTools_CopilotStudio_API"));

// Add services to DI container
builder.Services.AddControllers();
builder.Services.AddLogging();
builder.Services.AddEndpointsApiExplorer();

// Add OpenAPI
builder.Services.AddOpenApi();

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");

builder.Services.AddDbContext<CopilotStudioDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register repositories
builder.Services.AddScoped<ICopilotApplicationRepository, CopilotApplicationRepository>();
builder.Services.AddScoped<IKnowledgeToolRepository, KnowledgeToolRepository>();
builder.Services.AddScoped<IGovernancePolicyRepository, GovernancePolicyRepository>();
builder.Services.AddScoped<IPerformanceMetricsRepository, PerformanceMetricsRepository>();
builder.Services.AddScoped<IDeploymentConfigRepository, DeploymentConfigRepository>();
builder.Services.AddScoped<IVersionRepository, VersionRepository>();

// Register services
builder.Services.AddScoped<ICopilotApplicationService, CopilotApplicationService>();

// Build the application
var app = builder.Build();

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(config =>
    {
        config.SwaggerEndpoint("/openapi/v1.json", "Copilot Studio API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Apply migrations on startup
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<CopilotStudioDbContext>();
        dbContext.Database.Migrate();
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during migration");
    throw;
}

app.Run();
