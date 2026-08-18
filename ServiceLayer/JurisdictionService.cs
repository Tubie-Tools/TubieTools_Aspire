namespace TubieTools_Aspire.EnterpriseAutomation.MultiTenant.Jurisdiction;

using DataAccessLayer.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

/// <summary>
/// Service implementation for jurisdiction management
/// </summary>
public class JurisdictionService : IJurisdictionService
{
    private readonly IDbContextFactory<FoundryDbContext> _contextFactory;
    private readonly ILogger<JurisdictionService> _logger;

    public JurisdictionService(IDbContextFactory<FoundryDbContext> contextFactory, ILogger<JurisdictionService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<JurisdictionConfig?> GetJurisdictionByStateAsync(string stateCode)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Jurisdictions
            .FirstOrDefaultAsync(j => j.StateCode == stateCode && j.IsActive);
    }

    public async Task<JurisdictionConfig?> GetJurisdictionByIdAsync(string jurisdictionId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Jurisdictions.FirstOrDefaultAsync(j => j.JurisdictionId == jurisdictionId);
    }

    public async Task<IEnumerable<JurisdictionConfig>> GetAllJurisdictionsAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Jurisdictions.Where(j => j.IsActive).ToListAsync();
    }

    public async Task<JurisdictionContext?> GetJurisdictionContextAsync(string jurisdictionId)
    {
        var jurisdiction = await GetJurisdictionByIdAsync(jurisdictionId);
        if (jurisdiction == null)
            return null;

        return CreateContext(jurisdiction);
    }

    public async Task<JurisdictionContext?> GetContextByStateAsync(string stateCode)
    {
        var jurisdiction = await GetJurisdictionByStateAsync(stateCode);
        if (jurisdiction == null)
            return null;

        return CreateContext(jurisdiction);
    }

    public async Task<IEnumerable<JurisdictionContext>> GetContextsByTenantAsync(string tenantId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var jurisdictions = await context.TenantJurisdictionMappings
            .Where(m => m.TenantId == tenantId)
            .Include(m => m.Jurisdiction)
            .Select(m => m.Jurisdiction)
            .ToListAsync();

        return jurisdictions
            .Where(j => j != null)
            .Select(j => CreateContext(j!))
            .ToList();
    }

    public async Task CreateJurisdictionAsync(JurisdictionConfig jurisdiction)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        jurisdiction.JurisdictionId ??= Guid.NewGuid().ToString();
        jurisdiction.CreatedAt = DateTime.UtcNow;

        context.Jurisdictions.Add(jurisdiction);
        await context.SaveChangesAsync();

        _logger.LogInformation("Created jurisdiction {StateCode} ({JurisdictionId})",
            jurisdiction.StateCode, jurisdiction.JurisdictionId);
    }

    public async Task UpdateJurisdictionAsync(JurisdictionConfig jurisdiction)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        jurisdiction.UpdatedAt = DateTime.UtcNow;

        context.Jurisdictions.Update(jurisdiction);
        await context.SaveChangesAsync();

        _logger.LogInformation("Updated jurisdiction {JurisdictionId}", jurisdiction.JurisdictionId);
    }

    public async Task<bool> MapTenantToJurisdictionAsync(string tenantId, string jurisdictionId, bool isPrimary = false)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var jurisdiction = await context.Jurisdictions.FirstOrDefaultAsync(j => j.JurisdictionId == jurisdictionId);
        if (jurisdiction == null)
            return false;

        // If setting as primary, unset other primary mappings
        if (isPrimary)
        {
            var otherMappings = await context.TenantJurisdictionMappings
                .Where(m => m.TenantId == tenantId && m.IsPrimary)
                .ToListAsync();

            foreach (var mapping in otherMappings)
                mapping.IsPrimary = false;
        }

        var newMapping = new TenantJurisdictionMapping
        {
            MappingId = Guid.NewGuid().ToString(),
            TenantId = tenantId,
            JurisdictionId = jurisdictionId,
            IsPrimary = isPrimary
        };

        context.TenantJurisdictionMappings.Add(newMapping);
        await context.SaveChangesAsync();

        _logger.LogInformation("Mapped tenant {TenantId} to jurisdiction {JurisdictionId}", tenantId, jurisdictionId);
        return true;
    }

    public async Task<JurisdictionConfig?> GetPrimaryJurisdictionForTenantAsync(string tenantId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TenantJurisdictionMappings
            .Where(m => m.TenantId == tenantId && m.IsPrimary)
            .Include(m => m.Jurisdiction)
            .Select(m => m.Jurisdiction)
            .FirstOrDefaultAsync();
    }

    public async Task<StateRegulations?> GetStateRegulationsAsync(string stateCode)
    {
        var jurisdiction = await GetJurisdictionByStateAsync(stateCode);
        if (jurisdiction == null || string.IsNullOrEmpty(jurisdiction.RegulationsJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<StateRegulations>(jurisdiction.RegulationsJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing regulations for {StateCode}", stateCode);
            return null;
        }
    }

    public async Task<StateFeatures?> GetStateFeaturesAsync(string stateCode)
    {
        var jurisdiction = await GetJurisdictionByStateAsync(stateCode);
        if (jurisdiction == null || string.IsNullOrEmpty(jurisdiction.FeaturesJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<StateFeatures>(jurisdiction.FeaturesJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing features for {StateCode}", stateCode);
            return null;
        }
    }

    private JurisdictionContext CreateContext(JurisdictionConfig jurisdiction)
    {
        var regulations = string.IsNullOrEmpty(jurisdiction.RegulationsJson)
            ? null
            : JsonSerializer.Deserialize<StateRegulations>(jurisdiction.RegulationsJson);

        var features = string.IsNullOrEmpty(jurisdiction.FeaturesJson)
            ? null
            : JsonSerializer.Deserialize<StateFeatures>(jurisdiction.FeaturesJson);

        return new JurisdictionContext
        {
            JurisdictionId = jurisdiction.JurisdictionId,
            StateCode = jurisdiction.StateCode,
            JurisdictionName = jurisdiction.JurisdictionName,
            Regulations = regulations,
            Features = features,
            DatabaseSchema = jurisdiction.DatabaseSchema,
            ConnectionStringName = jurisdiction.ConnectionStringName
        };
    }
}