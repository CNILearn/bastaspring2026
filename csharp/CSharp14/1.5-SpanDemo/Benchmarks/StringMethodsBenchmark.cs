using BenchmarkDotNet.Attributes;

namespace SpanDemo.Benchmarks;

/// <summary>
/// Compares two approaches for working with character data:
///   - traditional <c>string</c> overloads that create substring allocations
///   - <c>ReadOnlySpan&lt;char&gt;</c> overloads that operate on a slice of the
///     original string without any additional heap allocation
///
/// Scenario: parsing a "timestamp message" log line such as
///   "2026-03-02T08:53:11 Server started on port 8080"
/// and extracting the timestamp and message portions.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class StringMethodsBenchmark
{
    // A representative log line used in all benchmarks
    private const string LogLine = "2026-03-02T08:53:11 Server started on port 8080";

    // -----------------------------------------------------------------------
    // Approach 1 – using string (allocates substrings)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Splits the log line using <c>string</c> operations.
    /// Uses <c>IndexOf</c> to find the split position, then allocates two new
    /// <c>string</c> instances (via slicing/substring) for the timestamp and message.
    /// </summary>
    public static (string Timestamp, string Message) ParseWithString(string line)
    {
        int spaceIndex = line.IndexOf(' ');
        if (spaceIndex < 0)
            return (line, string.Empty);

        // Two new string allocations here
        string timestamp = line[..spaceIndex];
        string message   = line[(spaceIndex + 1)..];
        return (timestamp, message);
    }

    /// <summary>
    /// Checks whether a log line starts with a given prefix using
    /// <c>string.StartsWith</c>.  Internally the BCL already uses spans,
    /// but the public API accepts a <c>string</c>.
    /// </summary>
    public static bool HasPrefixWithString(string line, string prefix) =>
        line.StartsWith(prefix, StringComparison.Ordinal);

    // -----------------------------------------------------------------------
    // Approach 2 – using ReadOnlySpan<char> (zero allocation)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Splits the log line using <c>ReadOnlySpan&lt;char&gt;</c>.
    /// No new string objects are allocated – slicing a span is a pointer-bump.
    ///
    /// <para>
    /// Note: <c>ReadOnlySpan&lt;char&gt;</c> is a ref struct and cannot be stored in
    /// a heap-allocated tuple, so we return the timestamp length as a lightweight
    /// observable result while keeping the benchmark allocation-free.
    /// </para>
    /// </summary>
    public static int ParseWithSpan(ReadOnlySpan<char> line)
    {
        int spaceIndex = line.IndexOf(' ');
        if (spaceIndex < 0)
            return line.Length;

        // Slicing a span is a pointer-bump – no heap allocation
        ReadOnlySpan<char> timestamp = line[..spaceIndex];
        // ReadOnlySpan<char> message = line[(spaceIndex + 1)..]; // further processing would go here
        return timestamp.Length; // return an observable value to prevent dead-code elimination
    }

    /// <summary>
    /// Checks whether a log line starts with a given prefix using
    /// <c>MemoryExtensions.StartsWith</c>, which accepts spans directly.
    /// </summary>
    public static bool HasPrefixWithSpan(ReadOnlySpan<char> line, ReadOnlySpan<char> prefix) =>
        line.StartsWith(prefix, StringComparison.Ordinal);

    // -----------------------------------------------------------------------
    // Benchmarks
    // -----------------------------------------------------------------------

    [Benchmark(Baseline = true, Description = "Parse – string (allocates substrings)")]
    public string ParseString()
    {
        var (ts, _) = ParseWithString(LogLine);
        return ts; // return something to prevent dead-code elimination
    }

    [Benchmark(Description = "Parse – ReadOnlySpan<char> (zero allocation)")]
    public int ParseSpan()
    {
        // ParseWithSpan returns the timestamp length – no string allocated.
        return ParseWithSpan(LogLine);
    }

    [Benchmark(Description = "StartsWith – string")]
    public bool StartsWithString() =>
        HasPrefixWithString(LogLine, "2026");

    [Benchmark(Description = "StartsWith – ReadOnlySpan<char>")]
    public bool StartsWithSpan() =>
        HasPrefixWithSpan(LogLine, "2026");
}
