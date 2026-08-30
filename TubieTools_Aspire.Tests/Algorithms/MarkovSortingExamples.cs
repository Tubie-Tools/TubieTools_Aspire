using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TubieTools_Aspire.Tests.Algorithms
{
    /// <summary>
    /// Demonstration and example usage of the Markov chain-based adaptive sorting system.
    /// This file provides practical examples of how to use the sorting service in real-world scenarios.
    /// </summary>
    public class MarkovSortingExamples
    {
        private readonly IAdaptiveSortingService _sortingService;

        public MarkovSortingExamples()
        {
            _sortingService = new AdaptiveSortingService();
        }

        /// <summary>
        /// Example 1: Basic adaptive sorting with recommendations
        /// </summary>
        public void Example1_BasicAdaptiveSorting()
        {
            Console.WriteLine("=== Example 1: Basic Adaptive Sorting ===\n");

            // Create sample data
            int[] data = GenerateRandomData(1000);

            // Get recommendation
            var prediction = _sortingService.GetAlgorithmRecommendation(data);

            Console.WriteLine($"Data Size: {data.Length}");
            Console.WriteLine($"Recommended Algorithm: {prediction.RecommendedAlgorithm}");
            Console.WriteLine($"Confidence Score: {prediction.ConfidenceScore:P1}");
            Console.WriteLine($"Reason: {prediction.RecommendationReason}");
            Console.WriteLine();

            // Perform adaptive sort
            _sortingService.AdaptiveSortByMarkovPrediction(data);
            Console.WriteLine($"✓ Array sorted successfully with {prediction.RecommendedAlgorithm}");
            Console.WriteLine();
        }

        /// <summary>
        /// Example 2: Detailed data analysis
        /// </summary>
        public void Example2_DataCharacteristicsAnalysis()
        {
            Console.WriteLine("=== Example 2: Data Characteristics Analysis ===\n");

            // Analyze different types of data
            var scenarios = new Dictionary<string, int[]>
            {
                { "Random Data", GenerateRandomData(1000) },
                { "Sorted Data", Enumerable.Range(1, 1000).ToArray() },
                { "Reverse Sorted", Enumerable.Range(1, 1000).Reverse().ToArray() },
                { "Nearly Sorted", GenerateNearlySortedData(1000) },
                { "Small Range", GenerateSmallRangeData(1000, 0, 50) }
            };

            foreach (var scenario in scenarios)
            {
                var characteristics = _sortingService.AnalyzeDataCharacteristics(scenario.Value);

                Console.WriteLine($"Scenario: {scenario.Key}");
                Console.WriteLine($"  Size: {characteristics.Size}");
                Console.WriteLine($"  Sortedness: {characteristics.SortednessRatio:F3}");
                Console.WriteLine($"  Entropy: {characteristics.Entropy:F3}");
                Console.WriteLine($"  Distinct Values: {characteristics.DistinctValues}");
                Console.WriteLine($"  Range Span: {characteristics.RangeSpan:F3}");
                Console.WriteLine($"  Is Monotonic: {characteristics.IsMonotonic}");
                Console.WriteLine($"  Average Cluster Size: {characteristics.AverageClusterSize:F3}");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Example 3: Comparing algorithm performance
        /// </summary>
        public void Example3_PerformanceComparison()
        {
            Console.WriteLine("=== Example 3: Algorithm Performance Comparison ===\n");

            int[] data = GenerateRandomData(5000);
            var prediction = _sortingService.GetAlgorithmRecommendation(data);

            Console.WriteLine($"Analyzing {data.Length} random elements...\n");
            Console.WriteLine($"Markov Prediction: {prediction.RecommendedAlgorithm}");
            Console.WriteLine($"(Confidence: {prediction.ConfidenceScore:P1})\n");

            // Compare actual performance
            var comparison = _sortingService.CompareAlgorithmPerformance(data);

            Console.WriteLine("Algorithm Performance Comparison:");
            Console.WriteLine("─────────────────────────────────────────────────────────");
            Console.WriteLine($"{"Algorithm",-20} {"Time (ms)",-12} {"Comparisons",-15} {"Complexity"}");
            Console.WriteLine("─────────────────────────────────────────────────────────");

            foreach (var result in comparison.OrderBy(x => x.Value.ElapsedMilliseconds))
            {
                var marker = result.Key == prediction.RecommendedAlgorithm ? "→ " : "  ";
                Console.WriteLine($"{marker}{result.Value.Algorithm,-18} {result.Value.ElapsedMilliseconds,-12} " +
                    $"{result.Value.EstimatedComparisons,-15} {result.Value.TimeComplexity}");
            }
            Console.WriteLine("─────────────────────────────────────────────────────────");
            Console.WriteLine();
        }

        /// <summary>
        /// Example 4: Detailed metrics and performance analysis
        /// </summary>
        public void Example4_DetailedMetricsAnalysis()
        {
            Console.WriteLine("=== Example 4: Detailed Metrics Analysis ===\n");

            int[] data = GenerateRandomData(2000);
            var prediction = _sortingService.GetAlgorithmRecommendation(data);

            Console.WriteLine($"Testing {prediction.RecommendedAlgorithm}...\n");

            var metrics = _sortingService.SortWithMetrics(data, prediction.RecommendedAlgorithm);

            Console.WriteLine("Sort Metrics:");
            Console.WriteLine($"  Algorithm: {metrics.Algorithm}");
            Console.WriteLine($"  Elapsed Time: {metrics.ElapsedMilliseconds}ms");
            Console.WriteLine($"  Elapsed Ticks: {metrics.ElapsedTicks}");
            Console.WriteLine($"  Estimated Comparisons: {metrics.EstimatedComparisons:N0}");
            Console.WriteLine($"  Estimated Swaps: {metrics.EstimatedSwaps:N0}");
            Console.WriteLine($"  Time Complexity: {metrics.TimeComplexity}");
            Console.WriteLine($"  Space Complexity: O({(metrics.SpaceComplexity == 0 ? "1" : metrics.SpaceComplexity)})");
            Console.WriteLine($"  Performance Ratio: {metrics.PerformanceRatio:F3}");
            Console.WriteLine($"  Sort Successful: {metrics.SortSuccessful}");
            Console.WriteLine();
        }

        /// <summary>
        /// Example 5: Markov chain learning and statistics
        /// </summary>
        public void Example5_MarkovChainLearning()
        {
            Console.WriteLine("=== Example 5: Markov Chain Learning ===\n");

            Console.WriteLine("Initial Statistics:");
            var initialStats = _sortingService.GetMarkovChainStatistics();
            PrintMarkovStatistics(initialStats);

            // Perform several sorts to build up Markov chain data
            Console.WriteLine("\nPerforming 10 sorts with different data patterns...\n");

            for (int i = 0; i < 10; i++)
            {
                int[] data = GenerateRandomData(500);
                _sortingService.AdaptiveSortByMarkovPrediction(data);
                Console.WriteLine($"✓ Sort {i + 1} completed");
            }

            Console.WriteLine("\n\nUpdated Statistics:");
            var updatedStats = _sortingService.GetMarkovChainStatistics();
            PrintMarkovStatistics(updatedStats);

            Console.WriteLine($"\nMarkov chain has learned from {updatedStats["TotalTransitions"]} transitions");
            Console.WriteLine();
        }

        /// <summary>
        /// Example 6: Real-world scenario - log file sorting
        /// </summary>
        public void Example6_RealWorldScenario_LogSorting()
        {
            Console.WriteLine("=== Example 6: Real-World Scenario - Log File Sorting ===\n");

            Console.WriteLine("Simulating: Sorting log entries by timestamp");
            Console.WriteLine("Characteristic: Data is often partially sorted (recently added logs)\n");

            // Nearly sorted data (like log timestamps)
            int[] logTimestamps = GenerateNearlySortedData(10000);

            var characteristics = _sortingService.AnalyzeDataCharacteristics(logTimestamps);
            var prediction = _sortingService.GetAlgorithmRecommendation(logTimestamps);

            Console.WriteLine($"Detected Characteristics:");
            Console.WriteLine($"  Sortedness Ratio: {characteristics.SortednessRatio:F3} (partially sorted)");
            Console.WriteLine($"  Entropy: {characteristics.Entropy:F3} (structured data)");
            Console.WriteLine();

            Console.WriteLine($"Recommendation: {prediction.RecommendedAlgorithm}");
            Console.WriteLine($"Reason: {prediction.RecommendationReason}");
            Console.WriteLine();

            // Sort and measure
            var metrics = _sortingService.SortWithMetrics(logTimestamps, prediction.RecommendedAlgorithm);

            Console.WriteLine($"Result:");
            Console.WriteLine($"  Sorting Time: {metrics.ElapsedMilliseconds}ms");
            Console.WriteLine($"  ✓ Successfully sorted {10000} log entries");
            Console.WriteLine();
        }

        /// <summary>
        /// Example 7: Real-world scenario - database result set
        /// </summary>
        public void Example7_RealWorldScenario_DatabaseResults()
        {
            Console.WriteLine("=== Example 7: Real-World Scenario - Database Results ===\n");

            Console.WriteLine("Simulating: Sorting database query results");
            Console.WriteLine("Characteristic: Result set with constrained value range\n");

            // Small range integers (like ID sequences or categories)
            int[] databaseIds = GenerateSmallRangeData(5000, 1, 500);

            var characteristics = _sortingService.AnalyzeDataCharacteristics(databaseIds);
            var prediction = _sortingService.GetAlgorithmRecommendation(databaseIds);

            Console.WriteLine($"Detected Characteristics:");
            Console.WriteLine($"  Range Span: {characteristics.RangeSpan:F3} (small range)");
            Console.WriteLine($"  Distinct Values: {characteristics.DistinctValues} / {databaseIds.Length}");
            Console.WriteLine($"  Entropy: {characteristics.Entropy:F3}");
            Console.WriteLine();

            Console.WriteLine($"Recommendation: {prediction.RecommendedAlgorithm}");
            Console.WriteLine($"Reason: {prediction.RecommendationReason}");
            Console.WriteLine();

            var metrics = _sortingService.SortWithMetrics(databaseIds, prediction.RecommendedAlgorithm);

            Console.WriteLine($"Result:");
            Console.WriteLine($"  Sorting Time: {metrics.ElapsedMilliseconds}ms");
            Console.WriteLine($"  Time Complexity: {metrics.TimeComplexity}");
            Console.WriteLine($"  ✓ Successfully sorted {5000} database IDs");
            Console.WriteLine();
        }

        /// <summary>
        /// Example 8: Recommendation confidence across different data types
        /// </summary>
        public void Example8_RecommendationConfidence()
        {
            Console.WriteLine("=== Example 8: Recommendation Confidence Analysis ===\n");

            var testCases = new Dictionary<string, Func<int[]>>
            {
                { "Random (1000)", () => GenerateRandomData(1000) },
                { "Sorted (1000)", () => Enumerable.Range(1, 1000).ToArray() },
                { "Nearly Sorted (1000)", () => GenerateNearlySortedData(1000) },
                { "High Entropy (1000)", () => GenerateRandomData(1000) },
                { "Small Range (1000)", () => GenerateSmallRangeData(1000, 0, 100) }
            };

            Console.WriteLine($"{"Data Type",-25} {"Top Recommendation",-20} {"Confidence",-15} {"2nd Place"}");
            Console.WriteLine("─────────────────────────────────────────────────────────────────");

            foreach (var testCase in testCases)
            {
                var data = testCase.Value();
                var prediction = _sortingService.GetAlgorithmRecommendation(data);

                // Get second-best recommendation
                var secondBest = prediction.AlgorithmScores
                    .OrderByDescending(x => x.Value)
                    .Skip(1)
                    .First();

                Console.WriteLine($"{testCase.Key,-25} {prediction.RecommendedAlgorithm,-20} " +
                    $"{prediction.ConfidenceScore,-15:P1} {secondBest.Key}");
            }

            Console.WriteLine("─────────────────────────────────────────────────────────────────");
            Console.WriteLine();
        }

        /// <summary>
        /// Example 9: Performance estimation accuracy
        /// </summary>
        public void Example9_PerformanceEstimationAccuracy()
        {
            Console.WriteLine("=== Example 9: Performance Estimation Accuracy ===\n");

            var algorithms = new[]
            {
                MarkovChainAnalyzer.SortAlgorithmState.RadixSort,
                MarkovChainAnalyzer.SortAlgorithmState.TimSort,
                MarkovChainAnalyzer.SortAlgorithmState.HeapSort,
                MarkovChainAnalyzer.SortAlgorithmState.QuickSort
            };

            int[] data = GenerateRandomData(10000);

            Console.WriteLine($"Testing {data.Length} random elements\n");
            Console.WriteLine($"{"Algorithm",-15} {"Est. (ms)",-12} {"Actual (ms)",-12} {"Ratio",-10} {"Accuracy"}");
            Console.WriteLine("─────────────────────────────────────────────────────────────");

            foreach (var algo in algorithms)
            {
                int[] dataCopy = new int[data.Length];
                Array.Copy(data, dataCopy, data.Length);

                var metrics = _sortingService.SortWithMetrics(dataCopy, algo);

                var oneCharacteristics = _sortingService.AnalyzeDataCharacteristics(data);
                var performance = new MarkovChainAnalyzer().EstimatePerformance(algo, oneCharacteristics, data.Length);

                string accuracy = metrics.PerformanceRatio switch
                {
                    > 0.9 => "Excellent",
                    > 0.7 => "Good",
                    > 0.5 => "Fair",
                    _ => "Poor"
                };

                Console.WriteLine($"{algo,-15} {performance.EstimatedTimeMs,-12:F2} " +
                    $"{metrics.ElapsedMilliseconds,-12} {metrics.PerformanceRatio,-10:F3} {accuracy}");
            }

            Console.WriteLine("─────────────────────────────────────────────────────────────");
            Console.WriteLine();
        }

        // ===== Helper Methods =====

        private int[] GenerateRandomData(int size, int seed = 42)
        {
            var data = new int[size];
            var random = new Random(seed);
            for (int i = 0; i < size; i++)
                data[i] = random.Next();
            return data;
        }

        private int[] GenerateNearlySortedData(int size, int percentSorted = 90)
        {
            var data = Enumerable.Range(1, size).ToArray();
            var random = new Random(42);

            int swaps = (size * (100 - percentSorted)) / 100;
            for (int i = 0; i < swaps; i++)
            {
                int idx1 = random.Next(size);
                int idx2 = random.Next(size);
                (data[idx1], data[idx2]) = (data[idx2], data[idx1]);
            }

            return data;
        }

        private int[] GenerateSmallRangeData(int size, int minValue, int maxValue)
        {
            var data = new int[size];
            var random = new Random(42);

            for (int i = 0; i < size; i++)
                data[i] = random.Next(minValue, maxValue + 1);

            return data;
        }

        private void PrintMarkovStatistics(Dictionary<string, object> statistics)
        {
            Console.WriteLine($"  Total Transitions: {statistics["TotalTransitions"]}");
            Console.WriteLine($"  Unique Transitions: {statistics["UniqueTransitions"]}");
            Console.WriteLine($"  States Visited: {statistics["StatesVisited"]}");

            if (statistics["TopPerformingStates"] is Dictionary<string, double> topPerformers)
            {
                Console.WriteLine("  Top Performing States:");
                foreach (var performer in topPerformers.Take(3))
                {
                    Console.WriteLine($"    - {performer.Key}: {performer.Value:F3}");
                }
            }

            if (statistics["MostCommonTransitions"] is Dictionary<string, int> commonTransitions)
            {
                Console.WriteLine("  Most Common Transitions:");
                foreach (var transition in commonTransitions.Take(3))
                {
                    Console.WriteLine($"    - {transition.Key}: {transition.Value} times");
                }
            }
        }
    }

    /// <summary>
    /// Runner class to execute all examples
    /// </summary>
    public class ExampleRunner
    {
        public static void RunAllExamples()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║     Markov Chain-Based Adaptive Sorting System - Examples              ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════════════╝\n");

            var examples = new MarkovSortingExamples();

            try
            {
                examples.Example1_BasicAdaptiveSorting();
                PauseForInput();

                examples.Example2_DataCharacteristicsAnalysis();
                PauseForInput();

                examples.Example3_PerformanceComparison();
                PauseForInput();

                examples.Example4_DetailedMetricsAnalysis();
                PauseForInput();

                examples.Example5_MarkovChainLearning();
                PauseForInput();

                examples.Example6_RealWorldScenario_LogSorting();
                PauseForInput();

                examples.Example7_RealWorldScenario_DatabaseResults();
                PauseForInput();

                examples.Example8_RecommendationConfidence();
                PauseForInput();

                examples.Example9_PerformanceEstimationAccuracy();

                Console.WriteLine("\n╔════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                   All Examples Completed Successfully!                  ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════════════════╝\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }

        private static void PauseForInput()
        {
            Console.WriteLine("\nPress Enter to continue to next example...");
            Console.ReadLine();
            Console.Clear();
        }
    }
}
