using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TubieTools_PublicAPI.Models;
using TubieTools_PublicAPI.Models.Requests;
using TubieTools_PublicAPI.Models.Responses;
using TubieTools_PublicAPI.Services;

namespace TubieTools_PublicAPI.Controllers
{
    /// <summary>
    /// Care Provider API Controller
    /// Manages CRUD operations for B2B care providers with support for multiple tiers:
    /// DayCare (50+ orders/year), ElderlyHome (100+ orders/year), HealthcareProvider (500+ orders/year)
    /// </summary>
    [Authorize(Policy = "OktaAccess")]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CareProvidersController : ControllerBase
    {
        private readonly ICareProviderService _careProviderService;
        private readonly ILogger<CareProvidersController> _logger;

        /// <summary>
        /// Constructor for CareProvidersController
        /// </summary>
        /// <param name="careProviderService">Injected care provider service</param>
        /// <param name="logger">Injected logger</param>
        public CareProvidersController(ICareProviderService careProviderService, ILogger<CareProvidersController> logger)
        {
            _careProviderService = careProviderService;
            _logger = logger;
        }

        /// <summary>
        /// Get all care providers with optional filtering by tier and status
        /// </summary>
        /// <param name="tier">Optional: Filter by care provider tier (1=DayCare, 2=ElderlyHome, 3=HealthcareProvider)</param>
        /// <param name="status">Optional: Filter by provider status (Active, Inactive, Pending, Suspended)</param>
        /// <returns>List of care providers matching filter criteria</returns>
        /// <response code="200">Returns list of care providers</response>
        /// <response code="400">If invalid tier or status provided</response>
        /// <response code="500">If internal server error occurs</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<CareProvider>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<CareProvider>>>> GetAllProviders(
            [FromQuery] int? tier = null,
            [FromQuery] string status = null)
        {
            try
            {
                CareProviderTier? tierFilter = null;
                if (tier.HasValue && Enum.IsDefined(typeof(CareProviderTier), tier.Value))
                {
                    tierFilter = (CareProviderTier)tier.Value;
                }

                var providers = await _careProviderService.GetAllProvidersAsync(tierFilter, status);

                return Ok(new ApiResponse<List<CareProvider>>(
                    success: true,
                    statusCode: 200,
                    message: $"Retrieved {providers.Count} care providers",
                    data: providers
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting all providers: {ex.Message}");
                return StatusCode(500, new ApiResponse<object>(
                    success: false,
                    statusCode: 500,
                    message: "Internal server error",
                    data: null
                ));
            }
        }

        /// <summary>
        /// Get a specific care provider by ID
        /// </summary>
        /// <param name="providerId">The unique provider ID (e.g., daycare-001)</param>
        /// <returns>The requested care provider</returns>
        /// <response code="200">Returns the care provider</response>
        /// <response code="404">If provider not found</response>
        /// <response code="500">If internal server error occurs</response>
        [HttpGet("{providerId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<CareProvider>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CareProvider>>> GetProviderById(
            [FromRoute, Required] string providerId)
        {
            try
            {
                var provider = await _careProviderService.GetProviderByIdAsync(providerId);

                if (provider == null)
                {
                    return NotFound(new ApiResponse<object>(
                        success: false,
                        statusCode: 404,
                        message: $"Provider {providerId} not found",
                        data: null
                    ));
                }

                return Ok(new ApiResponse<CareProvider>(
                    success: true,
                    statusCode: 200,
                    message: "Provider retrieved successfully",
                    data: provider
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting provider {providerId}: {ex.Message}");
                return StatusCode(500, new ApiResponse<object>(
                    success: false,
                    statusCode: 500,
                    message: "Internal server error",
                    data: null
                ));
            }
        }

        /// <summary>
        /// Get all providers of a specific tier
        /// </summary>
        /// <param name="tier">The care provider tier (1=DayCare, 2=ElderlyHome, 3=HealthcareProvider)</param>
        /// <returns>List of providers in the specified tier</returns>
        /// <response code="200">Returns providers in the tier</response>
        /// <response code="400">If invalid tier provided</response>
        /// <response code="500">If internal server error occurs</response>
        [HttpGet("tier/{tier}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<CareProvider>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<CareProvider>>>> GetProvidersByTier(
            [FromRoute, Required] int tier)
        {
            try
            {
                if (!Enum.IsDefined(typeof(CareProviderTier), tier))
                {
                    return BadRequest(new ApiResponse<object>(
                        success: false,
                        statusCode: 400,
                        message: "Invalid tier. Use: 1=DayCare, 2=ElderlyHome, 3=HealthcareProvider",
                        data: null
                    ));
                }

                var providers = await _careProviderService.GetProvidersByTierAsync((CareProviderTier)tier);

                return Ok(new ApiResponse<List<CareProvider>>(
                    success: true,
                    statusCode: 200,
                    message: $"Retrieved {providers.Count} providers for tier {(CareProviderTier)tier}",
                    data: providers
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting providers by tier: {ex.Message}");
                return StatusCode(500, new ApiResponse<object>(
                    success: false,
                    statusCode: 500,
                    message: "Internal server error",
                    data: null
                ));
            }
        }

        /// <summary>
        /// Create a new care provider
        /// </summary>
        /// <param name="request">The care provider creation request</param>
        /// <returns>The created care provider with assigned ID</returns>
        /// <response code="201">Provider created successfully</response>
        /// <response code="400">If validation fails or invalid data provided</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="500">If internal server error occurs</response>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CareProvider>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CareProvider>>> CreateProvider(
            [FromBody, Required] CreateCareProviderRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(new ApiResponse<object>(
                        success: false,
                        statusCode: 400,
                        message: "Validation failed",
                        data: null
                    ) { Errors = errors });
                }

                var provider = await _careProviderService.CreateProviderAsync(request);

                _logger.LogInformation($"New provider created: {provider.ProviderId}");

                return CreatedAtAction(nameof(GetProviderById), new { providerId = provider.ProviderId },
                    new ApiResponse<CareProvider>(
                        success: true,
                        statusCode: 201,
                        message: "Provider created successfully",
                        data: provider
                    ));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<object>(
                    success: false,
                    statusCode: 400,
                    message: ex.Message,
                    data: null
                ));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object>(
                    success: false,
                    statusCode: 400,
                    message: ex.Message,
                    data: null
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating provider: {ex.Message}");
                return StatusCode(500, new ApiResponse<object>(
                    success: false,
                    statusCode: 500,
                    message: "Internal server error",
                    data: null
                ));
            }
        }

        /// <summary>
        /// Update an existing care provider
        /// </summary>
        /// <param name="providerId">The provider ID to update</param>
        /// <param name="request">The updated provider data</param>
        /// <returns>The updated care provider</returns>
        /// <response code="200">Provider updated successfully</response>
        /// <response code="400">If validation fails or invalid data provided</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="404">If provider not found</response>
        /// <response code="500">If internal server error occurs</response>
        [HttpPut("{providerId}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CareProvider>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CareProvider>>> UpdateProvider(
            [FromRoute, Required] string providerId,
            [FromBody, Required] UpdateCareProviderRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(new ApiResponse<object>(
                        success: false,
                        statusCode: 400,
                        message: "Validation failed",
                        data: null
                    ) { Errors = errors });
                }

                var provider = await _careProviderService.UpdateProviderAsync(providerId, request);

                _logger.LogInformation($"Provider updated: {providerId}");

                return Ok(new ApiResponse<CareProvider>(
                    success: true,
                    statusCode: 200,
                    message: "Provider updated successfully",
                    data: provider
                ));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>(
                    success: false,
                    statusCode: 404,
                    message: ex.Message,
                    data: null
                ));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<object>(
                    success: false,
                    statusCode: 400,
                    message: ex.Message,
                    data: null
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating provider {providerId}: {ex.Message}");
                return StatusCode(500, new ApiResponse<object>(
                    success: false,
                    statusCode: 500,
                    message: "Internal server error",
                    data: null
                ));
            }
        }

        /// <summary>
        /// Delete/deactivate a care provider
        /// </summary>
        /// <param name="providerId">The provider ID to delete</param>
        /// <returns>Success message</returns>
        /// <response code="200">Provider deleted successfully</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="404">If provider not found</response>
        /// <response code="500">If internal server error occurs</response>
        [HttpDelete("{providerId}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteProvider(
            [FromRoute, Required] string providerId)
        {
            try
            {
                var result = await _careProviderService.DeleteProviderAsync(providerId);

                if (!result)
                {
                    return NotFound(new ApiResponse<object>(
                        success: false,
                        statusCode: 404,
                        message: $"Provider {providerId} not found",
                        data: null
                    ));
                }

                _logger.LogInformation($"Provider deleted: {providerId}");

                return Ok(new ApiResponse<object>(
                    success: true,
                    statusCode: 200,
                    message: "Provider deleted successfully",
                    data: null
                ));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>(
                    success: false,
                    statusCode: 404,
                    message: ex.Message,
                    data: null
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting provider {providerId}: {ex.Message}");
                return StatusCode(500, new ApiResponse<object>(
                    success: false,
                    statusCode: 500,
                    message: "Internal server error",
                    data: null
                ));
            }
        }

        /// <summary>
        /// Get all active care providers
        /// </summary>
        /// <returns>List of active providers</returns>
        /// <response code="200">Returns list of active providers</response>
        /// <response code="500">If internal server error occurs</response>
        [HttpGet("active")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<CareProvider>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<CareProvider>>>> GetActiveProviders()
        {
            try
            {
                var providers = await _careProviderService.GetActiveProvidersAsync();

                return Ok(new ApiResponse<List<CareProvider>>(
                    success: true,
                    statusCode: 200,
                    message: $"Retrieved {providers.Count} active providers",
                    data: providers
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting active providers: {ex.Message}");
                return StatusCode(500, new ApiResponse<object>(
                    success: false,
                    statusCode: 500,
                    message: "Internal server error",
                    data: null
                ));
            }
        }

        /// <summary>
        /// Search care providers by name
        /// </summary>
        /// <param name="searchTerm">The search term to find in provider names</param>
        /// <returns>List of providers matching the search term</returns>
        /// <response code="200">Returns matching providers</response>
        /// <response code="400">If search term is empty</response>
        /// <response code="500">If internal server error occurs</response>
        [HttpGet("search")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<CareProvider>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<CareProvider>>>> SearchProviders(
            [FromQuery, Required] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BadRequest(new ApiResponse<object>(
                        success: false,
                        statusCode: 400,
                        message: "Search term cannot be empty",
                        data: null
                    ));
                }

                var providers = await _careProviderService.SearchProvidersByNameAsync(searchTerm);

                return Ok(new ApiResponse<List<CareProvider>>(
                    success: true,
                    statusCode: 200,
                    message: $"Found {providers.Count} providers matching '{searchTerm}'",
                    data: providers
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching providers: {ex.Message}");
                return StatusCode(500, new ApiResponse<object>(
                    success: false,
                    statusCode: 500,
                    message: "Internal server error",
                    data: null
                ));
            }
        }
    }
}
