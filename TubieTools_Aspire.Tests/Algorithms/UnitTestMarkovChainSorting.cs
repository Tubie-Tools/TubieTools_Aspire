using System;
using System.Collections.Generic;
using System.Linq;

namespace TubieTools_Aspire.Tests.Algorithms
{
    [TestClass]
    public class UnitTestMarkovChainSorting
    {
        private IAdaptiveSortingService _adaptiveService;
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
            _adaptiveService = new AdaptiveSortingService();
            _testData = new int[1000];
            InitializeRandomData();
        }

        private void InitializeRandomData()
        {
            Random r = new Random(42);
            for (int i = 0; i < _testData.Length; i++)
                _testData[i] = r.Next();
        }

        #region Data Analysis Tests

        [TestMethod]
        public void TestDataAnalysisRandomArray()
        {
            var characteristics = _adaptiveService.AnalyzeDataCharacteristics(_testData);

            Assert.IsNotNull(characteristics);
            Assert.AreEqual(_testData.Length, characteristics.Size);
            Assert.IsTrue(characteristics.SortednessRatio >= 0 && characteristics.SortednessRatio <= 1);
            Assert.IsTrue(characteristics.Entropy >= 0 && characteristics.Entropy <= 1);
            Assert.IsTrue(characteristics.DistinctValues > 0);

            TestContext.WriteLine($"Random Array Analysis:");
            TestContext.WriteLine($"  Size: {characteristics.Size}");
            TestContext.WriteLine($"  Sortedness: {characteristics.SortednessRatio:F3}");
            TestContext.WriteLine($"  Entropy: {characteristics.Entropy:F3}");
            TestContext.WriteLine($"  Distinct Values: {characteristics.DistinctValues}");
            TestContext.WriteLine($"  Range Span: {characteristics.RangeSpan:F3}");
        }

        [TestMethod]
        public void TestDataAnalysisSortedArray()
        {
            int[] sortedData = Enumerable.Range(1, 1000).ToArray();
            var characteristics = _adaptiveService.AnalyzeDataCharacteristics(sortedData);

            Assert.IsTrue(characteristics.SortednessRatio > 0.95);
            Assert.IsTrue(characteristics.IsMonotonic);
            Assert.IsTrue(characteristics.Entropy < 0.1);  // Very low entropy for sorted data

            TestContext.WriteLine($"Sorted Array Analysis:");
            TestContext.WriteLine($"  Sortedness: {characteristics.SortednessRatio:F3}");
            TestContext.WriteLine($"  Monotonic: {characteristics.IsMonotonic}");
            TestContext.WriteLine($"  Entropy: {characteristics.Entropy:F3}");
        }

        [TestMethod]
        public void TestDataAnalysisReverseSortedArray()
        {
            int[] reverseSortedData = Enumerable.Range(1, 1000).Reverse().ToArray();
            var characteristics = _adaptiveService.AnalyzeDataCharacteristics(reverseSortedData);

            Assert.IsTrue(characteristics.SortednessRatio < 0.05);
            Assert.IsTrue(characteristics.IsMonotonic);

            TestContext.WriteLine($"Reverse Sorted Array Analysis:");
            TestContext.WriteLine($"  Sortedness: {characteristics.SortednessRatio:F3}");
            TestContext.WriteLine($"  Monotonic: {characteristics.IsMonotonic}");
        }

        [TestMethod]
        public void TestDataAnalysisSmallRangeArray()
        {
            int[] smallRangeData = new int[100];
            Random r = new Random(123);
            for (int i = 0; i < 100; i++)
                smallRangeData[i] = r.Next(0, 10);  // Very small range

            var characteristics = _adaptiveService.AnalyzeDataCharacteristics(smallRangeData);

            Assert.IsTrue(characteristics.RangeSpan < 0.1);
            Assert.IsTrue(characteristics.AverageClusterSize > 1);

            TestContext.WriteLine($"Small Range Array Analysis:");
            TestContext.WriteLine($"  Range Span: {characteristics.RangeSpan:F3}");
            TestContext.WriteLine($"  Average Cluster Size: {characteristics.AverageClusterSize:F3}");
            TestContext.WriteLine($"  Distinct Values: {characteristics.DistinctValues}");
        }

        [TestMethod]
        public void TestFeatureScores()
        {
            var characteristics = _adaptiveService.AnalyzeDataCharacteristics(_testData);

            Assert.IsTrue(characteristics.FeatureScores.ContainsKey("sortedness"));
            Assert.IsTrue(characteristics.FeatureScores.ContainsKey("entropy"));
            Assert.IsTrue(characteristics.FeatureScores.ContainsKey("distinctness_ratio"));
            Assert.IsTrue(characteristics.FeatureScores.ContainsKey("range_span"));
            Assert.IsTrue(characteristics.FeatureScores.ContainsKey("cluster_efficiency"));
            Assert.IsTrue(characteristics.FeatureScores.ContainsKey("monotonicity"));

            foreach (var score in characteristics.FeatureScores)
            {
                Assert.IsTrue(score.Value >= 0 && score.Value <= 1, 
                    $"Feature {score.Key} score out of range: {score.Value}");
                TestContext.WriteLine($"  {score.Key}: {score.Value:F3}");
            }
        }

