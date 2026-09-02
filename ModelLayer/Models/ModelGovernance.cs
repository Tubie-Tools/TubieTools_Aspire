namespace ModelLayer.Models;

/// <summary>
/// Model governance for AI components.
/// </summary>
public class ModelGovernance
{
    /// <summary>Approved model providers</summary>
    public List<string> ApprovedProviders { get; set; } = new();

    /// <summary>Approved model names/versions</summary>
    public List<string> ApprovedModels { get; set; } = new();

    /// <summary>Custom models allowed</summary>
    public bool CustomModelsAllowed { get; set; }

    /// <summary>Fine-tuning allowed</summary>
    public bool FineTuningAllowed { get; set; }

    /// <summary>Model training data sourcing requirements</summary>
    public string TrainingDataSourceRequirements { get; set; }

    /// <summary>Bias assessment required</summary>
    public bool BiasAssessmentRequired { get; set; }

    /// <summary>Fairness testing required</summary>
    public bool FairnessTestingRequired { get; set; }

    /// <summary>Explainability requirement</summary>
    public bool ExplainabilityRequired { get; set; }

    /// <summary>Regular model performance monitoring</summary>
    public string PerformanceMonitoringFrequency { get; set; } // Daily, Weekly, Monthly

    /// <summary>Model drift detection required</summary>
    public bool DriftDetectionRequired { get; set; }

    /// <summary>Model retraining SLA</summary>
    public string RetrainingAgreement { get; set; }

    /// <summary>Model versioning required</summary>
    public bool VersioningRequired { get; set; }

    /// <summary>Model rollback capability required</summary>
    public bool RollbackCapabilityRequired { get; set; }

    /// <summary>A/B testing for model updates</summary>
    public bool ABTestingRequired { get; set; }
}
