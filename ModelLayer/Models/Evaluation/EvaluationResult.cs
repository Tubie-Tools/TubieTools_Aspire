namespace ModelLayer.Models.Evaluation;

/// <summary>
/// Result of a single evaluation run.
/// </summary>
public class EvaluationResult
{
    public string ResultId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Evaluation ID this result belongs to</summary>
    public string EvaluationId { get; set; }

    /// <summary>Copilot ID being evaluated</summary>
    public string CopilotId { get; set; }

    /// <summary>Evaluation timestamp</summary>
    public DateTime EvaluationTime { get; set; }

    /// <summary>Score (0-1 or 0-100 depending on scale)</summary>
    public decimal Score { get; set; }

    /// <summary>Passed threshold</summary>
    public bool Passed { get; set; }

    /// <summary>Detailed results/breakdown</summary>
    public Dictionary<string, object> DetailedResults { get; set; } = new();

    /// <summary>Issues/warnings identified</summary>
    public List<string> Issues { get; set; } = new();

    /// <summary>Recommendations</summary>
    public List<string> Recommendations { get; set; } = new();

    /// <summary>Sample data used for evaluation</summary>
    public int SampleSize { get; set; }
}
