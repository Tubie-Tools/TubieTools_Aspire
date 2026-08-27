namespace TubieTools_Aspire.EnterpriseAutomation.Services;

using TubieTools_Aspire.EnterpriseAutomation.Models;

/// <summary>
/// Interface for AI Agent Lifecycle Management
/// Orchestrates movement through all CAF phases
/// </summary>
public interface IAIAgentLifecycleService
{
    /// <summary>
    /// Creates a new AI agent in the planning phase
    /// </summary>
    Task<AIAgent> CreateAgentAsync(AIAgent agent);

    /// <summary>
    /// Updates current lifecycle phase for an agent
    /// </summary>
    Task<AIAgent> AdvanceToPhaseAsync(string agentId, string lifecyclePhase);

    /// <summary>
    /// Gets an agent by ID with full context
    /// </summary>
    Task<AIAgent> GetAgentAsync(string agentId);

    /// <summary>
    /// Lists all agents with optional filtering by phase
    /// </summary>
    Task<IEnumerable<AIAgent>> ListAgentsAsync(string filterByPhase = null);

    /// <summary>
    /// Validates agent readiness for next phase
    /// </summary>
    Task<PhaseReadinessAssessment> AssessPhaseReadinessAsync(string agentId);

    /// <summary>
    /// Generates phase transition report
    /// </summary>
    Task<PhaseTransitionReport> GeneratePhaseTransitionReportAsync(string agentId, string targetPhase);
}
