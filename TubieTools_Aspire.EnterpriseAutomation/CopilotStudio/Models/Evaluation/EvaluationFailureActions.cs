namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models.Evaluation;

/// <summary>
/// Actions to take on evaluation failure.
/// </summary>
public class EvaluationFailureActions
{
    /// <summary>Block execution</summary>
    public bool BlockExecution { get; set; }

    /// <summary>Log the failure</summary>
    public bool LogFailure { get; set; } = true;

    /// <summary>Alert/notify</summary>
    public bool SendAlert { get; set; }

    /// <summary>Alert recipients</summary>
    public List<string> AlertRecipients { get; set; } = new();

    /// <summary>Require human review</summary>
    public bool RequireHumanReview { get; set; }

    /// <summary>Fallback action/response</summary>
    public string FallbackAction { get; set; }

    /// <summary>Retry logic</summary>
    public RetryConfig RetryLogic { get; set; }
}
