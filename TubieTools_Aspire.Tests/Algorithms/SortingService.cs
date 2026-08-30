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
    }
}
