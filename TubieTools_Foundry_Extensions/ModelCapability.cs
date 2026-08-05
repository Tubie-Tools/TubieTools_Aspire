namespace TubieTools_Foundry_Extensions.Models;

/// <summary>
/// Defines the capabilities of a custom AI model.
/// </summary>
public class ModelCapability
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ModelTask Task { get; set; }
    public float? ConfidenceThreshold { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = [];
}