        #endregion

        #region Algorithm Recommendation Tests

        [TestMethod]
        public void TestAlgorithmRecommendationRandomArray()
        {
            var prediction = _adaptiveService.GetAlgorithmRecommendation(_testData);

            Assert.IsNotNull(prediction);
            Assert.IsNotNull(prediction.RecommendedAlgorithm);
            Assert.IsTrue(prediction.ConfidenceScore > 0 && prediction.ConfidenceScore <= 1);
            Assert.IsNotNull(prediction.RecommendationReason);
            Assert.IsTrue(prediction.AlgorithmScores.Count > 0);

            TestContext.WriteLine($"Algorithm Recommendation:");
            TestContext.WriteLine($"  Recommended: {prediction.RecommendedAlgorithm}");
            TestContext.WriteLine($"  Confidence: {prediction.ConfidenceScore:F3}");
            TestContext.WriteLine($"  Reason: {prediction.RecommendationReason}");
        }

        [TestMethod]
        public void TestAlgorithmRecommendationSortedArray()
        {
            int[] sortedData = Enumerable.Range(1, 1000).ToArray();
            var prediction = _adaptiveService.GetAlgorithmRecommendation(sortedData);

            // For sorted data, should recommend Tim Sort, Modified Bubble Sort, or Insertion Sort
            var adaptiveAlgos = new[]
            {
                MarkovChainAnalyzer.SortAlgorithmState.TimSort,
                MarkovChainAnalyzer.SortAlgorithmState.ModifiedBubbleSort,
                MarkovChainAnalyzer.SortAlgorithmState.InsertionSort
            };

            Assert.IsTrue(adaptiveAlgos.Contains(prediction.RecommendedAlgorithm),
                $"Expected adaptive algorithm for sorted data, got {prediction.RecommendedAlgorithm}");

            TestContext.WriteLine($"Sorted Array Recommendation: {prediction.RecommendedAlgorithm}");
        }

        [TestMethod]
        public void TestAlgorithmRecommendationSmallArray()
        {
            int[] smallData = new int[10];
            Random r = new Random();
            for (int i = 0; i < 10; i++)
                smallData[i] = r.Next();

            var prediction = _adaptiveService.GetAlgorithmRecommendation(smallData);

            // For small arrays, should recommend Insertion Sort
            Assert.AreEqual(MarkovChainAnalyzer.SortAlgorithmState.InsertionSort, 
                prediction.RecommendedAlgorithm);

            TestContext.WriteLine($"Small Array Recommendation: {prediction.RecommendedAlgorithm}");
        }

        [TestMethod]
        public void TestAlgorithmRecommendationSmallRangeArray()
        {
            int[] smallRangeData = new int[500];
            Random r = new Random();
            for (int i = 0; i < 500; i++)
                smallRangeData[i] = r.Next(0, 100);

            var prediction = _adaptiveService.GetAlgorithmRecommendation(smallRangeData);

            // For small range, should recommend Counting Sort or Radix Sort
            var rangeOptimal = new[]
            {
                MarkovChainAnalyzer.SortAlgorithmState.CountingSort,
                MarkovChainAnalyzer.SortAlgorithmState.RadixSort
            };

            Assert.IsTrue(rangeOptimal.Contains(prediction.RecommendedAlgorithm),
                $"Expected range-optimal algorithm, got {prediction.RecommendedAlgorithm}");

            TestContext.WriteLine($"Small Range Recommendation: {prediction.RecommendedAlgorithm}");
        }

        [TestMethod]
        public void TestPerformanceEstimates()
        {
            var prediction = _adaptiveService.GetAlgorithmRecommendation(_testData);

            Assert.IsTrue(prediction.PerformanceEstimates.Count > 0);

            foreach (var estimate in prediction.PerformanceEstimates)
            {
                Assert.IsNotNull(estimate.Value);
                Assert.IsTrue(estimate.Value.EstimatedTimeMs > 0);
                Assert.IsTrue(estimate.Value.EstimatedComparisons > 0);
                Assert.IsNotNull(estimate.Value.TimeComplexity);

                TestContext.WriteLine($"{estimate.Key}:");
                TestContext.WriteLine($"  Estimated Time: {estimate.Value.EstimatedTimeMs:F3}ms");
                TestContext.WriteLine($"  Comparisons: {estimate.Value.EstimatedComparisons}");
                TestContext.WriteLine($"  Complexity: {estimate.Value.TimeComplexity}");
            }
        }

