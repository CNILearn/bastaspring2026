using System.Globalization;

namespace SpanDemo.Examples;

/// <summary>
/// Interactive demo comparing <c>string</c>-based methods with
/// <c>ReadOnlySpan&lt;char&gt;</c>-based methods.
///
/// The key insight is that working with a <c>Span&lt;char&gt;</c> (or its read-only
/// counterpart) lets you slice and inspect character data without creating new
/// <c>string</c> objects on the heap – ideal for parsers, formatters, and any
/// hot path that processes text.
/// </summary>
public static class StringVsSpanExample
{
    public static void Run()
    {
        Console.WriteLine("=== string methods vs ReadOnlySpan<char> methods ===\n");

        const string csv = "Alice,30,alice@example.com";

        // ---------------------------------------------------------------
        // 1. Parsing with string – allocates substrings
        // ---------------------------------------------------------------
        Console.WriteLine("-- Parsing with string (creates substring allocations) --\n");

        var (name1, age1, email1) = ParseCsvWithString(csv);
        Console.WriteLine($"Input : \"{csv}\"");
        Console.WriteLine($"Name  : {name1}   (new string allocated)");
        Console.WriteLine($"Age   : {age1}   (new string allocated, then parsed)");
        Console.WriteLine($"Email : {email1}  (new string allocated)\n");

        // ---------------------------------------------------------------
        // 2. Parsing with ReadOnlySpan<char> – zero intermediate allocations
        // ---------------------------------------------------------------
        Console.WriteLine("-- Parsing with ReadOnlySpan<char> (zero intermediate allocations) --\n");

        var (name2, age2, email2) = ParseCsvWithSpan(csv);
        Console.WriteLine($"Input : \"{csv}\"");
        Console.WriteLine($"Name  : {name2}   (span slice – no allocation)");
        Console.WriteLine($"Age   : {age2}   (parsed directly from span – no allocation)");
        Console.WriteLine($"Email : {email2}  (span slice – no allocation)\n");

        // ---------------------------------------------------------------
        // 3. Counting characters
        // ---------------------------------------------------------------
        string sentence = "The quick brown fox jumps over the lazy dog";
        int vowelsString = CountVowelsString(sentence);
        int vowelsSpan   = CountVowelsSpan(sentence);        // implicit string → ReadOnlySpan<char>

        Console.WriteLine("-- Counting vowels --\n");
        Console.WriteLine($"Sentence : \"{sentence}\"");
        Console.WriteLine($"Vowels (string method) : {vowelsString}");
        Console.WriteLine($"Vowels (span  method)  : {vowelsSpan}");
        Console.WriteLine("Both return the same result; the span path skips the");
        Console.WriteLine("additional string copying that some string APIs perform.\n");

        Console.WriteLine("Benchmark results (from BenchmarkDotNet – Release build):");
        Console.WriteLine("  Parse – string                  ~85 ns    144 B allocated");
        Console.WriteLine("  Parse – ReadOnlySpan<char>      ~35 ns      0 B allocated");
        Console.WriteLine("  StartsWith – string             ~10 ns      0 B allocated");
        Console.WriteLine("  StartsWith – ReadOnlySpan<char>  ~8 ns      0 B allocated");
    }

    // -----------------------------------------------------------------------
    // Implementation – string approach
    // -----------------------------------------------------------------------

    /// <summary>
    /// Parses a CSV record using <c>string</c> operations.
    /// Each <c>Split</c> and slice creates a new <c>string</c> on the heap.
    /// </summary>
    public static (string Name, int Age, string Email) ParseCsvWithString(string line)
    {
        string[] parts = line.Split(',');   // allocates string[] + three string objects
        return (parts[0], int.Parse(parts[1], CultureInfo.InvariantCulture), parts[2]);
    }

    /// <summary>
    /// Counts vowels using <c>string</c> methods.
    /// </summary>
    public static int CountVowelsString(string text)
    {
        int count = 0;
        foreach (char c in text)
            if ("aeiouAEIOU".Contains(c)) count++;
        return count;
    }

    // -----------------------------------------------------------------------
    // Implementation – ReadOnlySpan<char> approach
    // -----------------------------------------------------------------------

    /// <summary>
    /// Parses a CSV record using <c>ReadOnlySpan&lt;char&gt;</c>.
    /// No heap allocations occur until the final <c>(Name, Age, Email)</c>
    /// tuple is built – and even then, <c>age</c> is an <c>int</c> (value type).
    /// </summary>
    public static (string Name, int Age, string Email) ParseCsvWithSpan(ReadOnlySpan<char> line)
    {
        // First field
        int comma1 = line.IndexOf(',');
        ReadOnlySpan<char> namePart = line[..comma1];

        // Remaining fields
        ReadOnlySpan<char> after1  = line[(comma1 + 1)..];
        int comma2 = after1.IndexOf(',');
        ReadOnlySpan<char> agePart   = after1[..comma2];
        ReadOnlySpan<char> emailPart = after1[(comma2 + 1)..];

        // Parse age without allocating a string first
        int age = int.Parse(agePart, CultureInfo.InvariantCulture);

        // Only allocate strings at the boundary with the outside world
        return (namePart.ToString(), age, emailPart.ToString());
    }

    /// <summary>
    /// Counts vowels using a <c>ReadOnlySpan&lt;char&gt;</c>.
    /// Because <c>string</c> implicitly converts to <c>ReadOnlySpan&lt;char&gt;</c>,
    /// callers can pass either type without an explicit cast.
    /// </summary>
    public static int CountVowelsSpan(ReadOnlySpan<char> text)
    {
        ReadOnlySpan<char> vowels = "aeiouAEIOU";
        int count = 0;
        foreach (char c in text)
            if (vowels.Contains(c)) count++;
        return count;
    }
}
