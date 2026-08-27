namespace TubieTools_Aspire.EnterpriseAutomation.AIAgent
{
    /// <summary>
    /// Interface for AI Agent Orchestrator
    /// </summary>
    public interface IAgentOrchestrator
    {
        Task<OrchestrationResult> ExecuteWorkflowAsync(string workflowRequest);
        Task<OrchestrationResult> ExecuteMultiStepRequestAsync(string userRequest, int maxSteps = 5);
        void SetAgentContext(string contextDescription);
    }
}
