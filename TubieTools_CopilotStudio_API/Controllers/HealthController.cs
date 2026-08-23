using Microsoft.AspNetCore.Mvc;

namespace TubieTools_CopilotStudio_API.Controllers;

/// <summary>
/// Health check endpoint.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Get API health status.
    /// </summary>
    [HttpGet]
    public ActionResult<object> GetHealth()
    {
        return Ok(new { status = "Healthy", timestamp = DateTime.UtcNow });
    }
}
