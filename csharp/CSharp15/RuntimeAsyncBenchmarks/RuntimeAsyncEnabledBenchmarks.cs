using BenchmarkDotNet.Attributes;

namespace RuntimeAsyncBenchmarks;

/// <summary>
/// Benchmark specifically for .NET 10 with Runtime Async enabled
/// This requires the DOTNET_RuntimeAsync=1 environment variable
/// </summary>
[MemoryDiagnoser]
[SimpleJob(baseline: true)]
public class RuntimeAsyncEnabledBenchmarks
{
    private const int TaskCount = 100;
    private const int DelayMs = 10;
    
    // Large-scale benchmark parameters for specific scenarios
    private const int TaskYieldLargeCount = 100000; // Scaled down from 1M for demo purposes
    private const int ValueTaskCount = 50000;       // Scaled down from 500k for demo purposes
    private const int FileIoCount = 10000;          // Scaled down from 100k for demo purposes

    /// <summary>
    /// Benchmark: Simple async task workload with runtime async enabled
    /// </summary>
    [Benchmark(Description = "Simple Async Tasks (Runtime Async ON)")]
    public async Task SimpleAsyncTasksWithRuntimeAsync()
    {
        var tasks = new List<Task>();
        for (int i = 0; i < TaskCount; i++)
        {
            tasks.Add(SimpleAsyncTask(i));
        }
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Benchmark: CPU-bound async workload with runtime async enabled
    /// </summary>
    [Benchmark(Description = "CPU-bound Async Tasks (Runtime Async ON)")]
    public async Task CpuBoundAsyncTasksWithRuntimeAsync()
    {
        var tasks = new List<Task<int>>();
        for (int i = 0; i < TaskCount; i++)
        {
            tasks.Add(CpuBoundAsyncTask(i));
        }
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Benchmark: I/O simulation with runtime async enabled
    /// </summary>
    [Benchmark(Description = "I/O Simulation Tasks (Runtime Async ON)")]
    public async Task IoSimulationTasksWithRuntimeAsync()
    {
        var tasks = new List<Task>();
        for (int i = 0; i < TaskCount; i++)
        {
            tasks.Add(IoSimulationTask(i));
        }
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Benchmark: Mixed async workload with runtime async enabled
    /// </summary>
    [Benchmark(Description = "Mixed Workload Tasks (Runtime Async ON)")]
    public async Task MixedWorkloadTasksWithRuntimeAsync()
    {
        var tasks = new List<Task>();
        for (int i = 0; i < TaskCount; i++)
        {
            if (i % 2 == 0)
                tasks.Add(SimpleAsyncTask(i));
            else
                tasks.Add(CpuBoundAsyncTask(i));
        }
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Benchmark: High volume Task.Yield() calls with runtime async optimizations
    /// Expected improvement: ~24% faster, ~49% fewer allocations vs .NET 9
    /// </summary>
    [Benchmark(Description = "Task.Yield() High Volume (Runtime Async ON)")]
    public async Task TaskYieldHighVolumeWithRuntimeAsync()
    {
        var tasks = new List<Task>();
        for (int i = 0; i < TaskYieldLargeCount; i++)
        {
            tasks.Add(TaskYieldWorkload(i));
        }
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Benchmark: ValueTask completions with runtime async - direct support eliminates boxing
    /// Expected improvement: ~23% faster, allocations eliminated vs .NET 9
    /// </summary>
    [Benchmark(Description = "ValueTask Completions (Runtime Async ON)")]
    public async Task ValueTaskCompletionsWithRuntimeAsync()
    {
        var tasks = new List<ValueTask>();
        for (int i = 0; i < ValueTaskCount; i++)
        {
            tasks.Add(ValueTaskWorkload(i));
        }
        
        foreach (var task in tasks)
        {
            await task;
        }
    }

    /// <summary>
    /// Benchmark: Async file I/O simulation with improved continuation placement
    /// Expected improvement: ~20% faster, ~46% fewer allocations vs .NET 9
    /// </summary>
    [Benchmark(Description = "Async File I/O Simulation (Runtime Async ON)")]
    public async Task AsyncFileIoSimulationWithRuntimeAsync()
    {
        var tasks = new List<Task>();
        for (int i = 0; i < FileIoCount; i++)
        {
            tasks.Add(FileIoWorkload(i));
        }
        await Task.WhenAll(tasks);
    }

    private static async Task SimpleAsyncTask(int id)
    {
        await Task.Delay(DelayMs); // Simulate some async work
        await Task.Yield();        // Allow other tasks to proceed
        var result = id * id;      // Simple computation
    }

    private static async Task<int> CpuBoundAsyncTask(int id)
    {
        await Task.Yield(); // Yield to allow scheduling
        
        // CPU-bound work
        int result = 0;
        for (int i = 0; i < 1000; i++)
        {
            result += (id + i) * (id + i);
        }
        
        await Task.Yield(); // Yield again
        return result;
    }

    private static async Task IoSimulationTask(int id)
    {
        // Simulate multiple I/O operations
        await Task.Delay(5);  // First I/O
        await Task.Yield();
        await Task.Delay(5);  // Second I/O
        await Task.Yield();
        
        var result = id % 100; // Simple computation
    }

    // New workload methods for specific .NET 10 runtime async scenarios

    /// <summary>
    /// Workload focused on Task.Yield() to demonstrate async state machine improvements
    /// </summary>
    private static async Task TaskYieldWorkload(int id)
    {
        await Task.Yield(); // Primary focus: yield overhead reduction
        var temp = id * 2;
        await Task.Yield(); // Second yield to increase state machine complexity
        var result = temp + id;
    }

    /// <summary>
    /// Workload using ValueTask to demonstrate direct ValueTask support without boxing
    /// </summary>
    private static async ValueTask ValueTaskWorkload(int id)
    {
        // ValueTask operations that benefit from runtime async optimizations
        await ValueTask.CompletedTask;
        var temp = id % 1000;
        
        // Using ValueTask.FromResult to avoid Task allocation
        var intermediate = await ValueTask.FromResult(temp * 2);
        await ValueTask.CompletedTask;
        
        var result = intermediate + id;
    }

    /// <summary>
    /// Workload simulating file I/O patterns with improved continuation placement
    /// </summary>
    private static async Task FileIoWorkload(int id)
    {
        // Simulate file read operation
        await Task.Delay(1); // Shorter delay for higher volume
        await Task.Yield();
        
        // Simulate processing
        var data = id.ToString();
        await Task.Yield();
        
        // Simulate file write operation  
        await Task.Delay(1);
        var result = data.Length;
    }
}

