using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TubieTools_Aspire.Tests.Algorithms
{
    /// <summary>
    /// Analyzes data characteristics using Markov chain principles to predict
    /// which sorting algorithm will perform best on given data.
    /// </summary>
    public class MarkovChainAnalyzer
    {
        private readonly Dictionary<(SortAlgorithmState, SortAlgorithmState), int> _transitionCounts;
        private readonly Dictionary<SortAlgorithmState, double> _statePerformance;
        private readonly Dictionary<SortAlgorithmState, int> _stateVisits;
        private readonly int _minDataSize;

        /// <summary>
        /// Represents different sorting algorithm states/categories
        /// </summary>
        public enum SortAlgorithmState
        {
            /// <summary>Non-comparative, optimal for integers with small range</summary>
            CountingSort,
            /// <summary>Non-comparative, optimal for large integer datasets</summary>
            RadixSort,
            /// <summary>Hybrid with excellent real-world performance</summary>
            TimSort,
            /// <summary>Guaranteed log-linear, excellent worst-case</summary>
            IntroSort,
            /// <summary>Divide-and-conquer, stable, guaranteed performance</summary>
            MergeSort,
            /// <summary>In-place heap, constant space, guaranteed log-linear</summary>
            HeapSort,
            /// <summary>Quick and effective for large unsorted data</summary>
            QuickSort,
            /// <summary>Improved bubble, good for medium datasets</summary>
            CombSort,
            /// <summary>Simple and effective for small arrays</summary>
            InsertionSort,
            /// <summary>Good for nearly sorted data</summary>
            ModifiedBubbleSort,
            /// <summary>Simple and educational value</summary>
            SimpleSort
        }

        /// <summary>
        /// Data characteristics analyzed for algorithm selection
        /// </summary>
        public class DataCharacteristics
        {
            public int Size { get; set; }
            public double SortednessRatio { get; set; }  // 0 = reverse sorted, 1 = already sorted
            public int DistinctValues { get; set; }
            public double RangeSpan { get; set; }  // (max - min) / n
            public double Entropy { get; set; }  // Randomness measure
            public bool HasNegatives { get; set; }
            public bool IsMonotonic { get; set; }
            public List<int> InversionsPerSegment { get; set; } = new List<int>();
            public double AverageClusterSize { get; set; }
            public Dictionary<string, double> FeatureScores { get; set; } = new Dictionary<string, double>();
        }

        /// <summary>
        /// Result of algorithm prediction
        /// </summary>
        public class AlgorithmPrediction
        {
            public SortAlgorithmState RecommendedAlgorithm { get; set; }
            public double ConfidenceScore { get; set; }
            public Dictionary<SortAlgorithmState, double> AlgorithmScores { get; set; } = new Dictionary<SortAlgorithmState, double>();
            public DataCharacteristics Characteristics { get; set; }
            public string RecommendationReason { get; set; }
            public Dictionary<SortAlgorithmState, EstimatedPerformance> PerformanceEstimates { get; set; } = new Dictionary<SortAlgorithmState, EstimatedPerformance>();
        }

        /// <summary>
        /// Estimated performance metrics for an algorithm
        /// </summary>
        public class EstimatedPerformance
        {
            public double EstimatedTimeMs { get; set; }
            public string TimeComplexity { get; set; }
            public long EstimatedComparisons { get; set; }
            public long EstimatedSwaps { get; set; }
            public int SpaceComplexity { get; set; }
        }

        public MarkovChainAnalyzer(int minDataSize = 50)
        {
            _transitionCounts = new Dictionary<(SortAlgorithmState, SortAlgorithmState), int>();
            _statePerformance = new Dictionary<SortAlgorithmState, double>();
            _stateVisits = new Dictionary<SortAlgorithmState, int>();
            _minDataSize = minDataSize;
            InitializeStateMetrics();
        }

        private void InitializeStateMetrics()
        {
            foreach (SortAlgorithmState state in Enum.GetValues(typeof(SortAlgorithmState)))
            {
                _statePerformance[state] = 0.5;  // Neutral starting point
                _stateVisits[state] = 0;
            }
        }

        /// <summary>
        /// Analyzes the characteristics of the input array
        /// </summary>
        public DataCharacteristics AnalyzeData(int[] data)
        {
            var characteristics = new DataCharacteristics
            {
                Size = data.Length,
                HasNegatives = data.Any(x => x < 0)
            };

            if (data.Length == 0)
                return characteristics;

            // Calculate sortedness ratio
            characteristics.SortednessRatio = CalculateSortednessRatio(data);

            // Calculate distinct values and range
            var distinctValues = new HashSet<int>(data);
            characteristics.DistinctValues = distinctValues.Count;
            int min = data.Min();
            int max = data.Max();
            characteristics.RangeSpan = data.Length > 0 ? (double)(max - min) / data.Length : 0;

            // Calculate entropy (randomness measure)
            characteristics.Entropy = CalculateEntropy(data, distinctValues);

            // Check if monotonic
            characteristics.IsMonotonic = IsMonotonic(data);

            // Calculate inversions per segment
            characteristics.InversionsPerSegment = CalculateInversionsPerSegment(data);

            // Calculate average cluster size (consecutive equal or nearly equal elements)
            characteristics.AverageClusterSize = CalculateAverageClusterSize(data);

            // Compute feature scores
            ComputeFeatureScores(characteristics, data);

            return characteristics;
        }

        private double CalculateSortednessRatio(int[] data)
        {
            if (data.Length <= 1)
                return 1.0;

            int sortedPairs = 0;
            for (int i = 0; i < data.Length - 1; i++)
            {
                if (data[i] <= data[i + 1])
                    sortedPairs++;
            }

            return (double)sortedPairs / (data.Length - 1);
        }

        private double CalculateEntropy(int[] data, HashSet<int> distinctValues)
        {
            if (distinctValues.Count <= 1)
                return 0;

            var frequencies = new Dictionary<int, int>();
            foreach (int value in data)
            {
                if (frequencies.ContainsKey(value))
                    frequencies[value]++;
                else
                    frequencies[value] = 1;
            }

            double entropy = 0;
            foreach (var freq in frequencies.Values)
            {
                double probability = (double)freq / data.Length;
                entropy -= probability * Math.Log2(probability);
            }

            return entropy / Math.Log2(distinctValues.Count);
        }

        private bool IsMonotonic(int[] data)
        {
            if (data.Length <= 1)
                return true;

            bool isAscending = true;
            bool isDescending = true;

            for (int i = 0; i < data.Length - 1; i++)
            {
                if (data[i] > data[i + 1])
                    isAscending = false;
                if (data[i] < data[i + 1])
                    isDescending = false;
            }

            return isAscending || isDescending;
        }

        private List<int> CalculateInversionsPerSegment(int[] data, int segmentSize = 100)
        {
            var inversions = new List<int>();
            int actualSegmentSize = Math.Max(segmentSize, Math.Max(1, data.Length / 10));

            for (int i = 0; i < data.Length; i += actualSegmentSize)
            {
                int end = Math.Min(i + actualSegmentSize, data.Length);
                int segmentInversions = 0;

                for (int j = i; j < end - 1; j++)
                {
                    if (data[j] > data[j + 1])
                        segmentInversions++;
                }

                inversions.Add(segmentInversions);
            }

            return inversions;
        }

        private double CalculateAverageClusterSize(int[] data)
        {
            if (data.Length <= 1)
                return 1;

            int clusterCount = 1;
            int totalElements = data.Length;

            for (int i = 0; i < data.Length - 1; i++)
            {
                if (data[i] != data[i + 1])
                    clusterCount++;
            }

            return (double)totalElements / clusterCount;
        }

        private void ComputeFeatureScores(DataCharacteristics characteristics, int[] data)
        {
            characteristics.FeatureScores["sortedness"] = characteristics.SortednessRatio;
            characteristics.FeatureScores["entropy"] = characteristics.Entropy;
            characteristics.FeatureScores["distinctness_ratio"] = (double)characteristics.DistinctValues / data.Length;
            characteristics.FeatureScores["range_span"] = Math.Min(1.0, characteristics.RangeSpan);
            characteristics.FeatureScores["cluster_efficiency"] = 1.0 / characteristics.AverageClusterSize;
            characteristics.FeatureScores["monotonicity"] = characteristics.IsMonotonic ? 1.0 : 0.0;
        }

        /// <summary>
        /// Predicts the best sorting algorithm using Markov chain analysis
        /// </summary>
        public AlgorithmPrediction PredictBestAlgorithm(int[] data)
        {
            var characteristics = AnalyzeData(data);
            var prediction = new AlgorithmPrediction { Characteristics = characteristics };

            if (data.Length < _minDataSize)
            {
                prediction.RecommendedAlgorithm = SortAlgorithmState.InsertionSort;
                prediction.ConfidenceScore = 0.95;
                prediction.RecommendationReason = "Array is too small; Insertion Sort is optimal.";
                PopulateAlgorithmScores(prediction, characteristics);
                return prediction;
            }

            // Score each algorithm based on data characteristics
            var algorithmScores = new Dictionary<SortAlgorithmState, double>();

            foreach (SortAlgorithmState algorithm in Enum.GetValues(typeof(SortAlgorithmState)))
            {
                double score = ScoreAlgorithm(algorithm, characteristics, data);
                algorithmScores[algorithm] = score;
            }

            // Apply Markov chain transition probabilities
            var markovAdjustedScores = ApplyMarkovTransitions(algorithmScores);

            // Select best algorithm
            var bestAlgorithm = markovAdjustedScores.OrderByDescending(x => x.Value).First();
            prediction.RecommendedAlgorithm = bestAlgorithm.Key;
            prediction.ConfidenceScore = NormalizeConfidence(bestAlgorithm.Value, markovAdjustedScores.Values.Max());
            prediction.AlgorithmScores = markovAdjustedScores;

            // Generate recommendation reason
            prediction.RecommendationReason = GenerateRecommendationReason(
                prediction.RecommendedAlgorithm, characteristics);

            // Estimate performance for top algorithms
            var topAlgorithms = markovAdjustedScores
                .OrderByDescending(x => x.Value)
                .Take(5)
                .ToList();

            foreach (var algo in topAlgorithms)
            {
                prediction.PerformanceEstimates[algo.Key] = EstimatePerformance(
                    algo.Key, characteristics, data.Length);
            }

            return prediction;
        }
        //TODO: Implement PopulateAlgorithmScores to fill in the AlgorithmScores dictionary with detailed scoring for each algorithm based on characteristics.
        private void PopulateAlgorithmScores(AlgorithmPrediction prediction, DataCharacteristics characteristics)
        {
            throw new NotImplementedException();
        }

        private double ScoreAlgorithm(SortAlgorithmState algorithm, DataCharacteristics characteristics, int[] data)
        {
            double score = 0;

            switch (algorithm)
            {
                case SortAlgorithmState.CountingSort:
                    // Excellent for small range integers
                    score = (1 - characteristics.FeatureScores["range_span"]) * 0.8;
                    score += (1 - characteristics.FeatureScores["entropy"]) * 0.2;
                    break;

                case SortAlgorithmState.RadixSort:
                    // Excellent for large integer datasets with reasonable digit count
                    score = characteristics.FeatureScores["distinctness_ratio"] * 0.6;
                    score += (1 - characteristics.FeatureScores["entropy"]) * 0.4;
                    break;

                case SortAlgorithmState.TimSort:
                    // Excellent adaptive sort, works well on partially sorted data
                    score = characteristics.FeatureScores["sortedness"] * 0.5;
                    score += (1 - Math.Abs(characteristics.FeatureScores["entropy"] - 0.5)) * 0.3;
                    score += characteristics.FeatureScores["cluster_efficiency"] * 0.2;
                    break;

                case SortAlgorithmState.IntroSort:
                    // Guaranteed O(n log n), good general-purpose
                    score = 0.7;
                    score += characteristics.FeatureScores["sortedness"] * 0.15;
                    score -= characteristics.FeatureScores["entropy"] * 0.15;
                    break;

                case SortAlgorithmState.MergeSort:
                    // Stable, guaranteed O(n log n), good on linked structures
                    score = 0.65;
                    score += (1 - characteristics.FeatureScores["entropy"]) * 0.15;
                    score += characteristics.FeatureScores["distinctness_ratio"] * 0.2;
                    break;

                case SortAlgorithmState.HeapSort:
                    // In-place, guaranteed O(n log n), great for memory constraints
                    score = 0.6;
                    score += Math.Max(0, 0.3 - characteristics.FeatureScores["sortedness"] * 0.3);
                    break;

                case SortAlgorithmState.QuickSort:
                    // Fast average case, good for random data
                    score = characteristics.FeatureScores["entropy"] * 0.6;
                    score += (1 - characteristics.FeatureScores["sortedness"]) * 0.2;
                    score += (1 - Math.Abs(characteristics.FeatureScores["entropy"] - 1)) * 0.2;
                    break;

                case SortAlgorithmState.CombSort:
                    // Improved bubble, good for medium datasets
                    score = (1 - characteristics.FeatureScores["sortedness"]) * 0.4;
                    score += characteristics.FeatureScores["entropy"] * 0.3;
                    score += (characteristics.FeatureScores["cluster_efficiency"] * 0.3);
                    break;

                case SortAlgorithmState.InsertionSort:
                    // Best for small or nearly sorted arrays
                    score = characteristics.FeatureScores["sortedness"] * 0.7;
                    score += (1 - characteristics.FeatureScores["entropy"]) * 0.3;
                    break;

                case SortAlgorithmState.ModifiedBubbleSort:
                    // Good for nearly sorted data
                    score = characteristics.FeatureScores["sortedness"] * 0.8;
                    score += characteristics.FeatureScores["cluster_efficiency"] * 0.2;
                    break;

                case SortAlgorithmState.SimpleSort:
                    // Basic sort as fallback
                    score = 0.3;
                    break;
            }

            // Apply state performance history
            if (_stateVisits[algorithm] > 0)
            {
                score = score * 0.7 + _statePerformance[algorithm] * 0.3;
            }

            return Math.Max(0, Math.Min(1, score));  // Clamp to [0, 1]
        }

        private Dictionary<SortAlgorithmState, double> ApplyMarkovTransitions(Dictionary<SortAlgorithmState, double> scores)
        {
            var adjustedScores = new Dictionary<SortAlgorithmState, double>(scores);

            // Apply transition probabilities from Markov chain
            foreach (var algorithm in scores.Keys.ToList())
            {
                double transitionBoost = 0;
                int totalTransitions = 0;

                foreach (var curState in Enum.GetValues(typeof(SortAlgorithmState)).Cast<SortAlgorithmState>())
                {
                    var key = (curState, algorithm);
                    if (_transitionCounts.ContainsKey(key))
                    {
                        transitionBoost += _transitionCounts[key];
                        totalTransitions += _transitionCounts[key];
                    }
                }

                if (totalTransitions > 0)
                {
                    double transitionProbability = (double)transitionBoost / (totalTransitions + 1);
                    adjustedScores[algorithm] = adjustedScores[algorithm] * 0.8 + transitionProbability * 0.2;
                }
            }

            return adjustedScores;
        }

        private double NormalizeConfidence(double score, double maxScore)
        {
            if (maxScore == 0)
                return 0.5;
            return Math.Min(0.99, score / maxScore);
        }

        private string GenerateRecommendationReason(SortAlgorithmState algorithm, DataCharacteristics characteristics)
        {
            return algorithm switch
            {
                SortAlgorithmState.CountingSort =>
                    $"Small integer range detected (range span: {characteristics.FeatureScores["range_span"]:F3}). Counting Sort optimal.",

                SortAlgorithmState.RadixSort =>
                    $"Large dataset with good digit distribution. Radix Sort scales linearly.",

                SortAlgorithmState.TimSort =>
                    $"Partially sorted data detected (sortedness: {characteristics.FeatureScores["sortedness"]:F3}). Tim Sort exploits existing order.",

                SortAlgorithmState.IntroSort =>
                    $"Mixed characteristics with randomness ({characteristics.FeatureScores["entropy"]:F3}). Intro Sort provides optimal worst-case guarantee.",

                SortAlgorithmState.MergeSort =>
                    $"Stable sort required with high entropy ({characteristics.FeatureScores["entropy"]:F3}). Merge Sort maintains stability.",

                SortAlgorithmState.HeapSort =>
                    $"Memory-efficient sort needed. Heap Sort works in-place with guaranteed O(n log n).",

                SortAlgorithmState.QuickSort =>
                    $"High entropy data ({characteristics.FeatureScores["entropy"]:F3}). Quick Sort excels on random data.",

                SortAlgorithmState.CombSort =>
                    $"Medium-sized dataset with moderate clustering. Comb Sort combines efficiency with simplicity.",

                SortAlgorithmState.InsertionSort =>
                    $"Small array or highly sorted data ({characteristics.FeatureScores["sortedness"]:F3}). Insertion Sort optimal.",

                SortAlgorithmState.ModifiedBubbleSort =>
                    $"Nearly sorted data detected ({characteristics.FeatureScores["sortedness"]:F3}). Modified Bubble Sort adapts quickly.",

                _ => "Default algorithm selected based on general characteristics."
            };
        }

        public EstimatedPerformance EstimatePerformance(SortAlgorithmState algorithm, 
            DataCharacteristics characteristics, int n)
        {
            var performance = new EstimatedPerformance();

            // Estimate complexity and operations based on algorithm and data characteristics
            switch (algorithm)
            {
                case SortAlgorithmState.CountingSort:
                    performance.TimeComplexity = "O(n + k)";
                    performance.EstimatedComparisons = n;
                    performance.EstimatedSwaps = n;
                    performance.EstimatedTimeMs = (n + Math.Max(0, n * characteristics.RangeSpan)) / 1_000_000;
                    break;

                case SortAlgorithmState.RadixSort:
                    int digits = (int)Math.Ceiling(Math.Log10(int.MaxValue));
                    performance.TimeComplexity = "O(n * k)";
                    performance.EstimatedComparisons = n * digits;
                    performance.EstimatedSwaps = n * digits;
                    performance.EstimatedTimeMs = (n * digits) / 500_000;
                    break;

                case SortAlgorithmState.TimSort:
                    performance.TimeComplexity = "O(n log n)";
                    performance.EstimatedComparisons = (long)(n * Math.Log2(n) * characteristics.SortednessRatio);
                    performance.EstimatedSwaps = (long)(n * Math.Log2(n) * (1 - characteristics.SortednessRatio));
                    performance.EstimatedTimeMs = (n * Math.Log2(n) * (1 - characteristics.SortednessRatio * 0.5)) / 500_000;
                    break;

                case SortAlgorithmState.IntroSort:
                    performance.TimeComplexity = "O(n log n)";
                    performance.EstimatedComparisons = (long)(n * Math.Log2(n) * 1.2);
                    performance.EstimatedSwaps = (long)(n * Math.Log2(n) * 0.8);
                    performance.EstimatedTimeMs = (n * Math.Log2(n)) / 400_000;
                    break;

                case SortAlgorithmState.MergeSort:
                    performance.TimeComplexity = "O(n log n)";
                    performance.EstimatedComparisons = (long)(n * Math.Log2(n));
                    performance.EstimatedSwaps = (long)(n * Math.Log2(n));
                    performance.EstimatedTimeMs = (n * Math.Log2(n)) / 450_000;
                    break;

                case SortAlgorithmState.HeapSort:
                    performance.TimeComplexity = "O(n log n)";
                    performance.EstimatedComparisons = (long)(2 * n * Math.Log2(n));
                    performance.EstimatedSwaps = (long)(n * Math.Log2(n));
                    performance.EstimatedTimeMs = (2 * n * Math.Log2(n)) / 500_000;
                    break;

                case SortAlgorithmState.QuickSort:
                    performance.TimeComplexity = characteristics.SortednessRatio > 0.8 ? "O(n²)" : "O(n log n)";
                    performance.EstimatedComparisons = (long)(n * Math.Log2(n) * Math.Max(1, 1 / characteristics.SortednessRatio));
                    performance.EstimatedSwaps = (long)(n * Math.Log2(n) * 0.5);
                    performance.EstimatedTimeMs = (n * Math.Log2(n)) / 300_000;
                    break;

                case SortAlgorithmState.CombSort:
                    performance.TimeComplexity = "O(n²)";
                    performance.EstimatedComparisons = (long)(n * n * 0.3);
                    performance.EstimatedSwaps = (long)(n * n * 0.2);
                    performance.EstimatedTimeMs = (n * n * 0.25) / 1_000_000;
                    break;

                case SortAlgorithmState.InsertionSort:
                    performance.TimeComplexity = "O(n) to O(n²)";
                    performance.EstimatedComparisons = (long)(n * n * characteristics.SortednessRatio);
                    performance.EstimatedSwaps = (long)(n * n * characteristics.SortednessRatio * 0.5);
                    performance.EstimatedTimeMs = (n * n * characteristics.SortednessRatio) / 2_000_000;
                    break;

                case SortAlgorithmState.ModifiedBubbleSort:
                    performance.TimeComplexity = "O(n) to O(n²)";
                    performance.EstimatedComparisons = (long)(n * n * (1 - characteristics.SortednessRatio) * 0.5);
                    performance.EstimatedSwaps = (long)(n * n * (1 - characteristics.SortednessRatio) * 0.3);
                    performance.EstimatedTimeMs = (n * n * (1 - characteristics.SortednessRatio) * 0.4) / 2_000_000;
                    break;

                default:
                    performance.TimeComplexity = "Unknown";
                    performance.EstimatedComparisons = n * n;
                    performance.EstimatedSwaps = n * n;
                    performance.EstimatedTimeMs = 0;
                    break;
            }

            // Ensure non-negative estimates
            performance.EstimatedTimeMs = Math.Max(0.01, performance.EstimatedTimeMs);
            performance.SpaceComplexity = GetSpaceComplexityValue(algorithm);

            return performance;
        }

        private int GetSpaceComplexityValue(SortAlgorithmState algorithm)
        {
            return algorithm switch
            {
                SortAlgorithmState.CountingSort => 2,  // O(n + k)
                SortAlgorithmState.RadixSort => 2,     // O(n + k)
                SortAlgorithmState.TimSort => 1,       // O(n)
                SortAlgorithmState.MergeSort => 1,     // O(n)
                SortAlgorithmState.HeapSort => 0,      // O(1)
                SortAlgorithmState.QuickSort => 0,     // O(log n) ~= O(1)
                SortAlgorithmState.IntroSort => 0,     // O(log n)
                SortAlgorithmState.CombSort => 0,      // O(1)
                SortAlgorithmState.InsertionSort => 0, // O(1)
                SortAlgorithmState.ModifiedBubbleSort => 0, // O(1)
                _ => 0
            };
        }

        /// <summary>
        /// Records a successful sort to update Markov chain transition probabilities
        /// </summary>
        public void RecordSortSuccess(SortAlgorithmState fromState, SortAlgorithmState toState, double performanceRatio)
        {
            var key = (fromState, toState);
            if (_transitionCounts.ContainsKey(key))
                _transitionCounts[key]++;
            else
                _transitionCounts[key] = 1;

            _stateVisits[toState]++;
            _statePerformance[toState] = (_statePerformance[toState] * 0.8) + (performanceRatio * 0.2);
        }

        /// <summary>
        /// Gets the transition probability from one algorithm state to another
        /// </summary>
        public double GetTransitionProbability(SortAlgorithmState fromState, SortAlgorithmState toState)
        {
            var key = (fromState, toState);
            int transitionCount = _transitionCounts.ContainsKey(key) ? _transitionCounts[key] : 0;

            int totalFromState = _transitionCounts.Keys
                .Where(k => k.Item1 == fromState)
                .Sum(k => _transitionCounts[k]);

            if (totalFromState == 0)
                return 1.0 / Enum.GetNames(typeof(SortAlgorithmState)).Length;  // Uniform distribution

            return (double)transitionCount / totalFromState;
        }

        /// <summary>
        /// Gets statistical summary of the Markov chain
        /// </summary>
        public Dictionary<string, object> GetMarkovChainStatistics()
        {
            var stats = new Dictionary<string, object>
            {
                { "TotalTransitions", _transitionCounts.Sum(x => x.Value) },
                { "UniqueTransitions", _transitionCounts.Count },
                { "StatesVisited", _stateVisits.Count(x => x.Value > 0) }
            };

            // Sort state performance
            var statePerf = _statePerformance
                .OrderByDescending(x => x.Value)
                .Take(5)
                .ToDictionary(x => x.Key.ToString(), x => x.Value);

            stats["TopPerformingStates"] = statePerf;

            // Most common transitions
            var transitions = _transitionCounts
                .OrderByDescending(x => x.Value)
                .Take(3)
                .ToDictionary(x => $"{x.Key.Item1} -> {x.Key.Item2}", x => x.Value);

            stats["MostCommonTransitions"] = transitions;

            return stats;
        }
    }
}
