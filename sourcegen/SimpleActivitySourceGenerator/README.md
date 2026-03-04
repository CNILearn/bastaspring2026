# SimpleActivitySourceGenerator

A straightforward C# source generator that creates wrapper methods for automatic activity tracing using `System.Diagnostics.ActivitySource`. This is the simpler approach that generates separate wrapper classes.

## Features

- **Automatic Activity Creation**: Methods marked with `[Activity]` get automatic activity tracing
- **Wrapper Method Generation**: Creates separate wrapper classes for instrumented code
- **Exception Handling**: Captures and records exceptions as activity errors
- **Async/Await Support**: Full support for asynchronous methods
- **Zero Runtime Dependencies**: Pure compile-time generation with no runtime overhead
- **OpenTelemetry Compatible**: Uses standard `ActivitySource` and `Activity` APIs

## How It Works

The simple generator creates wrapper methods that you call instead of the original methods. This approach maintains clear separation between original and instrumented code.

## Usage

### 1. Mark Methods with [Activity] Attribute

```csharp
using ActivitySourceGenerator.Attributes;

public class BusinessService
{
    [Activity]
    public static string ProcessData(string input)
    {
        // Your business logic here
        return $"Processed: {input}";
    }

    [Activity(ActivityName = "CustomOperation")]
    public static async Task<int> ProcessAsync(string data)
    {
        await Task.Delay(100);
        return data.Length;
    }
}
```

### 2. Use Generated Wrapper Methods

```csharp
// Use generated wrapper methods for automatic tracing
var result = BusinessServiceActivityWrapper.ProcessDataWithActivity("Hello World");
var length = await BusinessServiceActivityWrapper.ProcessAsyncWithActivity("Test Data");
```

## Generated Code Example

The simple generator creates wrapper classes with activity instrumentation:

```csharp
// Auto-generated wrapper class
public static class BusinessServiceActivityWrapper
{
    private static readonly ActivitySource _activitySource = new("BusinessService");

    public static string ProcessDataWithActivity(string input)
    {
        using var activity = _activitySource.StartActivity("ProcessData");
        try
        {
            activity?.SetTag("method.name", "ProcessData");
            activity?.SetTag("class.name", "BusinessService");
            var result = BusinessService.ProcessData(input);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("error.type", ex.GetType().Name);
            activity?.SetTag("error.message", ex.Message);
            throw;
        }
    }
}
```

## Configuration

### ActivityAttribute Properties

- **ActivitySourceName**: Custom name for the ActivitySource (defaults to class name)
- **ActivityName**: Custom name for the activity (defaults to method name)
- **RecordExceptions**: Whether to record exceptions (default: true)
- **Tags**: Custom tags in "key1=value1;key2=value2" format

### Example with Custom Configuration

```csharp
[Activity(
    ActivitySourceName = "MyApplication.Orders", 
    ActivityName = "ProcessOrder",
    RecordExceptions = true,
    Tags = "component=order-service;version=1.0"
)]
public static void ProcessOrder(Order order)
{
    // Processing logic
}
```

## Installation

1. Add the source generator to your project:

```xml
<ItemGroup>
  <ProjectReference Include="path/to/SimpleActivitySourceGenerator.csproj" 
                    OutputItemType="Analyzer" 
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

2. Use the `[Activity]` attribute on your methods
3. Call the generated wrapper methods for automatic activity tracing

## Integration with OpenTelemetry

This generator works seamlessly with OpenTelemetry:

```csharp
using OpenTelemetry;
using OpenTelemetry.Trace;

// Configure OpenTelemetry
using var tracerProvider = TracerProvider.CreateBuilder()
    .AddSource("BusinessService")  // Add your ActivitySource
    .AddConsoleExporter()
    .Build();

// Configure ActivityListener to observe activities
using var listener = new ActivityListener
{
    ShouldListenTo = _ => true,
    Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
    ActivityStarted = activity => Console.WriteLine($"Started: {activity.DisplayName}"),
    ActivityStopped = activity => Console.WriteLine($"Stopped: {activity.DisplayName} - Status: {activity.Status}")
};

ActivitySource.AddActivityListener(listener);

// Use generated wrapper methods for automatic tracing
var result = BusinessServiceActivityWrapper.ProcessDataWithActivity("Hello World");
```

## Sample Output

```
🔄 Started: ProcessData | Source: BusinessService
✅ Stopped: ProcessData | Status: Ok | Duration: 101.28ms

🔄 Started: ProcessAsync | Source: BusinessService  
✅ Stopped: ProcessAsync | Status: Ok | Duration: 202.23ms

🔄 Started: ProcessData | Source: BusinessService
✅ Stopped: ProcessData | Status: Error | Duration: 102.73ms
```

## Requirements

- **.NET 8.0** or later 
- **C# 12.0** or later for modern language features
- **MSBuild-based projects** (.NET SDK style)

## Benefits

- **Standardized Tracing**: Consistent activity creation across your application
- **Reduced Boilerplate**: No need to manually create activities
- **Exception Safety**: Automatic exception handling and recording
- **Performance**: Zero runtime overhead, compile-time code generation  
- **OpenTelemetry Ready**: Works with any OpenTelemetry-compatible tracing system
- **Clear Separation**: Wrapper approach maintains separation between original and instrumented code

## Examples Repository

Check out the [SimpleActivitySourceGenerator.Sample](SimpleActivitySourceGenerator.Sample/) project for complete working examples demonstrating all features.

---

Part of the [sourcegenerators-samples](../../README.md) repository demonstrating advanced C# source generator techniques.