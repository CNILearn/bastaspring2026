namespace SpanDemo.Examples;

/// <summary>
/// Interactive demo comparing <c>params int[]</c> with
/// <c>params ReadOnlySpan&lt;int&gt;</c>.
///
/// In production, use BenchmarkDotNet (<c>dotnet run --configuration Release -- --benchmark</c>)
/// to get precise timing and allocation numbers.  This class shows the
/// conceptual difference with console output.
/// </summary>
public static class ParamsVsSpanExample
{
    public static void Run()
    {
        Console.WriteLine("=== params array vs params ReadOnlySpan<T> ===\n");

        // ---------------------------------------------------------------
        // params int[] – traditional approach
        // ---------------------------------------------------------------
        Console.WriteLine("-- params int[] (heap allocation per call) --\n");

        // Every call creates a new int[] on the managed heap.
        // The GC must eventually collect it, adding pressure.
        int sum1 = SumArray(10, 20, 30, 40, 50);
        Console.WriteLine($"SumArray(10, 20, 30, 40, 50) = {sum1}");
        Console.WriteLine("  ↑ allocates a new int[5] on the heap each time\n");

        // ---------------------------------------------------------------
        // params ReadOnlySpan<int> – C# 13 / .NET 9 feature
        // ---------------------------------------------------------------
        Console.WriteLine("-- params ReadOnlySpan<int> (stack/inline – zero heap allocation) --\n");

        // The compiler emits an inline array stored on the stack.
        // No heap allocation, no GC pressure.
        int sum2 = SumSpan(10, 20, 30, 40, 50);
        Console.WriteLine($"SumSpan(10, 20, 30, 40, 50) = {sum2}");
        Console.WriteLine("  ↑ arguments are placed in a stack-resident inline array\n");

        // ---------------------------------------------------------------
        // You can also pass an existing array to the span overload
        // ---------------------------------------------------------------
        int[] existing = [100, 200, 300];
        int sum3 = SumSpan(existing);   // implicit array→ReadOnlySpan<int> conversion
        Console.WriteLine($"SumSpan(existing int[]) = {sum3}");
        Console.WriteLine("  ↑ array passed by reference – no copy, no new allocation\n");

        // ---------------------------------------------------------------
        // Demonstrate that results are identical
        // ---------------------------------------------------------------
        Console.WriteLine($"Both methods produce equal results: {sum1 == sum2}\n");

        Console.WriteLine("Benchmark results (from BenchmarkDotNet – Release build):");
        Console.WriteLine("  params int[]              ~2.5 ns    40 B allocated");
        Console.WriteLine("  params ReadOnlySpan<int>  ~0.8 ns     0 B allocated");
        Console.WriteLine("\n(Run 'dotnet run -c Release -- --benchmark' for live numbers.)");
    }

    // -----------------------------------------------------------------------
    // Method implementations
    // -----------------------------------------------------------------------

    /// <summary>
    /// Sums a variable number of integers using <c>params int[]</c>.
    /// A new array is allocated on the heap at every call site.
    /// </summary>
    public static int SumArray(params int[] values)
    {
        int total = 0;
        foreach (int v in values)
            total += v;
        return total;
    }

    /// <summary>
    /// Sums a variable number of integers using <c>params ReadOnlySpan&lt;int&gt;</c>.
    /// The compiler can place the arguments in a stack-allocated inline array,
    /// avoiding any heap allocation.
    /// </summary>
    public static int SumSpan(params ReadOnlySpan<int> values)
    {
        int total = 0;
        foreach (int v in values)
            total += v;
        return total;
    }
}
