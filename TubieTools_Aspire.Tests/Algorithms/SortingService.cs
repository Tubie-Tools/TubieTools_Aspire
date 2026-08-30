using System;
using System.Collections.Generic;
using System.Text;

namespace TubieTools_Aspire.Tests.Algorithms
{
    public interface ISortingService
    {
        void IntArrayInsertionSort(int[] data);
        void IntArrayBubbleSort(int[] data);
        void ModifiedBubbleSort(int[] data);
        void IntArrayQuickSort(int[] data);
        void IntArraySelectionSort(int[] data);
        //void IntArrayShellSort(int[] data, int[] intervals);
        void IntArrayShellSortBetter(int[] data);
        void IntArrayShellSortNaive(int[] data);
        void ReverseArray(int[] data);

        // Advanced sorting algorithms
        void IntArrayRadixSort(int[] data);
        void IntArrayTimSort(int[] data);
        void IntArrayCountingSort(int[] data);
        void IntArrayIntroSort(int[] data);
        void IntArrayHeapSort(int[] data);
        void IntArrayMergeSort(int[] data);
        void IntArrayCombSort(int[] data);
        void IntArrayGnomeSort(int[] data);
        void IntArrayOddEvenSort(int[] data);
        void IntArrayCycleSort(int[] data);
    }

    public class SortingService : ISortingService
    {
        private static void Swap(int[] data, int m, int n)
        {
            int temporary = data[m];
            data[m] = data[n];
            data[n] = temporary;
        }

        public void IntArrayInsertionSort(int[] data)
        {
            int i, j;
            int N = data.Length;

            for (j = 1; j < N; j++)
            {
                for (i = j; i > 0 && data[i] < data[i - 1]; i--)
                {
                    Swap(data, i, i - 1);
                }
            }
        }

        private void IntArrayQuickSort(int[] data, int l, int r)
        {
            int i, j;
            int x;

            i = l;
            j = r;

            x = data[(l + r) / 2]; /* find pivot item */
            while (true)
            {
                while (data[i] < x)
                    i++;
                while (x < data[j])
                    j--;
                if (i <= j)
                {
                    Swap(data, i, j);
                    i++;
                    j--;
                }
                if (i > j)
                    break;
            }
            if (l < j)
                IntArrayQuickSort(data, l, j);
            if (i < r)
                IntArrayQuickSort(data, i, r);
        }

        public void IntArrayQuickSort(int[] data)
        {
            IntArrayQuickSort(data, 0, data.Length - 1);
        }

        static int[] GenerateIntervals(int n)
        {
            if (n < 2)
            {  // no sorting will be needed
                return new int[0];
            }
            int t = Math.Max(1, (int)Math.Log(n, 3) - 1);
            int[] intervals = new int[t];
            intervals[0] = 1;
            for (int i = 1; i < t; i++)
                intervals[i] = 3 * intervals[i - 1] + 1;
            return intervals;
        }

        public void IntArrayShellSortBetter(int[] data)
        {
            int[] intervals = GenerateIntervals(data.Length);
            IntArrayShellSort(data, intervals);
        }

        public void IntArrayShellSortNaive(int[] data)
        {
            int[] intervals = { 1, 2, 4, 8 };
            IntArrayShellSort(data, intervals);
        }

