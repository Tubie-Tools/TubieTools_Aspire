# Comprehensive Sorting Algorithms Guide

This document describes all sorting algorithms implemented in the `SortingService` class within `TubieTools_Aspire.Tests.Algorithms`.

## Table of Contents
1. [Basic Sorting Algorithms](#basic-sorting-algorithms)
2. [Advanced Sorting Algorithms](#advanced-sorting-algorithms)
3. [Comparison Table](#comparison-table)
4. [When to Use Each Algorithm](#when-to-use-each-algorithm)

---

## Basic Sorting Algorithms

### 1. Bubble Sort
**Method:** `IntArrayBubbleSort(int[] data)`

**Description:** Repeatedly steps through the array, compares adjacent elements, and swaps them if they're in the wrong order.

- **Time Complexity:** O(n²) average and worst case, O(n) best case
- **Space Complexity:** O(1)
- **Stable:** Yes
- **In-place:** Yes
- **Use Case:** Educational purposes, small datasets

**How It Works:**
```
Compare and swap adjacent pairs until no more swaps are needed.
Pass 1: [5, 2, 8, 1, 9] → [2, 5, 1, 8, 9]
Pass 2: [2, 5, 1, 8, 9] → [2, 1, 5, 8, 9]
...continues until sorted
```

### 2. Modified Bubble Sort
**Method:** `ModifiedBubbleSort(int[] data)`

**Description:** Enhanced bubble sort that tracks if any swaps occurred during a pass. Stops early if the array is already sorted.

- **Time Complexity:** O(n²) average and worst case, O(n) best case
- **Space Complexity:** O(1)
- **Stable:** Yes
- **In-place:** Yes
- **Improvement:** Better performance on nearly sorted data

### 3. Insertion Sort
**Method:** `IntArrayInsertionSort(int[] data)`

**Description:** Builds the sorted array one item at a time by inserting elements into their correct position.

- **Time Complexity:** O(n²) average and worst case, O(n) best case
- **Space Complexity:** O(1)
- **Stable:** Yes
- **In-place:** Yes
- **Use Case:** Good for small arrays, partially sorted data

**How It Works:**
```
For each element, find its correct position in the sorted portion
and insert it there.
```

### 4. Selection Sort
**Method:** `IntArraySelectionSort(int[] data)`

**Description:** Repeatedly finds the minimum element from unsorted portion and places it at the beginning.

- **Time Complexity:** O(n²)
- **Space Complexity:** O(1)
- **Stable:** No
- **In-place:** Yes
- **Use Case:** When memory writes are expensive

### 5. Shell Sort
**Methods:** 
- `IntArrayShellSortBetter(int[] data)` - Uses optimal gap sequence
- `IntArrayShellSortNaive(int[] data)` - Uses simple gap sequence [1, 2, 4, 8]

**Description:** Generalization of insertion sort that allows comparing and swapping elements that are far apart. The gap decreases until it becomes 1.

- **Time Complexity:** O(n log n) to O(n^(3/2)) depending on gap sequence
- **Space Complexity:** O(1)
- **Stable:** No
- **In-place:** Yes
- **Use Case:** Medium-sized arrays with reasonable speed

### 6. Quick Sort
**Method:** `IntArrayQuickSort(int[] data)`

**Description:** Divide-and-conquer algorithm that partitions array around a pivot and recursively sorts partitions.

- **Time Complexity:** O(n log n) average, O(n²) worst case
- **Space Complexity:** O(log n) due to recursion
- **Stable:** No
- **In-place:** Yes
- **Use Case:** General-purpose sorting, fast average performance

---

## Advanced Sorting Algorithms

### 7. Radix Sort
**Method:** `IntArrayRadixSort(int[] data)`

**Description:** Non-comparative algorithm that sorts numbers by processing individual digits. Works from least significant digit to most significant.

- **Time Complexity:** O(nk) where k = number of digits
- **Space Complexity:** O(n + k)
- **Stable:** Yes
- **In-place:** No
- **Use Case:** Large datasets of integers with fixed digit count

**How It Works:**
```
1. Find max number to determine number of digits
2. For each digit position:
   - Perform counting sort on that digit
3. Result: sorted array
Example: [170, 45, 75] → [045, 170, 075] → [045, 075, 170]
```

**Advantages:**
- Faster than comparison-based sorts for large datasets
- Linear time complexity
- Deterministic performance

### 8. Tim Sort
**Method:** `IntArrayTimSort(int[] data)`

**Description:** Hybrid algorithm combining merge sort and insertion sort. Divides array into small runs, sorts with insertion sort, then merges.

- **Time Complexity:** O(n log n) average and worst case, O(n) best case
- **Space Complexity:** O(n)
- **Stable:** Yes
- **In-place:** No
- **Use Case:** Python's default sort, general-purpose sorting, especially good for partially sorted data

**How It Works:**
```
1. Calculate minimum run length
2. Sort chunks using insertion sort
3. Merge sorted chunks using merge sort
4. Adaptive: exploits existing order in data
```

**Advantages:**
- Excellent on real-world data
- Good on partially sorted arrays
- Guaranteed O(n log n)

### 9. Counting Sort
**Method:** `IntArrayCountingSort(int[] data)`

**Description:** Non-comparative algorithm that counts occurrences of each value, then reconstructs the sorted array.

- **Time Complexity:** O(n + k) where k = range of input
- **Space Complexity:** O(n + k)
- **Stable:** Yes
- **In-place:** No
- **Use Case:** Integers within a specific range, when range is not too large

**How It Works:**
```
1. Find min and max values
2. Create count array of size (max - min + 1)
3. Count occurrences of each element
4. Reconstruct sorted array from counts
Example: [3, 1, 4, 1, 5] → counts → [1, 1, 3, 4, 5]
```

**Best For:**
- Small range integers
- Stable sort requirement
- When range is similar to number of elements

### 10. Intro Sort (Introspective Sort)
**Method:** `IntArrayIntroSort(int[] data)`

**Description:** Hybrid algorithm starting with quicksort, switching to heapsort if recursion depth exceeds limit, and using insertion sort for small arrays.

- **Time Complexity:** O(n log n) guaranteed
- **Space Complexity:** O(log n)
- **Stable:** No
- **In-place:** Yes (mostly)
- **Use Case:** C++ standard library sort, guaranteed performance

**How It Works:**
```
1. Start with quicksort
2. If recursion too deep → switch to heapsort
3. If array < 16 elements → switch to insertion sort
4. Combines best of all three algorithms
```

**Advantages:**
- Guaranteed O(n log n) worst case
- Efficient in-place sorting
- Good cache locality

### 11. Heap Sort
**Method:** `IntArrayHeapSort(int[] data)`

**Description:** Comparison-based algorithm using heap data structure. Builds max heap then repeatedly extracts maximum element.

- **Time Complexity:** O(n log n)
- **Space Complexity:** O(1)
- **Stable:** No
- **In-place:** Yes
- **Use Case:** When worst-case guarantee is important, limited memory

**How It Works:**
```
1. Build max heap from input array
2. Repeatedly:
   - Swap root (max) with last element
   - Reduce heap size by 1
   - Restore heap property
3. Result: sorted array
```

**Properties:**
- Predictable performance
- Never degrades to O(n²)
- Works well in memory-constrained environments

### 12. Merge Sort
**Method:** `IntArrayMergeSort(int[] data)`

**Description:** Divide-and-conquer algorithm that divides array in half, recursively sorts both halves, then merges them.

- **Time Complexity:** O(n log n) guaranteed
- **Space Complexity:** O(n)
- **Stable:** Yes
- **In-place:** No
- **Use Case:** Linked lists, guaranteed performance needed, stability required

**How It Works:**
```
1. Divide: Split array into two halves
2. Conquer: Recursively sort both halves
3. Combine: Merge the sorted halves
Example: [38, 27, 43, 3]
		 → [38, 27] and [43, 3]
		 → [27, 38] and [3, 43]
		 → [3, 27, 38, 43]
```

**Advantages:**
- Guaranteed O(n log n)
- Stable sorting
- Good cache performance in practice

### 13. Comb Sort
**Method:** `IntArrayCombSort(int[] data)`

**Description:** Improved bubble sort that eliminates "turtles" (small elements near end) by using a gap that shrinks over iterations.

- **Time Complexity:** O(n²) worst case, O(n log n) average
- **Space Complexity:** O(1)
- **Stable:** No
- **In-place:** Yes
- **Use Case:** When simplicity is desired with better performance than bubble sort

**How It Works:**
```
1. Start with large gap (gap = n)
2. Compare and swap elements gap positions apart
3. Shrink gap further: gap = gap * 10 / 13
4. When gap becomes 1, it's essentially bubble sort
5. Continue until no swaps occur
```

**Improvement over Bubble Sort:**
- Eliminates small values at end quickly
- Gap sequence (n, n*10/13, ..., 1) is optimal

### 14. Gnome Sort
**Method:** `IntArrayGnomeSort(int[] data)`

**Description:** Simple comparison-based algorithm similar to insertion sort. Moves element to correct position then continues.

- **Time Complexity:** O(n²)
- **Space Complexity:** O(1)
- **Stable:** Yes
- **In-place:** Yes
- **Use Case:** Educational purposes, very simple algorithm

**How It Works:**
```
1. Start at position 0
2. If current element >= previous: move right
3. If current element < previous: swap and move left
4. Continue until at end
```

**Like a Garden Gnome:**
- Gnome picks up element if wrong position
- Places in correct spot
- Returns to continue

### 15. Odd-Even Sort (Brick Sort)
**Method:** `IntArrayOddEvenSort(int[] data)`

**Description:** Parallel version of bubble sort that compares all odd-indexed elements with even-indexed ones in alternation.

- **Time Complexity:** O(n²)
- **Space Complexity:** O(1)
- **Stable:** Yes
- **In-place:** Yes
- **Use Case:** Parallel processing, theoretical interest

**How It Works:**
```
1. Odd phase: Compare (1,2), (3,4), (5,6)...
2. Even phase: Compare (0,1), (2,3), (4,5)...
3. Repeat until sorted
Advantage: Odd and even phase comparisons are independent
```

**Parallelism:**
- Each phase can be parallelized
- Good for GPU/SIMD implementations

### 16. Cycle Sort
**Method:** `IntArrayCycleSort(int[] data)`

**Description:** In-place, unstable sorting algorithm that minimizes the number of memory writes. Used when writes are expensive.

- **Time Complexity:** O(n²)
- **Space Complexity:** O(1)
- **Stable:** No
- **In-place:** Yes
- **Use Case:** Flash memory where writes are expensive, theoretical interest

**How It Works:**
```
1. For each position, determine where element should go
2. Place element in correct position
3. Take displaced element and repeat
4. Creates a "cycle" of movements
Example: Minimizes writes by rotating elements through their cycles
```

**Unique Feature:**
- Theoretically minimal number of writes (at most n-1)
- Used in scenarios like EEPROM or flash storage optimization

---

## Comparison Table

| Algorithm | Best Case | Average Case | Worst Case | Space | Stable | In-Place |
|-----------|-----------|--------------|-----------|-------|--------|----------|
| Bubble Sort | O(n) | O(n²) | O(n²) | O(1) | Yes | Yes |
| Insertion Sort | O(n) | O(n²) | O(n²) | O(1) | Yes | Yes |
| Selection Sort | O(n²) | O(n²) | O(n²) | O(1) | No | Yes |
| Shell Sort | O(n log n) | O(n^(3/2)) | O(n²) | O(1) | No | Yes |
| Quick Sort | O(n log n) | O(n log n) | O(n²) | O(log n) | No | Yes |
| Radix Sort | O(nk) | O(nk) | O(nk) | O(n+k) | Yes | No |
| Tim Sort | O(n) | O(n log n) | O(n log n) | O(n) | Yes | No |
| Counting Sort | O(n+k) | O(n+k) | O(n+k) | O(n+k) | Yes | No |
| Intro Sort | O(n log n) | O(n log n) | O(n log n) | O(log n) | No | Yes |
| Heap Sort | O(n log n) | O(n log n) | O(n log n) | O(1) | No | Yes |
| Merge Sort | O(n log n) | O(n log n) | O(n log n) | O(n) | Yes | No |
| Comb Sort | O(n log n) | O(n²) | O(n²) | O(1) | No | Yes |
| Gnome Sort | O(n) | O(n²) | O(n²) | O(1) | Yes | Yes |
| Odd-Even Sort | O(n) | O(n²) | O(n²) | O(1) | Yes | Yes |
| Cycle Sort | O(n²) | O(n²) | O(n²) | O(1) | No | Yes |

---

## When to Use Each Algorithm

### For Production Systems
- **General Purpose:** Tim Sort, Intro Sort
- **Guaranteed O(n log n):** Merge Sort, Heap Sort, Intro Sort
- **Need Stability:** Tim Sort, Merge Sort
- **Small Arrays:** Insertion Sort (becomes faster for n < 16)

### For Specific Data Types
- **Integers with small range:** Counting Sort, Radix Sort
- **Linked Lists:** Merge Sort (random access not needed)
- **Nearly Sorted Data:** Tim Sort, Modified Bubble Sort
- **External Sorting:** Merge Sort

### For Special Requirements
- **Minimize Writes:** Cycle Sort
- **Minimize Memory:** Heap Sort, Intro Sort
- **Parallelizable:** Odd-Even Sort, Merge Sort
- **Cache Efficient:** Quick Sort, Intro Sort

### Performance by Use Case
1. **Best Average Performance:** Tim Sort > Intro Sort > Quick Sort
2. **Best Worst Case:** Merge Sort = Heap Sort = Intro Sort = Tim Sort
3. **Best for Small Arrays:** Insertion Sort
4. **Best for Nearly Sorted:** Tim Sort > Modified Bubble Sort
5. **Best with Limited Memory:** Heap Sort, Intro Sort
6. **Fastest for Large Integers:** Radix Sort

### Educational/Theoretical
- Bubble Sort - Learn basics
- Insertion Sort - Adaptive, simple
- Selection Sort - Selection process
- Quick Sort - Divide and conquer
- Merge Sort - Divide and conquer, stability
- Radix Sort - Non-comparative approach
- Counting Sort - Non-comparative approach

---

## Testing Information

All algorithms are tested in `UnitTestAdvancedSorting.cs` with:
- Small arrays verification
- Edge cases (empty, single element, duplicates)
- Large arrays (1000+ elements)
- Performance comparison

### Running Tests
```bash
dotnet test TubieTools_Aspire.Tests -c Release
```

Each test verifies:
1. Array is sorted correctly
2. No data loss
3. Handles edge cases
4. Consistent results

---

## Implementation Notes

- All algorithms work on `int[] data` arrays
- Algorithms sort in **ascending order**
- Most in-place algorithms use the provided `Swap()` helper
- Non-in-place algorithms create temporary arrays as needed
- All implementations handle negative numbers
- All implementations are stable where applicable

---

## Performance Optimization Tips

1. **For Known Small Ranges:** Use Counting Sort
2. **For Already Sorted Data:** Use Tim Sort or Modified Bubble Sort
3. **For Guaranteed Performance:** Use Intro Sort or Merge Sort
4. **For Memory Constrained:** Use Heap Sort or Intro Sort
5. **For Large Data Sets:** Use Radix Sort (if only integers) or Tim Sort
6. **For Minimal Writes:** Use Cycle Sort (theoretical interest)

