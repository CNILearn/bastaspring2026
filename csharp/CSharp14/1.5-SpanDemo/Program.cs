using BenchmarkDotNet.Running;
using SpanDemo.Examples;

namespace SpanDemo;

/// <summary>
/// Demonstrates Span&lt;T&gt; advantages over params arrays and OverloadResolutionPriority.
///
/// Run modes:
///   dotnet run --configuration Release -- --benchmark   runs BenchmarkDotNet
///   dotnet run                                          runs interactive demos
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--benchmark")
        {
            RunBenchmarks();
            return;
        }

        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   Span<T> vs params Array & OverloadResolutionPriority   ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝\n");

        Console.WriteLine("This demo covers three topics:");
        Console.WriteLine("  1. params array  vs  params ReadOnlySpan<T>  (allocations)");
        Console.WriteLine("  2. string methods vs Span<char> methods       (performance)");
        Console.WriteLine("  3. [OverloadResolutionPriority] attribute     (overload resolution)\n");

        Console.WriteLine("Tip: run with --benchmark to execute BenchmarkDotNet benchmarks.\n");
        Console.WriteLine("Press any key to start...");
        Console.ReadKey();
        Console.Clear();

        // --- Demo 1: params array vs params Span ---
        ParamsVsSpanExample.Run();
        PressAnyKey();

        // --- Demo 2: string vs Span<char> ---
        StringVsSpanExample.Run();
        PressAnyKey();

        // --- Demo 3: OverloadResolutionPriority ---
        OverloadResolutionPriorityExample.Run();

        Console.WriteLine("\n=== Summary ===\n");
        Console.WriteLine("  ✓ params ReadOnlySpan<T> avoids heap allocation for small argument lists");
        Console.WriteLine("  ✓ Span<char> enables zero-copy string slicing and parsing");
        Console.WriteLine("  ✓ [OverloadResolutionPriority] lets you steer callers to the best overload");
        Console.WriteLine("  ✓ All three features combine to write faster, GC-friendlier code\n");
        Console.WriteLine("See docs/csharp14/SpanDemo.md for a full write-up and benchmark results.");
    }

    static void RunBenchmarks()
    {
        Console.WriteLine("Running BenchmarkDotNet benchmarks – this takes a few minutes...\n");
        BenchmarkRunner.Run<Benchmarks.ParamsVsSpanBenchmark>();
        BenchmarkRunner.Run<Benchmarks.StringMethodsBenchmark>();
    }

    static void PressAnyKey()
    {
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
        Console.Clear();
    }
}
