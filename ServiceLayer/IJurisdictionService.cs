namespace TubieTools_Aspire.EnterpriseAutomation.MultiTenant.Jurisdiction;

using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Service for managing jurisdictions and state-specific configurations
/// </summary>
public interface IJurisdictionService
{
    Task<JurisdictionConfig?> GetJurisdictionByStateAsync(string stateCode);
    Task<JurisdictionConfig?> GetJurisdictionByIdAsync(string jurisdictionId);
    Task<IEnumerable<JurisdictionConfig>> GetAllJurisdictionsAsync();
    Task<JurisdictionContext?> GetJurisdictionContextAsync(string jurisdictionId);
    Task<JurisdictionContext?> GetContextByStateAsync(string stateCode);
    Task<IEnumerable<JurisdictionContext>> GetContextsByTenantAsync(string tenantId);
    Task CreateJurisdictionAsync(JurisdictionConfig jurisdiction);
    Task UpdateJurisdictionAsync(JurisdictionConfig jurisdiction);
    Task<bool> MapTenantToJurisdictionAsync(string tenantId, string jurisdictionId, bool isPrimary = false);
    Task<JurisdictionConfig?> GetPrimaryJurisdictionForTenantAsync(string tenantId);
    Task<StateRegulations?> GetStateRegulationsAsync(string stateCode);
    Task<StateFeatures?> GetStateFeaturesAsync(string stateCode);
}