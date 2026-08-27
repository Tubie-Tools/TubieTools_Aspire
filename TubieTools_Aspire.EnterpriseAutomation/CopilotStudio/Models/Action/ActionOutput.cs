namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models.Action;

/// <summary>
/// Action output schema.
/// </summary>
public class ActionOutput
{
    public string OutputId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Success response status code</summary>
    public int SuccessStatusCode { get; set; } = 200;

    /// <summary>Response fields/properties</summary>
    public Dictionary<string, string> ResponseFields { get; set; } = new();

    /// <summary>Error response schema</summary>
    public ErrorResponseSchema ErrorSchema { get; set; }
}
