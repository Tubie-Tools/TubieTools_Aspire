using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TubieTools_Aspire.Tests.Algorithms
{
    /// <summary>
    /// Ethically-Enhanced Adaptive Sorting Service
    /// Integrates ethical assessment, fairness checking, transparency, and accountability
    /// into the algorithm selection and execution process.
    /// 
    /// Adheres to principles:
    /// - FAIRNESS: Ensures all algorithms get equal opportunity
    /// - TRANSPARENCY: Explains every decision with clear rationale
    /// - ACCOUNTABILITY: Complete audit trail of all recommendations
    /// - TRUSTWORTHINESS: Detects and mitigates bias
    /// - RESPONSIBILITY: Safe guardrails prevent harmful recommendations
    /// </summary>
    public class EthicallyEnhancedAdaptiveSortingService : AdaptiveSortingService
    {
        private readonly MarkovChainAnalyzer.EthicalAssessmentEngine _ethicalEngine;
        private readonly List<EthicalDecisionContext> _decisionHistory;

        /// <summary>
        /// Context for each sorting decision with full ethical audit trail
        /// </summary>
        public class EthicalDecisionContext
        {
            public string DecisionId { get; set; } = Guid.NewGuid().ToString();
            public DateTime DecisionTime { get; set; } = DateTime.UtcNow;

            // Input data
            public int[] InputData { get; set; }
            public int DataSize { get; set; }

            // Analysis results
            public MarkovChainAnalyzer.DataCharacteristics DataCharacteristics { get; set; }

            // Recommendation
            public MarkovChainAnalyzer.SortAlgorithmState RecommendedAlgorithm { get; set; }
            public Dictionary<MarkovChainAnalyzer.SortAlgorithmState, double> AllAlgorithmScores { get; set; }
            public double InitialConfidenceScore { get; set; }

            // Ethical assessment
            public MarkovChainAnalyzer.EthicalAssessmentEngine.EthicalRecommendationAssessment EthicalAssessment { get; set; }
            public bool PassedEthicalGuardrails { get; set; }
            public bool WasDiversityApplied { get; set; }
            public string DiversityReason { get; set; }

            // Execution
            public SortMetrics ExecutionMetrics { get; set; }
            public bool SortSuccessful { get; set; }

            // Transparency
            public List<string> DecisionExplanations { get; set; } = new();
            public List<string> EthicalConsiderations { get; set; } = new();
            public List<string> LimitationsAndCaveats { get; set; } = new();

            // User feedback
            public bool UserAccepted { get; set; }
            public string UserFeedback { get; set; }
        }

        /// <summary>
        /// Enhanced prediction result with ethical context
        /// </summary>
        public class EthicalAlgorithmPrediction : MarkovChainAnalyzer.AlgorithmPrediction
        {
            public MarkovChainAnalyzer.EthicalAssessmentEngine.EthicalRecommendationAssessment EthicalAssessment { get; set; }
            public bool IsEthicallySound { get; set; }
            public List<string> EthicalConcerns { get; set; } = new();
            public List<string> TransparencyExplanations { get; set; } = new();
            public bool ExperiencedBiasMitigation { get; set; }
            public string BiasAdjustmentApplied { get; set; }
            public bool AlgorithmWasDiversified { get; set; }
            public string DiversityReason { get; set; }
        }

        public EthicallyEnhancedAdaptiveSortingService() : base()
        {
            _ethicalEngine = new MarkovChainAnalyzer.EthicalAssessmentEngine();
            _decisionHistory = new List<EthicalDecisionContext>();
        }

        /// <summary>
        /// Enhanced recommendation with full ethical assessment and transparency
        /// </summary>
        public EthicalAlgorithmPrediction GetEthicalAlgorithmRecommendation(int[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var result = new EthicalAlgorithmPrediction();
            var context = new EthicalDecisionContext { InputData = data, DataSize = data.Length };

            try
            {
                // 1. Analyze data characteristics
                context.DataCharacteristics = MarkovAnalyzer.AnalyzeData(data);

                // 2. Get base prediction from Markov system
                var basePrediction = MarkovAnalyzer.PredictBestAlgorithm(data);
                result.RecommendedAlgorithm = basePrediction.RecommendedAlgorithm;
                result.ConfidenceScore = basePrediction.ConfidenceScore;
                result.Characteristics = basePrediction.Characteristics;
                result.AlgorithmScores = basePrediction.AlgorithmScores;
                result.PerformanceEstimates = basePrediction.PerformanceEstimates;

                context.RecommendedAlgorithm = result.RecommendedAlgorithm;
                context.AllAlgorithmScores = result.AlgorithmScores;
                context.InitialConfidenceScore = result.ConfidenceScore;

                // 3. Conduct ethical assessment
                result.EthicalAssessment = _ethicalEngine.AssessRecommendationEthics(
                    result.RecommendedAlgorithm,
                    result.AlgorithmScores,
                    context.DataCharacteristics,
                    result.ConfidenceScore);

                context.EthicalAssessment = result.EthicalAssessment;
                result.IsEthicallySound = result.EthicalAssessment.IsEthicallySound;
                result.EthicalConcerns = result.EthicalAssessment.EthicalConcerns;

                // 4. Apply diversity exploration if guardrails allow
                var (selectedAlgo, diversityReason) = _ethicalEngine.ApplyDiversityExploration(
                    result.RecommendedAlgorithm,
                    result.AlgorithmScores);

                if (selectedAlgo != result.RecommendedAlgorithm)
                {
                    result.AlgorithmWasDiversified = true;
                    result.DiversityReason = diversityReason;
                    result.RecommendedAlgorithm = selectedAlgo;
                    context.WasDiversityApplied = true;
                    context.DiversityReason = diversityReason;
                }

                //result.PassedEthicalGuardrails = result.IsEthicallySound;
                context.PassedEthicalGuardrails = result.IsEthicallySound;

                // 5. Generate transparency explanations
                result.TransparencyExplanations = GenerateTransparencyExplanations(
                    result, context.DataCharacteristics);

                context.DecisionExplanations = result.TransparencyExplanations;

                // 6. Add ethical considerations
                if (!result.IsEthicallySound)
                {
                    context.EthicalConsiderations.Add(
                        "⚠️ WARNING: This recommendation did not pass all ethical guardrails. " +
                        "Consider manually reviewing or consulting alternatives.");

                    foreach (var concern in result.EthicalConcerns)
                    {
                        context.EthicalConsiderations.Add($"• {concern}");
                    }
                }
                else
                {
                    context.EthicalConsiderations.Add(
                        "✓ Passed ethical assessment. Recommendation is fair, transparent, and trustworthy.");
                }

                // 7. Add limitations and caveats
                if (result.ConfidenceScore < 0.75)
                {
                    context.LimitationsAndCaveats.Add(
                        $"Low confidence ({result.ConfidenceScore:P}). Consider algorithm comparison.");
                }

                if (result.AlgorithmScores.Values.OrderByDescending(x => x).Take(2).ToList()[0] -
                    result.AlgorithmScores.Values.OrderByDescending(x => x).Skip(1).First() < 0.1)
                {
                    context.LimitationsAndCaveats.Add(
                        "Close scoring: Multiple algorithms are nearly equivalent. " +
                        "Performance may vary based on implementation details.");
                }

                // 8. Log audit trail
                _ethicalEngine.LogRecommendationAudit(
                    result.RecommendedAlgorithm,
                    result.AlgorithmScores.Keys.ToArray(),
                    result.AlgorithmScores,
                    result.ConfidenceScore,
                    string.Join(" | ", result.TransparencyExplanations),
                    context.DataCharacteristics,
                    result.IsEthicallySound,
                    string.Join(" | ", result.EthicalConcerns));

                // 9. Store in history
                _decisionHistory.Add(context);

                result.RecommendationReason = GenerateDetailedRecommendationReason(
                    result, context.DataCharacteristics);
            }
            catch (Exception ex)
            {
                context.EthicalConsiderations.Add($"ERROR during ethical assessment: {ex.Message}");
                _decisionHistory.Add(context);
                throw;
            }

            return result;
        }

        /// <summary>
        /// Enhanced adaptive sort with ethical guardrails and full audit trail
        /// </summary>
        public void EthicallyAdaptiveSortByMarkovPrediction(int[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var prediction = GetEthicalAlgorithmRecommendation(data);

            // Log warnings if ethical concerns exist
            if (!prediction.IsEthicallySound)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"⚠️ Ethical Guardrail Warning: Recommendation may not be ethically sound. Concerns: " +
                    $"{string.Join("; ", prediction.EthicalConcerns)}");
            }

            // Log diversity application if it occurred
            if (prediction.AlgorithmWasDiversified)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"ℹ️ Diversity Exploration Applied: {prediction.DiversityReason}");
            }

            // Execute sort with recommended algorithm
            var metrics = SortWithMetrics(data, prediction.RecommendedAlgorithm);

            // Log performance outcome
            if (metrics.SortSuccessful)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"✓ Sort successful: {metrics.Algorithm} completed in {metrics.ElapsedMilliseconds}ms");

                MarkovAnalyzer.RecordSortSuccess(
                    MarkovChainAnalyzer.SortAlgorithmState.SimpleSort,
                    prediction.RecommendedAlgorithm,
                    metrics.PerformanceRatio);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(
                    $"✗ Sort failed: {metrics.Algorithm} did not complete successfully");
            }
        }

        /// <summary>
        /// Enhanced sort with metrics that includes ethical audit trail
        /// </summary>
        public EthicalSortMetrics EthicalSortWithMetrics(
            int[] data,
            MarkovChainAnalyzer.SortAlgorithmState algorithmToUse)
        {
            var context = new EthicalDecisionContext { InputData = data, DataSize = data.Length };

            try
            {
                // Get ethical recommendation for context
                var ethicalPrediction = GetEthicalAlgorithmRecommendation(data);
                context.RecommendedAlgorithm = ethicalPrediction.RecommendedAlgorithm;
                context.EthicalAssessment = ethicalPrediction.EthicalAssessment;
                context.PassedEthicalGuardrails = ethicalPrediction.IsEthicallySound;

                // Execute sort
                var baseSortMetrics = SortWithMetrics(data, algorithmToUse);

                // Create ethical metrics wrapper
                var result = new EthicalSortMetrics
                {
                    SortMetrics = baseSortMetrics,
                    DecisionId = context.DecisionId,
                    EthicalAssessment = context.EthicalAssessment,
                    PassedEthicalGuardrails = context.PassedEthicalGuardrails,
                    AuditTrailId = context.DecisionId,
                    AlgorithmWasRecommended = algorithmToUse == ethicalPrediction.RecommendedAlgorithm,
                    PerformanceAlignedWithPrediction = Math.Abs(baseSortMetrics.PerformanceRatio - 1.0) < 0.3
                };

                context.ExecutionMetrics = baseSortMetrics;
                context.SortSuccessful = baseSortMetrics.SortSuccessful;

                // Verify recommendation accuracy
                if (!result.AlgorithmWasRecommended && result.PerformanceAlignedWithPrediction)
                {
                    result.AlternativePerformedWell = true;
                    result.NotificationMessage =
                        $"Non-recommended algorithm performed well. Consider including {algorithmToUse} in future scoring.";
                }

                _decisionHistory.Add(context);
                return result;
            }
            catch (Exception ex)
            {
                context.EthicalConsiderations.Add($"ERROR during ethical sort: {ex.Message}");
                _decisionHistory.Add(context);
                throw;
            }
        }

        /// <summary>
        /// Get comprehensive ethical audit report for auditing and compliance
        /// </summary>
        public EthicalAuditSummary GetEthicalAuditSummary()
        {
            var report = _ethicalEngine.GetEthicalAuditReport();

            var summary = new EthicalAuditSummary
            {
                ReportId = Guid.NewGuid().ToString(),
                GeneratedTime = DateTime.UtcNow,
                TotalSortOperations = _decisionHistory.Count,
                OperationsPassed = _decisionHistory.Count(x => x.PassedEthicalGuardrails),
                OperationsFailed = _decisionHistory.Count(x => !x.PassedEthicalGuardrails),
                DiversityApplicationCount = _decisionHistory.Count(x => x.WasDiversityApplied),

                OverallEthicalScore = report.OverallEthicalScore,
                FairnessScore = report.FairnessMetrics.FairnessScore,
                BiasScores = report.BiasAssessments.ToDictionary(x => x.Algorithm.ToString(), x => x.BiasScore),

                GuardrailsStatusByType = report.GuardrailsActive,
                CriticalIssues = report.CriticalIssues,
                ImprovementRecommendations = report.ImprovementRecommendations,

                AlgorithmRecommendationCounts = report.FairnessMetrics.AlgorithmRecommendationCounts
                    .ToDictionary(x => x.Key.ToString(), x => x.Value),
                AlgorithmRecommendationRates = report.FairnessMetrics.AlgorithmRecommendationRates
                    .ToDictionary(x => x.Key.ToString(), x => (double?)x.Value),

                DecisionHistory = _decisionHistory
                    .Select(x => new EthicalDecisionSummary
                    {
                        DecisionId = x.DecisionId,
                        DecisionTime = x.DecisionTime,
                        DataSize = x.DataSize,
                        RecommendedAlgorithm = x.RecommendedAlgorithm.ToString(),
                        PassedEthicalGuardrails = x.PassedEthicalGuardrails,
                        WasDiversified = x.WasDiversityApplied,
                        ExecutionTime = x.ExecutionMetrics?.ElapsedMilliseconds ?? -1,
                        SortSuccessful = x.SortSuccessful,
                        EthicalConsiderations = x.EthicalConsiderations
                    })
                    .ToList(),

                ComplianceStatement = GenerateComplianceStatement(report),
                CertificationTimestamp = DateTime.UtcNow
            };

            return summary;
        }

        /// <summary>
        /// Export ethical audit trail in machine-readable format for compliance
        /// </summary>
        public string ExportEthicalAuditTrail()
        {
            var summary = GetEthicalAuditSummary();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Decision ID,Timestamp,Data Size,Algorithm,Passed Guardrails,Diversified,Execution Time (ms)");

            foreach (var decision in summary.DecisionHistory)
            {
                csv.AppendLine(
                    $"\"{decision.DecisionId}\"," +
                    $"{decision.DecisionTime:O}," +
                    $"{decision.DataSize}," +
                    $"{decision.RecommendedAlgorithm}," +
                    $"{decision.PassedEthicalGuardrails}," +
                    $"{decision.WasDiversified}," +
                    $"{decision.ExecutionTime}");
            }

            return csv.ToString();
        }

        private List<string> GenerateTransparencyExplanations(
            EthicalAlgorithmPrediction prediction,
            MarkovChainAnalyzer.DataCharacteristics characteristics)
        {
            var explanations = new List<string>
            {
                $"Algorithm: {prediction.RecommendedAlgorithm} " +
                $"(Score: {prediction.AlgorithmScores[prediction.RecommendedAlgorithm]:F3}/1.0)",

                $"Why this algorithm? It matches data characteristics: " +
                $"Sortedness={characteristics.SortednessRatio:F3}, " +
                $"Entropy={characteristics.Entropy:F3}, " +
                $"Range={characteristics.RangeSpan:F3}",

                $"Confidence: {prediction.ConfidenceScore:P} - " +
                $"{GetConfidenceLevel(prediction.ConfidenceScore)}",

                $"Data Size: {characteristics.Size} elements | " +
                $"Distinct Values: {characteristics.DistinctValues} | " +
                $"Duplicates: {(100.0 * (1 - (double)characteristics.DistinctValues / characteristics.Size)):F1}%"
            };

            // Add alternative algorithms if available
            var topAlternatives = prediction.AlgorithmScores
                .OrderByDescending(x => x.Value)
                .Skip(1)
                .Take(2)
                .ToList();

            if (topAlternatives.Any())
            {
                explanations.Add(
                    $"Alternatives: {string.Join(", ", topAlternatives.Select(x => $"{x.Key} ({x.Value:F3})"))}");
            }

            // Add ethical considerations
            if (!prediction.IsEthicallySound)
            {
                explanations.Add(
                    "⚠️ Ethical Concerns: " + string.Join("; ", prediction.EthicalConcerns));
            }
            else
            {
                explanations.Add("✓ Passed all ethical guardrails");
            }

            // Add diversity note if applied
            if (prediction.AlgorithmWasDiversified)
            {
                explanations.Add($"🔄 Diversity Applied: {prediction.DiversityReason}");
            }

            return explanations;
        }

        private string GenerateDetailedRecommendationReason(
            EthicalAlgorithmPrediction prediction,
            MarkovChainAnalyzer.DataCharacteristics characteristics)
        {
            var reason = $"Recommended {prediction.RecommendedAlgorithm} " +
                $"(confidence: {prediction.ConfidenceScore:P}) based on data analysis: " +
                $"sortedness={characteristics.SortednessRatio:F3}, " +
                $"entropy={characteristics.Entropy:F3}. ";

            if (!prediction.IsEthicallySound)
            {
                reason += $"⚠️ Ethical concerns detected. ";
            }

            if (prediction.AlgorithmWasDiversified)
            {
                reason += $"Diversity applied. ";
            }

            reason += "Review transparency explanations for full details.";

            return reason;
        }

        private string GetConfidenceLevel(double score)
        {
            return score switch
            {
                >= 0.9 => "Very High - Strong recommendation",
                >= 0.75 => "High - Good confidence",
                >= 0.6 => "Medium - Reasonable choice",
                >= 0.4 => "Low - Consider alternatives",
                _ => "Very Low - Manual review recommended"
            };
        }

        private string GenerateComplianceStatement(
            MarkovChainAnalyzer.EthicalAssessmentEngine.EthicalAuditReport report)
        {
            var statement = $"ETHICAL COMPLIANCE STATEMENT\n" +
                $"Generated: {DateTime.UtcNow:O}\n" +
                $"Overall Ethical Score: {report.OverallEthicalScore:P}\n";

            if (report.CriticalIssues.Any())
            {
                statement += $"❌ CRITICAL ISSUES FOUND: {string.Join("; ", report.CriticalIssues)}\n";
            }
            else
            {
                statement += $"✓ No critical ethical issues detected.\n";
            }

            statement += $"\nActive Guardrails:\n";
            foreach (var guardrail in report.GuardrailsActive)
            {
                statement += $"  {(guardrail.Value ? "✓" : "✗")} {guardrail.Key}\n";
            }

            if (report.ImprovementRecommendations.Any())
            {
                statement += $"\nRecommendations for Improvement:\n";
                foreach (var rec in report.ImprovementRecommendations)
                {
                    statement += $"  • {rec}\n";
                }
            }

            return statement;
        }

        // Supporting classes
        public class EthicalSortMetrics
        {
            public SortMetrics SortMetrics { get; set; }
            public string DecisionId { get; set; }
            public MarkovChainAnalyzer.EthicalAssessmentEngine.EthicalRecommendationAssessment EthicalAssessment { get; set; }
            public bool PassedEthicalGuardrails { get; set; }
            public string AuditTrailId { get; set; }
            public bool AlgorithmWasRecommended { get; set; }
            public bool PerformanceAlignedWithPrediction { get; set; }
            public bool AlternativePerformedWell { get; set; }
            public string NotificationMessage { get; set; }
        }

        public class EthicalAuditSummary
        {
            public string ReportId { get; set; }
            public DateTime GeneratedTime { get; set; }
            public int TotalSortOperations { get; set; }
            public int OperationsPassed { get; set; }
            public int OperationsFailed { get; set; }
            public int DiversityApplicationCount { get; set; }

            public double OverallEthicalScore { get; set; }
            public double FairnessScore { get; set; }
            public Dictionary<string, double> BiasScores { get; set; }

            public Dictionary<string, bool> GuardrailsStatusByType { get; set; }
            public List<string> CriticalIssues { get; set; }
            public List<string> ImprovementRecommendations { get; set; }

            public Dictionary<string, int> AlgorithmRecommendationCounts { get; set; }
            public Dictionary<string, double?> AlgorithmRecommendationRates { get; set; }

            public List<EthicalDecisionSummary> DecisionHistory { get; set; }

            public string ComplianceStatement { get; set; }
            public DateTime CertificationTimestamp { get; set; }
        }

        public class EthicalDecisionSummary
        {
            public string DecisionId { get; set; }
            public DateTime DecisionTime { get; set; }
            public int DataSize { get; set; }
            public string RecommendedAlgorithm { get; set; }
            public bool PassedEthicalGuardrails { get; set; }
            public bool WasDiversified { get; set; }
            public long ExecutionTime { get; set; }
            public bool SortSuccessful { get; set; }
            public List<string> EthicalConsiderations { get; set; }
        }
    }
}
