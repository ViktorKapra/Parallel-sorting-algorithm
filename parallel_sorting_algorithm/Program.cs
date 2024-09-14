using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

class ParallelOddEvenSort
{
    static volatile int[] arr = new int[75000];
    /*{ 40, 39, 38, 37, 36, 35, 34, 33,32, 31,
        30,  29, 28, 27, 26, 25, 24,23, 22, 21,
        20,  19,  18, 17,  16,  15, 14, 13, 12,11,
        10, 9, 8, 7, 6, 5, 4,  3, 2, 1 }; */
    static void SettInitialArray()
    {
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = arr.Length - i;
        }
    }
    static int[] SubArray(int[] data, int index, int length)
    {
        int[] result = new int[length];
        Array.Copy(data, index, result, 0, length);
        return result;
    }
    static void Swap(int[] arr, int i, int j)
    {
        int a = Volatile.Read(ref arr[i]);
        int b = Volatile.Read(ref arr[j]);
        Volatile.Write(ref arr[i], b);
        Volatile.Write(ref arr[j], a);

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

    static void Bubble(int[] arr, int beg, int end)
    {
        for (int i = beg; i < end - 1; i++)
        {
            if (arr[i] > arr[i + 1])
                Swap(arr, i, i + 1);
        }
    }


    static long SortChunkNodes(int numStages, int chunkSize)
    {
        SettInitialArray();

        // Create a dataflow block for each stage in the assembly line
        var stages = new TransformBlock<Tuple<int, int>, Tuple<int, int>>[numStages];

        stages[0] = new TransformBlock<Tuple<int, int>, Tuple<int, int>>(
                 range =>
                 {
                     OddEven(arr, range.Item1, range.Item2);
                     var updatedRange = new Tuple<int, int>(range.Item1, range.Item2 - 1); // updating range in order to make overlap
                     return updatedRange;
                 },
                  new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 } // Ensure sequential processing within each stage
            );

        for (int i = 1; i < numStages; i++)
        {
            stages[i] = new TransformBlock<Tuple<int, int>, Tuple<int, int>>(
                 range =>
                 {
                     OddEven(arr, range.Item1, range.Item2);
                     return range;
                 },
                   new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 } // Ensure sequential processing within each stage
            );
        }

        // creating end of stage
        var endStage = new ActionBlock<Tuple<int, int>>(range => { });//, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 });

        // Link stages together to form the assembly line
        for (int i = 0; i < numStages - 1; i++)
        {
            stages[i].LinkTo(stages[i + 1], new DataflowLinkOptions { PropagateCompletion = true });
        }

        stages[numStages - 1].LinkTo(endStage, new DataflowLinkOptions { PropagateCompletion = true });
        // Start the production process by posting an initial product to the first stage


        int chunks = (arr.Length) / chunkSize;
        var iterationsCount = arr.Length;//(arr.Length  / (numStages/2)); //* Math.Max(1,numStages - 1) )

        if(numStages >5) iterationsCount/=(numStages/5);
        var watch = System.Diagnostics.Stopwatch.StartNew();

        for (int j = 0; j < iterationsCount; j++)
        {
            for (int i = 0; i < chunks; i++)
            {
                int beg = Math.Max(0, i * chunkSize - 1);
                int end = Math.Min((i + 1) * chunkSize, arr.Length);
                stages[0].Post(new Tuple<int, int>(beg, end));
            }
            while (endStage.InputCount > 0)
            {
                Thread.Sleep(2);
            }

            // Console.WriteLine("End stage messages" + endStage.InputCount);

        }
        // Signal completion to the first stage
        stages[0].Complete();
        endStage.Completion.Wait();
        // Wait for the last stage to complete

        endStage.Completion.Wait();

        // the code that you want to measure comes here
        watch.Stop();
        return watch.ElapsedMilliseconds;
        //Console.WriteLine(watch.ElapsedMilliseconds + " ms");

        //Console.WriteLine(String.Join(' ', arr));
        Console.WriteLine($"END.");



    }

    static long SortCopyChunks(int numStages, int chunkSize)
    {
        SettInitialArray();

        // Create a dataflow block for each stage in the assembly line
        var stages = new TransformBlock<Tuple<int[], Tuple<int, int>>, Tuple<int[], Tuple<int, int>>>[numStages];

        stages[0] = new TransformBlock<Tuple<int[], Tuple<int, int>>, Tuple<int[], Tuple<int, int>>>(
                 range =>
                 {
                     OddEven(range.Item1, 0, range.Item1.Length);
                     //var updatedRange = new Tuple<int, int>(range.Item1, range.Item2 - 1); // updating range in order to make overlap
                     int[] infoArr = new int[range.Item1.Length - 1];
                     Array.Copy(range.Item1, infoArr, range.Item1.Length - 1);
                     var newChunk = new Tuple<int[], Tuple<int, int>>(infoArr, new Tuple<int, int>(range.Item2.Item1, range.Item2.Item2));

                     arr[Math.Min(range.Item2.Item2 - 1, arr.Length - 1)] = range.Item1[range.Item1.Length - 1];

                     return newChunk;
                 },
                  new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 } // Ensure sequential processing within each stage
            );

        for (int i = 1; i < numStages; i++)
        {
            stages[i] = new TransformBlock<Tuple<int[], Tuple<int, int>>, Tuple<int[], Tuple<int, int>>>(
                 range =>
                 {
                     OddEven(range.Item1, 0, range.Item1.Length);
                     return range;
                 },
                   new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 } // Ensure sequential processing within each stage
            );
        }

        // creating end of stage
        var endStage = new ActionBlock<Tuple<int[], Tuple<int, int>>>(range => { saveChunk(range.Item1, range.Item2); });//, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 });

        // Link stages together to form the assembly line
        for (int i = 0; i < numStages - 1; i++)
        {
            stages[i].LinkTo(stages[i + 1], new DataflowLinkOptions { PropagateCompletion = true });
        }

        stages[numStages - 1].LinkTo(endStage, new DataflowLinkOptions { PropagateCompletion = true });
        // Start the production process by posting an initial product to the first stage


        int chunks = (arr.Length) / chunkSize;
        var iterationsCount = arr.Length;//(arr.Length / (numStages)) + 1; //* Math.Max(1,numStages - 1) )

        var watch = System.Diagnostics.Stopwatch.StartNew();

        for (int j = 0; j < iterationsCount; j++)
        {
            for (int i = 0; i < chunks; i++)
            {
                int beg = Math.Max(0, i * chunkSize - 1);
                int end = Math.Min((i + 1) * chunkSize, arr.Length);
                var data = new int[chunkSize];
                Array.Copy(arr, beg, data, 0, chunkSize);
                var chunk = new Tuple<int[], Tuple<int, int>>(data, new Tuple<int, int>(beg, end));
                stages[0].Post(chunk);
            }
            while (endStage.InputCount > 0)
            {
                Thread.Sleep(2);
            }

        }
        // Signal completion to the first stage
        stages[0].Complete();
        endStage.Completion.Wait();
        // Wait for the last stage to complete

        endStage.Completion.Wait();

        // the code that you want to measure comes here
        watch.Stop();
        //Console.WriteLine(watch.ElapsedMilliseconds + " ms");
        Console.WriteLine(String.Join(' ', arr));
        Console.WriteLine($"END.");
        return watch.ElapsedMilliseconds;




    }

    private static void saveChunk(int[] data, Tuple<int, int> interval)
    {

        for (int i = 0; i < data.Length; i++)
        {
            arr[interval.Item1 + i] = data[i];
        }
    }

    static void Main(string[] args)
    {
        //Console.WriteLine(  SortChunkNodes(1, 5000) + "ms ");
        //Console.WriteLine(SortCopyChunks(2, 50));
        
        long[] times = new long[33];
        for (int i = 0; i < times.Length; i++)
        {
            times[i] = long.MaxValue;
        }
        int z = 0;
        int chunkSize = 8000;
        int[] numberOfStages = {1, 2, 4, 8, 12, 16, 20, 24, 28 };
       foreach (int stage in numberOfStages)
       {
            //for (int i = 1; i <= 3; i++)
            //{
                // Console.WriteLine("Number of nodes n= " + j);
                long t = SortChunkNodes(stage, chunkSize);
                if (t < times[z])
                    times[z] = t;      
           // }
            Console.WriteLine("Time for n = " + stage + " : " + times[z] + " ms");
            z++;
        }
        Console.WriteLine("Size of Chunk " + chunkSize +" :");
        for (int t = 0; t < numberOfStages.Length; t++)
        {
            Console.WriteLine("Time for n = " + numberOfStages[t] + " : " + times[t] + " ms");
        }
        
    }
}
