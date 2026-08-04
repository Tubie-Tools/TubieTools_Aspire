using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.ServiceDiscovery;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using TubieTools_Aspire.Security.Authorization;
using TubieTools_Aspire.Security.Claims;
using TubieTools_Aspire.Security.Configuration;

namespace Microsoft.Extensions.Hosting;

// Adds common Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    //  add user service exteinsions here, for example:

    /// <summary>
    /// Add Entra ID (Azure AD) authentication and authorization services
    /// </summary>
    public static TBuilder AddEntraIdAuthentication<TBuilder>(this TBuilder builder) 
        where TBuilder : IHostApplicationBuilder
    {
        var config = builder.Configuration;
        var services = builder.Services;
        var logger = services.BuildServiceProvider().GetRequiredService<ILogger<object>>();

        // Load Entra ID configuration
        var entraIdOptions = new EntraIdOptions();
        var entraIdSection = config.GetSection(EntraIdOptions.SectionName);
        entraIdSection.Bind(entraIdOptions);

        // Validate required configuration
        if (string.IsNullOrEmpty(entraIdOptions.TenantId) || string.IsNullOrEmpty(entraIdOptions.ClientId))
        {
            logger.LogWarning("Entra ID configuration is incomplete. Please configure {SectionName} section in appsettings.json", 
                EntraIdOptions.SectionName);
        }

        // Build authority URLs
        entraIdOptions.Authority ??= $"https://login.microsoftonline.com/{entraIdOptions.TenantId}/v2.0";
        entraIdOptions.Scope ??= $"api://{entraIdOptions.ClientId}/.default";

        // Register configuration
        services.Configure<EntraIdOptions>(entraIdSection);
        services.AddSingleton(entraIdOptions);

        // Register security services
        services.AddScoped<IEntraIdClaimsTransformer, EntraIdClaimsTransformer>();
        services.AddScoped<IAuthorizationService, AuthorizationService>();

        // Add authentication
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = "MultiScheme";
            options.DefaultChallengeScheme = "MultiScheme";
        })
        .AddJwtBearer("Bearer", options =>
        {
            options.Authority = entraIdOptions.Authority;
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = entraIdOptions.TokenValidation.ValidateIssuer,
                ValidIssuer = entraIdOptions.TokenValidation.Issuer ?? entraIdOptions.Authority,
                ValidateAudience = entraIdOptions.TokenValidation.ValidateAudience,
                ValidAudience = entraIdOptions.TokenValidation.Audience ?? entraIdOptions.ClientId,
                ValidateLifetime = entraIdOptions.TokenValidation.ValidateLifetime,
                ClockSkew = TimeSpan.FromSeconds(entraIdOptions.TokenValidation.ClockSkewSeconds)
            };
            options.SaveToken = true;
        })
        .AddCookie("Interactive");

        // Add policy-based authorization
        services.AddAuthorization();

        logger.LogInformation("Entra ID authentication configured for tenant {TenantId}", entraIdOptions.TenantId);

        return builder;
    }

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        // Uncomment the following to restrict the allowed schemes for service discovery.
        builder.Services.Configure<ServiceDiscoveryOptions>(options =>
        {
            options.AllowedSchemes = ["https"];
        });

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation(tracing =>
                        // Exclude health check requests from tracing
                        tracing.Filter = context =>
                            !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                            && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath)
                    )
                    // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                    //.AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
        //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        //{
        //    builder.Services.AddOpenTelemetry()
        //       .UseAzureMonitor();
        //}

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    /// <summary>
    /// Add Entra ID health check to validate token endpoint accessibility
    /// </summary>
    public static TBuilder AddEntraIdHealthCheck<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            .AddCheck<TubieTools_Aspire.Security.Health.EntraIdHealthCheck>("entra-id");

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
        if (app.Environment.IsDevelopment())
        {
            // All health checks must pass for app to be considered ready to accept traffic after starting
            app.MapHealthChecks(HealthEndpointPath);

            // Only health checks tagged with the "live" tag must pass for app to be considered alive
            app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }

        return app;
    }
}
