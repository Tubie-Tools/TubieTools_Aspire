using MapApp.API.Data;
using MapApp.API.Mapping;
using MapApp.API.Services;
using MapApp.API.Services.TMS;
//using MapApp.API.Mapping;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MapApp.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File("logs/mapapp-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            builder.Host.UseSerilog();

            // Add services to the container
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Map Application API",
                    Version = "v1",
                    Description = "API for managing US state capitals, sales locations, and transportation routes"
                });
            });

            // Database
            builder.Services.AddDbContext<MapAppDbContext>(options =>
                options.UseInMemoryDatabase("MapAppDb"));

            // Services
            builder.Services.AddScoped<IRouteOptimizationService, RouteOptimizationService>();
            builder.Services.AddHttpClient<IOSRMService, OSRMService>();
            builder.Services.AddScoped<IOSRMService>(provider =>
                provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(OSRMService)) as IOSRMService
                ?? throw new InvalidOperationException("Failed to resolve OSRMService"));

            // TMS Services Registration (Schneider International)
            builder.Services.AddScoped<IRealtimeEventProcessor, RealtimeEventProcessor>();
            builder.Services.AddScoped<IBatchProcessingService, BatchProcessingService>();
            builder.Services.AddScoped<IJustInTimeService, JustInTimeService>();
            builder.Services.AddScoped<IFuelMetricsService, FuelMetricsService>();
            builder.Services.AddScoped<IBillingService, BillingService>();

            // HttpClientFactory
            builder.Services.AddHttpClient();

            // AutoMapper
            //builder.Services.AddAutoMapper(typeof(MappingProfile));

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // Initialize database with seed data
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<MapAppDbContext>();
                db.Database.EnsureCreated();
            }

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Map Application API v1");
                    options.RoutePrefix = string.Empty; // Swagger at root
                });
            }

            app.UseCors("AllowAll");
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            // Health check endpoint
            app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
                .WithName("Health")
                .WithOpenApi();

            // Root endpoint
            app.MapGet("/", () => Results.Redirect("/swagger"))
                .WithName("Root");

            try
            {
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}