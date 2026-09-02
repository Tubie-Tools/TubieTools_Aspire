namespace ModelLayer.Models;

/// <summary>
/// Model safety settings to prevent harmful outputs.
/// </summary>
public class ModelSafetySettings
{
    /// <summary>Content filter enabled</summary>
    public bool ContentFilterEnabled { get; set; }

    /// <summary>Maximum violence content threshold</summary>
    public string ViolenceThreshold { get; set; } = "High"; // Low, Medium, High

    /// <summary>Maximum sexual content threshold</summary>
    public string SexualContentThreshold { get; set; } = "High";

    /// <summary>Maximum hate speech threshold</summary>
    public string HateSpeechThreshold { get; set; } = "High";

    /// <summary>Enable jailbreak detection</summary>
    public bool EnableJailbreakDetection { get; set; } = true;

    /// <summary>Prompt injection filtering</summary>
    public bool EnablePromptInjectionFiltering { get; set; } = true;

    /// <summary>PII redaction enabled</summary>
    public bool EnablePIIRedaction { get; set; }

    /// <summary>Approved topics only</summary>
    public bool RestrictToApprovedTopics { get; set; }

    /// <summary>List of approved topics if restricted</summary>
    public List<string> ApprovedTopics { get; set; } = new();
}
