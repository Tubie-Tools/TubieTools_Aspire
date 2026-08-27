namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

/// <summary>
/// Error handling configuration.
/// </summary>
public class ErrorHandlingConfig
{
    /// <summary>Fallback response</summary>
    public string FallbackResponse { get; set; }

    /// <summary>Continue on error flag</summary>
    public bool ContinueOnError { get; set; }

    /// <summary>Log errors</summary>
    public bool LogErrors { get; set; } = true;

    /// <summary>Alert on critical errors</summary>
    public bool AlertOnCriticalErrors { get; set; }

    /// <summary>Circuits breaker enabled</summary>
    public bool EnableCircuitBreaker { get; set; }

    /// <summary>Circuit breaker failure threshold</summary>
    public int CircuitBreakerFailureThreshold { get; set; } = 5;

    /// <summary>Circuit breaker recovery timeout (seconds)</summary>
    public int CircuitBreakerTimeoutSeconds { get; set; } = 60;
}
