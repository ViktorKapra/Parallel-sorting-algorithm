using System;
using System.Threading.Tasks;

class ParallelOddEvenSort
{
    static void Main(string[] args)
    {
        //  0 1 2 3 5 5 6 9  
        int[] arr = { 5, 2, 9, 1, 5, 6, 0, 3,11, 18,
            8,  9, 22, 13, 10, 18, 11,7, 14, 22,
            4,  8,  2, 6,  4,  1, 17, 13, 16,
            15, 11, 12, 19, 15, 17, 14,  5,  7 };

        ParallelOddEvenChunkSorter(arr, 0, arr.Length);

        Console.WriteLine("Sorted array:");
        foreach (int num in arr)
        {
            Console.Write(num + " ");
        }
        Console.WriteLine();
    }

    static void Orchestrate(int[] arr)
    {
        int n = arr.Length;
        int chunkSize = 5;


    }


    static void OddEvenSort(int[] arr, int beg, int end)
    {

        bool sorted = false;



        while (!sorted)
        {
            sorted = true;

            // Perform odd phase
            for (int i = beg + (beg + 1) % 2; i <= end - 1; i += 2)
            {
                if (i + 1 < end && arr[i] > arr[i + 1])
                {
                    Swap(arr, i, i + 1);
                    sorted = false;
                }
            }

            // Perform even phase
            for (int i = beg + (beg) % 2; i <= end - 1; i += 2)
            {
                if ( i + 1 < end && arr[i] > arr[i + 1])
                {
                    Swap(arr, i, i + 1);
                    sorted = false;
                }
            }
        }
    }

    static void Swap(int[] arr, int i, int j)
    {
        int temp = arr[i];
        arr[i] = arr[j];
        arr[j] = temp;
    }
}