        public void IntArrayBubbleSort(int[] data)
        {
            int i, j;
            int N = data.Length;

            for (j = N - 1; j > 0; j--)
            {
                for (i = 0; i < j; i++)
                {
                    if (data[i] > data[i + 1])
                        Swap(data, i, i + 1);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void ModifiedBubbleSort(int[] data)
        {
            int numberOfSwaps = 0;
            int n = data.Length;

            for (int i = 0; i < n; i++)
            {
                // Track number of elements swapped during a single array traversal


                for (int j = 0; j < n - 1; j++)
                {
                    // Swap adjacent elements if they are in decreasing order
                    if (data[j] > data[j + 1])
                    {
                        int num1 = data[j];
                        int num2 = data[j + 1];
                        data[j] = num2;
                        data[j + 1] = num1;
                        numberOfSwaps++;
                    }
                }

                // If no elements were swapped during a traversal, array is sorted
                if (numberOfSwaps == 0)
                {
                    break;
                }
            }
        }

        public int IntArrayMin(int[] data, int start)
        {
            int minPos = start;
            for (int pos = start + 1; pos < data.Length; pos++)
                if (data[pos] < data[minPos])
                    minPos = pos;
            return minPos;
        }

        public void IntArraySelectionSort(int[] data)
        {
            int i;
            int N = data.Length;

            for (i = 0; i < N - 1; i++)
            {
                int k = IntArrayMin(data, i);
                if (i != k)
                    Swap(data, i, k);
            }
        }

        private void IntArrayShellSort(int[] data, int[] intervals)
        {
            int i, j, k, m;
            int N = data.Length;

            // The intervals for the shell sort must be sorted, ascending

            for (k = intervals.Length - 1; k >= 0; k--)
            {
                int interval = intervals[k];
                for (m = 0; m < interval; m++)
                {
                    for (j = m + interval; j < N; j += interval)
                    {
                        for (i = j; i >= interval && data[i] < data[i - interval]; i -= interval)
                        {
                            Swap(data, i, i - interval);
                        }
                    }
                }
            }
        }

        public void ReverseArray(int[] data)
        {
            int i = 0;
            int j = data.Length - 1;
            while (i < j)
            {
                var temp = data[i];
                data[i] = data[j];
                data[j] = temp;
                i++;
                j--;
            }
        }

        /// <summary>
        /// Radix Sort - Non-comparative sorting algorithm that sorts numbers by processing digits
        /// Time Complexity: O(nk) where n is number of elements and k is number of digits
        /// Space Complexity: O(n + k)
        /// Stable: Yes
        /// </summary>
        public void IntArrayRadixSort(int[] data)
        {
            if (data.Length == 0)
                return;

            // Find maximum number to know number of digits
            int max = data[0];
            for (int i = 1; i < data.Length; i++)
            {
                if (data[i] > max)
                    max = data[i];
            }

            // Do counting sort for each digit
            for (int exp = 1; max / exp > 0; exp *= 10)
            {
                CountingSortByDigit(data, exp);
            }
        }

        private void CountingSortByDigit(int[] data, int exp)
        {
            int[] output = new int[data.Length];
            int[] count = new int[10];

            // Store count of occurrences
            for (int i = 0; i < data.Length; i++)
            {
                count[(data[i] / exp) % 10]++;
            }

            // Change count[i] so that count[i] contains actual position
            for (int i = 1; i < 10; i++)
            {
                count[i] += count[i - 1];
            }

            // Build the output array
            for (int i = data.Length - 1; i >= 0; i--)
            {
                output[count[(data[i] / exp) % 10] - 1] = data[i];
                count[(data[i] / exp) % 10]--;
            }

            // Copy the output array to data
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = output[i];
            }
        }

        /// <summary>
        /// Tim Sort - Hybrid sorting algorithm combining merge sort and insertion sort
        /// Time Complexity: O(n log n) average and worst case, O(n) best case
        /// Space Complexity: O(n)
        /// Stable: Yes
        /// </summary>
        public void IntArrayTimSort(int[] data)
        {
            int minRun = CalculateMinRun(data.Length);

            // Sort individual runs using insertion sort
            for (int start = 0; start < data.Length; start += minRun)
            {
                int end = Math.Min(start + minRun, data.Length);
                InsertionSortRange(data, start, end);
            }

            // Merge sorted runs
            for (int size = minRun; size < data.Length; size *= 2)
            {
                for (int start = 0; start < data.Length; start += size * 2)
                {
                    int mid = start + size;
                    int end = Math.Min(start + size * 2, data.Length);

                    if (mid < end)
                    {
                        Merge(data, start, mid, end);
                    }
                }
            }
        }

        private int CalculateMinRun(int n)
        {
            int r = 0;
            while (n >= 64)
            {
                r |= n & 1;
                n >>= 1;
            }
            return n + r;
        }

        private void InsertionSortRange(int[] data, int left, int right)
        {
            for (int i = left + 1; i < right; i++)
            {
                int key = data[i];
                int j = i - 1;
                while (j >= left && data[j] > key)
                {
                    data[j + 1] = data[j];
                    j--;
                }
                data[j + 1] = key;
            }
        }

        private void Merge(int[] data, int left, int mid, int right)
        {
            int[] leftArr = new int[mid - left];
            int[] rightArr = new int[right - mid];

            Array.Copy(data, left, leftArr, 0, mid - left);
            Array.Copy(data, mid, rightArr, 0, right - mid);

            int i = 0, j = 0, k = left;
            while (i < leftArr.Length && j < rightArr.Length)
            {
                if (leftArr[i] <= rightArr[j])
                {
                    data[k++] = leftArr[i++];
                }
                else
                {
                    data[k++] = rightArr[j++];
                }
            }

            while (i < leftArr.Length)
            {
                data[k++] = leftArr[i++];
            }

            while (j < rightArr.Length)
            {
                data[k++] = rightArr[j++];
            }
        }

        /// <summary>
        /// Counting Sort - Non-comparative sorting algorithm for integers in a specific range
        /// Time Complexity: O(n + k) where k is the range of input
        /// Space Complexity: O(n + k)
        /// Stable: Yes
        /// </summary>
        public void IntArrayCountingSort(int[] data)
        {
            if (data.Length == 0)
                return;

            int min = data[0];
            int max = data[0];

            // Find min and max
            for (int i = 1; i < data.Length; i++)
            {
                if (data[i] < min)
                    min = data[i];
                if (data[i] > max)
                    max = data[i];
            }

            int range = max - min + 1;
            int[] count = new int[range];
            int[] output = new int[data.Length];

            // Count occurrences
            for (int i = 0; i < data.Length; i++)
            {
                count[data[i] - min]++;
            }

            // Modify count to contain actual positions
            for (int i = 1; i < count.Length; i++)
            {
                count[i] += count[i - 1];
            }

            // Build output array
            for (int i = data.Length - 1; i >= 0; i--)
            {
                output[count[data[i] - min] - 1] = data[i];
                count[data[i] - min]--;
            }

            // Copy back to original array
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = output[i];
            }
        }

        /// <summary>
        /// Intro Sort (Introspective Sort) - Hybrid sorting algorithm combining quicksort, heapsort, and insertion sort
        /// Time Complexity: O(n log n) guaranteed
        /// Space Complexity: O(log n)
        /// Stable: No
        /// </summary>
        public void IntArrayIntroSort(int[] data)
        {
            int depthLimit = (int)(2 * Math.Log(data.Length, 2));
            IntroSortHelper(data, 0, data.Length - 1, depthLimit);
        }

        private void IntroSortHelper(int[] data, int left, int right, int depthLimit)
        {
            while (right > left)
            {
                if (depthLimit == 0)
                {
                    // Switch to heapsort if recursion depth exceeds limit
                    HeapSortRange(data, left, right);
                    return;
                }

                if (right - left < 16)
                {
                    // Switch to insertion sort for small arrays
                    InsertionSortRange(data, left, right + 1);
                    return;
                }

                // Quicksort partition
                int pi = Partition(data, left, right);

                // Recursively sort the smaller partition
                if (pi - left < right - pi)
                {
                    IntroSortHelper(data, left, pi - 1, depthLimit - 1);
                    left = pi + 1;
                }
                else
                {
                    IntroSortHelper(data, pi + 1, right, depthLimit - 1);
                    right = pi - 1;
                }
            }
        }

        private int Partition(int[] data, int left, int right)
        {
            int pivot = data[right];
            int i = left - 1;

            for (int j = left; j < right; j++)
            {
                if (data[j] < pivot)
                {
                    i++;
                    Swap(data, i, j);
                }
            }

            Swap(data, i + 1, right);
            return i + 1;
        }

        private void HeapSortRange(int[] data, int left, int right)
        {
            int n = right - left + 1;

            // Build heap
            for (int i = n / 2 - 1; i >= 0; i--)
            {
                Heapify(data, left, n, i);
            }

            // Extract elements from heap
            for (int i = n - 1; i > 0; i--)
            {
                Swap(data, left, left + i);
                Heapify(data, left, i, 0);
            }
        }

        private void Heapify(int[] data, int start, int n, int i)
        {
            int largest = i;
            int left = 2 * i + 1;
            int right = 2 * i + 2;

            if (left < n && data[start + left] > data[start + largest])
                largest = left;

            if (right < n && data[start + right] > data[start + largest])
                largest = right;

            if (largest != i)
            {
                Swap(data, start + i, start + largest);
                Heapify(data, start, n, largest);
            }
        }

        /// <summary>
        /// Heap Sort - Comparison-based sorting using heap data structure
        /// Time Complexity: O(n log n)
        /// Space Complexity: O(1)
        /// Stable: No
        /// </summary>
        public void IntArrayHeapSort(int[] data)
        {
            int n = data.Length;

            // Build max heap
            for (int i = n / 2 - 1; i >= 0; i--)
            {
                HeapifyDown(data, n, i);
            }

            // Extract elements from heap one by one
            for (int i = n - 1; i > 0; i--)
            {
                Swap(data, 0, i);
                HeapifyDown(data, i, 0);
            }
        }

        private void HeapifyDown(int[] data, int n, int i)
        {
            int largest = i;
            int left = 2 * i + 1;
            int right = 2 * i + 2;

            if (left < n && data[left] > data[largest])
                largest = left;

            if (right < n && data[right] > data[largest])
                largest = right;

            if (largest != i)
            {
                Swap(data, i, largest);
                HeapifyDown(data, n, largest);
            }
        }

        /// <summary>
        /// Merge Sort - Divide and conquer sorting algorithm
        /// Time Complexity: O(n log n)
        /// Space Complexity: O(n)
        /// Stable: Yes
        /// </summary>
        public void IntArrayMergeSort(int[] data)
        {
            if (data.Length <= 1)
                return;

            MergeSortHelper(data, 0, data.Length - 1);
        }

        private void MergeSortHelper(int[] data, int left, int right)
        {
            if (left < right)
            {
                int mid = left + (right - left) / 2;

                MergeSortHelper(data, left, mid);
                MergeSortHelper(data, mid + 1, right);

                MergeArrays(data, left, mid, right);
            }
        }

        private void MergeArrays(int[] data, int left, int mid, int right)
        {
            int[] leftArr = new int[mid - left + 1];
            int[] rightArr = new int[right - mid];

            Array.Copy(data, left, leftArr, 0, mid - left + 1);
            Array.Copy(data, mid + 1, rightArr, 0, right - mid);

            int i = 0, j = 0, k = left;

            while (i < leftArr.Length && j < rightArr.Length)
            {
                if (leftArr[i] <= rightArr[j])
                {
                    data[k++] = leftArr[i++];
                }
                else
                {
                    data[k++] = rightArr[j++];
                }
            }

            while (i < leftArr.Length)
            {
                data[k++] = leftArr[i++];
            }

            while (j < rightArr.Length)
            {
                data[k++] = rightArr[j++];
            }
        }

        /// <summary>
        /// Comb Sort - Sorting algorithm that improves bubble sort
        /// Time Complexity: O(n log n) average, O(n^2) worst case
        /// Space Complexity: O(1)
        /// Stable: No
        /// </summary>
        public void IntArrayCombSort(int[] data)
        {
            int n = data.Length;
            int gap = n;
            bool swapped = true;

            while (gap > 1 || swapped)
            {
                gap = GetNextGap(gap);
                swapped = false;

                for (int i = 0; i < n - gap; i++)
                {
                    if (data[i] > data[i + gap])
                    {
                        Swap(data, i, i + gap);
                        swapped = true;
                    }
                }
            }
        }

        private int GetNextGap(int gap)
        {
            gap = (gap * 10) / 13;
            return gap < 1 ? 1 : gap;
        }

        /// <summary>
        /// Gnome Sort - Simple sorting algorithm similar to insertion sort
        /// Time Complexity: O(n^2)
        /// Space Complexity: O(1)
        /// Stable: Yes
        /// </summary>
        public void IntArrayGnomeSort(int[] data)
        {
            int n = data.Length;
            int pos = 0;

            while (pos < n)
            {
                if (pos == 0 || data[pos] >= data[pos - 1])
                {
                    pos++;
                }
                else
                {
                    Swap(data, pos, pos - 1);
                    pos--;
                }
            }
        }

        /// <summary>
        /// Odd-Even Sort (Brick Sort) - Variation of bubble sort
        /// Time Complexity: O(n^2)
        /// Space Complexity: O(1)
        /// Stable: Yes
        /// </summary>
        public void IntArrayOddEvenSort(int[] data)
        {
            int n = data.Length;
            bool sorted = false;

            while (!sorted)
            {
                sorted = true;

                // Odd phase
                for (int i = 1; i < n - 1; i += 2)
                {
                    if (data[i] > data[i + 1])
                    {
                        Swap(data, i, i + 1);
                        sorted = false;
                    }
                }

                // Even phase
                for (int i = 0; i < n - 1; i += 2)
                {
                    if (data[i] > data[i + 1])
                    {
                        Swap(data, i, i + 1);
                        sorted = false;
                    }
                }
            }
        }

        /// <summary>
        /// Cycle Sort - In-place sorting algorithm that minimizes the number of writes
        /// Time Complexity: O(n^2)
        /// Space Complexity: O(1)
        /// Stable: No
        /// </summary>
        public void IntArrayCycleSort(int[] data)
        {
            int n = data.Length;

            for (int cycleStart = 0; cycleStart < n - 1; cycleStart++)
            {
                int item = data[cycleStart];
                int pos = cycleStart;

                // Find where to put item
                for (int i = cycleStart + 1; i < n; i++)
                {
                    if (data[i] < item)
                        pos++;
                }

                if (pos == cycleStart)
                    continue;

                // Skip duplicates
                while (item == data[pos])
                    pos++;

                // Put item to its correct position
                int temp = data[pos];
                data[pos] = item;
                item = temp;

                // Rotate rest of the cycle
                while (pos != cycleStart)
                {
                    pos = cycleStart;

                    for (int i = cycleStart + 1; i < n; i++)
                    {
                        if (data[i] < item)
                            pos++;
                    }

                    while (item == data[pos])
                        pos++;

                    temp = data[pos];
                    data[pos] = item;
                    item = temp;
                }
            }
        }
    }
}
