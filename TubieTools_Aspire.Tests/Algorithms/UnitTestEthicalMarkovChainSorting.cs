using System;
using System.Collections.Generic;
using System.Linq;

namespace TubieTools_Aspire.Tests.Algorithms
{
    [TestClass]
    public class UnitTestEthicalMarkovChainSorting
    {
        private EthicallyEnhancedAdaptiveSortingService _ethicalService;
        private int[] _testData;
        private TestContext _testContextInstance;

        public TestContext TestContext
        {
            get { return _testContextInstance; }
            set { _testContextInstance = value; }
        }

        [TestInitialize]
        public void Init()
        {
            _ethicalService = new EthicallyEnhancedAdaptiveSortingService();
            _testData = new int[1000];
            InitializeRandomData();
        }

        private void InitializeRandomData()
        {
            Random r = new Random(42);
            for (int i = 0; i < _testData.Length; i++)
                _testData[i] = r.Next();
        }

        #region Ethical Assessment Tests

        [TestMethod]
        public void TestEthicalAlgorithmRecommendation()
        {
            var prediction = _ethicalService.GetEthicalAlgorithmRecommendation(_testData);

            Assert.IsNotNull(prediction);
            Assert.IsNotNull(prediction.EthicalAssessment);
            Assert.IsTrue(prediction.ConfidenceScore >= 0 && prediction.ConfidenceScore <= 1);
            Assert.IsNotNull(prediction.TransparencyExplanations);
            Assert.IsTrue(prediction.TransparencyExplanations.Count > 0);

            TestContext.WriteLine($"Recommended: {prediction.RecommendedAlgorithm}");
            TestContext.WriteLine($"Ethical Sound: {prediction.IsEthicallySound}");
            TestContext.WriteLine($"Explanations: {string.Join("; ", prediction.TransparencyExplanations)}");
        }

        [TestMethod]
        public void TestEthicalAssessmentComponents()
        {
            var prediction = _ethicalService.GetEthicalAlgorithmRecommendation(_testData);
            var assessment = prediction.EthicalAssessment;

            Assert.IsNotNull(assessment.BiasCheckResults);
            Assert.IsNotNull(assessment.FairnessCheckResults);
            Assert.IsNotNull(assessment.ConfidenceCheckResults);
            Assert.IsNotNull(assessment.TransparencyRequirements);

            TestContext.WriteLine($"Bias Detected: {assessment.BiasCheckResults.BiasDetected}");
            TestContext.WriteLine($"Fairness Score: {assessment.FairnessCheckResults.FairnessScore:F3}");
            TestContext.WriteLine($"Confidence Appropriate: {assessment.ConfidenceCheckResults.IsConfidenceAppropriate}");
            TestContext.WriteLine($"Transparency Required: {assessment.TransparencyRequirements.IsRequired}");
        }

        [TestMethod]
        public void TestBiasDetection()
        {
            // Perform multiple sorts to build history
            for (int i = 0; i < 20; i++)
            {
                int[] dataCopy = new int[100];
                Random r = new Random(i * 10);
                for (int j = 0; j < 100; j++)
                    dataCopy[j] = r.Next();

                _ethicalService.EthicallyAdaptiveSortByMarkovPrediction(dataCopy);
            }

            var auditSummary = _ethicalService.GetEthicalAuditSummary();

            Assert.IsNotNull(auditSummary.BiasScores);

            // Check that bias scores are tracked
            var totalBias = auditSummary.BiasScores.Values.Sum();
            TestContext.WriteLine($"Total Bias Score: {totalBias:F3}");
            TestContext.WriteLine($"Algorithms with bias: {string.Join(", ", auditSummary.BiasScores.Where(x => x.Value > 0).Select(x => x.Key))}");
        }

