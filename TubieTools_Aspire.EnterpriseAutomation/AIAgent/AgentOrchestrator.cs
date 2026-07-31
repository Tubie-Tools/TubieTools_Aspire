using Microsoft.Extensions.Logging;

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

    /// <summary>
    /// Agent Orchestrator result
    /// </summary>
    public class OrchestrationResult
    {
        public bool Success { get; set; }
        public string FinalMessage { get; set; }
        public List<string> ExecutedSteps { get; set; } = new();
        public Dictionary<string, object> FinalResult { get; set; }
        public int StepsExecuted { get; set; }
        public TimeSpan ExecutionTime { get; set; }
    }

    /// <summary>
    /// Agent Orchestrator implementation
    /// </summary>
    public class AgentOrchestrator : IAgentOrchestrator
    {
        private readonly IAIAgent _agent;
        private readonly ILogger<AgentOrchestrator> _logger;
        private string _agentContext;

        public AgentOrchestrator(IAIAgent agent, ILogger<AgentOrchestrator> logger)
        {
            _agent = agent;
            _logger = logger;
            _agentContext = "ServiceNow Incident Management";
        }

        public async Task<OrchestrationResult> ExecuteWorkflowAsync(string workflowRequest)
        {
            var startTime = DateTime.UtcNow;
            try
            {
                _logger.LogInformation("Starting workflow execution: {Request}", workflowRequest);

                var response = await _agent.ProcessRequestAsync(workflowRequest);

                var result = new OrchestrationResult
                {
                    Success = response.Success,
                    FinalMessage = response.Message,
                    ExecutedSteps = response.ExecutedTools,
                    FinalResult = response.Result as Dictionary<string, object>,
                    StepsExecuted = response.ExecutedTools.Count,
                    ExecutionTime = DateTime.UtcNow - startTime
                };

                _logger.LogInformation("Workflow completed in {Duration}ms with {Steps} steps", 
                    result.ExecutionTime.TotalMilliseconds, result.StepsExecuted);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing workflow");
                return new OrchestrationResult
                {
                    Success = false,
                    FinalMessage = $"Workflow failed: {ex.Message}",
                    ExecutionTime = DateTime.UtcNow - startTime
                };
            }
        }

        public async Task<OrchestrationResult> ExecuteMultiStepRequestAsync(string userRequest, int maxSteps = 5)
        {
            var startTime = DateTime.UtcNow;
            var executedSteps = new List<string>();
            var stepCount = 0;

            try
            {
                _logger.LogInformation("Starting multi-step workflow: {Request}", userRequest);

                // Initial request
                var response = await _agent.ProcessRequestAsync(userRequest);
                executedSteps.Add($"Step 1: Initial request processed");
                stepCount++;

                if (!response.Success || stepCount >= maxSteps)
                {
                    return new OrchestrationResult
                    {
                        Success = response.Success,
                        FinalMessage = response.Message,
                        ExecutedSteps = executedSteps,
                        FinalResult = response.Result as Dictionary<string, object>,
                        StepsExecuted = stepCount,
                        ExecutionTime = DateTime.UtcNow - startTime
                    };
                }

                // Follow-up processing if needed
                var followUpPrompt = $"Based on the previous action result, please provide a summary and any recommended next steps. Original request: {userRequest}";
                var followUpResponse = await _agent.ProcessRequestAsync(followUpPrompt);
                executedSteps.Add($"Step 2: Follow-up analysis completed");
                stepCount++;

                return new OrchestrationResult
                {
                    Success = followUpResponse.Success,
                    FinalMessage = followUpResponse.Message,
                    ExecutedSteps = executedSteps,
                    FinalResult = followUpResponse.Result as Dictionary<string, object>,
                    StepsExecuted = stepCount,
                    ExecutionTime = DateTime.UtcNow - startTime
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing multi-step workflow");
                return new OrchestrationResult
                {
                    Success = false,
                    FinalMessage = $"Multi-step workflow failed: {ex.Message}",
                    ExecutedSteps = executedSteps,
                    StepsExecuted = stepCount,
                    ExecutionTime = DateTime.UtcNow - startTime
                };
            }
        }

        public void SetAgentContext(string contextDescription)
        {
            _agentContext = contextDescription;
            _logger.LogInformation("Agent context set to: {Context}", _agentContext);
        }
    }
}
