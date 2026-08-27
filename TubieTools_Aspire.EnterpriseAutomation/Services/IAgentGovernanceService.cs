namespace TubieTools_Aspire.EnterpriseAutomation.Services;

using TubieTools_Aspire.EnterpriseAutomation.Models;

/// <summary>
/// Interface for AI Agent Governance and Security Operations
/// </summary>
public interface IAgentGovernanceService
{
    /// <summary>
    /// Creates governance configuration for an agent
    /// </summary>
    Task<GovernanceConfiguration> CreateGovernanceAsync(string agentId, GovernanceConfiguration governance);

    /// <summary>
    /// Updates governance configuration
    /// </summary>
    Task<GovernanceConfiguration> UpdateGovernanceAsync(string agentId, GovernanceConfiguration governance);

    /// <summary>
    /// Gets governance configuration for an agent
    /// </summary>
    Task<GovernanceConfiguration> GetGovernanceAsync(string agentId);

    /// <summary>
    /// Applies governance policies to an agent
    /// </summary>
    Task<bool> ApplyPoliciesAsync(string agentId, List<GovernancePolicy> policies);

    /// <summary>
    /// Checks compliance status against all policies
    /// </summary>
    Task<ComplianceStatus> CheckComplianceStatusAsync(string agentId);

    /// <summary>
    /// Validates approval workflow completion
    /// </summary>
    Task<bool> ValidateApprovalWorkflowAsync(string agentId);

    /// <summary>
    /// Records a compliance audit
    /// </summary>
    Task<ComplianceRecord> RecordComplianceAuditAsync(string agentId, ComplianceRecord audit);
}