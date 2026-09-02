namespace ModelLayer.Models;

/// <summary>
/// Copilot model configuration (underlying LLM and settings).
/// </summary>
public class CopilotModelConfiguration
{
    public string ConfigId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Model provider (OpenAI, Anthropic, Custom, etc.)</summary>
    public string ModelProvider { get; set; }

    /// <summary>Model name/identifier</summary>
    public string ModelName { get; set; }

    /// <summary>Model version</summary>
    public string ModelVersion { get; set; }

    /// <summary>Temperature (0-1, controls randomness)</summary>
    public decimal Temperature { get; set; } = 0.7m;

    /// <summary>Top-p value (nucleus sampling)</summary>
    public decimal TopP { get; set; } = 0.9m;

    /// <summary>Max tokens for response</summary>
    public int MaxTokens { get; set; } = 2000;

    /// <summary>Frequency penalty</summary>
    public decimal FrequencyPenalty { get; set; } = 0m;

    /// <summary>Presence penalty</summary>
    public decimal PresencePenalty { get; set; } = 0m;

    /// <summary>System prompt/instructions for the model</summary>
    public string SystemPrompt { get; set; }

    /// <summary>Custom parameters specific to model</summary>
    public Dictionary<string, object> CustomParameters { get; set; } = new();

    /// <summary>Model safety settings</summary>
    public ModelSafetySettings SafetySettings { get; set; }

    /// <summary>Context window size</summary>
    public int ContextWindowSize { get; set; }

    /// <summary>Supports function calling</summary>
    public bool SupportsFunctionCalling { get; set; }
    public object Id { get; set; }
}
