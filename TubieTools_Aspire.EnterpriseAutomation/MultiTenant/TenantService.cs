using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace TubieTools_Aspire.EnterpriseAutomation.MultiTenant
{

    /// <summary>
    /// Interface for tenant service operations
    /// </summary>
    public interface ITenantService
    {
        Task<TenantConfig> GetTenantAsync(string tenantId);
        Task<TenantConfig> CreateTenantAsync(TenantConfig tenantConfig);
        Task<bool> UpdateTenantAsync(TenantConfig tenantConfig);
        Task<bool> DeleteTenantAsync(string tenantId);
        Task<TenantSubscription> GetSubscriptionAsync(string tenantId);
        Task<bool> UpdateSubscriptionAsync(TenantSubscription subscription);
        Task<TenantQuota> GetQuotaAsync(string tenantId);
        Task<bool> IncrementUsageAsync(string tenantId, int apiCallCount);
        Task<bool> IsQuotaExceededAsync(string tenantId);
        Task<List<TenantCustomAgent>> GetTenantAgentsAsync(string tenantId);
        Task<TenantCustomAgent> CreateAgentAsync(TenantCustomAgent agent);
        Task<bool> UpdateAgentAsync(TenantCustomAgent agent);
        Task<bool> DeleteAgentAsync(string agentId);
        Task<List<TenantTeamMember>> GetTeamMembersAsync(string tenantId);
        Task<bool> AddTeamMemberAsync(TenantTeamMember member);
        Task<bool> RemoveTeamMemberAsync(string memberId);
        Task<List<TenantUsage>> GetUsageStatsAsync(string tenantId, DateTime startDate, DateTime endDate);
        Task<TenantBillingRecord> GenerateBillingRecordAsync(string tenantId);
    }

/// <summary>
/// Implementation of tenant service
/// </summary>
public class TenantService : ITenantService
    {
        private readonly ILogger<TenantService> _logger;
        private readonly Dictionary<string, TenantConfig> _tenants; // Replace with DB
        private readonly Dictionary<string, TenantSubscription> _subscriptions; // Replace with DB
        private readonly Dictionary<string, TenantQuota> _quotas; // Replace with DB
        private readonly Dictionary<string, List<TenantCustomAgent>> _customAgents; // Replace with DB
        private readonly Dictionary<string, List<TenantTeamMember>> _teamMembers; // Replace with DB
        private readonly Dictionary<string, List<TenantUsage>> _usageStats; // Replace with DB

        /// <summary>
        /// tenantConfig does not seem to be binding to the json file 7/30/2026. figure it out later. remember your training.
        /// </summary>
        /// <param name="logger"></param>
        public TenantService(
    ILogger<TenantService> logger,
    IOptions<TenantConfigurationOptions> tenantOptions)
        {
            _logger = logger;
            _tenants = new Dictionary<string, TenantConfig>();
            _subscriptions = new Dictionary<string, TenantSubscription>();
            _quotas = new Dictionary<string, TenantQuota>();
            _customAgents = new Dictionary<string, List<TenantCustomAgent>>();
            _teamMembers = new Dictionary<string, List<TenantTeamMember>>();
            _usageStats = new Dictionary<string, List<TenantUsage>>();

            // Load tenants from configuration
            if (tenantOptions?.Value?.Tenants != null)
            {
                foreach (var tenant in tenantOptions.Value.Tenants)
                {
                    _tenants[tenant.TenantId] = tenant;
                }
            }

            // Load subscriptions from configuration
            if (tenantOptions?.Value?.Subscriptions != null)
            {
                foreach (var subscription in tenantOptions.Value.Subscriptions)
                {
                    _subscriptions[subscription.SubscriptionId] = subscription;
                }
            }
        }

        public async Task<TenantConfig> GetTenantAsync(string tenantId)
        {
            try
            {
                _logger.LogInformation("Retrieving tenant: {TenantId}", tenantId);
                return _tenants.TryGetValue(tenantId, out var tenant) ? tenant : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tenant: {TenantId}", tenantId);
                throw;
            }
        }

        public async Task<TenantConfig> CreateTenantAsync(TenantConfig tenantConfig)
        {
            try
            {
                if (string.IsNullOrEmpty(tenantConfig?.TenantId))
                    throw new ArgumentException("TenantId is required");

                _logger.LogInformation("Creating tenant: {TenantId}", tenantConfig.TenantId);

                tenantConfig.ApiKey = GenerateApiKey();
                tenantConfig.SecretKey = GenerateSecretKey();
                tenantConfig.IsActive = true;

                _tenants[tenantConfig.TenantId] = tenantConfig;

                // Create default quota
                _quotas[tenantConfig.TenantId] = new TenantQuota
                {
                    TenantId = tenantConfig.TenantId,
                    MonthlyApiCallLimit = 100,
                    DailyApiCallLimit = 20,
                    ResetDate = DateTime.UtcNow.AddMonths(1)
                };

                return tenantConfig;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tenant");
                throw;
            }
        }

        public async Task<bool> UpdateTenantAsync(TenantConfig tenantConfig)
        {
            try
            {
                _logger.LogInformation("Updating tenant: {TenantId}", tenantConfig.TenantId);
                _tenants[tenantConfig.TenantId] = tenantConfig;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tenant: {TenantId}", tenantConfig.TenantId);
                return false;
            }
        }

        public async Task<bool> DeleteTenantAsync(string tenantId)
        {
            try
            {
                _logger.LogInformation("Deleting tenant: {TenantId}", tenantId);
                _tenants.Remove(tenantId);
                _subscriptions.Remove(tenantId);
                _quotas.Remove(tenantId);
                _customAgents.Remove(tenantId);
                _teamMembers.Remove(tenantId);
                _usageStats.Remove(tenantId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tenant: {TenantId}", tenantId);
                return false;
            }
        }

        public async Task<TenantSubscription> GetSubscriptionAsync(string tenantId)
        {
            return _subscriptions.TryGetValue(tenantId, out var subscription) ? subscription : null;
        }

        public async Task<bool> UpdateSubscriptionAsync(TenantSubscription subscription)
        {
            try
            {
                _logger.LogInformation("Updating subscription for tenant: {TenantId}", subscription.TenantId);
                _subscriptions[subscription.TenantId] = subscription;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subscription for tenant: {TenantId}", subscription.TenantId);
                return false;
            }
        }

        public async Task<TenantQuota> GetQuotaAsync(string tenantId)
        {
            return _quotas.TryGetValue(tenantId, out var quota) ? quota : null;
        }

        public async Task<bool> IncrementUsageAsync(string tenantId, int apiCallCount)
        {
            try
            {
                if (!_quotas.TryGetValue(tenantId, out var quota))
                    return false;

                quota.MonthlyApiCallsUsed += apiCallCount;
                quota.DailyApiCallsUsed += apiCallCount;

                // Track usage statistics
                if (!_usageStats.ContainsKey(tenantId))
                    _usageStats[tenantId] = new List<TenantUsage>();

                var today = DateTime.UtcNow.Date;
                var todayStats = _usageStats[tenantId].FirstOrDefault(u => u.Date.Date == today);

                if (todayStats == null)
                {
                    todayStats = new TenantUsage
                    {
                        TenantId = tenantId,
                        Date = today,
                        ApiCallsUsed = apiCallCount
                    };
                    _usageStats[tenantId].Add(todayStats);
                }
                else
                {
                    todayStats.ApiCallsUsed += apiCallCount;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing usage for tenant: {TenantId}", tenantId);
                return false;
            }
        }

        public async Task<bool> IsQuotaExceededAsync(string tenantId)
        {
            try
            {
                if (!_quotas.TryGetValue(tenantId, out var quota))
                    return true;

                if (quota.MonthlyApiCallLimit > 0 && quota.MonthlyApiCallsUsed >= quota.MonthlyApiCallLimit)
                    return true;

                if (quota.DailyApiCallLimit > 0 && quota.DailyApiCallsUsed >= quota.DailyApiCallLimit)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking quota for tenant: {TenantId}", tenantId);
                return true; // Fail secure
            }
        }

        public async Task<List<TenantCustomAgent>> GetTenantAgentsAsync(string tenantId)
        {
            return _customAgents.TryGetValue(tenantId, out var agents) ? agents : new List<TenantCustomAgent>();
        }

        public async Task<TenantCustomAgent> CreateAgentAsync(TenantCustomAgent agent)
        {
            try
            {
                if (!_customAgents.ContainsKey(agent.TenantId))
                    _customAgents[agent.TenantId] = new List<TenantCustomAgent>();

                agent.AgentId = Guid.NewGuid().ToString();
                agent.CreatedDate = DateTime.UtcNow;
                agent.UpdatedDate = DateTime.UtcNow;

                _customAgents[agent.TenantId].Add(agent);
                _logger.LogInformation("Created custom agent {AgentId} for tenant {TenantId}", agent.AgentId, agent.TenantId);

                return agent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating agent for tenant: {TenantId}", agent.TenantId);
                throw;
            }
        }

        public async Task<bool> UpdateAgentAsync(TenantCustomAgent agent)
        {
            try
            {
                if (!_customAgents.TryGetValue(agent.TenantId, out var agents))
                    return false;

                var existingAgent = agents.FirstOrDefault(a => a.AgentId == agent.AgentId);
                if (existingAgent == null)
                    return false;

                agent.UpdatedDate = DateTime.UtcNow;
                var index = agents.IndexOf(existingAgent);
                agents[index] = agent;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating agent: {AgentId}", agent.AgentId);
                return false;
            }
        }

        public async Task<bool> DeleteAgentAsync(string agentId)
        {
            try
            {
                foreach (var kvp in _customAgents)
                {
                    var agent = kvp.Value.FirstOrDefault(a => a.AgentId == agentId);
                    if (agent != null)
                    {
                        kvp.Value.Remove(agent);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting agent: {AgentId}", agentId);
                return false;
            }
        }

        public async Task<List<TenantTeamMember>> GetTeamMembersAsync(string tenantId)
        {
            return _teamMembers.TryGetValue(tenantId, out var members) ? members : new List<TenantTeamMember>();
        }

        public async Task<bool> AddTeamMemberAsync(TenantTeamMember member)
        {
            try
            {
                if (!_teamMembers.ContainsKey(member.TenantId))
                    _teamMembers[member.TenantId] = new List<TenantTeamMember>();

                member.MemberId = Guid.NewGuid().ToString();
                member.JoinedDate = DateTime.UtcNow;
                member.IsActive = true;

                _teamMembers[member.TenantId].Add(member);
                _logger.LogInformation("Added team member {Email} to tenant {TenantId}", member.Email, member.TenantId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding team member to tenant: {TenantId}", member.TenantId);
                return false;
            }
        }

        public async Task<bool> RemoveTeamMemberAsync(string memberId)
        {
            try
            {
                foreach (var kvp in _teamMembers)
                {
                    var member = kvp.Value.FirstOrDefault(m => m.MemberId == memberId);
                    if (member != null)
                    {
                        kvp.Value.Remove(member);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing team member: {MemberId}", memberId);
                return false;
            }
        }

        public async Task<List<TenantUsage>> GetUsageStatsAsync(string tenantId, DateTime startDate, DateTime endDate)
        {
            return _usageStats.TryGetValue(tenantId, out var stats)
                ? stats.Where(s => s.Date >= startDate && s.Date <= endDate).ToList()
                : new List<TenantUsage>();
        }

        public async Task<TenantBillingRecord> GenerateBillingRecordAsync(string tenantId)
        {
            try
            {
                var subscription = await GetSubscriptionAsync(tenantId);
                if (subscription == null)
                    throw new InvalidOperationException($"No subscription found for tenant {tenantId}");

                var billingRecord = new TenantBillingRecord
                {
                    BillingId = Guid.NewGuid().ToString(),
                    TenantId = tenantId,
                    SubscriptionId = subscription.SubscriptionId,
                    Amount = subscription.BillingAmount,
                    BillingDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(30),
                    Status = "pending"
                };

                return billingRecord;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating billing record for tenant: {TenantId}", tenantId);
                throw;
            }
        }

        private string GenerateApiKey()
        {
            return "sk_" + Guid.NewGuid().ToString("N").Substring(0, 24);
        }

        private string GenerateSecretKey()
        {
            return "secret_" + Guid.NewGuid().ToString("N").Substring(0, 24);
        }
    }
}