        #endregion

        #region Adaptive Sorting Tests

        [TestMethod]
        public void TestAdaptiveSortByMarkovPrediction()
        {
            int[] dataCopy = new int[_testData.Length];
            Array.Copy(_testData, dataCopy, _testData.Length);

            _adaptiveService.AdaptiveSortByMarkovPrediction(dataCopy);

            Assert.IsTrue(IsSorted(dataCopy));
            TestContext.WriteLine("Adaptive Sort by Markov Prediction: PASSED");
        }

        [TestMethod]
        public void TestSortWithMetricsRadixSort()
        {
            int[] dataCopy = new int[100];
            Random r = new Random();
            for (int i = 0; i < 100; i++)
                dataCopy[i] = r.Next(0, 1000);

            var metrics = _adaptiveService.SortWithMetrics(dataCopy, 
                MarkovChainAnalyzer.SortAlgorithmState.RadixSort);

            Assert.IsTrue(metrics.SortSuccessful);
            Assert.AreEqual(MarkovChainAnalyzer.SortAlgorithmState.RadixSort, metrics.Algorithm);
            Assert.IsTrue(metrics.ElapsedMilliseconds >= 0);

            TestContext.WriteLine($"Radix Sort Metrics: {metrics}");
        }

        [TestMethod]
        public void TestSortWithMetricsTimSort()
        {
            int[] dataCopy = new int[_testData.Length];
            Array.Copy(_testData, dataCopy, _testData.Length);

            var metrics = _adaptiveService.SortWithMetrics(dataCopy, 
                MarkovChainAnalyzer.SortAlgorithmState.TimSort);

            Assert.IsTrue(metrics.SortSuccessful);
            Assert.AreEqual(MarkovChainAnalyzer.SortAlgorithmState.TimSort, metrics.Algorithm);
            Assert.IsTrue(IsSorted(dataCopy));

            TestContext.WriteLine($"Tim Sort Metrics: {metrics}");
        }

        [TestMethod]
        public void TestSortWithMetricsHeapSort()
        {
            int[] dataCopy = new int[_testData.Length];
            Array.Copy(_testData, dataCopy, _testData.Length);

            var metrics = _adaptiveService.SortWithMetrics(dataCopy, 
                MarkovChainAnalyzer.SortAlgorithmState.HeapSort);

            Assert.IsTrue(metrics.SortSuccessful);
            Assert.IsTrue(IsSorted(dataCopy));

            TestContext.WriteLine($"Heap Sort Metrics: {metrics}");
        }

        #endregion

        #region Performance Comparison Tests

        [TestMethod]
        public void TestCompareAlgorithmPerformanceSmallArray()
        {
            int[] smallData = new int[100];
            Random r = new Random(555);
            for (int i = 0; i < 100; i++)
                smallData[i] = r.Next();

            var results = _adaptiveService.CompareAlgorithmPerformance(smallData);

            Assert.IsTrue(results.Count > 0);
            Assert.IsTrue(results.All(x => x.Value.SortSuccessful));

            TestContext.WriteLine("Performance Comparison (100 elements):");
            foreach (var result in results.OrderBy(x => x.Value.ElapsedMilliseconds))
            {
                TestContext.WriteLine($"  {result.Value}");
            }
        }

        [TestMethod]
        public void TestCompareAlgorithmPerformanceLargeArray()
        {
            int[] largeData = new int[10000];
            Random r = new Random(666);
            for (int i = 0; i < 10000; i++)
                largeData[i] = r.Next();

            var results = _adaptiveService.CompareAlgorithmPerformance(largeData);

            Assert.IsTrue(results.Count > 0);
            Assert.IsTrue(results.All(x => x.Value.SortSuccessful));

            TestContext.WriteLine("Performance Comparison (10,000 elements):");
            foreach (var result in results.OrderBy(x => x.Value.ElapsedMilliseconds))
            {
                TestContext.WriteLine($"  {result.Value}");
            }
        }

        [TestMethod]
        public void TestCompareAlgorithmPerformanceSortedArray()
        {
            int[] sortedData = Enumerable.Range(1, 1000).ToArray();

            var results = _adaptiveService.CompareAlgorithmPerformance(sortedData);

            Assert.IsTrue(results.Count > 0);

            TestContext.WriteLine("Performance Comparison (1000 sorted elements):");
            foreach (var result in results.OrderBy(x => x.Value.ElapsedMilliseconds))
            {
                TestContext.WriteLine($"  {result.Value}");
            }

            // Tim Sort should be fastest on sorted data
            var timSortMetrics = results[MarkovChainAnalyzer.SortAlgorithmState.TimSort];
            var fastestMetrics = results.Values.OrderBy(x => x.ElapsedMilliseconds).First();

            TestContext.WriteLine($"Fastest: {fastestMetrics.Algorithm}");
            TestContext.WriteLine($"Tim Sort Time: {timSortMetrics.ElapsedMilliseconds}ms");
        }

