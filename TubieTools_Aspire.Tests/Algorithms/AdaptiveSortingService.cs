//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;

//namespace TubieTools_Aspire.Tests.Algorithms
//{
//    /// <summary>
//    /// Adaptive sorting service that uses Markov chain analysis to predict
//    /// and select the best sorting algorithm for given data characteristics.
//    /// 
//    /// This service:
//    /// - Analyzes data characteristics (size, sortedness, entropy, etc.)
//    /// - Recommends appropriate algorithms based on Markov analysis
//    /// - Executes sorts with performance metrics collection
//    /// - Records performance feedback to improve future predictions
//    /// - Supports algorithm comparison and statistical analysis
//    /// 
//    /// The core principle is that different algorithms perform better under
//    /// different conditions, and this service learns and predicts those conditions.
//    /// </summary>
//    public class AdaptiveSortingService 
//    {
//        protected readonly SortingService SortingService;
//        protected readonly MarkovChainAnalyzer MarkovAnalyzer;

//        /// <summary>
//        /// Metrics collected during a sort operation
//        /// </summary>
//        public class SortMetrics
//        {
//            public MarkovChainAnalyzer.SortAlgorithmState Algorithm { get; set; }
//            public long ElapsedMilliseconds { get; set; }
//            public bool SortSuccessful { get; set; }
//            public double PerformanceRatio { get; set; }  // How well it performed relative to estimate
//            public int ComparisonCount { get; set; }
//            public int SwapCount { get; set; }
//            public long MemoryUsedBytes { get; set; }
//            public string Notes { get; set; }
//        }

//        public AdaptiveSortingService()
//        {
//            SortingService = new SortingService();
//            MarkovAnalyzer = new MarkovChainAnalyzer();
//        }

//        /// <summary>
//        /// Gets algorithm recommendation based on data analysis
//        /// </summary>
//        public virtual MarkovChainAnalyzer.AlgorithmPrediction GetAlgorithmRecommendation(int[] data)
//        {
//            if (data == null)
//                throw new ArgumentNullException(nameof(data));

//            return MarkovAnalyzer.PredictBestAlgorithm(data);
//        }

//        /// <summary>
//        /// Performs adaptive sort based on Markov recommendation
//        /// </summary>
//        public virtual void AdaptiveSortByMarkovPrediction(int[] data)
//        {
//            if (data == null)
//                throw new ArgumentNullException(nameof(data));

//            var prediction = GetAlgorithmRecommendation(data);
//            var metrics = SortWithMetrics(data, prediction.RecommendedAlgorithm);

//            if (metrics.SortSuccessful)
//            {
//                MarkovAnalyzer.RecordSortSuccess(
//                    MarkovChainAnalyzer.SortAlgorithmState.SimpleSort,
//                    prediction.RecommendedAlgorithm,
//                    metrics.PerformanceRatio);
//            }
//        }

//        /// <summary>
//        /// Executes sort with performance metrics collection
//        /// </summary>
//        public virtual SortMetrics SortWithMetrics(
//            int[] data,
//            MarkovChainAnalyzer.SortAlgorithmState algorithm)
//        {
//            if (data == null)
//                throw new ArgumentNullException(nameof(data));

//            var metrics = new SortMetrics { Algorithm = algorithm };

//            try
//            {
//                int[] dataCopy = new int[data.Length];
//                Array.Copy(data, dataCopy, data.Length);

//                var stopwatch = Stopwatch.StartNew();
//                long initialMemory = GC.GetTotalMemory(false);

//                // Execute the appropriate sort algorithm
//                ExecuteAlgorithm(dataCopy, algorithm);

//                stopwatch.Stop();
//                long finalMemory = GC.GetTotalMemory(false);

//                metrics.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
//                metrics.MemoryUsedBytes = Math.Max(0, finalMemory - initialMemory);
//                metrics.SortSuccessful = IsSorted(dataCopy);

//                // Estimate performance (compare to estimate)
//                var prediction = MarkovAnalyzer.PredictBestAlgorithm(data);
//                if (prediction.PerformanceEstimates.TryGetValue(algorithm, out var estimate))
//                {
//                    metrics.PerformanceRatio = estimate.EstimatedTimeMs > 0
//                        ? metrics.ElapsedMilliseconds / estimate.EstimatedTimeMs
//                        : 1.0;
//                }
//                else
//                {
//                    metrics.PerformanceRatio = 1.0;
//                }

//                // Copy sorted data back
//                Array.Copy(dataCopy, data, data.Length);
//            }
//            catch (Exception ex)
//            {
//                metrics.SortSuccessful = false;
//                metrics.Notes = $"Exception: {ex.Message}";
//            }

//            return metrics;
//        }

//        /// <summary>
//        /// Analyzes data characteristics
//        /// </summary>
//        public virtual MarkovChainAnalyzer.DataCharacteristics AnalyzeDataCharacteristics(int[] data)
//        {
//            if (data == null)
//                throw new ArgumentNullException(nameof(data));

//            return MarkovAnalyzer.AnalyzeData(data);
//        }

