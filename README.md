# Parallel-sorting algorithm
## Overview
This program represents parallel sorting algorithm with chain architecture, static syncronization and shared memory.
## Chain Architecture:
* In the context of parallel processing, a chain architecture refers to a setup where processors or threads are organized in a linear sequence, forming a chain.
Data is passed from one processing unit to the next in a controlled manner, typically involving some form of pipeline processing.
Each node in the chain performs a specific task, and the results flow down the chain.

* For this algorithm, a chain architecture means:  
  > - The input is divided into smaller subarrays or chunks.  
  > - Each processor/thread is responsible for sorting its chunk.  
  > - The sorted chunks are passed to the next processor in the chain for further processing the final sorted sequence is obtained.
  ## Shared Memory:
The shared memory model indicates that all processors or threads in the system have access to a common memory space. 
They can read and write to this memory, which allows them to communicate and share intermediate results.
In parallel sorting, shared memory can be used to store the original data and the sorted results. 
Each processor/thread works on a specific portion of the data, but they all share access to the same memory pool.
In this case, shared memory helps facilitate communication and coordination between different stages of the chain, reducing the need for explicit message passing.
