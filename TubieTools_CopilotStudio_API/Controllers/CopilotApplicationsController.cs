using Microsoft.AspNetCore.Mvc;
using TubieTools_CopilotStudio_API.Services;
using TubieTools_CopilotStudio_API.Services.DTOs;

namespace TubieTools_CopilotStudio_API.Controllers;

/// <summary>
/// API endpoints for managing Copilot Applications.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class CopilotApplicationsController : ControllerBase
{
    private readonly ICopilotApplicationService _service;
    private readonly ILogger<CopilotApplicationsController> _logger;

    public CopilotApplicationsController(
        ICopilotApplicationService service,
        ILogger<CopilotApplicationsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get all Copilot applications.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CopilotApplicationDto>>> GetAll(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all copilot applications");
        var result = await _service.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific Copilot application by ID.
    /// </summary>
    [HttpGet("{copilotId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CopilotApplicationDto>> GetById(string copilotId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting copilot application: {CopilotId}", copilotId);
        var result = await _service.GetByIdAsync(copilotId, cancellationToken);

        if (result == null)
            return NotFound(new { message = $"Copilot '{copilotId}' not found" });

        return Ok(result);
    }

    /// <summary>
    /// Get Copilot applications by landing zone.
    /// </summary>
    [HttpGet("landing-zone/{landingZone}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CopilotApplicationDto>>> GetByLandingZone(
        string landingZone, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting copilots for landing zone: {LandingZone}", landingZone);
        var result = await _service.GetByLandingZoneAsync(landingZone, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new Copilot application.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CopilotApplicationDto>> Create(
        [FromBody] CreateCopilotRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Name is required" });

        if (string.IsNullOrWhiteSpace(request.LandingZone))
            return BadRequest(new { message = "Landing zone is required" });

        _logger.LogInformation("Creating new copilot: {Name}", request.Name);
        var result = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { copilotId = result.CopilotId }, result);
    }

    /// <summary>
    /// Update an existing Copilot application.
    /// </summary>
    [HttpPut("{copilotId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CopilotApplicationDto>> Update(
        string copilotId, [FromBody] UpdateCopilotRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(copilotId))
            return BadRequest(new { message = "Copilot ID is required" });

        try
        {
            _logger.LogInformation("Updating copilot: {CopilotId}", copilotId);
            var result = await _service.UpdateAsync(copilotId, request, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Copilot '{copilotId}' not found" });
        }
    }

    /// <summary>
    /// Delete a Copilot application.
    /// </summary>
    [HttpDelete("{copilotId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string copilotId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(copilotId))
            return BadRequest(new { message = "Copilot ID is required" });

        _logger.LogInformation("Deleting copilot: {CopilotId}", copilotId);

        try
        {
            await _service.DeleteAsync(copilotId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Copilot '{copilotId}' not found" });
        }
    }
}
