using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Data.Entities;

/// <summary>
/// Entity model for Copilot Model Configuration - LLM settings and parameters.
/// </summary>
[Table("CopilotModelConfigurations")]
public class CopilotModelConfiguration
{
    [Key]
    [StringLength(36)]
    public string ConfigId { get; set; } = Guid.NewGuid().ToString();

    [StringLength(100)]
    public string ModelProvider { get; set; }

    [Required]
    [StringLength(255)]
    public string ModelName { get; set; }

    [StringLength(100)]
    public string ModelVersion { get; set; }

    [Range(0, 1)]
    public decimal Temperature { get; set; } = 0.7m;

    [Range(0, 1)]
    public decimal TopP { get; set; } = 0.9m;

    public int MaxTokens { get; set; } = 2000;

    [Range(-2, 2)]
    public decimal FrequencyPenalty { get; set; } = 0m;

    [Range(-2, 2)]
    public decimal PresencePenalty { get; set; } = 0m;

    [StringLength(3000)]
    public string SystemPrompt { get; set; }

    // JSON serialized custom parameters
    public string CustomParameters { get; set; }

    // JSON serialized safety settings
    public string SafetySettings { get; set; }

    public int ContextWindowSize { get; set; }

    public bool SupportsFunctionCalling { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
}
