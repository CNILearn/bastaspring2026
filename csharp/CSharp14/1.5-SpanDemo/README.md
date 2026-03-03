# SpanDemo

Demonstrates the advantages of `Span<T>` and `ReadOnlySpan<T>` in C# 13/14,
comparing them with traditional `params` arrays, and showing how
`[OverloadResolutionPriority]` lets you guide callers to the most efficient overload.

## What's Demonstrated

1. **`params` array vs `params ReadOnlySpan<T>`** – allocation differences  
2. **`string` methods vs `ReadOnlySpan<char>` methods** – zero-copy text processing  
3. **`[OverloadResolutionPriority]`** – steering the compiler to preferred overloads  

## Running the Demo

### Interactive demo (no benchmark runner needed)

```bash
cd samples/csharp14/SpanDemo
dotnet run
```

### BenchmarkDotNet benchmarks (Release build required)

```bash
cd samples/csharp14/SpanDemo
dotnet run --configuration Release -- --benchmark
```

> **Note**: benchmarks take several minutes.  
> Pre-computed results are in [`BenchmarkResults/SpanDemo-BenchmarkResults.md`](../../../BenchmarkResults/SpanDemo-BenchmarkResults.md).

## Project Structure

```
SpanDemo/
├── SpanDemo.csproj
├── Program.cs                              ← entry point
├── Benchmarks/
│   ├── ParamsVsSpanBenchmark.cs            ← params int[] vs params ReadOnlySpan<int>
│   └── StringMethodsBenchmark.cs          ← string vs ReadOnlySpan<char>
└── Examples/
    ├── ParamsVsSpanExample.cs              ← interactive demo #1
    ├── StringVsSpanExample.cs              ← interactive demo #2
    └── OverloadResolutionPriorityExample.cs ← interactive demo #3
```

## Key Examples

### 1. `params ReadOnlySpan<int>` – zero heap allocation

```csharp
// Traditional: new int[] allocated on the heap every call
public static int SumArray(params int[] values) { ... }

// C# 13+: compiler emits a stack-resident inline array – 0 bytes allocated
public static int SumSpan(params ReadOnlySpan<int> values) { ... }

// Call sites look identical
int a = SumArray(1, 2, 3, 4, 5);  // allocates ~40 B
int b = SumSpan (1, 2, 3, 4, 5);  // allocates   0 B
```

### 2. Zero-copy string parsing

```csharp
// string – allocates two new strings
public static (string Name, int Age, string Email) ParseCsvWithString(string line)
{
    string[] parts = line.Split(',');   // allocates string[] + three strings
    return (parts[0], int.Parse(parts[1]), parts[2]);
}

// ReadOnlySpan<char> – zero intermediate allocations
public static (string Name, int Age, string Email) ParseCsvWithSpan(ReadOnlySpan<char> line)
{
    int comma1 = line.IndexOf(',');
    ReadOnlySpan<char> namePart = line[..comma1];        // pointer bump
    ReadOnlySpan<char> agePart  = ...;                   // pointer bump
    int age = int.Parse(agePart, CultureInfo.InvariantCulture); // no string needed
    return (namePart.ToString(), age, emailPart.ToString()); // allocate only at boundary
}
```

### 3. `[OverloadResolutionPriority]`

```csharp
public static class TextAnalyzer
{
    // Priority 0 (default) – kept for backward compatibility
    public static int CountUpperCase(string text) => CountUpperCase((ReadOnlySpan<char>)text);

    // Priority 1 – compiler prefers this overload even when a string is passed
    [OverloadResolutionPriority(1)]
    public static int CountUpperCase(ReadOnlySpan<char> text) { ... }
}

// Caller writes natural code; compiler routes to the span overload automatically
int count = TextAnalyzer.CountUpperCase(someString); // → span overload, 0 allocations
```

## Benchmark Results

See [`BenchmarkResults/SpanDemo-BenchmarkResults.md`](../../../BenchmarkResults/SpanDemo-BenchmarkResults.md)
for full BenchmarkDotNet output with timing and allocation numbers.

| Scenario | Method | Mean | Allocated |
|---|---|---:|---:|
| params – 5 args | `params int[]` | ~2.5 ns | 40 B |
| params – 5 args | `params ReadOnlySpan<int>` | ~0.8 ns | 0 B |
| Parse log line | `string` | ~85 ns | 144 B |
| Parse log line | `ReadOnlySpan<char>` | ~35 ns | 0 B |

## Key Takeaways

- **Use `params ReadOnlySpan<T>`** in hot paths to eliminate the per-call array allocation
- **Use `ReadOnlySpan<char>`** for parsers and formatters to avoid intermediate string objects
- **Use `[OverloadResolutionPriority]`** to expose a clean API that callers use naturally
  while the library transparently uses the most efficient overload
- Spans are **stack-only** – they cannot be stored in fields or used in `async` methods;
  use `Memory<T>` / `ReadOnlyMemory<T>` for those scenarios

## Related Documentation

- [Implicit Span Conversions (C# 14)](../../../docs/csharp14/ImplicitSpanConversions.md)
- [SpanDemo Write-up](../../../docs/csharp14/SpanDemo.md)
- [Microsoft Docs – Span&lt;T&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.span-1)
- [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet)
