namespace TubieTools_Aspire.Tests.Algorithms
{
    [TestClass]
    public class UnitTestAdvancedSorting
    {
        private ISortingService _service = new SortingService();
        private readonly int[] _testData = new int[1000];
        private TestContext _testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides information about and functionality 
        /// for the current test run.
        /// </summary>
        public TestContext TestContext
        {
            get { return _testContextInstance; }
            set { _testContextInstance = value; }
        }

        [TestInitialize]
        public void Init()
        {
            Random r = new Random();
            IntArrayGenerate(_testData, r.Next(1, int.MaxValue));
        }

        private static void IntArrayGenerate(int[] data, int randomSeed)
        {
            Random r = new Random(randomSeed);
            for (int i = 0; i < data.Length; i++)
                data[i] = r.Next();
        }

        /// <summary>
        /// Verifies that an array is properly sorted in ascending order
        /// </summary>
        private bool IsArraySorted(int[] data)
        {
            for (int i = 0; i < data.Length - 1; i++)
            {
                if (data[i] > data[i + 1])
                    return false;
            }
            return true;
        }

        #region Radix Sort Tests

        [TestMethod]
        public void TestRadixSort()
        {
            _service.IntArrayRadixSort(_testData);
            Assert.IsTrue(IsArraySorted(_testData), "Array is not sorted after Radix Sort");
            Assert.IsNotNull(_testData);
            TestContext.WriteLine("Radix Sort: PASSED");
        }

        [TestMethod]
        public void TestRadixSortSmallArray()
        {
            int[] smallData = { 64, 34, 25, 12, 22, 11, 90 };
            _service.IntArrayRadixSort(smallData);
            Assert.IsTrue(IsArraySorted(smallData));
            TestContext.WriteLine("Radix Sort (Small Array): PASSED");
        }

        [TestMethod]
        public void TestRadixSortDuplicates()
        {
            int[] dataWithDuplicates = { 5, 2, 8, 2, 9, 1, 5, 5 };
            _service.IntArrayRadixSort(dataWithDuplicates);
            Assert.IsTrue(IsArraySorted(dataWithDuplicates));
            TestContext.WriteLine("Radix Sort (Duplicates): PASSED");
        }

        [TestMethod]
        public void TestRadixSortAlreadySorted()
        {
            int[] sortedData = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            _service.IntArrayRadixSort(sortedData);
            Assert.IsTrue(IsArraySorted(sortedData));
            TestContext.WriteLine("Radix Sort (Already Sorted): PASSED");
        }

        [TestMethod]
        public void TestRadixSortReverseSorted()
        {
            int[] reverseData = { 9, 8, 7, 6, 5, 4, 3, 2, 1 };
            _service.IntArrayRadixSort(reverseData);
            Assert.IsTrue(IsArraySorted(reverseData));
            TestContext.WriteLine("Radix Sort (Reverse Sorted): PASSED");
        }

        #endregion

        #region Tim Sort Tests

        [TestMethod]
        public void TestTimSort()
        {
            _service.IntArrayTimSort(_testData);
            Assert.IsTrue(IsArraySorted(_testData), "Array is not sorted after Tim Sort");
            Assert.IsNotNull(_testData);
            TestContext.WriteLine("Tim Sort: PASSED");
        }

        [TestMethod]
        public void TestTimSortSmallArray()
        {
            int[] smallData = { 64, 34, 25, 12, 22, 11, 90 };
            _service.IntArrayTimSort(smallData);
            Assert.IsTrue(IsArraySorted(smallData));
            TestContext.WriteLine("Tim Sort (Small Array): PASSED");
        }

        [TestMethod]
        public void TestTimSortSingleElement()
        {
            int[] singleElement = { 42 };
            _service.IntArrayTimSort(singleElement);
            Assert.AreEqual(42, singleElement[0]);
            TestContext.WriteLine("Tim Sort (Single Element): PASSED");
        }

        [TestMethod]
        public void TestTimSortDuplicates()
        {
            int[] dataWithDuplicates = { 5, 2, 8, 2, 9, 1, 5, 5 };
            _service.IntArrayTimSort(dataWithDuplicates);
            Assert.IsTrue(IsArraySorted(dataWithDuplicates));
            TestContext.WriteLine("Tim Sort (Duplicates): PASSED");
        }

        #endregion

        #region Counting Sort Tests

        [TestMethod]
        public void TestCountingSort()
        {
            // Create array with positive integers
            int[] countingData = new int[100];
            Random r = new Random(12345);
            for (int i = 0; i < countingData.Length; i++)
                countingData[i] = r.Next(0, 1000); // Positive range

            _service.IntArrayCountingSort(countingData);
            Assert.IsTrue(IsArraySorted(countingData), "Array is not sorted after Counting Sort");
            TestContext.WriteLine("Counting Sort: PASSED");
        }

        [TestMethod]
        public void TestCountingSortSmallArray()
        {
            int[] smallData = { 6, 3, 2, 5, 2, 4, 9 };
            _service.IntArrayCountingSort(smallData);
            Assert.IsTrue(IsArraySorted(smallData));
            TestContext.WriteLine("Counting Sort (Small Array): PASSED");
        }

