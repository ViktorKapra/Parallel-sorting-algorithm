using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

class ParallelOddEvenSort
{
    static int[] arr = { 40, 39, 38, 37, 36, 35, 34, 33,32, 31,
            30,  29, 28, 27, 26, 25, 24,23, 22, 21,
            20,  19,  18, 17,  16,  15, 14, 13, 12,11,
            10, 9, 8, 7, 6, 5, 4,  3, 2, 1 };
    static int[] SubArray(int[] data, int index, int length)
    {
        int[] result = new int[length];
        Array.Copy(data, index, result, 0, length);
        return result;
    }
    static void Swap(int[] arr, int i, int j)
    {
        int temp = arr[i];
        arr[i] = arr[j];
        arr[j] = temp;
    }
    static void OddEven(int[] arr, int beg, int end)
    {

        // Perform odd phase
        for (int i = beg + (beg + 1) % 2; i <= end - 1; i += 2)
        {
            if (i + 1 < end && arr[i] > arr[i + 1])
            {
                Swap(arr, i, i + 1);
            }
        }

        // Perform even phase
        for (int i = beg + (beg) % 2; i <= end - 1; i += 2)
        {
            if (i + 1 < end && arr[i] > arr[i + 1])
            {
                Swap(arr, i, i + 1);
            }
        }

    }

    static void Main(string[] args)
    {
        // Define the number of stages (stations) in the assembly line
        int numStages = 2;


        // Create a dataflow block for each stage in the assembly line
        var stages = new TransformBlock<Tuple<int,int>, Tuple<int, int>>[numStages];

        stages[0] = new TransformBlock<Tuple<int, int>, Tuple<int, int>>(
                 range =>
                 {

                     OddEven(arr, range.Item1, range.Item2);
                     var updatedRange = new Tuple<int, int>(range.Item1 + 1 , range.Item2 );
                     return updatedRange; // Product is modified as it moves to the next stage                  

                 },
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 } // Ensure sequential processing within each stage
            );
        for (int i = 1; i < numStages; i++)
        {
            stages[i] = new TransformBlock<Tuple<int, int>, Tuple<int, int>>(
                 product =>
                {

                   OddEven(arr,product.Item1,product.Item2);
                    return product ; // Product is modified as it moves to the next stage                  

                },
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 } // Ensure sequential processing within each stage
            );
        }
         
        var endStage = new ActionBlock<Tuple<int, int>>(
                 product =>
                 {

                     for(int i=product.Item1;i<product.Item2;i++)
                     Console.WriteLine($" {arr[i]}");
                     Console.WriteLine("End of Chunk");
                     // Pass the product to the next stage (if any                 

                 },
                   new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 } // Ensure sequential processing within each stage
            );
        // Link stages together to form the assembly line
        for (int i = 0; i < numStages - 1; i++)
        {
            stages[i].LinkTo(stages[i + 1], new DataflowLinkOptions { PropagateCompletion = true });
        }

        stages[numStages-1].LinkTo(endStage, new DataflowLinkOptions { PropagateCompletion = true });
        // Start the production process by posting an initial product to the first stage

        

        int chunkSize = 10;
        int chunks = (arr.Length)/chunkSize;
        for (int j = 0; j < arr.Length; j++)
        {
            for (int i = 0; i < chunks; i++)
            {

                stages[0].Post(new Tuple<int, int>(Math.Max( 0,i * chunkSize - 1), (i + 1) * chunkSize));
            }
            
        }
        // Signal completion to the first stage
        stages[0].Complete();
        endStage.Completion.Wait();
        // Wait for the last stage to complete

        endStage.Completion.Wait();

        Console.WriteLine(String.Join(' ',arr));
        Console.WriteLine($"END.");
    }
}

    /*
    static void Main(string[] args)
    {
        //  0 1 2 3 5 5 6 9  
        
       
        

        OddEvenSort(arr, 0, arr.Length);

        Console.WriteLine("Sorted array:");
        foreach (int num in arr)
        {
            Console.Write(num + " ");
        }
        Console.WriteLine();
    }

    

   

    static void Swap(int[] arr, int i, int j)
    {
        int temp = arr[i];
        arr[i] = arr[j];
        arr[j] = temp;
    }
   */

