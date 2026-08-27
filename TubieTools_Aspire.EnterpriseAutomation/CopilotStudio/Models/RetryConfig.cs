namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

/// <summary>
/// Retry configuration for failed actions.
/// </summary>
public class RetryConfig
{
    /// <summary>Retries enabled</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Max retry attempts</summary>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>Initial backoff (milliseconds)</summary>
    public int InitialBackoffMs { get; set; } = 1000;

    /// <summary>Max backoff (milliseconds)</summary>
    public int MaxBackoffMs { get; set; } = 30000;

    /// <summary>Backoff multiplier</summary>
    public decimal BackoffMultiplier { get; set; } = 2m;

    /// <summary>Retry on status codes</summary>
    public List<int> RetryOnStatusCodes { get; set; } = new() { 408, 429, 500, 502, 503, 504 };
}
