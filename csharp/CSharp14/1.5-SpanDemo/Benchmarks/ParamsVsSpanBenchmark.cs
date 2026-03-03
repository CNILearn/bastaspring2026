using BenchmarkDotNet.Attributes;

namespace SpanDemo.Benchmarks;

/// <summary>
/// Compares the call overhead and heap allocations of:
///   - a method that receives variable arguments via <c>params int[]</c>
///   - the same method using <c>params ReadOnlySpan&lt;int&gt;</c> (C# 13 / .NET 9+)
///
/// Key insight: <c>params int[]</c> always allocates a new array on the heap, even
/// for a small, fixed set of arguments.  <c>params ReadOnlySpan&lt;int&gt;</c> lets the
/// compiler emit the arguments inline (often on the stack via an inline array),
/// so no heap allocation occurs for short, constant-length call sites.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class ParamsVsSpanBenchmark
{
    // -----------------------------------------------------------------------
    // Method pair – identical logic, different parameter style
    // -----------------------------------------------------------------------

    /// <summary>Sums integers passed as a <c>params</c> array.</summary>
    /// <remarks>Every call-site creates a new <c>int[]</c> on the heap.</remarks>
    public static int SumWithParamsArray(params int[] values)
    {
        int total = 0;
        foreach (int v in values)
            total += v;
        return total;
    }

    /// <summary>Sums integers passed as a <c>params ReadOnlySpan</c>.</summary>
    /// <remarks>
    /// C# 13 / .NET 9 feature: the compiler can allocate the arguments inline
    /// (stack-based inline array) instead of creating a heap array.
    /// </remarks>
    public static int SumWithParamsSpan(params ReadOnlySpan<int> values)
    {
        int total = 0;
        foreach (int v in values)
            total += v;
        return total;
    }

    // -----------------------------------------------------------------------
    // Benchmarks – small, fixed argument list (most common call pattern)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Calls the array-based overload with five literal integers.
    /// Allocates a new <c>int[5]</c> on the heap every invocation.
    /// </summary>
    [Benchmark(Baseline = true, Description = "params int[]")]
    public int Array_FiveArgs() => SumWithParamsArray(1, 2, 3, 4, 5);

    /// <summary>
    /// Calls the span-based overload with five literal integers.
    /// The compiler emits an inline array – zero heap allocation.
    /// </summary>
    [Benchmark(Description = "params ReadOnlySpan<int>")]
    public int Span_FiveArgs() => SumWithParamsSpan(1, 2, 3, 4, 5);

    // -----------------------------------------------------------------------
    // Larger call-site for comparison
    // -----------------------------------------------------------------------

    [Benchmark(Description = "params int[] – 20 args")]
    public int Array_TwentyArgs() =>
        SumWithParamsArray(1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                           11, 12, 13, 14, 15, 16, 17, 18, 19, 20);

    [Benchmark(Description = "params ReadOnlySpan<int> – 20 args")]
    public int Span_TwentyArgs() =>
        SumWithParamsSpan(1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                          11, 12, 13, 14, 15, 16, 17, 18, 19, 20);
}
