using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TubieTools_Aspire.EnterpriseAutomation.MultiTenant.Jurisdiction;
using DataAccessLayer.Data.Contexts;

namespace TubieTools_Aspire.EnterpriseAutomation.Seeds
{
    public class JurisdictionSeeder
    {
        public static async Task SeedJurisdictionsAsync(FoundryDbContext context)
        {
            try
            {
                var configPath = Path.Combine(AppContext.BaseDirectory, 
                    "MultiTenant", "Jurisdiction", "Config", "jurisdictions.json");

                if (!File.Exists(configPath))
                {
                    Console.WriteLine($"⚠ Config file not found: {configPath}");
                    return;
                }

                var json = await File.ReadAllTextAsync(configPath);
                using var doc = JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("jurisdictions", out var jurArray))
                {
                    Console.WriteLine("⚠ No jurisdictions array found in config");
                    return;
                }

                foreach (var jurElement in jurArray.EnumerateArray())
                {
                    var jurisdiction = JsonSerializer.Deserialize<JurisdictionConfig>(
                        jurElement.GetRawText(),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (jurisdiction == null || string.IsNullOrEmpty(jurisdiction.StateCode))
                        continue;

                    var existing = await context.Jurisdictions
                        .FirstOrDefaultAsync(j => j.StateCode == jurisdiction.StateCode);

                    if (existing != null)
                    {
                        Console.WriteLine($"✓ Jurisdiction already exists: {jurisdiction.StateCode} ({jurisdiction.JurisdictionName})");
                        continue;
                    }

                    jurisdiction.CreatedAt = DateTime.UtcNow;
                    context.Jurisdictions.Add(jurisdiction);
                    Console.WriteLine($"+ Adding jurisdiction: {jurisdiction.StateCode} ({jurisdiction.JurisdictionName})");
                }

                await context.SaveChangesAsync();
                Console.WriteLine($"\n✓ Jurisdictions seeded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error seeding jurisdictions: {ex.Message}");
                throw;
            }
        }
    }
}
