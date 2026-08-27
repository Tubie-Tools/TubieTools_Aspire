namespace TubieTools_Aspire.EnterpriseAutomation.AIAgent
{
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
}
