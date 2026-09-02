namespace ModelLayer.Models.Evaluation;

/// <summary>
/// Evaluation implementation details.
/// </summary>
public class EvaluationImplementation
{
    /// <summary>Evaluation framework/library used</summary>
    public string Framework { get; set; }

    /// <summary>Model for evaluation (if using ML-based evaluation)</summary>
    public string EvaluationModel { get; set; }

    /// <summary>Reference/golden data set</summary>
    public string ReferenceDataset { get; set; }

    /// <summary>Evaluation parameters</summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>Metrics to track</summary>
    public List<string> TrackedMetrics { get; set; } = new();

    /// <summary>Query for evaluation (if database-backed)</summary>
    public string EvaluationQuery { get; set; }
}
