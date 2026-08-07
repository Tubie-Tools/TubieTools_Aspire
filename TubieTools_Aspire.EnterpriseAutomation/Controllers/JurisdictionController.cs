namespace TubieTools_Aspire.EnterpriseAutomation.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TubieTools_Aspire.EnterpriseAutomation.Extensions;
using TubieTools_Aspire.EnterpriseAutomation.MultiTenant.Jurisdiction;

[ApiController]
[Route("api/[controller]")]
public class JurisdictionController : ControllerBase
{
    private readonly IJurisdictionService _jurisdictionService;
    private readonly IJurisdictionContextAccessor _contextAccessor;
    private readonly ILogger<JurisdictionController> _logger;

    public JurisdictionController(
        IJurisdictionService jurisdictionService,
        IJurisdictionContextAccessor contextAccessor,
        ILogger<JurisdictionController> logger)
    {
        _jurisdictionService = jurisdictionService;
        _contextAccessor = contextAccessor;
        _logger = logger;
    }

    [HttpGet("states")]
    public async Task<IActionResult> GetAllJurisdictions()
    {
        try
        {
            var jurisdictions = await _jurisdictionService.GetAllJurisdictionsAsync();
            return Ok(new { jurisdictions });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving jurisdictions");
            return this.HandleAccessDenial("Error retrieving jurisdictions");
        }
    }

    [HttpGet("states/{stateCode}")]
    public async Task<IActionResult> GetJurisdictionByState(string stateCode)
    {
        try
        {
            var jurisdiction = await _jurisdictionService.GetJurisdictionByStateAsync(stateCode);
            if (jurisdiction == null)
                return NotFound(new { error = "Jurisdiction not found" });

            return Ok(jurisdiction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving jurisdiction for state {StateCode}", stateCode);
            return this.HandleAccessDenial();
        }
    }

    [HttpGet("states/{stateCode}/regulations")]
    public async Task<IActionResult> GetStateRegulations(string stateCode)
    {
        try
        {
            var regulations = await _jurisdictionService.GetStateRegulationsAsync(stateCode);
            if (regulations == null)
                return NotFound(new { error = "Regulations not found" });

            return Ok(regulations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving regulations for state {StateCode}", stateCode);
            return this.HandleAccessDenial();
        }
    }

    [HttpGet("states/{stateCode}/features")]
    public async Task<IActionResult> GetStateFeatures(string stateCode)
    {
        try
        {
            var features = await _jurisdictionService.GetStateFeaturesAsync(stateCode);
            if (features == null)
                return NotFound(new { error = "Features not found" });

            return Ok(features);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving features for state {StateCode}", stateCode);
            return this.HandleAccessDenial();
        }
    }

    [HttpPost("tenants/{tenantId}/jurisdictions/{stateCode}")]
    public async Task<IActionResult> MapTenantToJurisdiction(string tenantId, string stateCode)
    {
        try
        {
            var jurisdiction = await _jurisdictionService.GetJurisdictionByStateAsync(stateCode);
            if (jurisdiction == null)
                return NotFound(new { error = "Jurisdiction not found" });

            var success = await _jurisdictionService.MapTenantToJurisdictionAsync(
                tenantId, jurisdiction.JurisdictionId, isPrimary: true);

            if (!success)
                return BadRequest(new { error = "Failed to map tenant to jurisdiction" });

            return Ok(new { message = "Tenant mapped to jurisdiction successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mapping tenant {TenantId} to jurisdiction {StateCode}", tenantId, stateCode);
            return this.HandleAccessDenial();
        }
    }

    [HttpGet("tenants/{tenantId}/context")]
    public async Task<IActionResult> GetTenantJurisdictionContext(string tenantId)
    {
        try
        {
            var context = await _contextAccessor.ResolveJurisdictionAsync(tenantId);
            if (context == null)
                return NotFound(new { error = "No jurisdiction context found for tenant" });

            return Ok(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving jurisdiction context for tenant {TenantId}", tenantId);
            return this.HandleAccessDenial();
        }
    }
}