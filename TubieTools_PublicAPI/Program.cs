
using Microsoft.OpenApi;
using TubieTools_PublicAPI.Middleware;
using TubieTools_PublicAPI.Models;
using TubieTools_PublicAPI.Services;
using TubieTools_Aspire.Security.Extensions;
using Microsoft.Extensions.Hosting;

namespace TubieTools_PublicAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHttpContextAccessor();

            // Add Entra ID authentication (replaces Okta)
            builder.AddServiceDefaults();
            builder.AddEntraIdAuthentication();

            // Add distributed cache for token caching
            builder.Services.AddDistributedMemoryCache();
            //builder.Services.AddStackExchangeRedisCache(options => ...);// For token caching in production

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
                c.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "JWT Authorization header using the Bearer scheme."
                });
                c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("bearer", document)] = []
                });
            });   

            SetDependencyInjectedTenant(builder);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEntraIdAuthentication();


            app.MapControllers();

            app.Run();
        }

        private static void SetDependencyInjectedTenant(WebApplicationBuilder builder)
        {
            builder.Services.AddTransient<IServiceTenant, ServiceTenant>(p =>
            {
                var tehConfig = p.GetRequiredService<IConfiguration>();
                //var fulltenant = p.GetRequiredService<IConfiguration>()["Tenant"] ?? throw new InvalidOperationException("Tenant configuration is required.");
                var tenantId = p.GetRequiredService<IConfiguration>()["Tenant:TenantId"] ?? throw new InvalidOperationException("TenantId configuration is required.");
                var serviceName = p.GetRequiredService<IConfiguration>()["Tenant:ServiceName"] ?? throw new InvalidOperationException("ServiceName configuration is required.");

                // httpcontext user, null on splash, but should be populated on actual API calls, so we can log which user is making the call in the future if needed.
                var user = p.GetRequiredService<IHttpContextAccessor>()?.HttpContext?.User?.Identity?.Name ?? "UnknownUser";
                return new ServiceTenant(tenantId, serviceName);
            });
        }
    }
}
