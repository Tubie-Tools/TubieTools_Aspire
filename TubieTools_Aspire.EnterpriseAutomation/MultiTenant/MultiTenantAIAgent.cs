using Microsoft.Extensions.Logging;
using TubieTools_Aspire.EnterpriseAutomation.AIAgent;

namespace TubieTools_Aspire.EnterpriseAutomation.MultiTenant
{
    /// <summary>
    /// Multi-tenant wrapper for AI Agent with subscription enforcement
    /// </summary>
    public interface IMultiTenantAIAgent
    {
        Task<AgentResponse> ProcessRequestAsync(string tenantId, string userRequest);
        Task<AgentResponse> ProcessRequestAsync(string tenantId, string userRequest, List<AIChatTool> tools);
        Task<bool> ValidateAccessAsync(string tenantId, string feature, string toolName = null);
        Task<AgentResponse> GetTenantConversationHistoryAsync(string tenantId);
    }

    /// <summary>
    /// Implementation of multi-tenant AI Agent
    /// </summary>
    public class MultiTenantAIAgent : IMultiTenantAIAgent
    {
        private readonly IAIAgent _baseAgent;
        private readonly ITenantService _tenantService;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly ITenantContextAccessor _tenantContextAccessor;
        private readonly ILogger<MultiTenantAIAgent> _logger;

        public MultiTenantAIAgent(
            IAIAgent baseAgent,
            ITenantService tenantService,
            ISubscriptionManager subscriptionManager,
            ITenantContextAccessor tenantContextAccessor,
            ILogger<MultiTenantAIAgent> logger)
        {
            _baseAgent = baseAgent;
            _tenantService = tenantService;
            _subscriptionManager = subscriptionManager;
            _tenantContextAccessor = tenantContextAccessor;
            _logger = logger;
        }

        public async Task<AgentResponse> ProcessRequestAsync(string tenantId, string userRequest)
        {
            try
            {
                // Validate tenant access
                if (!await ValidateAccessAsync(tenantId, "api_access"))
                {
                    return new AgentResponse
                    {
                        Success = false,
                        Message = "Access denied. Subscription tier does not allow API access."
                    };
                }

                // Check quota
                var isQuotaExceeded = await _tenantService.IsQuotaExceededAsync(tenantId);
                if (isQuotaExceeded)
                {
                    return new AgentResponse
                    {
                        Success = false,
                        Message = "API quota exceeded. Please upgrade your subscription or wait for quota reset."
                    };
                }

                // Get available tools for tenant tier
                var tenantConfig = await _tenantService.GetTenantAsync(tenantId);
                var tierConfig = await _subscriptionManager.GetTierConfigAsync(tenantConfig.CurrentTier);
                var availableTools = new List<AIChatTool>();

                foreach (var toolName in tierConfig.AvailableTools)
                {
                    var toolAccess = await _subscriptionManager.GetToolAccessAsync(toolName, tenantConfig.CurrentTier);
                    if (toolAccess?.IsAvailable == true)
                    {
                        availableTools.Add(new AIChatTool
                        {
                            Name = toolName,
                            Description = GetToolDescription(toolName)
                        });
                    }
                }

                // Process request with filtered tools
                var response = await _baseAgent.ProcessRequestAsync(userRequest, availableTools);

                // Update usage
                await _tenantService.IncrementUsageAsync(tenantId, 1);

                _logger.LogInformation("Request processed for tenant {TenantId}", tenantId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request for tenant {TenantId}", tenantId);
                return new AgentResponse
                {
                    Success = false,
                    Message = $"Error processing request: {ex.Message}"
                };
            }
        }

        public async Task<AgentResponse> ProcessRequestAsync(string tenantId, string userRequest, List<AIChatTool> tools)
        {
            try
            {
                // Validate tenant and tools
                var tenantConfig = await _tenantService.GetTenantAsync(tenantId);
                if (tenantConfig == null)
                {
                    return new AgentResponse
                    {
                        Success = false,
                        Message = "Tenant not found"
                    };
                }

                // Filter tools based on subscription tier
                var filteredTools = new List<AIChatTool>();
                foreach (var tool in tools)
                {
                    var canAccess = await ValidateAccessAsync(tenantId, "tool", tool.Name);
                    if (canAccess)
                    {
                        filteredTools.Add(tool);
                    }
                }

                if (!filteredTools.Any() && tools.Any())
                {
                    return new AgentResponse
                    {
                        Success = false,
                        Message = "No tools available for your subscription tier"
                    };
                }

                var response = await _baseAgent.ProcessRequestAsync(userRequest, filteredTools);
                await _tenantService.IncrementUsageAsync(tenantId, 1);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request for tenant {TenantId}", tenantId);
                return new AgentResponse
                {
                    Success = false,
                    Message = $"Error processing request: {ex.Message}"
                };
            }
        }

        public async Task<bool> ValidateAccessAsync(string tenantId, string feature, string toolName = null)
        {
            try
            {
                var tenant = await _tenantService.GetTenantAsync(tenantId);
                if (tenant == null || !tenant.IsActive)
                    return false;

                var tierConfig = await _subscriptionManager.GetTierConfigAsync(tenant.CurrentTier);
                if (tierConfig == null)
                    return false;

                // Check feature access
                var hasFeature = feature switch
                {
                    "custom_prompts" => tierConfig.AllowCustomPrompts,
                    "multiple_agents" => tierConfig.AllowMultipleAgents,
                    "workflow_orchestration" => tierConfig.AllowWorkflowOrchestration,
                    "analytics" => tierConfig.AllowAnalytics,
                    "api_access" => tierConfig.AllowAPIAccess,
                    "webhooks" => tierConfig.AllowWebhooks,
                    "priority_support" => tierConfig.PrioritySupport,
                    "data_export" => tierConfig.AllowDataExport,
                    "public_api" => tierConfig.PublicAPI,
                    "tool" => toolName != null && await IsToolAccessibleAsync(toolName, tenant.CurrentTier, tierConfig),
                    _ => false
                };

                return hasFeature;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating access for tenant {TenantId}", tenantId);
                return false;
            }
        }

        private async Task<bool> IsToolAccessibleAsync(string toolName, SubscriptionTier tier, SubscriptionTierConfig tierConfig)
        {
            if (!tierConfig.AvailableTools.Contains(toolName))
                return false;

            var toolAccess = await _subscriptionManager.GetToolAccessAsync(toolName, tier);
            return toolAccess?.IsAvailable == true;
        }

        public async Task<AgentResponse> GetTenantConversationHistoryAsync(string tenantId)
        {
            try
            {
                if (!await ValidateAccessAsync(tenantId, "api_access"))
                {
                    return new AgentResponse
                    {
                        Success = false,
                        Message = "Access denied"
                    };
                }

                var history = _baseAgent.GetConversationHistory();
                return new AgentResponse
                {
                    Success = true,
                    ConversationHistory = history
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversation history for tenant {TenantId}", tenantId);
                return new AgentResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        private string GetToolDescription(string toolName)
        {
            return toolName switch
            {
                "create_incident" => "Creates a new ServiceNow incident",
                "search_incident" => "Searches for ServiceNow incidents",
                "close_incident" => "Closes a ServiceNow incident",
                _ => "Unknown tool"
            };
        }
    }
}
