using TubieTools_Aspire.EnterpriseAutomation.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TubieTools_Aspire.EnterpriseAutomation.MultiTenant;

namespace TubieTools_Aspire.EnterpriseAutomation.Controllers
{
    /// <summary>
    /// API controller for multi-tenant operations
    /// </summary>
    [ApiController]
    [Route("api/v1/tenants")]
    public class MultiTenantController : ControllerBase
    {
        private readonly IMultiTenantAIAgent _multiTenantAgent;
        private readonly ITenantService _tenantService;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly ILogger<MultiTenantController> _logger;

        public MultiTenantController(
            IMultiTenantAIAgent multiTenantAgent,
            ITenantService tenantService,
            ISubscriptionManager subscriptionManager,
            ILogger<MultiTenantController> logger)
        {
            _multiTenantAgent = multiTenantAgent;
            _tenantService = tenantService;
            _subscriptionManager = subscriptionManager;
            _logger = logger;
        }

        /// <summary>
        /// Register a new tenant
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterTenant([FromBody] RegisterTenantRequest request)
        {
            try
            {
                var tenantConfig = new TenantConfig
                {
                    TenantId = Guid.NewGuid().ToString(),
                    TenantName = request.TenantName,
                    Description = request.Description,
                    CurrentTier = SubscriptionTier.Free
                };

                var createdTenant = await _tenantService.CreateTenantAsync(tenantConfig);

                // Create subscription
                var subscription = new TenantSubscription
                {
                    TenantId = createdTenant.TenantId,
                    SubscriptionId = Guid.NewGuid().ToString(),
                    Tier = SubscriptionTier.Free,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(1),
                    AutoRenew = true,
                    Status = "active",
                    BillingInterval = "monthly"
                };

                await _tenantService.UpdateSubscriptionAsync(subscription);

                return Ok(new
                {
                    tenantId = createdTenant.TenantId,
                    apiKey = createdTenant.ApiKey,
                    message = "Tenant registered successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering tenant");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get tenant details
        /// </summary>
        [HttpGet("{tenantId}")]
        public async Task<IActionResult> GetTenant(string tenantId)
        {
            try
            {
                var tenant = await _tenantService.GetTenantAsync(tenantId);
                if (tenant == null)
                    return NotFound(new { error = "Tenant not found" });

                var subscription = await _tenantService.GetSubscriptionAsync(tenantId);
                var tierConfig = await _subscriptionManager.GetTierConfigAsync(tenant.CurrentTier);

                return Ok(new
                {
                    tenant,
                    subscription,
                    tierConfig
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tenant");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Upgrade tenant subscription tier
        /// </summary>
        [HttpPost("{tenantId}/upgrade")]
        public async Task<IActionResult> UpgradeTier(string tenantId, [FromBody] UpgradeTierRequest request)
        {
            try
            {
                var tenant = await _tenantService.GetTenantAsync(tenantId);
                if (tenant == null)
                    return NotFound(new { error = "Tenant not found" });

                tenant.CurrentTier = request.NewTier;
                await _tenantService.UpdateTenantAsync(tenant);

                var subscription = await _tenantService.GetSubscriptionAsync(tenantId);
                subscription.Tier = request.NewTier;
                subscription.RenewalDate = DateTime.UtcNow.AddMonths(1);
                await _tenantService.UpdateSubscriptionAsync(subscription);

                return Ok(new { message = "Subscription upgraded successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upgrading subscription");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Send request to AI agent for tenant
        /// </summary>
        /// <summary>
        /// Send request to AI agent for tenant
        /// </summary>
        /// <summary>
        /// Send request to AI agent for tenant
        /// </summary>
                [HttpPost("{tenantId}/agent/ask")]
        public async Task<IActionResult> AskAgent(string tenantId, [FromBody] AskAgentRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tenantId))
                    return BadRequest(new { error = "TenantId is required" });

                if (request == null || string.IsNullOrWhiteSpace(request.Message))
                    return BadRequest(new { error = "Message is required" });

                var response = await _multiTenantAgent.ProcessRequestAsync(tenantId, request.Message);

                if (response == null)
                    return StatusCode(500, new { error = "Failed to process request" });

                if (!response.Success)
                {
                    if (this.IsAccessDenialResponse(response.Message))
                    {
                        _logger.LogWarning("Access denied for tenant {TenantId}: {Message}", tenantId, response.Message);
                        return this.HandleAccessDenial(response.Message ?? "Access denied");
                    }

                    return BadRequest(new { error = response.Message ?? "Request processing failed" });
                }

                _logger.LogInformation("Agent request processed successfully for tenant {TenantId}", tenantId);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access for tenant {TenantId}", tenantId);
                return this.HandleUnauthorizedAccess("Unauthorized access");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing agent request for tenant {TenantId}", tenantId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get subscription details
        /// </summary>
        [HttpGet("{tenantId}/subscription")]
        public async Task<IActionResult> GetSubscription(string tenantId)
        {
            try
            {
                var subscription = await _tenantService.GetSubscriptionAsync(tenantId);
                if (subscription == null)
                    return NotFound(new { error = "Subscription not found" });

                return Ok(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subscription");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get usage statistics
        /// </summary>
        [HttpGet("{tenantId}/usage")]
        public async Task<IActionResult> GetUsage(string tenantId, [FromQuery] int daysBack = 30)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddDays(-daysBack);
                var endDate = DateTime.UtcNow;

                var usage = await _tenantService.GetUsageStatsAsync(tenantId, startDate, endDate);
                var quota = await _tenantService.GetQuotaAsync(tenantId);

                return Ok(new
                {
                    quota,
                    usage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving usage");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get available subscription tiers
        /// </summary>
        [HttpGet("tiers")]
        public async Task<IActionResult> GetAvailableTiers()
        {
            try
            {
                var tiers = await _subscriptionManager.GetAllTierConfigsAsync();
                return Ok(tiers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tiers");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create custom agent for tenant
        /// </summary>
        [HttpPost("{tenantId}/agents")]
        public async Task<IActionResult> CreateCustomAgent(string tenantId, [FromBody] CreateAgentRequest request)
        {
            try
            {
                var canCreateAgent = await _multiTenantAgent.ValidateAccessAsync(tenantId, "multiple_agents");
                if (!canCreateAgent)
                {
                    return BadRequest(new { error = "Your subscription tier does not allow multiple agents" });
                }

                var agent = new TenantCustomAgent
                {
                    TenantId = tenantId,
                    AgentName = request.AgentName,
                    SystemPrompt = request.SystemPrompt,
                    AssignedTools = request.AssignedTools,
                    PreferredModel = request.PreferredModel ?? "gpt-4",
                    IsActive = true
                };

                var createdAgent = await _tenantService.CreateAgentAsync(agent);
                return Ok(createdAgent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating agent");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get tenant's custom agents
        /// </summary>
        [HttpGet("{tenantId}/agents")]
        public async Task<IActionResult> GetTenantAgents(string tenantId)
        {
            try
            {
                var agents = await _tenantService.GetTenantAgentsAsync(tenantId);
                return Ok(agents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving agents");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Add team member to tenant
        /// </summary>
        [HttpPost("{tenantId}/team")]
        public async Task<IActionResult> AddTeamMember(string tenantId, [FromBody] AddTeamMemberRequest request)
        {
            try
            {
                var tenant = await _tenantService.GetTenantAsync(tenantId);
                var tierConfig = await _subscriptionManager.GetTierConfigAsync(tenant.CurrentTier);

                var currentMembers = await _tenantService.GetTeamMembersAsync(tenantId);
                if (tierConfig.MaxTeamMembers > 0 && currentMembers.Count >= tierConfig.MaxTeamMembers)
                {
                    return BadRequest(new { error = "Team member limit reached for your subscription tier" });
                }

                var member = new TenantTeamMember
                {
                    TenantId = tenantId,
                    Email = request.Email,
                    Role = request.Role ?? "user"
                };

                var success = await _tenantService.AddTeamMemberAsync(member);
                return success ? Ok(new { message = "Team member added" }) : BadRequest(new { error = "Failed to add team member" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding team member");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Request/Response Models
    public class RegisterTenantRequest
    {
        public string TenantName { get; set; }
        public string Description { get; set; }
    }

    public class UpgradeTierRequest
    {
        public SubscriptionTier NewTier { get; set; }
    }

    

    public class CreateAgentRequest
    {
        public string AgentName { get; set; }
        public string SystemPrompt { get; set; }
        public List<string> AssignedTools { get; set; } = new();
        public string PreferredModel { get; set; }
    }

    public class AddTeamMemberRequest
    {
        public string Email { get; set; }
        public string Role { get; set; }
    }
}


