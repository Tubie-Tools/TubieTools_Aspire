namespace TubieTools_Foundry_Extensions.Models;

/// <summary>
/// Represents a custom AI model managed by the Foundry extension.
/// </summary>
public class CustomAiModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ModelProvider Provider { get; set; }
    public string ModelType { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public Dictionary<string, object> Configuration { get; set; } = [];
    public ModelStatus Status { get; set; } = ModelStatus.Inactive;
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
