using System.Runtime.CompilerServices;

namespace SpanDemo.Examples;

/// <summary>
/// Demonstrates the <see cref="OverloadResolutionPriorityAttribute"/> introduced
/// in C# 13 / .NET 9.
///
/// <para>
/// Problem: given two overloads – one accepting <c>string</c> and one accepting
/// <c>ReadOnlySpan&lt;char&gt;</c> – the compiler traditionally prefers the
/// <c>string</c> overload when a <c>string</c> argument is passed, even though
/// the span overload is more efficient (zero-copy).
/// </para>
///
/// <para>
/// Solution: annotate the preferred overload with
/// <c>[OverloadResolutionPriority(1)]</c>.  The compiler now favours it over
/// any overload whose priority is lower (default is 0).  Callers do not need to
/// change their code – they simply pass a <c>string</c> and the compiler
/// automatically routes the call to the span overload.
/// </para>
/// </summary>
public static class OverloadResolutionPriorityExample
{
    public static void Run()
    {
        Console.WriteLine("=== OverloadResolutionPriority Demo ===\n");

        // -------------------------------------------------------------------
        // Part 1 – overload resolution WITHOUT the attribute
        // -------------------------------------------------------------------
        Console.WriteLine("--- Without [OverloadResolutionPriority] ---\n");

        string input = "Hello, BASTA Spring 2026!";

        // The compiler resolves CountUpperCase(string) because it is an exact
        // match for the argument type.  The span overload is not preferred.
        int countViaString = TextAnalyzer.CountUpperCase(input);
        Console.WriteLine($"CountUpperCase(\"{input}\")");
        Console.WriteLine($"  → routed to: string overload");
        Console.WriteLine($"  → upper-case letters: {countViaString}\n");

        // Caller must explicitly cast to use the span overload.
        int countViaSpan = TextAnalyzer.CountUpperCase((ReadOnlySpan<char>)input);
        Console.WriteLine($"CountUpperCase((ReadOnlySpan<char>)\"{input}\")");
        Console.WriteLine($"  → routed to: ReadOnlySpan<char> overload");
        Console.WriteLine($"  → upper-case letters: {countViaSpan}\n");

        // -------------------------------------------------------------------
        // Part 2 – overload resolution WITH the attribute
        // -------------------------------------------------------------------
        Console.WriteLine("--- With [OverloadResolutionPriority(1)] on Span overload ---\n");

        // No cast required.  The compiler prefers the span overload because its
        // priority (1) beats the string overload's default priority (0).
        // A string is implicitly convertible to ReadOnlySpan<char>, so this
        // compiles and runs without any extra allocation.
        int countPriority = TextAnalyzerWithPriority.CountUpperCase(input);
        Console.WriteLine($"CountUpperCase(\"{input}\")");
        Console.WriteLine($"  → routed to: ReadOnlySpan<char> overload (priority = 1)");
        Console.WriteLine($"  → upper-case letters: {countPriority}\n");

        // -------------------------------------------------------------------
        // Part 3 – practical example: log parser
        // -------------------------------------------------------------------
        Console.WriteLine("--- Practical example: LogParser ---\n");

        string[] logLines =
        [
            "2026-03-02T08:00:00 INFO  Application started",
            "2026-03-02T08:01:05 WARN  Disk space below 10 %",
            "2026-03-02T08:05:33 ERROR Connection timeout"
        ];

        foreach (string line in logLines)
        {
            // The compiler picks the ReadOnlySpan<char> overload automatically,
            // thanks to [OverloadResolutionPriority].
            LogEntry entry = LogParser.Parse(line);
            Console.WriteLine($"  [{entry.Level,-5}] {entry.Timestamp}  {entry.Message}");
        }

        Console.WriteLine();
        Console.WriteLine("Key takeaway: callers write natural code (pass a string) while");
        Console.WriteLine("the library transparently uses the zero-allocation span overload.");
    }
}

// =============================================================================
// Helper types used by the demo
// =============================================================================

/// <summary>
/// Text analysis class WITHOUT OverloadResolutionPriority.
/// The compiler favours the string overload when a string argument is passed.
/// </summary>
public static class TextAnalyzer
{
    /// <summary>Counts upper-case letters using a string parameter.</summary>
    public static int CountUpperCase(string text)
    {
        int count = 0;
        foreach (char c in text)
            if (char.IsUpper(c)) count++;
        return count;
    }

    /// <summary>Counts upper-case letters using a ReadOnlySpan parameter.</summary>
    public static int CountUpperCase(ReadOnlySpan<char> text)
    {
        int count = 0;
        foreach (char c in text)
            if (char.IsUpper(c)) count++;
        return count;
    }
}

/// <summary>
/// Text analysis class WITH OverloadResolutionPriority on the span overload.
/// Passing a <c>string</c> now automatically resolves to the span overload.
/// </summary>
public static class TextAnalyzerWithPriority
{
    /// <summary>
    /// String overload – kept for backward compatibility (priority 0, default).
    /// In practice, the compiler will not select this overload when a string
    /// literal or variable is passed, because the span overload has higher priority.
    /// </summary>
    public static int CountUpperCase(string text)
    {
        // Delegates to the span overload to avoid duplicate logic
        return CountUpperCase((ReadOnlySpan<char>)text);
    }

    /// <summary>
    /// Span overload – preferred by the compiler thanks to priority = 1.
    /// Operates directly on the character memory without allocating.
    /// </summary>
    [OverloadResolutionPriority(1)]
    public static int CountUpperCase(ReadOnlySpan<char> text)
    {
        int count = 0;
        foreach (char c in text)
            if (char.IsUpper(c)) count++;
        return count;
    }
}

/// <summary>Parsed representation of a single log line.</summary>
public record LogEntry(string Timestamp, string Level, string Message);

/// <summary>
/// A log-line parser that uses <see cref="OverloadResolutionPriorityAttribute"/>
/// so library users can simply pass a <c>string</c> and get zero-allocation
/// span-based parsing for free.
/// </summary>
public static class LogParser
{
    // Callers pass a string; the compiler routes to the span overload.
    public static LogEntry Parse(string line) => Parse((ReadOnlySpan<char>)line);

    /// <summary>
    /// Core parsing logic operating on a span – no extra string allocations
    /// until the final <c>LogEntry</c> record is constructed.
    ///
    /// Format: "TIMESTAMP LEVEL  MESSAGE"
    /// </summary>
    [OverloadResolutionPriority(1)]
    public static LogEntry Parse(ReadOnlySpan<char> line)
    {
        // Split on first space → timestamp
        int firstSpace = line.IndexOf(' ');
        if (firstSpace < 0)
            return new LogEntry(line.ToString(), "UNKNOWN", string.Empty);

        ReadOnlySpan<char> timestamp = line[..firstSpace];
        ReadOnlySpan<char> rest      = line[(firstSpace + 1)..].TrimStart();

        // Split on next space → log level
        int secondSpace = rest.IndexOf(' ');
        if (secondSpace < 0)
            return new LogEntry(timestamp.ToString(), rest.ToString(), string.Empty);

        ReadOnlySpan<char> level   = rest[..secondSpace];
        ReadOnlySpan<char> message = rest[(secondSpace + 1)..].TrimStart();

        // Only allocate strings once, at the boundary with the object model
        return new LogEntry(timestamp.ToString(), level.ToString(), message.ToString());
    }
}
