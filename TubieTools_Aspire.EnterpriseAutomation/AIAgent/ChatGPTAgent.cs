using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TubieTools_Aspire.EnterpriseAutomation.AIAgent
{
    /// <summary>
    /// ChatGPT-based AI Agent implementation
    /// </summary>
    public class ChatGPTAgent : IAIAgent
    {
        private readonly IMCPClient _mcpClient;
        private readonly ChatGPTAgentConfig _config;
        private readonly ILogger<ChatGPTAgent> _logger;
        private readonly HttpClient _httpClient;
        private List<AgentMessage> _conversationHistory;
        private const string OpenAIApiUrl = "https://api.openai.com/v1/chat/completions";

        public ChatGPTAgent(IMCPClient mcpClient, ChatGPTAgentConfig config, HttpClient httpClient, ILogger<ChatGPTAgent> logger)
        {
            _mcpClient = mcpClient;
            _config = config;
            _httpClient = httpClient;
            _logger = logger;
            _conversationHistory = new List<AgentMessage>();

            _logger.LogInformation("ChatGPTAgent initialized with model: {Model}", _config.Model);
        }

        public async Task<AgentResponse> ProcessRequestAsync(string userRequest)
        {
            var tools = await _mcpClient.GetAvailableToolsAsync();
            return await ProcessRequestAsync(userRequest, tools);
        }

        public async Task<AgentResponse> ProcessRequestAsync(string userRequest, List<AIChatTool> availableTools)
        {
            try
            {
                _logger.LogInformation("Processing user request: {Request}", userRequest);

                // Add user message to history
                var userMessage = new AgentMessage
                {
                    Role = "user",
                    Content = userRequest
                };
                _conversationHistory.Add(userMessage);

                // Create system message for agent role
                var systemMessage = new AgentMessage
                {
                    Role = "system",
                    Content = "You are an AI assistant that helps manage ServiceNow incidents. Use the available tools to create, search, and close incidents based on user requests. Always confirm actions with the user before executing critical operations."
                };

                // Call ChatGPT
                var chatGPTRequest = new
                {
                    model = _config.Model,
                    messages = BuildMessages(systemMessage),
                    temperature = (double)_config.Temperature,
                    max_tokens = _config.MaxTokens,
                    tools = BuildToolDefinitions(availableTools)
                };

                var response = await CallChatGPTAsync(chatGPTRequest);

                if (!response.Success)
                {
                    return new AgentResponse
                    {
                        Success = false,
                        Message = response.Message,
                        ConversationHistory = _conversationHistory
                    };
                }

                // Parse tool calls and execute them
                var toolResults = await HandleToolCalls(response.ToolCalls);

                // Add assistant response to history
                var assistantMessage = new AgentMessage
                {
                    Role = "assistant",
                    Content = response.Content
                };
                _conversationHistory.Add(assistantMessage);

                return new AgentResponse
                {
                    Success = true,
                    Message = response.Content,
                    Result = toolResults,
                    ExecutedTools = response.ToolCalls.Select(t => t.ToolName).ToList(),
                    ConversationHistory = _conversationHistory
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request");
                return new AgentResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    ConversationHistory = _conversationHistory
                };
            }
        }

        private async Task<ChatGPTResponse> CallChatGPTAsync(object request)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config.ApiKey);

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(request),
                    System.Text.Encoding.UTF8,
                    "application/json");

                var httpResponse = await _httpClient.PostAsync(OpenAIApiUrl, jsonContent);
                var responseContent = await httpResponse.Content.ReadAsStringAsync();

                if (!httpResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("ChatGPT API error: {StatusCode} - {Content}", httpResponse.StatusCode, responseContent);
                    return new ChatGPTResponse
                    {
                        Success = false,
                        Message = $"ChatGPT API returned {httpResponse.StatusCode}: {responseContent}"
                    };
                }

                var parsedResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var message = parsedResponse.GetProperty("choices")[0].GetProperty("message");

                var toolCalls = new List<ToolCall>();
                if (message.TryGetProperty("tool_calls", out var toolCallsElement))
                {
                    toolCalls = ParseToolCalls(toolCallsElement);
                }

                var content = message.GetProperty("content").GetString() ?? string.Empty;

                return new ChatGPTResponse
                {
                    Success = true,
                    Content = content,
                    ToolCalls = toolCalls
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling ChatGPT API");
                return new ChatGPTResponse
                {
                    Success = false,
                    Message = $"Error calling ChatGPT: {ex.Message}"
                };
            }
        }

        private List<ToolCall> ParseToolCalls(JsonElement toolCallsElement)
        {
            var toolCalls = new List<ToolCall>();

            foreach (var toolCall in toolCallsElement.EnumerateArray())
            {
                var function = toolCall.GetProperty("function");
                var toolName = function.GetProperty("name").GetString();
                var argsJson = function.GetProperty("arguments").GetString();
                var args = JsonSerializer.Deserialize<Dictionary<string, object>>(argsJson) ?? new Dictionary<string, object>();

                toolCalls.Add(new ToolCall
                {
                    ToolName = toolName,
                    Parameters = args,
                    CallId = toolCall.GetProperty("id").GetString()
                });
            }

            return toolCalls;
        }

        private async Task<Dictionary<string, object>> HandleToolCalls(List<ToolCall> toolCalls)
        {
            var results = new Dictionary<string, object>();

            foreach (var toolCall in toolCalls)
            {
                try
                {
                    _logger.LogInformation("Executing tool call: {ToolName}", toolCall.ToolName);
                    var result = await _mcpClient.InvokeToolAsync(toolCall.ToolName, toolCall.Parameters);
                    results[toolCall.CallId] = result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing tool call: {ToolName}", toolCall.ToolName);
                    results[toolCall.CallId] = new { error = ex.Message };
                }
            }

            return results;
        }

        private List<object> BuildMessages(AgentMessage systemMessage)
        {
            var messages = new List<object>();

            // Add system message
            messages.Add(new
            {
                role = systemMessage.Role,
                content = systemMessage.Content
            });

            // Add conversation history
            foreach (var msg in _conversationHistory)
            {
                messages.Add(new
                {
                    role = msg.Role,
                    content = msg.Content
                });
            }

            return messages;
        }

        private List<object> BuildToolDefinitions(List<AIChatTool> tools)
        {
            var toolDefinitions = new List<object>();

            foreach (var tool in tools)
            {
                toolDefinitions.Add(new
                {
                    type = "function",
                    function = new
                    {
                        name = tool.Name,
                        description = tool.Description,
                        parameters = new
                        {
                            type = "object",
                            properties = tool.Parameters,
                            required = GetRequiredParameters(tool.Name)
                        }
                    }
                });
            }

            return toolDefinitions;
        }

        private List<string> GetRequiredParameters(string toolName)
        {
            return toolName switch
            {
                "create_incident" => new List<string> { "title", "description" },
                "search_incident" => new List<string>(),
                "close_incident" => new List<string> { "incident_number" },
                _ => new List<string>()
            };
        }

        public void SetConversationContext(List<AgentMessage> history)
        {
            _conversationHistory = history ?? new List<AgentMessage>();
            _logger.LogInformation("Conversation context set with {Count} messages", _conversationHistory.Count);
        }

        public List<AgentMessage> GetConversationHistory()
        {
            return _conversationHistory;
        }

        public void ClearConversation()
        {
            _conversationHistory.Clear();
            _logger.LogInformation("Conversation history cleared");
        }

        private class ChatGPTResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public string Content { get; set; }
            public List<ToolCall> ToolCalls { get; set; } = new();
        }
    }
}
