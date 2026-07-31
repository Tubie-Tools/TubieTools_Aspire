namespace TubieTools_Aspire.EnterpriseAutomation.AIAgent
{
    /// <summary>
    /// Configuration for ChatGPT AI Agent
    /// </summary>
    public class ChatGPTAgentConfig
    {
        public string ApiKey { get; set; }
        public string Model { get; set; } = "gpt-4";
        public decimal Temperature { get; set; } = 0.7m;
        public int MaxTokens { get; set; } = 2000;
    }

    /// <summary>
    /// Represents a tool that can be called by the AI Agent
    /// </summary>
    public class AIChatTool
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new();
    }

    /// <summary>
    /// Represents a message in the conversation
    /// </summary>
    public class AgentMessage
    {
        public string Role { get; set; } // "user", "assistant", "system"
        public string Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Response from the AI Agent
    /// </summary>
    public class AgentResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Result { get; set; }
        public List<string> ExecutedTools { get; set; } = new();
        public List<AgentMessage> ConversationHistory { get; set; } = new();
    }

    /// <summary>
    /// Tool call request from ChatGPT
    /// </summary>
    public class ToolCall
    {
        public string ToolName { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public string CallId { get; set; }
    }

    /// <summary>
    /// AI Agent interface
    /// </summary>
    public interface IAIAgent
    {
        Task<AgentResponse> ProcessRequestAsync(string userRequest);
        Task<AgentResponse> ProcessRequestAsync(string userRequest, List<AIChatTool> availableTools);
        void SetConversationContext(List<AgentMessage> history);
        List<AgentMessage> GetConversationHistory();
        void ClearConversation();
    }

    /// <summary>
    /// MCP Client interface for tool invocation
    /// </summary>
    public interface IMCPClient
    {
        Task<object> InvokeToolAsync(string toolName, Dictionary<string, object> parameters);
        Task<List<AIChatTool>> GetAvailableToolsAsync();
        bool IsConnected { get; }
    }
}
