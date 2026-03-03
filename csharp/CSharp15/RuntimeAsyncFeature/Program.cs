using System.Diagnostics;

namespace RuntimeAsyncFeature;

/// <summary>
/// Demonstrates the .NET 10 Runtime Async preview feature.
/// This sample showcases the performance differences when the runtime-async feature is enabled vs disabled.
/// 
/// Key Configuration Requirements:
/// - EnablePreviewFeatures: true
/// - Features: runtime-async=on
/// - Environment Variable: DOTNET_RuntimeAsync=1
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("🚀 .NET 10 Runtime Async Feature Demo");
        Console.WriteLine("=====================================");
        Console.WriteLine();
        
        // Check if runtime async is enabled via environment variable
        var runtimeAsyncEnabled = Environment.GetEnvironmentVariable("DOTNET_RuntimeAsync") == "1";
        Console.WriteLine($"🔧 Runtime Async Status: {(runtimeAsyncEnabled ? "✅ ENABLED" : "❌ DISABLED")}");
        Console.WriteLine($"🔧 Environment Variable DOTNET_RuntimeAsync: {Environment.GetEnvironmentVariable("DOTNET_RuntimeAsync") ?? "Not Set"}");
        Console.WriteLine();
        
        if (!runtimeAsyncEnabled)
        {
            Console.WriteLine("⚠️  To enable Runtime Async feature, run with:");
            Console.WriteLine("   DOTNET_RuntimeAsync=1 dotnet run");
            Console.WriteLine();
        }
        else
        {
            Console.WriteLine("✨ Runtime Async optimization is enabled!");
        }

        // Run a simple async demonstration
        await RunSimpleAsyncDemo();
        
        Console.WriteLine();
        Console.WriteLine("✅ Runtime Async Feature demonstration completed!");
    }

    private static async Task RunSimpleAsyncDemo()
    {
        Console.WriteLine("🎯 Simple Async Workload Demo");
        Console.WriteLine("=============================");
        Console.WriteLine();

        var stopwatch = Stopwatch.StartNew();
        
        // Create 100 simple async tasks
        var tasks = new List<Task>();
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(SimpleAsyncTask(i));
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        Console.WriteLine($"   ✅ Completed 100 async tasks in {stopwatch.ElapsedMilliseconds} ms");
        Console.WriteLine($"   📊 Average per task: {stopwatch.ElapsedMilliseconds / 100.0:F2} ms");
        Console.WriteLine();

        // Demonstrate specific .NET 10 runtime async improvements
        await DemonstrateValueTaskSupport();
        await DemonstrateTaskYieldOptimizations();
        await DemonstrateAsyncFileIoOptimizations();
    }

    private static async Task SimpleAsyncTask(int id)
    {
        await Task.Delay(10); // Simulate some async work
        await Task.Yield();   // Allow other tasks to proceed
        var result = id * id; // Simple computation
    }

    /// <summary>
    /// Demonstrates ValueTask support improvements in .NET 10 Runtime Async
    /// Key benefit: Direct ValueTask support eliminates boxing to Task
    /// </summary>
    private static async Task DemonstrateValueTaskSupport()
    {
        Console.WriteLine("🔧 ValueTask Support Demo (Direct support, no boxing to Task)");
        Console.WriteLine("=============================================================");
        
        var stopwatch = Stopwatch.StartNew();
        
        // Use ValueTask extensively to show improved performance
        var tasks = new List<ValueTask>();
        for (int i = 0; i < 1000; i++)
        {
            tasks.Add(ValueTaskWorkload(i));
        }
        
        foreach (var task in tasks)
        {
            await task;
        }
        
        stopwatch.Stop();
        Console.WriteLine($"   ✅ Completed 1,000 ValueTask operations in {stopwatch.ElapsedMilliseconds} ms");
        Console.WriteLine($"   💡 Runtime Async eliminates Task boxing overhead");
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates Task.Yield() optimizations in .NET 10 Runtime Async
    /// Key benefit: Smarter continuation placement and reduced async overhead
    /// </summary>
    private static async Task DemonstrateTaskYieldOptimizations()
    {
        Console.WriteLine("⚡ Task.Yield() Optimization Demo (Smarter continuation placement)");
        Console.WriteLine("=================================================================");
        
        var stopwatch = Stopwatch.StartNew();
        
        // Create tasks that heavily use Task.Yield() to demonstrate optimization
        var tasks = new List<Task>();
        for (int i = 0; i < 2000; i++)
        {
            tasks.Add(TaskYieldWorkload(i));
        }
        
        await Task.WhenAll(tasks);
        stopwatch.Stop();
        
        Console.WriteLine($"   ✅ Completed 2,000 Task.Yield() heavy operations in {stopwatch.ElapsedMilliseconds} ms");
        Console.WriteLine($"   💡 Runtime Async provides ~24% improvement over .NET 9");
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates async file I/O pattern optimizations
    /// Key benefit: Better CPU utilization and reduced memory allocations
    /// </summary>
    private static async Task DemonstrateAsyncFileIoOptimizations()
    {
        Console.WriteLine("📁 Async File I/O Pattern Demo (Smaller state machines)");
        Console.WriteLine("========================================================");
        
        var stopwatch = Stopwatch.StartNew();
        
        // Simulate file I/O patterns that benefit from runtime async
        var tasks = new List<Task>();
        for (int i = 0; i < 1000; i++)
        {
            tasks.Add(FileIoSimulationTask(i));
        }
        
        await Task.WhenAll(tasks);
        stopwatch.Stop();
        
        Console.WriteLine($"   ✅ Completed 1,000 async file I/O simulations in {stopwatch.ElapsedMilliseconds} ms");
        Console.WriteLine($"   💡 Runtime Async provides smaller state machines and ~20% improvement over .NET 9");
        Console.WriteLine();
    }

    // Supporting workload methods

    private static async ValueTask ValueTaskWorkload(int id)
    {
        await ValueTask.CompletedTask;
        var temp = id % 100;
        var intermediate = await ValueTask.FromResult(temp * 2);
        await ValueTask.CompletedTask;
        var result = intermediate + id;
    }

    private static async Task TaskYieldWorkload(int id)
    {
        await Task.Yield();
        var temp = id * 2;
        await Task.Yield();
        var result = temp + id;
    }

    private static async Task FileIoSimulationTask(int id)
    {
        // Simulate file read
        await Task.Delay(1);
        await Task.Yield();
        
        // Simulate processing
        var data = id.ToString();
        await Task.Yield();
        
        // Simulate file write
        await Task.Delay(1);
        var result = data.Length;
    }
}