        [TestMethod]
        public void TestFairnessMonitoring()
        {
            // Perform multiple sorts with diverse data
            for (int i = 0; i < 30; i++)
            {
                int[] dataCopy = new int[100];
                Random r = new Random(i * 7);

                if (i % 5 == 0)
                    dataCopy = Enumerable.Range(1, 100).ToArray();  // Sorted
                else if (i % 5 == 1)
                    dataCopy = Enumerable.Range(1, 100).Reverse().ToArray();  // Reverse
                else
                {
                    for (int j = 0; j < 100; j++)
                        dataCopy[j] = r.Next();  // Random
                }

                _ethicalService.EthicallyAdaptiveSortByMarkovPrediction(dataCopy);
            }

            var auditSummary = _ethicalService.GetEthicalAuditSummary();

            Assert.IsTrue(auditSummary.AlgorithmRecommendationCounts.Values.Sum() >= 30);

            // Check fairness distribution
            var recommendationRates = auditSummary.AlgorithmRecommendationRates.Values.Where(x => x.HasValue).ToList();
            TestContext.WriteLine($"Fairness Score: {auditSummary.FairnessScore:F3}");
            TestContext.WriteLine($"Recommendation Distribution: {string.Join(", ", auditSummary.AlgorithmRecommendationRates.Select(x => $"{x.Key}: {x.Value:P1}"))}");
        }

