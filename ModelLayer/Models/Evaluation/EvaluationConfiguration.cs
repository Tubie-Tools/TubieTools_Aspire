namespace ModelLayer.Models.Evaluation;

/// <summary>
/// Represents an Evaluation/Quality Check Configuration.
/// </summary>
public class EvaluationConfiguration
{
    public string EvaluationId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Evaluation name</summary>
    public string Name { get; set; }

    /// <summary>Evaluation description</summary>
    public string Description { get; set; }

    /// <summary>Evaluation pattern (SemanticSimilarity, Compliance, Quality, Safety, etc.)</summary>
    public string Pattern { get; set; }

    /// <summary>Evaluation implementation details</summary>
    public EvaluationImplementation Implementation { get; set; }

    /// <summary>Scoring/grading model</summary>
    public ScoringModel ScoringModel { get; set; }

    /// <summary>Pass/Fail threshold</summary>
    public decimal PassThreshold { get; set; } = 0.7m;

    /// <summary>Warning threshold (below pass but above critical)</summary>
    public decimal WarningThreshold { get; set; } = 0.5m;

    /// <summary>Actions on evaluation failure</summary>
    public EvaluationFailureActions FailureActions { get; set; }

    /// <summary>Monitoring and alerting</summary>
    public EvaluationMonitoring Monitoring { get; set; }

    /// <summary>Is enabled</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Applied to phases (Planning, Testing, Production)</summary>
    public List<string> AppliedToPhases { get; set; } = new();

    /// <summary>Evaluation results history</summary>
    public List<EvaluationResult> ResultsHistory { get; set; } = new();

    /// <summary>Created date</summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public object CopilotApplicationId { get; set; }
    public object? Id { get; set; }
}
