namespace ModelLayer.Models;

/// <summary>
/// Scoring model for evaluation results.
/// </summary>
public class ScoringModel
{
    /// <summary>Scoring method (Numeric, BooleanPass/Fail, Weighted, Normalized)</summary>
    public string Method { get; set; } = "Numeric";

    /// <summary>Scale (0-1, 0-100, etc.)</summary>
    public string Scale { get; set; } = "0-1";

    /// <summary>Weight factors for multi-criteria evaluation</summary>
    public Dictionary<string, decimal> WeightFactors { get; set; } = new();

    /// <summary>Normalization function if applicable</summary>
    public string NormalizationFunction { get; set; }
}
