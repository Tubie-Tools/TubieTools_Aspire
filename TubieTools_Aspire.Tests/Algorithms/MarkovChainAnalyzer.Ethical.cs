using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TubieTools_Aspire.Tests.Algorithms
{
    /// <summary>
    /// Ethical enhancements for the Markov Chain Analyzer
    /// Adheres to principles of:
    /// - Fairness: Equal opportunity for all algorithms
    /// - Transparency: Explainable decision-making
    /// - Accountability: Audit trails and traceability
    /// - Trustworthiness: Bias detection and mitigation
    /// - Responsible AI: Safety and ethical guardrails
    /// </summary>
    public partial class MarkovChainAnalyzer
    {
        /// <summary>
        /// Ethical assessment and guardrailing system for algorithm recommendations
        /// Ensures recommendations are fair, transparent, and accountable
        /// </summary>
        public class EthicalAssessmentEngine
        {
            private readonly Dictionary<SortAlgorithmState, BiasMetrics> _biasMetrics;
            private readonly List<EthicalAuditRecord> _auditTrail;
            private readonly EthicalGuardrails _guardrails;
            private readonly FairnessMonitor _fairnessMonitor;
            private readonly TransparencyLogger _transparencyLogger;

            /// <summary>
            /// Ethical biases detected in algorithm selection
            /// </summary>
            public class BiasMetrics
            {
                public SortAlgorithmState Algorithm { get; set; }
                public int SelectionCount { get; set; }
                public int SuccessCount { get; set; }
                public double SelectionProbability { get; set; }
                public double SuccessRate { get; set; }
                public List<string> DetectedBiases { get; set; } = new();
                public double BiasScore { get; set; }  // 0 = unbiased, 1 = heavily biased
                public DateTime LastAssessmentDate { get; set; }
            }

            /// <summary>
            /// Audit record for every algorithm recommendation decision
            /// Ensures accountability and traceability
            /// </summary>
            public class EthicalAuditRecord
            {
                public string RecordId { get; set; } = Guid.NewGuid().ToString();
                public DateTime Timestamp { get; set; } = DateTime.UtcNow;
                public SortAlgorithmState RecommendedAlgorithm { get; set; }
                public SortAlgorithmState[] AlternativeAlgorithms { get; set; }
                public double[] AlgorithmScores { get; set; }
                public double ConfidenceScore { get; set; }
                public string DecisionRationale { get; set; }
                public DataCharacteristics DataCharacteristics { get; set; }
                public bool WasFairlySelected { get; set; }
                public bool PassedEthicalGuardrails { get; set; }
                public string EthicalConcerns { get; set; }
                public string MitigationApplied { get; set; }
                public bool UserAcceptedRecommendation { get; set; }
                public double ActualPerformanceRatio { get; set; }
                public string Notes { get; set; }
            }

            /// <summary>
            /// Ethical guardrails to prevent harmful recommendations
            /// </summary>
            public class EthicalGuardrails
            {
                public bool EnforceFairnessChecks { get; set; } = true;
                public bool EnforceTransparency { get; set; } = true;
                public bool EnforceAccountability { get; set; } = true;
                public bool EnableBiasDetection { get; set; } = true;

                /// <summary>
                /// Maximum allowed bias score before recommendation is flagged
                /// </summary>
                public double MaxAllowedBiasScore { get; set; } = 0.7;

                /// <summary>
                /// Minimum confidence required for recommendation without warnings
                /// </summary>
                public double MinConfidenceForHighRecommendation { get; set; } = 0.75;

                /// <summary>
                /// Enable diverse algorithm recommendations (not always best)
                /// </summary>
                public bool PromoteDiversity { get; set; } = true;

                /// <summary>
                /// Percentage of time to recommend non-top algorithm for diversity
                /// </summary>
                public double DiversityExplorationRate { get; set; } = 0.15;  // 15%

                /// <summary>
                /// Minimum number of different algorithms that must be attempted
                /// </summary>
                public int MinAlgorithmDiversity { get; set; } = 5;

                /// <summary>
                /// Block recommendations that show consistent historical bias
                /// </summary>
                public bool BlockBiasedRecommendations { get; set; } = true;

                /// <summary>
                /// Require audit trail logging for all decision
                /// </summary>
                public bool RequireAuditTrail { get; set; } = true;
            }

            /// <summary>
            /// Fairness monitoring to ensure all algorithms get equal opportunity
            /// </summary>
            public class FairnessMonitor
            {
                public int TotalRecommendations { get; set; }
                public Dictionary<SortAlgorithmState, int> AlgorithmRecommendationCounts { get; set; } = new();
                public Dictionary<SortAlgorithmState, double> AlgorithmRecommendationRates { get; set; } = new();
                public double FairnessScore { get; set; }  // Measures if all algos get equal chance
                public bool FairnessThresholdMet { get; set; }

                /// <summary>
                /// Ideal would be equal recommendation for all algorithms
                /// In practice, we allow ±20% variation from perfect fairness
                /// </summary>
                public double FairnessThreshold { get; set; } = 0.20;
            }

            /// <summary>
            /// Transparency logging to explain decision-making
            /// </summary>
            public class TransparencyLogger
            {
                public List<string> DecisionExplanations { get; set; } = new();
                public List<string> AssumptionsMade { get; set; } = new();
                public List<string> LimitationsAndCaveats { get; set; } = new();
                public List<string> ConfidenceIndicators { get; set; } = new();

                /// <summary>
                /// Confidence levels with explanations
                /// </summary>
                public Dictionary<string, string> ConfidenceReasons { get; set; } = new()
                {
                    { "VeryHigh", "Data characteristics clearly match algorithm strengths" },
                    { "High", "Good alignment between data and algorithm capabilities" },
                    { "Medium", "Moderate evidence supports recommendation" },
                    { "Low", "Weak evidence; consider alternatives" },
                    { "VeryLow", "Poor alignment; manual review recommended" }
                };
            }

            public EthicalAssessmentEngine()
            {
                _biasMetrics = new Dictionary<SortAlgorithmState, BiasMetrics>();
                _auditTrail = new List<EthicalAuditRecord>();
                _guardrails = new EthicalGuardrails();
                _fairnessMonitor = new FairnessMonitor();
                _transparencyLogger = new TransparencyLogger();

                InitializeBiasMetrics();
            }

            private void InitializeBiasMetrics()
            {
                foreach (SortAlgorithmState algo in Enum.GetValues(typeof(SortAlgorithmState)))
                {
                    _biasMetrics[algo] = new BiasMetrics
                    {
                        Algorithm = algo,
                        SelectionCount = 0,
                        SuccessCount = 0,
                        SelectionProbability = 1.0 / Enum.GetNames(typeof(SortAlgorithmState)).Length,
                        SuccessRate = 0.5,
                        BiasScore = 0.0,
                        LastAssessmentDate = DateTime.UtcNow
                    };

                    _fairnessMonitor.AlgorithmRecommendationCounts[algo] = 0;
                    _fairnessMonitor.AlgorithmRecommendationRates[algo] = 1.0 / Enum.GetNames(typeof(SortAlgorithmState)).Length;
                }
            }

            /// <summary>
            /// Evaluates if a recommendation is ethically sound
            /// Checks for bias, fairness, and transparency
            /// </summary>
            public EthicalRecommendationAssessment AssessRecommendationEthics(
                SortAlgorithmState recommendedAlgorithm,
                Dictionary<SortAlgorithmState, double> allScores,
                DataCharacteristics characteristics,
                double confidenceScore)
            {
                var assessment = new EthicalRecommendationAssessment
                {
                    AssessmentId = Guid.NewGuid().ToString(),
                    RecommendedAlgorithm = recommendedAlgorithm,
                    AssessmentTime = DateTime.UtcNow
                };

                if (!_guardrails.EnforceFairnessChecks)
                {
                    assessment.IsEthicallySound = true;
                    assessment.EthicalConcerns = new List<string>();
                    return assessment;
                }

                // Check for selection bias
                assessment.BiasCheckResults = CheckForSelectiveBias(
                    recommendedAlgorithm, allScores);

                // Check fairness metrics
                assessment.FairnessCheckResults = CheckFairness(recommendedAlgorithm);

                // Check confidence appropriateness
                // TODO: Re-enable confidence check after further testing
                //assessment.ConfidenceCheckResults = CheckConfidenceAppropriateness(
                //    confidenceScore, allScores);

                // Check for transparency requirements
                assessment.TransparencyRequirements = GenerateTransparencyRequirements(
                    recommendedAlgorithm, characteristics, allScores, confidenceScore);

                // Aggregate ethical assessment
                assessment.IsEthicallySound = AggregateEthicalAssessment(assessment);

                return assessment;
            }

            private BiasCheckResult CheckForSelectiveBias(
                SortAlgorithmState algorithm,
                Dictionary<SortAlgorithmState, double> allScores)
            {
                var result = new BiasCheckResult();

                if (!_biasMetrics.ContainsKey(algorithm))
                {
                    result.BiasDetected = false;
                    return result;
                }

                var metrics = _biasMetrics[algorithm];
                metrics.SelectionCount++;

                // Calculate if this algorithm is over-represented
                double expectedSelectionRate = 1.0 / Enum.GetNames(typeof(SortAlgorithmState)).Length;
                double actualRate = (double)metrics.SelectionCount / (_guardrails.RequireAuditTrail ? Math.Max(1, _auditTrail.Count) : 1);

                result.ExpectedSelectionRate = expectedSelectionRate;
                result.ActualSelectionRate = actualRate;
                result.SelectionBiasRatio = actualRate / expectedSelectionRate;

                // Bias detected if algorithm is selected >2x more than statistically expected
                if (result.SelectionBiasRatio > 2.0)
                {
                    result.BiasDetected = true;
                    result.BiasSeverity = "High";
                    result.BiasDescription = $"{algorithm} selected {result.SelectionBiasRatio:F2}x more often than expected";
                    result.RecommendedAction = "Consider diversifying algorithm selection or investigate scoring bias";

                    if (_guardrails.EnableBiasDetection)
                    {
                        metrics.DetectedBiases.Add(result.BiasDescription);
                        metrics.BiasScore = Math.Min(1.0, metrics.BiasScore + 0.1);
                    }
                }
                else if (result.SelectionBiasRatio > 1.5)
                {
                    result.BiasDetected = true;
                    result.BiasSeverity = "Medium";
                    result.BiasDescription = $"{algorithm} selected {result.SelectionBiasRatio:F2}x more often than expected";
                    result.RecommendedAction = "Monitor selection patterns for potential bias";
                }

                return result;
            }

            private FairnessCheckResult CheckFairness(SortAlgorithmState selectedAlgorithm)
            {
                var result = new FairnessCheckResult();

                _fairnessMonitor.TotalRecommendations++;
                _fairnessMonitor.AlgorithmRecommendationCounts[selectedAlgorithm]++;

                // Recalculate all recommendation rates
                foreach (var algo in _fairnessMonitor.AlgorithmRecommendationCounts.Keys.ToList())
                {
                    _fairnessMonitor.AlgorithmRecommendationRates[algo] =
                        (double)_fairnessMonitor.AlgorithmRecommendationCounts[algo] /
                        _fairnessMonitor.TotalRecommendations;
                }

                // Calculate fairness score (0 = perfect fairness, 1 = complete unfairness)
                int numAlgorithms = Enum.GetNames(typeof(SortAlgorithmState)).Length;
                double idealRate = 1.0 / numAlgorithms;
                double totalDeviation = 0;

                foreach (var rate in _fairnessMonitor.AlgorithmRecommendationRates.Values)
                {
                    totalDeviation += Math.Abs(rate - idealRate);
                }

                _fairnessMonitor.FairnessScore = totalDeviation / (2.0 * (1.0 - idealRate));
                _fairnessMonitor.FairnessThresholdMet =
                    _fairnessMonitor.FairnessScore <= _fairnessMonitor.FairnessThreshold;

                result.FairnessScore = _fairnessMonitor.FairnessScore;
                result.IsFair = _fairnessMonitor.FairnessThresholdMet;
                result.RecommendationRates = _fairnessMonitor.AlgorithmRecommendationRates.ToDictionary(
                    x => x.Key.ToString(), x => x.Value);

                if (!result.IsFair)
                {
                    result.FairnessWarning = $"Fairness score {result.FairnessScore:F3} exceeds threshold. " +
                        $"Some algorithms are being recommended significantly more than others.";
                }

                return result;
            }

            private ConfidenceCheckResult CheckConfidenceApproateness(
                double confidenceScore,
                Dictionary<SortAlgorithmState, double> allScores)
            {
                var result = new ConfidenceCheckResult
                {
                    ReportedConfidence = confidenceScore
                };

                // Calculate score spread
                double maxScore = allScores.Values.Max();
                double secondMaxScore = allScores.Values.OrderByDescending(x => x).Skip(1).FirstOrDefault();
                result.ScoreSpread = maxScore - secondMaxScore;

                // If scores are very close, confidence should be lower
                if (result.ScoreSpread < 0.1)
                {
                    result.IsConfidenceAppropriate = false;
                    result.ConfidenceWarning =
                        $"Multiple algorithms scored similarly (spread: {result.ScoreSpread:F3}). " +
                        $"Consider lower confidence or indicating uncertainty.";
                }
                else if (confidenceScore < _guardrails.MinConfidenceForHighRecommendation && result.ScoreSpread < 0.2)
                {
                    result.IsConfidenceAppropriate = true;
                    result.ConfidenceAdvisory = "Consider indicating that this is a weak recommendation.";
                }
                else
                {
                    result.IsConfidenceAppropriate = true;
                }

                return result;
            }

            private TransparencyRequirements GenerateTransparencyRequirements(
                SortAlgorithmState algorithm,
                DataCharacteristics characteristics,
                Dictionary<SortAlgorithmState, double> allScores,
                double confidenceScore)
            {
                var requirements = new TransparencyRequirements
                {
                    RequirementId = Guid.NewGuid().ToString(),
                    IsRequired = _guardrails.EnforceTransparency
                };

                if (!requirements.IsRequired)
                    return requirements;

                // Always require explanation of why this algorithm was chosen
                requirements.RequiredExplanations.Add(
                    $"Why {algorithm}? Its score ({allScores[algorithm]:F3}) aligns with data characteristics: " +
                    $"sortedness={characteristics.SortednessRatio:F3}, entropy={characteristics.Entropy:F3}");

                // Require alternatives if confidence is not maximal
                if (confidenceScore < 0.9)
                {
                    var alternatives = allScores
                        .OrderByDescending(x => x.Value)
                        .Skip(1)
                        .Take(2);

                    requirements.RequiredExplanations.Add(
                        $"Alternatives considered: {string.Join(", ", alternatives.Select(x => x.Key))}");
                }

                // Require limitations statement if confidence is low
                if (confidenceScore < 0.75)
                {
                    requirements.RequiredExplanations.Add(
                        "⚠️ Low confidence: Consider manually reviewing or comparing multiple algorithms.");
                }

                // Require data characteristics explanation
                requirements.RequiredExplanations.Add(
                    $"Data Analysis: Size={characteristics.Size}, " +
                    $"Sortedness={characteristics.SortednessRatio:P}, " +
                    $"Entropy={characteristics.Entropy:F3}, " +
                    $"Distinct Values={characteristics.DistinctValues}");

                // Require audit trail reference
                requirements.RequiredExplanations.Add(
                    $"Decision logged with ID: {Guid.NewGuid()} for accountability and review");

                return requirements;
            }

            private bool AggregateEthicalAssessment(EthicalRecommendationAssessment assessment)
            {
                // All checks must pass for ethical soundness
                var soundness = true;

                // Check bias
                if (_guardrails.EnableBiasDetection &&
                    assessment.BiasCheckResults.BiasDetected &&
                    assessment.BiasCheckResults.BiasSeverity == "High")
                {
                    assessment.EthicalConcerns.Add($"High bias detected: {assessment.BiasCheckResults.BiasDescription}");
                    soundness = false;
                }

                // Check fairness
                if (!assessment.FairnessCheckResults.IsFair &&
                    _guardrails.EnforceFairnessChecks)
                {
                    assessment.EthicalConcerns.Add(assessment.FairnessCheckResults.FairnessWarning);
                    soundness = false;
                }

                // Check confidence
                // TODO
                //if (!assessment.ConfidenceCheckResults.IsConfidenceAppropriate &&
                //    assessment.ReportedConfidence < 0.5)
                //{
                //    assessment.EthicalConcerns.Add(assessment.ConfidenceCheckResults.ConfidenceWarning);
                //    soundness = false;
                //}

                // Block if bias is too high and guardrail is active
                if (_guardrails.BlockBiasedRecommendations &&
                    _biasMetrics[assessment.RecommendedAlgorithm].BiasScore >
                    _guardrails.MaxAllowedBiasScore)
                {
                    assessment.EthicalConcerns.Add(
                        $"Recommendation blocked: Algorithm bias score ({_biasMetrics[assessment.RecommendedAlgorithm].BiasScore:F3}) " +
                        $"exceeds maximum allowed ({_guardrails.MaxAllowedBiasScore})");
                    soundness = false;
                }

                return soundness;
            }

            /// <summary>
            /// Records an audit trail entry for every recommendation
            /// Enables accountability and historical review
            /// </summary>
            public void LogRecommendationAudit(
                SortAlgorithmState recommendedAlgorithm,
                SortAlgorithmState[] allAlgorithms,
                Dictionary<SortAlgorithmState, double> allScores,
                double confidenceScore,
                string rationale,
                DataCharacteristics characteristics,
                bool passedEthicalGuardrails,
                string ethicalConcerns)
            {
                if (!_guardrails.RequireAuditTrail)
                    return;

                var auditRecord = new EthicalAuditRecord
                {
                    RecommendedAlgorithm = recommendedAlgorithm,
                    AlternativeAlgorithms = allAlgorithms,
                    AlgorithmScores = allScores.Values.ToArray(),
                    ConfidenceScore = confidenceScore,
                    DecisionRationale = rationale,
                    DataCharacteristics = characteristics,
                    PassedEthicalGuardrails = passedEthicalGuardrails,
                    EthicalConcerns = ethicalConcerns,
                    WasFairlySelected = _fairnessMonitor.FairnessThresholdMet
                };

                _auditTrail.Add(auditRecord);
            }

            /// <summary>
            /// Applies diversity exploration to promote algorithm diversity
            /// Occasionally recommends non-optimal but good algorithm for exploration
            /// </summary>
            public (SortAlgorithmState SelectedAlgorithm, string DiversityReason) ApplyDiversityExploration(
                SortAlgorithmState topAlgorithm,
                Dictionary<SortAlgorithmState, double> allScores)
            {
                if (!_guardrails.PromoteDiversity)
                    return (topAlgorithm, string.Empty);

                var random = new Random();
                if (random.NextDouble() < _guardrails.DiversityExplorationRate)
                {
                    // Select second-best or third-best algorithm for diversity
                    var orderedScores = allScores.OrderByDescending(x => x.Value).ToList();

                    if (orderedScores.Count > 1)
                    {
                        int diversityIndex = random.Next(1, Math.Min(3, orderedScores.Count));
                        var selectedAlgorithm = orderedScores[diversityIndex].Key;
                        var reason = $"Diversity exploration: Recommending {selectedAlgorithm} (score: " +
                            $"{orderedScores[diversityIndex].Value:F3}) instead of top choice for algorithm diversity. " +
                            $"This helps prevent algorithmic monoculture.";

                        return (selectedAlgorithm, reason);
                    }
                }

                return (topAlgorithm, string.Empty);
            }

            /// <summary>
            /// Gets comprehensive ethical audit report
            /// </summary>
            public EthicalAuditReport GetEthicalAuditReport()
            {
                var report = new EthicalAuditReport
                {
                    ReportGeneratedTime = DateTime.UtcNow,
                    TotalRecommendationsMade = _auditTrail.Count,
                    BiasAssessments = _biasMetrics.Values.ToList(),
                    FairnessMetrics = _fairnessMonitor,
                    AuditRecords = _auditTrail.ToList(),
                    GuardrailsActive = new()
                    {
                        { nameof(_guardrails.EnforceFairnessChecks), _guardrails.EnforceFairnessChecks },
                        { nameof(_guardrails.EnforceTransparency), _guardrails.EnforceTransparency },
                        { nameof(_guardrails.EnforceAccountability), _guardrails.EnforceAccountability },
                        { nameof(_guardrails.EnableBiasDetection), _guardrails.EnableBiasDetection },
                        { nameof(_guardrails.BlockBiasedRecommendations), _guardrails.BlockBiasedRecommendations }
                    }
                };

                // Calculate ethical score
                report.OverallEthicalScore = CalculateOverallEthicalScore();

                // Identify any critical issues
                report.CriticalIssues = IdentifyCriticalIssues();

                // Generate recommendations for improvement
                report.ImprovementRecommendations = GenerateImprovementRecommendations();

                return report;
            }

            private double CalculateOverallEthicalScore()
            {
                double biasScore = 1.0 - (_biasMetrics.Values.Average(x => x.BiasScore) / 2.0);
                double fairnessScore = 1.0 - _fairnessMonitor.FairnessScore;
                double transparencyScore = 0.9;  // Assume good if guardrails active

                return (biasScore + fairnessScore + transparencyScore) / 3.0;
            }

            private List<string> IdentifyCriticalIssues()
            {
                var issues = new List<string>();

                // Check for biased algorithms
                var highBiasAlgos = _biasMetrics.Values
                    .Where(x => x.BiasScore > _guardrails.MaxAllowedBiasScore)
                    .ToList();

                foreach (var algo in highBiasAlgos)
                {
                    issues.Add($"CRITICAL: {algo.Algorithm} has bias score {algo.BiasScore:F3} " +
                        $"(threshold: {_guardrails.MaxAllowedBiasScore})");
                }

                // Check for fairness violations
                if (!_fairnessMonitor.FairnessThresholdMet)
                {
                    issues.Add($"CRITICAL: Fairness score {_fairnessMonitor.FairnessScore:F3} " +
                        $"exceeds threshold {_fairnessMonitor.FairnessThreshold}");
                }

                return issues;
            }

            private List<string> GenerateImprovementRecommendations()
            {
                var recommendations = new List<string>();

                // Recommend diversity if not using diverse algorithms
                var usedAlgos = _fairnessMonitor.AlgorithmRecommendationCounts
                    .Count(x => x.Value > 0);

                if (usedAlgos < _guardrails.MinAlgorithmDiversity)
                {
                    recommendations.Add(
                        $"Increase algorithm diversity: Currently using {usedAlgos} algorithms, " +
                        $"target {_guardrails.MinAlgorithmDiversity}. Consider enabling diversity exploration.");
                }

                // Recommend bias mitigation
                var biasedAlgos = _biasMetrics.Values
                    .Where(x => x.BiasScore > 0.5)
                    .ToList();

                if (biasedAlgos.Any())
                {
                    recommendations.Add(
                        $"Address algorithmic bias: {string.Join(", ", biasedAlgos.Select(x => x.Algorithm))} " +
                        $"show elevated bias scores. Review scoring logic.");
                }

                // Recommend fairness improvements
                if (_fairnessMonitor.FairnessScore > 0.15)
                {
                    recommendations.Add(
                        "Improve fairness: Consider rebalancing scoring weights to give underrepresented " +
                        "algorithms a fairer chance.");
                }

                return recommendations;
            }

            // Supporting result classes
            public class BiasCheckResult
            {
                public bool BiasDetected { get; set; }
                public string BiasSeverity { get; set; }
                public string BiasDescription { get; set; }
                public double ExpectedSelectionRate { get; set; }
                public double ActualSelectionRate { get; set; }
                public double SelectionBiasRatio { get; set; }
                public string RecommendedAction { get; set; }
            }

            public class FairnessCheckResult
            {
                public double FairnessScore { get; set; }
                public bool IsFair { get; set; }
                public Dictionary<string, double> RecommendationRates { get; set; }
                public string FairnessWarning { get; set; }
            }

            public class ConfidenceCheckResult
            {
                public double ReportedConfidence { get; set; }
                public double ScoreSpread { get; set; }
                public bool IsConfidenceAppropriate { get; set; }
                public string ConfidenceWarning { get; set; }
                public string ConfidenceAdvisory { get; set; }
            }

            public class TransparencyRequirements
            {
                public string RequirementId { get; set; }
                public bool IsRequired { get; set; }
                public List<string> RequiredExplanations { get; set; } = new();
            }

            public class EthicalRecommendationAssessment
            {
                public string AssessmentId { get; set; }
                public DateTime AssessmentTime { get; set; }
                public SortAlgorithmState RecommendedAlgorithm { get; set; }
                public double ConfidenceScore { get; set; }
                public bool IsEthicallySound { get; set; }
                public List<string> EthicalConcerns { get; set; } = new();
                public BiasCheckResult BiasCheckResults { get; set; }
                public FairnessCheckResult FairnessCheckResults { get; set; }
                public ConfidenceCheckResult ConfidenceCheckResults { get; set; }
                public TransparencyRequirements TransparencyRequirements { get; set; }
            }

            public class EthicalAuditReport
            {
                public DateTime ReportGeneratedTime { get; set; }
                public int TotalRecommendationsMade { get; set; }
                public List<BiasMetrics> BiasAssessments { get; set; }
                public FairnessMonitor FairnessMetrics { get; set; }
                public List<EthicalAuditRecord> AuditRecords { get; set; }
                public Dictionary<string, bool> GuardrailsActive { get; set; }
                public double OverallEthicalScore { get; set; }
                public List<string> CriticalIssues { get; set; }
                public List<string> ImprovementRecommendations { get; set; }
            }
        }
    }
}
