namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models.Action;

/// <summary>
/// Action schema for parameter validation.
/// </summary>
public class ActionSchema
{
    /// <summary>Input parameters</summary>
    public List<ActionParameter> InputParameters { get; set; } = new();

    /// <summary>Output response schema</summary>
    public ActionOutput OutputSchema { get; set; }

    /// <summary>Supported response formats</summary>
    public List<string> SupportedFormats { get; set; } = new();
}