        #endregion

        #region Markov Chain Learning Tests

        [TestMethod]
        public void TestMarkovChainRecordingSortSuccess()
        {
            var initialStats = _adaptiveService.GetMarkovChainStatistics();
            var initialTransitions = (int)initialStats["TotalTransitions"];

            // Perform a sort and record success
            int[] dataCopy = new int[100];
            Random r = new Random();
            for (int i = 0; i < 100; i++)
                dataCopy[i] = r.Next();

            _adaptiveService.AdaptiveSortByMarkovPrediction(dataCopy);

            var updatedStats = _adaptiveService.GetMarkovChainStatistics();
            var updatedTransitions = (int)updatedStats["TotalTransitions"];

            Assert.IsTrue(updatedTransitions >= initialTransitions);
            TestContext.WriteLine($"Transitions before: {initialTransitions}, after: {updatedTransitions}");
        }

        [TestMethod]
        public void TestMarkovChainStatistics()
        {
            // Perform multiple sorts to build up statistics
            for (int i = 0; i < 5; i++)
            {
                int[] dataCopy = new int[100];
                Random r = new Random(i * 10);
                for (int j = 0; j < 100; j++)
                    dataCopy[j] = r.Next();

                _adaptiveService.AdaptiveSortByMarkovPrediction(dataCopy);
            }

            var stats = _adaptiveService.GetMarkovChainStatistics();

            Assert.IsNotNull(stats);
            Assert.IsTrue(stats.ContainsKey("TotalTransitions"));
            Assert.IsTrue(stats.ContainsKey("UniqueTransitions"));
            Assert.IsTrue(stats.ContainsKey("StatesVisited"));
            Assert.IsTrue(stats.ContainsKey("TopPerformingStates"));
            Assert.IsTrue(stats.ContainsKey("MostCommonTransitions"));

            TestContext.WriteLine("Markov Chain Statistics:");
            TestContext.WriteLine($"  Total Transitions: {stats["TotalTransitions"]}");
            TestContext.WriteLine($"  Unique Transitions: {stats["UniqueTransitions"]}");
            TestContext.WriteLine($"  States Visited: {stats["StatesVisited"]}");
        }

        [TestMethod]
        public void TestTransitionProbabilities()
        {
            var analyzer = new MarkovChainAnalyzer();

            // Record some transitions
            analyzer.RecordSortSuccess(
                MarkovChainAnalyzer.SortAlgorithmState.QuickSort,
                MarkovChainAnalyzer.SortAlgorithmState.TimSort,
                0.95);

            analyzer.RecordSortSuccess(
                MarkovChainAnalyzer.SortAlgorithmState.QuickSort,
                MarkovChainAnalyzer.SortAlgorithmState.TimSort,
                0.92);

            analyzer.RecordSortSuccess(
                MarkovChainAnalyzer.SortAlgorithmState.QuickSort,
                MarkovChainAnalyzer.SortAlgorithmState.MergeSort,
                0.88);

            double timSortProb = analyzer.GetTransitionProbability(
                MarkovChainAnalyzer.SortAlgorithmState.QuickSort,
                MarkovChainAnalyzer.SortAlgorithmState.TimSort);

            double mergeSortProb = analyzer.GetTransitionProbability(
                MarkovChainAnalyzer.SortAlgorithmState.QuickSort,
                MarkovChainAnalyzer.SortAlgorithmState.MergeSort);

            Assert.IsTrue(timSortProb > mergeSortProb);
            TestContext.WriteLine($"QuickSort -> TimSort probability: {timSortProb:F3}");
            TestContext.WriteLine($"QuickSort -> MergeSort probability: {mergeSortProb:F3}");
        }

        #endregion

        #region Edge Cases

        [TestMethod]
        public void TestAdaptiveSortEmptyArray()
        {
            int[] emptyData = new int[0];
            _adaptiveService.AdaptiveSortByMarkovPrediction(emptyData);
            Assert.AreEqual(0, emptyData.Length);
        }

        [TestMethod]
        public void TestAdaptiveSortSingleElement()
        {
            int[] singleData = new int[1] { 42 };
            _adaptiveService.AdaptiveSortByMarkovPrediction(singleData);
            Assert.AreEqual(42, singleData[0]);
        }

        [TestMethod]
        public void TestAdaptiveSortDuplicates()
        {
            int[] duplicateData = new int[100];
            for (int i = 0; i < 100; i++)
                duplicateData[i] = 5;

            _adaptiveService.AdaptiveSortByMarkovPrediction(duplicateData);
            Assert.IsTrue(IsSorted(duplicateData));
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