        [TestMethod]
        public void TestTransparencyExplanations()
        {
            var prediction = _ethicalService.GetEthicalAlgorithmRecommendation(_testData);

            Assert.IsTrue(prediction.TransparencyExplanations.Count > 0);

            foreach (var explanation in prediction.TransparencyExplanations)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(explanation));
                TestContext.WriteLine($"• {explanation}");
            }
        }

        #endregion

        #region Ethical Sorting Tests

        [TestMethod]
        public void TestEthicallyAdaptiveSorting()
        {
            int[] dataCopy = new int[_testData.Length];
            Array.Copy(_testData, dataCopy, _testData.Length);

            _ethicalService.EthicallyAdaptiveSortByMarkovPrediction(dataCopy);

            Assert.IsTrue(IsSorted(dataCopy));
            TestContext.WriteLine("Ethically Adaptive Sort: PASSED");
        }

        [TestMethod]
        public void TestEthicalSortWithMetrics()
        {
            int[] dataCopy = new int[_testData.Length];
            Array.Copy(_testData, dataCopy, _testData.Length);

            var metrics = _ethicalService.EthicalSortWithMetrics(dataCopy,
                MarkovChainAnalyzer.SortAlgorithmState.TimSort);

            Assert.IsNotNull(metrics);
            Assert.IsNotNull(metrics.SortMetrics);
            Assert.IsNotNull(metrics.EthicalAssessment);
            Assert.IsTrue(metrics.SortMetrics.SortSuccessful);

            TestContext.WriteLine($"Algorithm Recommended: {metrics.SortMetrics.Algorithm}");
            TestContext.WriteLine($"Passed Ethical Guardrails: {metrics.PassedEthicalGuardrails}");
            TestContext.WriteLine($"Audit Trail ID: {metrics.AuditTrailId}");
        }

        [TestMethod]
        public void TestDiversityExploration()
        {
            var prediction = _ethicalService.GetEthicalAlgorithmRecommendation(_testData);

            // Check if diversity was applied
            if (prediction.AlgorithmWasDiversified)
            {
                TestContext.WriteLine($"Diversity Applied: {prediction.DiversityReason}");
                Assert.IsNotNull(prediction.DiversityReason);
                Assert.IsTrue(prediction.DiversityReason.Length > 0);
            }

            // Perform multiple predictions to increase chance of diversity
            int diversityCount = 0;
            for (int i = 0; i < 50; i++)
            {
                int[] dataCopy = new int[100];
                Random r = new Random(i * 11);
                for (int j = 0; j < 100; j++)
                    dataCopy[j] = r.Next();

                var pred = _ethicalService.GetEthicalAlgorithmRecommendation(dataCopy);
                if (pred.AlgorithmWasDiversified)
                    diversityCount++;
            }

            TestContext.WriteLine($"Diversity Applied {diversityCount} times out of 50 (target: ~7-8 times at 15% rate)");
        }

        #endregion

        #region Audit Trail Tests

        [TestMethod]
        public void TestAuditTrailLogging()
        {
            for (int i = 0; i < 10; i++)
            {
                int[] dataCopy = new int[100];
                Random r = new Random(i * 13);
                for (int j = 0; j < 100; j++)
                    dataCopy[j] = r.Next();

                _ethicalService.EthicallyAdaptiveSortByMarkovPrediction(dataCopy);
            }

            var auditSummary = _ethicalService.GetEthicalAuditSummary();

            Assert.IsTrue(auditSummary.TotalSortOperations >= 10);
            Assert.IsNotNull(auditSummary.DecisionHistory);
            Assert.IsTrue(auditSummary.DecisionHistory.Count >= 10);

            TestContext.WriteLine($"Total Sort Operations: {auditSummary.TotalSortOperations}");
            TestContext.WriteLine($"Passed Guardrails: {auditSummary.OperationsPassed}");
            TestContext.WriteLine($"Failed Guardrails: {auditSummary.OperationsFailed}");
        }

        [TestMethod]
        public void TestEthicalAuditSummary()
        {
            // Perform several sorts
            for (int i = 0; i < 15; i++)
            {
                int[] dataCopy = new int[100];
                Random r = new Random(i * 17);
                for (int j = 0; j < 100; j++)
                    dataCopy[j] = r.Next();

                _ethicalService.EthicallyAdaptiveSortByMarkovPrediction(dataCopy);
            }

            var summary = _ethicalService.GetEthicalAuditSummary();

            Assert.IsNotNull(summary);
            Assert.IsTrue(summary.OverallEthicalScore >= 0 && summary.OverallEthicalScore <= 1);
            Assert.IsNotNull(summary.ComplianceStatement);
            Assert.IsTrue(summary.ComplianceStatement.Length > 0);

            TestContext.WriteLine("ETHICAL AUDIT SUMMARY");
            TestContext.WriteLine("=====================");
            TestContext.WriteLine($"Report ID: {summary.ReportId}");
            TestContext.WriteLine($"Overall Ethical Score: {summary.OverallEthicalScore:P}");
            TestContext.WriteLine($"Fairness Score: {summary.FairnessScore:F3}");
            TestContext.WriteLine($"Total Operations: {summary.TotalSortOperations}");
            TestContext.WriteLine($"Passed Guardrails: {summary.OperationsPassed}");
            TestContext.WriteLine($"Diversity Applications: {summary.DiversityApplicationCount}");
            TestContext.WriteLine(summary.ComplianceStatement);
        }

        [TestMethod]
        public void TestAuditTrailExport()
        {
            // Perform several sorts
            for (int i = 0; i < 10; i++)
            {
                int[] dataCopy = new int[100];
                Random r = new Random(i * 19);
                for (int j = 0; j < 100; j++)
                    dataCopy[j] = r.Next();

                _ethicalService.EthicallyAdaptiveSortByMarkovPrediction(dataCopy);
            }

            var csvExport = _ethicalService.ExportEthicalAuditTrail();

            Assert.IsNotNull(csvExport);
            Assert.IsTrue(csvExport.Length > 0);
            Assert.IsTrue(csvExport.Contains("Decision ID"));
            Assert.IsTrue(csvExport.Contains("Algorithm"));

            TestContext.WriteLine("CSV EXPORT (First 5 entries):");
            var lines = csvExport.Split('\n');
            foreach (var line in lines.Take(6))
            {
                TestContext.WriteLine(line);
            }
        }

        #endregion

        #region Guardrails Tests

        [TestMethod]
        public void TestEthicalGuardrailsActive()
        {
            var prediction = _ethicalService.GetEthicalAlgorithmRecommendation(_testData);
            var summary = _ethicalService.GetEthicalAuditSummary();

            Assert.IsNotNull(summary.GuardrailsStatusByType);
            Assert.IsTrue(summary.GuardrailsStatusByType.Count > 0);

            TestContext.WriteLine("Active Guardrails:");
            foreach (var guardrail in summary.GuardrailsStatusByType)
            {
                TestContext.WriteLine($"  {(guardrail.Value ? "✓" : "✗")} {guardrail.Key}");
            }
        }

        [TestMethod]
        public void TestCriticalIssueDetection()
        {
            for (int i = 0; i < 50; i++)
            {
                int[] dataCopy = new int[100];
                Random r = new Random(i * 23);
                for (int j = 0; j < 100; j++)
                    dataCopy[j] = r.Next();

                _ethicalService.EthicallyAdaptiveSortByMarkovPrediction(dataCopy);
            }

            var summary = _ethicalService.GetEthicalAuditSummary();

            TestContext.WriteLine($"Critical Issues Found: {summary.CriticalIssues.Count}");
            foreach (var issue in summary.CriticalIssues)
            {
                TestContext.WriteLine($"  • {issue}");
            }
        }

        [TestMethod]
        public void TestImprovementRecommendations()
        {
            for (int i = 0; i < 30; i++)
            {
                int[] dataCopy = new int[100];
                Random r = new Random(i * 29);
                for (int j = 0; j < 100; j++)
                    dataCopy[j] = r.Next();

                _ethicalService.EthicallyAdaptiveSortByMarkovPrediction(dataCopy);
            }

            var summary = _ethicalService.GetEthicalAuditSummary();

            TestContext.WriteLine($"Improvement Recommendations: {summary.ImprovementRecommendations.Count}");
            foreach (var rec in summary.ImprovementRecommendations)
            {
                TestContext.WriteLine($"  • {rec}");
            }
        }

        #endregion

        #region Edge Cases

        [TestMethod]
        public void TestEthicalAssessmentEmptyArray()
        {
            int[] emptyData = new int[0];
            var prediction = _ethicalService.GetEthicalAlgorithmRecommendation(emptyData);

            Assert.IsNotNull(prediction);
            TestContext.WriteLine("Empty array handled gracefully");
        }

        [TestMethod]
        public void TestEthicalAssessmentSmallArray()
        {
            int[] smallData = new int[5];
            Random r = new Random();
            for (int i = 0; i < 5; i++)
                smallData[i] = r.Next();

            var prediction = _ethicalService.GetEthicalAlgorithmRecommendation(smallData);

            Assert.IsNotNull(prediction);
            TestContext.WriteLine($"Small array recommended: {prediction.RecommendedAlgorithm}");
        }

        [TestMethod]
        public void TestEthicalAssessmentLargeArray()
        {
            int[] largeData = new int[100000];
            Random r = new Random();
            for (int i = 0; i < 100000; i++)
                largeData[i] = r.Next();

            var prediction = _ethicalService.GetEthicalAlgorithmRecommendation(largeData);

            Assert.IsNotNull(prediction);
            TestContext.WriteLine($"Large array recommended: {prediction.RecommendedAlgorithm}");
        }

        [TestMethod]
        public void TestEthicalAssessmentAllDuplicates()
        {
            int[] duplicates = new int[1000];
            for (int i = 0; i < 1000; i++)
                duplicates[i] = 42;

            var prediction = _ethicalService.GetEthicalAlgorithmRecommendation(duplicates);

            Assert.IsNotNull(prediction);
            TestContext.WriteLine($"Duplicates array recommended: {prediction.RecommendedAlgorithm}");
        }

        #endregion

        private bool IsSorted(int[] data)
        {
            for (int i = 0; i < data.Length - 1; i++)
            {
                if (data[i] > data[i + 1])
                    return false;
            }
            return true;
        }
    }
}