//        /// <summary>
//        /// Compares multiple algorithms on given data
//        /// </summary>
//        public virtual Dictionary<MarkovChainAnalyzer.SortAlgorithmState, SortMetrics> CompareAlgorithmPerformance(int[] data)
//        {
//            if (data == null)
//                throw new ArgumentNullException(nameof(data));

//            var results = new Dictionary<MarkovChainAnalyzer.SortAlgorithmState, SortMetrics>();

//            foreach (MarkovChainAnalyzer.SortAlgorithmState algo in Enum.GetValues(typeof(MarkovChainAnalyzer.SortAlgorithmState)))
//            {
//                int[] dataCopy = new int[data.Length];
//                Array.Copy(data, dataCopy, data.Length);

//                try
//                {
//                    var metrics = SortWithMetrics(dataCopy, algo);
//                    results[algo] = metrics;
//                }
//                catch
//                {
//                    results[algo] = new SortMetrics { Algorithm = algo, SortSuccessful = false };
//                }
//            }

//            return results;
//        }

//        /// <summary>
//        /// Gets Markov chain statistics for analysis
//        /// </summary>
//        public virtual Dictionary<string, object> GetMarkovChainStatistics()
//        {
//            return MarkovAnalyzer.GetMarkovChainStatistics();
//        }

//        /// <summary>
//        /// Executes a specific sorting algorithm
//        /// </summary>
//        protected void ExecuteAlgorithm(int[] data, MarkovChainAnalyzer.SortAlgorithmState algorithm)
//        {
//            switch (algorithm)
//            {
//                case MarkovChainAnalyzer.SortAlgorithmState.SimpleSort:
//                    SortingService.SimpleSort(data);
//                    break;
//                case MarkovChainAnalyzer.SortAlgorithmState.BubbleSort:
//                    SortingService.BubbleSort(data);
//                    break;
//                case MarkovChainAnalyzer.SortAlgorithmState.ModifiedBubbleSort:
//                    SortingService.ModifiedBubbleSort(data);
//                    break;
//                case MarkovChainAnalyzer.SortAlgorithmState.QuickSort:
//                    SortingService.QuickSort(data, 0, data.Length - 1);
//                    break;
//                case MarkovChainAnalyzer.SortAlgorithmState.InsertionSort:
//                    SortingService.InsertionSort(data);
//                    break;
//                case MarkovChainAnalyzer.SortAlgorithmState.SelectionSort:
//                    SortingService.SelectionSort(data);
//                    break;
//                case MarkovChainAnalyzer.SortAlgorithmState.ShellSort:
//                    SortingService.ShellSort(data);
//                    break;
//                case MarkovChainAnalyzer.SortAlgorithmState.RadixSort:
//                    SortingService.IntArrayRadixSort(data);
//                    break;
//                case MarkovChainAnalyzer.SortAlgorithmState.TimSort:
//                    SortingService.IntArrayTimSort(data);
//                    break;
//                case MarkovChainAnalyzer.SortAlgorithmState.CountingSort:
//                    if (CanUseCountingSort(data))
//                        data = SortingService.IntArrayCountingSort(data);
//                    else
//                        SortingService.MergeSort(data, 0, data.Length - 1);
//                    break;
//                case MarkovChainAnalyzer.SortAlgorithmState.IntroSort:
//                    SortingService.IntArrayIntroSort(data);
//                    break;
//                case MarkovChainAnalyzer.SortAlgorithmState.HeapSort:
//                    SortingService.IntArrayHeapSort(data);
//                    break;
//                case MarkovChainAnalyzer.SortAlgorithmState.MergeSort:
//                    SortingService.MergeSort(data, 0, data.Length - 1);
//                    break;
//                case MarkovChainAnalyzer.SortAlgorithmState.CombSort:
//                    SortingService.IntArrayCombSort(data);
//                    break;
//                case MarkovChainAnalyzer.SortAlgorithmState.GnomeSort:
//                    SortingService.IntArrayGnomeSort(data);
//                    break;
//                case MarkovChainAnalyzer.SortAlgorithmState.OddEvenSort:
//                    SortingService.IntArrayOddEvenSort(data);
//                    break;
//                case MarkovChainAnalyzer.SortAlgorithmState.CycleSort:
//                    SortingService.IntArrayCycleSort(data);
//                    break;
//                default:
//                    throw new ArgumentException($"Unknown algorithm: {algorithm}");
//            }
//        }

//        /// <summary>
//        /// Checks if counting sort can be used (limited integer range)
//        /// </summary>
//        private bool CanUseCountingSort(int[] data)
//        {
//            if (data.Length == 0)
//                return false;

//            int min = data.Min();
//            int max = data.Max();
//            long range = (long)max - min;

//            // Counting sort is practical if range is not too large
//            return range >= 0 && range <= data.Length * 10;
//        }

//        /// <summary>
//        /// Checks if an array is sorted
//        /// </summary>
//        protected bool IsSorted(int[] data)
//        {
//            for (int i = 0; i < data.Length - 1; i++)
//            {
//                if (data[i] > data[i + 1])
//                    return false;
//            }
//            return true;
//        }
//    }
//}
