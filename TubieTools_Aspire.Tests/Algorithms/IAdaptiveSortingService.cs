using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TubieTools_Aspire.Tests.Algorithms
{
    /// <summary>
    /// Extended sorting service interface with Markov chain-based algorithm selection
    /// and adaptive sorting capabilities
    /// </summary>
    public interface IAdaptiveSortingService : ISortingService
    {
        /// <summary>
        /// Gets or sets the Markov chain analyzer
        /// </summary>
        MarkovChainAnalyzer MarkovAnalyzer { get; set; }

        /// <summary>
        /// Analyzes data and recommends the best sorting algorithm
        /// </summary>
        MarkovChainAnalyzer.AlgorithmPrediction GetAlgorithmRecommendation(int[] data);

        /// <summary>
        /// Sorts array using the Markov chain-recommended algorithm
        /// </summary>
        void AdaptiveSortByMarkovPrediction(int[] data);

        /// <summary>
        /// Sorts array and records performance metrics for Markov chain learning
        /// </summary>
        SortMetrics SortWithMetrics(int[] data, MarkovChainAnalyzer.SortAlgorithmState algorithmToUse);

        /// <summary>
        /// Analyzes data characteristics
        /// </summary>
        MarkovChainAnalyzer.DataCharacteristics AnalyzeDataCharacteristics(int[] data);

        /// <summary>
        /// Compares performance of multiple algorithms on the same data
        /// </summary>
        Dictionary<MarkovChainAnalyzer.SortAlgorithmState, SortMetrics> CompareAlgorithmPerformance(int[] data);

        /// <summary>
        /// Gets Markov chain statistics and transitions
        /// </summary>
        Dictionary<string, object> GetMarkovChainStatistics();
    }

    /// <summary>
    /// Metrics collected during a sort operation
    /// </summary>
    public class SortMetrics
    {
        public MarkovChainAnalyzer.SortAlgorithmState Algorithm { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public long ElapsedTicks { get; set; }
        public long EstimatedComparisons { get; set; }
        public long EstimatedSwaps { get; set; }
        public double PerformanceRatio { get; set; }  // Actual vs estimated time
        public bool SortSuccessful { get; set; }
        public string TimeComplexity { get; set; }
        public int SpaceComplexity { get; set; }

        public override string ToString()
        {
            return $"{Algorithm}: {ElapsedMilliseconds}ms, Ratio: {PerformanceRatio:F3}, " +
                   $"Comparisons: {EstimatedComparisons:N0}, Complexity: {TimeComplexity}";
        }
    }

    /// <summary>
    /// Adaptive sorting service implementation with Markov chain optimization
    /// </summary>
    public class AdaptiveSortingService : SortingService, IAdaptiveSortingService
    {
        public MarkovChainAnalyzer MarkovAnalyzer { get; set; }

        public AdaptiveSortingService()
        {
            MarkovAnalyzer = new MarkovChainAnalyzer(minDataSize: 50);
        }

        public MarkovChainAnalyzer.AlgorithmPrediction GetAlgorithmRecommendation(int[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return MarkovAnalyzer.PredictBestAlgorithm(data);
        }

        public void AdaptiveSortByMarkovPrediction(int[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var prediction = GetAlgorithmRecommendation(data);
            var metrics = SortWithMetrics(data, prediction.RecommendedAlgorithm);

            if (metrics.SortSuccessful)
            {
                // Record for Markov chain learning
                MarkovAnalyzer.RecordSortSuccess(
                    MarkovChainAnalyzer.SortAlgorithmState.SimpleSort,  // Previous state
                    prediction.RecommendedAlgorithm,
                    metrics.PerformanceRatio);
            }
        }

        public SortMetrics SortWithMetrics(int[] data, MarkovChainAnalyzer.SortAlgorithmState algorithmToUse)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var metrics = new SortMetrics
            {
                Algorithm = algorithmToUse,
                SortSuccessful = false
            };

            var stopwatch = Stopwatch.StartNew();
            var characteristics = MarkovAnalyzer.AnalyzeData(data);

            try
            {
                switch (algorithmToUse)
                {
                    case MarkovChainAnalyzer.SortAlgorithmState.RadixSort:
                        IntArrayRadixSort(data);
                        break;
                    case MarkovChainAnalyzer.SortAlgorithmState.TimSort:
                        IntArrayTimSort(data);
                        break;
                    case MarkovChainAnalyzer.SortAlgorithmState.CountingSort:
                        IntArrayCountingSort(data);
                        break;
                    case MarkovChainAnalyzer.SortAlgorithmState.IntroSort:
                        IntArrayIntroSort(data);
                        break;
                    case MarkovChainAnalyzer.SortAlgorithmState.HeapSort:
                        IntArrayHeapSort(data);
                        break;
                    case MarkovChainAnalyzer.SortAlgorithmState.MergeSort:
                        IntArrayMergeSort(data);
                        break;
                    case MarkovChainAnalyzer.SortAlgorithmState.QuickSort:
                        IntArrayQuickSort(data);
                        break;
                    case MarkovChainAnalyzer.SortAlgorithmState.CombSort:
                        IntArrayCombSort(data);
                        break;
                    case MarkovChainAnalyzer.SortAlgorithmState.InsertionSort:
                        IntArrayInsertionSort(data);
                        break;
                    case MarkovChainAnalyzer.SortAlgorithmState.ModifiedBubbleSort:
                        ModifiedBubbleSort(data);
                        break;
                    default:
                        IntArrayQuickSort(data);
                        break;
                }

                stopwatch.Stop();
                metrics.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                metrics.ElapsedTicks = stopwatch.ElapsedTicks;
                metrics.SortSuccessful = IsSorted(data);

                // Get estimated performance
                var performance = MarkovAnalyzer.EstimatePerformance(
                    algorithmToUse, characteristics, data.Length);
                metrics.EstimatedComparisons = performance.EstimatedComparisons;
                metrics.EstimatedSwaps = performance.EstimatedSwaps;
                metrics.TimeComplexity = performance.TimeComplexity;
                metrics.SpaceComplexity = performance.SpaceComplexity;
                metrics.PerformanceRatio = stopwatch.ElapsedMilliseconds > 0 
                    ? performance.EstimatedTimeMs / stopwatch.ElapsedMilliseconds
                    : 1.0;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                metrics.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                metrics.SortSuccessful = false;
            }

            return metrics;
        }

        public MarkovChainAnalyzer.DataCharacteristics AnalyzeDataCharacteristics(int[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return MarkovAnalyzer.AnalyzeData(data);
        }

        public Dictionary<MarkovChainAnalyzer.SortAlgorithmState, SortMetrics> CompareAlgorithmPerformance(int[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var results = new Dictionary<MarkovChainAnalyzer.SortAlgorithmState, SortMetrics>();

            var algorithmsToTest = new[]
            {
                MarkovChainAnalyzer.SortAlgorithmState.RadixSort,
                MarkovChainAnalyzer.SortAlgorithmState.TimSort,
                MarkovChainAnalyzer.SortAlgorithmState.IntroSort,
                MarkovChainAnalyzer.SortAlgorithmState.MergeSort,
                MarkovChainAnalyzer.SortAlgorithmState.HeapSort,
                MarkovChainAnalyzer.SortAlgorithmState.QuickSort
            };

            foreach (var algorithm in algorithmsToTest)
            {
                // Create a copy of the data for each sort
                int[] dataCopy = new int[data.Length];
                Array.Copy(data, dataCopy, data.Length);

                var metrics = SortWithMetrics(dataCopy, algorithm);
                results[algorithm] = metrics;
            }

            return results;
        }

        public Dictionary<string, object> GetMarkovChainStatistics()
        {
            return MarkovAnalyzer.GetMarkovChainStatistics();
        }

        /// <summary>
        /// Helper method to verify if array is sorted
        /// </summary>
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
