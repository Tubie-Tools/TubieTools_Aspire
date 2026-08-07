namespace TubieTools_Aspire.EnterpriseAutomation.MultiTenant.Jurisdiction;

/// <summary>
/// Provides access to the current jurisdiction context
/// </summary>
public interface IJurisdictionContextAccessor
{
    JurisdictionContext? CurrentJurisdiction { get; set; }
    string? CurrentStateCode { get; set; }
    Task<JurisdictionContext?> ResolveJurisdictionAsync(string tenantId, string? stateCodeOverride = null);
}

public class JurisdictionContextAccessor : IJurisdictionContextAccessor
{
    private readonly IJurisdictionService _jurisdictionService;
    private JurisdictionContext? _currentJurisdiction;

    public JurisdictionContext? CurrentJurisdiction
    {
        get => _currentJurisdiction;
        set => _currentJurisdiction = value;
    }

    public string? CurrentStateCode => _currentJurisdiction?.StateCode;

    string? IJurisdictionContextAccessor.CurrentStateCode { get => CurrentStateCode; set => _currentJurisdiction?.StateCode = value; }

    public JurisdictionContextAccessor(IJurisdictionService jurisdictionService)
    {
        _jurisdictionService = jurisdictionService;
    }

    public async Task<JurisdictionContext?> ResolveJurisdictionAsync(string tenantId, string? stateCodeOverride = null)
    {
        if (!string.IsNullOrEmpty(stateCodeOverride))
        {
            _currentJurisdiction = await _jurisdictionService.GetContextByStateAsync(stateCodeOverride);
        }
        else
        {
            var primaryJurisdiction = await _jurisdictionService.GetPrimaryJurisdictionForTenantAsync(tenantId);
            if (primaryJurisdiction != null)
            {
                _currentJurisdiction = await _jurisdictionService.GetJurisdictionContextAsync(primaryJurisdiction.JurisdictionId);
            }
        }

        return _currentJurisdiction;
    }
}