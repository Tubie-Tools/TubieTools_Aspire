namespace TubieTools_Aspire.EnterpriseAutomation.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using TubieTools_Aspire.EnterpriseAutomation.MultiTenant.Jurisdiction;
using System.Text.Json;
using System.IO;

public static class JurisdictionServiceExtensions
{
    public static IServiceCollection AddJurisdictionServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register services
        services.AddScoped<IJurisdictionService, JurisdictionService>();
        services.AddScoped<IJurisdictionContextAccessor, JurisdictionContextAccessor>();

        // Load jurisdictions from JSON file
        var jurisdictionsConfig = LoadJurisdictionsConfig();
        services.AddSingleton(jurisdictionsConfig);

        services.AddDbContextFactory<FoundryDbContext>(options =>
        {
            // Configure your DbContext options here (connection string, provider, etc.)
        });

        return services;
    }

    private static Dictionary<string, JurisdictionConfig> LoadJurisdictionsConfig()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory,
            "MultiTenant", "Jurisdiction", "Config", "jurisdictions.json");

        if (!File.Exists(configPath))
            return new Dictionary<string, JurisdictionConfig>();

        var json = File.ReadAllText(configPath);
        var doc = JsonDocument.Parse(json);
        var jurisdictions = new Dictionary<string, JurisdictionConfig>();

        if (doc.RootElement.TryGetProperty("jurisdictions", out var jurArray))
        {
            foreach (var jur in jurArray.EnumerateArray())
            {
                var config = JsonSerializer.Deserialize<JurisdictionConfig>(jur.GetRawText());
                if (config != null)
                {
                    jurisdictions[config.StateCode] = config;
                }
            }
        }

        return jurisdictions;
    }
}