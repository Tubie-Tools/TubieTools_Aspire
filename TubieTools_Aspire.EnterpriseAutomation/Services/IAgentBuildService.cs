namespace TubieTools_Aspire.EnterpriseAutomation.Services;

using TubieTools_Aspire.EnterpriseAutomation.Models;

/// <summary>
/// Interface for AI Agent Build Operations
/// </summary>
public interface IAgentBuildService
{
    /// <summary>
    /// Creates build configuration for agent
    /// </summary>
    Task<AgentBuild> CreateBuildAsync(string agentId, AgentBuild build);

    /// <summary>
    /// Triggers build pipeline
    /// </summary>
    Task<BuildPipelineExecution> TriggerBuildAsync(string agentId);

    /// <summary>
    /// Gets build status
    /// </summary>
    Task<AgentBuild> GetBuildStatusAsync(string agentId);

    /// <summary>
    /// Records test results
    /// </summary>
    Task<bool> RecordTestResultsAsync(string agentId, TestingStrategy testResults);

    /// <summary>
    /// Records deployment
    /// </summary>
    Task<DeploymentRecord> RecordDeploymentAsync(string agentId, DeploymentRecord deployment);

    /// <summary>
    /// Validates security testing completion
    /// </summary>
    Task<SecurityValidationResult> ValidateSecurityTestingAsync(string agentId);

    /// <summary>
    /// Validates model performance against criteria
    /// </summary>
    Task<ModelPerformanceValidation> ValidateModelPerformanceAsync(string agentId);
} 