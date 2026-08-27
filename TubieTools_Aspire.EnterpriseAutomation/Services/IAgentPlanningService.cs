namespace TubieTools_Aspire.EnterpriseAutomation.Services;

using TubieTools_Aspire.EnterpriseAutomation.Models;

/// <summary>
/// Interface for AI Agent Planning Phase Operations
/// </summary>
public interface IAgentPlanningService
{
    /// <summary>
    /// Creates a comprehensive plan for a new agent
    /// </summary>
    Task<AgentPlan> CreateAgentPlanAsync(string agentId, AgentPlan plan);

    /// <summary>
    /// Updates an existing plan
    /// </summary>
    Task<AgentPlan> UpdateAgentPlanAsync(string agentId, AgentPlan plan);

    /// <summary>
    /// Gets the plan for an agent
    /// </summary>
    Task<AgentPlan> GetAgentPlanAsync(string agentId);

    /// <summary>
    /// Validates plan completeness
    /// </summary>
    Task<PlanValidationResult> ValidatePlanAsync(string agentId);

    /// <summary>
    /// Approves a plan for progression to next phase
    /// </summary>
    Task<bool> ApprovePlanAsync(string agentId, string approver, string comments);

    /// <summary>
    /// Performs risk assessment on the plan
    /// </summary>
    Task<List<RiskAssessment>> PerformRiskAssessmentAsync(string agentId);
}