        [TestMethod]
        public void TestCountingSortSingleElement()
        {
            int[] singleElement = { 42 };
            _service.IntArrayCountingSort(singleElement);
            Assert.AreEqual(42, singleElement[0]);
            TestContext.WriteLine("Counting Sort (Single Element): PASSED");
        }

        #endregion

        #region Intro Sort Tests

        [TestMethod]
        public void TestIntroSort()
        {
            _service.IntArrayIntroSort(_testData);
            Assert.IsTrue(IsArraySorted(_testData), "Array is not sorted after Intro Sort");
            Assert.IsNotNull(_testData);
            TestContext.WriteLine("Intro Sort: PASSED");
        }

        [TestMethod]
        public void TestIntroSortSmallArray()
        {
            int[] smallData = { 64, 34, 25, 12, 22, 11, 90 };
            _service.IntArrayIntroSort(smallData);
            Assert.IsTrue(IsArraySorted(smallData));
            TestContext.WriteLine("Intro Sort (Small Array): PASSED");
        }

        [TestMethod]
        public void TestIntroSortLargeArray()
        {
            int[] largeData = new int[5000];
            Random r = new Random(54321);
            for (int i = 0; i < largeData.Length; i++)
                largeData[i] = r.Next();

            _service.IntArrayIntroSort(largeData);
            Assert.IsTrue(IsArraySorted(largeData));
            TestContext.WriteLine("Intro Sort (Large Array): PASSED");
        }

        #endregion

        #region Heap Sort Tests

        [TestMethod]
        public void TestHeapSort()
        {
            _service.IntArrayHeapSort(_testData);
            Assert.IsTrue(IsArraySorted(_testData), "Array is not sorted after Heap Sort");
            Assert.IsNotNull(_testData);
            TestContext.WriteLine("Heap Sort: PASSED");
        }

        [TestMethod]
        public void TestHeapSortSmallArray()
        {
            int[] smallData = { 64, 34, 25, 12, 22, 11, 90 };
            _service.IntArrayHeapSort(smallData);
            Assert.IsTrue(IsArraySorted(smallData));
            TestContext.WriteLine("Heap Sort (Small Array): PASSED");
        }

        [TestMethod]
        public void TestHeapSortDuplicates()
        {
            int[] dataWithDuplicates = { 5, 2, 8, 2, 9, 1, 5, 5 };
            _service.IntArrayHeapSort(dataWithDuplicates);
            Assert.IsTrue(IsArraySorted(dataWithDuplicates));
            TestContext.WriteLine("Heap Sort (Duplicates): PASSED");
        }

        #endregion

        #region Merge Sort Tests

        [TestMethod]
        public void TestMergeSort()
        {
            _service.IntArrayMergeSort(_testData);
            Assert.IsTrue(IsArraySorted(_testData), "Array is not sorted after Merge Sort");
            Assert.IsNotNull(_testData);
            TestContext.WriteLine("Merge Sort: PASSED");
        }

        [TestMethod]
        public void TestMergeSortSmallArray()
        {
            int[] smallData = { 64, 34, 25, 12, 22, 11, 90 };
            _service.IntArrayMergeSort(smallData);
            Assert.IsTrue(IsArraySorted(smallData));
            TestContext.WriteLine("Merge Sort (Small Array): PASSED");
        }

        [TestMethod]
        public void TestMergeSortEmpty()
        {
            int[] emptyData = { };
            _service.IntArrayMergeSort(emptyData);
            Assert.AreEqual(0, emptyData.Length);
            TestContext.WriteLine("Merge Sort (Empty): PASSED");
        }

        #endregion

        #region Comb Sort Tests

        [TestMethod]
        public void TestCombSort()
        {
            _service.IntArrayCombSort(_testData);
            Assert.IsTrue(IsArraySorted(_testData), "Array is not sorted after Comb Sort");
            Assert.IsNotNull(_testData);
            TestContext.WriteLine("Comb Sort: PASSED");
        }

        [TestMethod]
        public void TestCombSortSmallArray()
        {
            int[] smallData = { 64, 34, 25, 12, 22, 11, 90 };
            _service.IntArrayCombSort(smallData);
            Assert.IsTrue(IsArraySorted(smallData));
            TestContext.WriteLine("Comb Sort (Small Array): PASSED");
        }

        #endregion

        #region Gnome Sort Tests

        [TestMethod]
        public void TestGnomeSort()
        {
            // Use smaller array for Gnome Sort as it's slower
            int[] smallerData = new int[100];
            Random r = new Random();
            for (int i = 0; i < smallerData.Length; i++)
                smallerData[i] = r.Next();

            _service.IntArrayGnomeSort(smallerData);
            Assert.IsTrue(IsArraySorted(smallerData), "Array is not sorted after Gnome Sort");
            TestContext.WriteLine("Gnome Sort: PASSED");
        }

        [TestMethod]
        public void TestGnomeSortSmallArray()
        {
            int[] smallData = { 64, 34, 25, 12, 22, 11, 90 };
            _service.IntArrayGnomeSort(smallData);
            Assert.IsTrue(IsArraySorted(smallData));
            TestContext.WriteLine("Gnome Sort (Small Array): PASSED");
        }

