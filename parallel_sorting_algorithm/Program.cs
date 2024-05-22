using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

class ParallelOddEvenSort
{

    static void Main(string[] args)
    {
        // Define the number of stages (stations) in the assembly line
        int numStages = 3;
        int[] classification = { 0, 1, 2, 3 };

        // Create a dataflow block for each stage in the assembly line
        var stages = new TransformBlock<int, int>[numStages];
        for (int i = 0; i < numStages; i++)
        {
            stages[i] = new TransformBlock<int, int>(
                 product =>
                {

                    // Perform stage-specific operation on the product
                    Console.WriteLine($"Stage {classification[i]} processed product {product}");

                    // Pass the product to the next stage (if any)                  
                    Console.WriteLine($"Product {product} moved to Stage { classification[i]}");
                    return product + 1; // Product is modified as it moves to the next stage                  

                },
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 } // Ensure sequential processing within each stage
            );
        }
          /*  stages[1] = new TransformBlock<int, int>(
             product =>
             {

                     // Perform stage-specific operation on the product
                     Console.WriteLine($"Stage {1} processed product {product}");

                     // Pass the product to the next stage (if any)                  
                     Console.WriteLine($"Product {product} moved to Stage {2}");
                 return product + 1; // Product is modified as it moves to the next stage                  

                 },
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 } // Ensure sequential processing within each stage
        );

        stages[2] = new TransformBlock<int, int>(
                 product =>
                 {

                     // Perform stage-specific operation on the product
                     Console.WriteLine($"Stage {2} processed product {product}");

                     // Pass the product to the next stage (if any)                  
                     Console.WriteLine($"Return product");
                     return product + 1; // Product is modified as it moves to the next stage                  

                 },
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 } // Ensure sequential processing within each stage
            );*/
        var endStage = new ActionBlock<int>(
                 product =>
                 {

                     // Perform stage-specific operation on the product
                     Console.WriteLine($"Stage {2} processed product {product}");

                     // Pass the product to the next stage (if any)                  
                     Console.WriteLine($"Return product {product}");
                     //return product + 1; // Product is modified as it moves to the next stage                  

                 },
                   new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 } // Ensure sequential processing within each stage
            );
        // Link stages together to form the assembly line
        for (int i = 0; i < numStages - 1; i++)
        {
            stages[i].LinkTo(stages[i + 1], new DataflowLinkOptions { PropagateCompletion = true });
        }

        stages[2].LinkTo(endStage, new DataflowLinkOptions { PropagateCompletion = true });
        // Start the production process by posting an initial product to the first stage
        stages[0].Post(0);
        stages[0].Post(1);
        stages[0].Post(15);
        stages[0].Post(-12);

        // Signal completion to the first stage
        stages[0].Complete();

        // Wait for the last stage to complete

        endStage.Completion.Wait();
        Console.WriteLine($"END.");
    }
}

    /*
    static void Main(string[] args)
    {
        //  0 1 2 3 5 5 6 9  
        
        int[] arr = { 5, 2, 9, 1, 5, 6, 0, 3,11, 18,
            8,  9, 22, 13, 10, 18, 11,7, 14, 22,
            4,  8,  2, 6,  4,  1, 17, 13, 16,
            15, 11, 12, 19, 15, 17, 14,  5,  7 };
        

        OddEvenSort(arr, 0, arr.Length);

        Console.WriteLine("Sorted array:");
        foreach (int num in arr)
        {
            Console.Write(num + " ");
        }
        Console.WriteLine();
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
                if ( i + 1 < end && arr[i] > arr[i + 1])
                {
                    Swap(arr, i, i + 1);                
                }
            }
        
    }

    static void Swap(int[] arr, int i, int j)
    {
        int temp = arr[i];
        arr[i] = arr[j];
        arr[j] = temp;
    }
   */

