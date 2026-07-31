using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TubieTools_Aspire.EnterpriseAutomation.AIAgent;

namespace TubieTools_Aspire.EnterpriseAutomation.Controllers
{
    /// <summary>
    /// API controller for AI Agent operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AIAgentController : ControllerBase
    {
        private readonly IAIAgent _agent;
        private readonly ILogger<AIAgentController> _logger;

        public AIAgentController(IAIAgent agent, ILogger<AIAgentController> logger)
        {
            _agent = agent;
            _logger = logger;
        }

        /// <summary>
        /// Send a request to the AI Agent
        /// </summary>
        /// <param name="request">User request</param>
        /// <returns>Agent response with executed tools and results</returns>
        [HttpPost("ask")]
        public async Task<IActionResult> AskAgent([FromBody] AskAgentRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Message))
                {
                    return BadRequest(new { error = "Message is required" });
                }

                _logger.LogInformation("Received agent request: {Message}", request.Message);

                var response = await _agent.ProcessRequestAsync(request.Message);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing agent request");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get current conversation history
        /// </summary>
        /// <returns>Conversation history</returns>
        [HttpGet("history")]
        public IActionResult GetHistory()
        {
            try
            {
                var history = _agent.GetConversationHistory();
                return Ok(new { messages = history });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversation history");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Clear conversation history
        /// </summary>
        /// <returns>Success result</returns>
        [HttpPost("clear")]
        public IActionResult ClearHistory()
        {
            try
            {
                _agent.ClearConversation();
                return Ok(new { message = "Conversation history cleared" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing conversation history");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Set conversation context from existing history
        /// </summary>
        /// <param name="request">Request with conversation history</param>
        /// <returns>Success result</returns>
        [HttpPost("set-context")]
        public IActionResult SetContext([FromBody] SetContextRequest request)
        {
            try
            {
                _agent.SetConversationContext(request?.Messages);
                return Ok(new { message = "Conversation context set successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting conversation context");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class SetContextRequest
    {
        public List<AgentMessage> Messages { get; set; }
    }
}