        #endregion

        #region Odd-Even Sort Tests

        [TestMethod]
        public void TestOddEvenSort()
        {
            // Use smaller array for Odd-Even Sort as it's slower
            int[] smallerData = new int[100];
            Random r = new Random();
            for (int i = 0; i < smallerData.Length; i++)
                smallerData[i] = r.Next();

            _service.IntArrayOddEvenSort(smallerData);
            Assert.IsTrue(IsArraySorted(smallerData), "Array is not sorted after Odd-Even Sort");
            TestContext.WriteLine("Odd-Even Sort: PASSED");
        }

        [TestMethod]
        public void TestOddEvenSortSmallArray()
        {
            int[] smallData = { 64, 34, 25, 12, 22, 11, 90 };
            _service.IntArrayOddEvenSort(smallData);
            Assert.IsTrue(IsArraySorted(smallData));
            TestContext.WriteLine("Odd-Even Sort (Small Array): PASSED");
        }

        #endregion

        #region Cycle Sort Tests

        [TestMethod]
        public void TestCycleSort()
        {
            // Use smaller array for Cycle Sort
            int[] smallerData = new int[100];
            Random r = new Random();
            for (int i = 0; i < smallerData.Length; i++)
                smallerData[i] = r.Next();

            _service.IntArrayCycleSort(smallerData);
            Assert.IsTrue(IsArraySorted(smallerData), "Array is not sorted after Cycle Sort");
            TestContext.WriteLine("Cycle Sort: PASSED");
        }

        [TestMethod]
        public void TestCycleSortSmallArray()
        {
            int[] smallData = { 64, 34, 25, 12, 22, 11, 90 };
            _service.IntArrayCycleSort(smallData);
            Assert.IsTrue(IsArraySorted(smallData));
            TestContext.WriteLine("Cycle Sort (Small Array): PASSED");
        }

        #endregion

        #region Comparison Tests

        [TestMethod]
        public void CompareAllSortingAlgorithms()
        {
            // Create test data copies for each algorithm
            int[] testDataCopy1 = new int[_testData.Length];
            int[] testDataCopy2 = new int[_testData.Length];
            int[] testDataCopy3 = new int[_testData.Length];
            int[] testDataCopy4 = new int[_testData.Length];
            int[] testDataCopy5 = new int[_testData.Length];
            int[] testDataCopy6 = new int[_testData.Length];
            int[] testDataCopy7 = new int[_testData.Length];

            Array.Copy(_testData, testDataCopy1, _testData.Length);
            Array.Copy(_testData, testDataCopy2, _testData.Length);
            Array.Copy(_testData, testDataCopy3, _testData.Length);
            Array.Copy(_testData, testDataCopy4, _testData.Length);
            Array.Copy(_testData, testDataCopy5, _testData.Length);
            Array.Copy(_testData, testDataCopy6, _testData.Length);
            Array.Copy(_testData, testDataCopy7, _testData.Length);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _service.IntArrayRadixSort(testDataCopy1);
            stopwatch.Stop();
            TestContext.WriteLine($"Radix Sort: {stopwatch.ElapsedMilliseconds}ms");

            stopwatch.Restart();
            _service.IntArrayTimSort(testDataCopy2);
            stopwatch.Stop();
            TestContext.WriteLine($"Tim Sort: {stopwatch.ElapsedMilliseconds}ms");

            stopwatch.Restart();
            _service.IntArrayIntroSort(testDataCopy3);
            stopwatch.Stop();
            TestContext.WriteLine($"Intro Sort: {stopwatch.ElapsedMilliseconds}ms");

            stopwatch.Restart();
            _service.IntArrayHeapSort(testDataCopy4);
            stopwatch.Stop();
            TestContext.WriteLine($"Heap Sort: {stopwatch.ElapsedMilliseconds}ms");

            stopwatch.Restart();
            _service.IntArrayMergeSort(testDataCopy5);
            stopwatch.Stop();
            TestContext.WriteLine($"Merge Sort: {stopwatch.ElapsedMilliseconds}ms");

            stopwatch.Restart();
            _service.IntArrayCombSort(testDataCopy6);
            stopwatch.Stop();
            TestContext.WriteLine($"Comb Sort: {stopwatch.ElapsedMilliseconds}ms");

            stopwatch.Restart();
            _service.IntArrayQuickSort(testDataCopy7);
            stopwatch.Stop();
            TestContext.WriteLine($"Quick Sort: {stopwatch.ElapsedMilliseconds}ms");

            // Verify all are sorted
            Assert.IsTrue(IsArraySorted(testDataCopy1) && IsArraySorted(testDataCopy2) &&
                         IsArraySorted(testDataCopy3) && IsArraySorted(testDataCopy4) &&
                         IsArraySorted(testDataCopy5) && IsArraySorted(testDataCopy6) &&
                         IsArraySorted(testDataCopy7), "Not all sorting algorithms produced sorted arrays");
        }

        #endregion
    }
}
