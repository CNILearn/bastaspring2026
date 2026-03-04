using Generated;

namespace HelloWorldGenerator.Sample;

public class SampleBusinessLogic
{
    public string ProcessData(string input)
    {
        return $"Processed: {input}";
    }

    public async Task<int> CalculateAsync(int value)
    {
        await Task.Delay(10);
        return value * 2;
    }

    public void LogMessage(string message)
    {
        Console.WriteLine($"[LOG] {message}");
    }
}

public class DataProcessor
{
    public void ProcessItems(List<string> items)
    {
        foreach (var item in items)
        {
            Console.WriteLine($"Processing: {item}");
        }
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== HelloWorld Source Generator Demo ===");
        Console.WriteLine();

        // Demonstrate the generated HelloWorld method
        Console.WriteLine("1. Testing generated HelloWorld method:");
        var greeting = GeneratedHelloWorld.SayHello();
        Console.WriteLine($"Generated greeting: {greeting}");
        Console.WriteLine();

        // Demonstrate the type introspection method
        Console.WriteLine("2. Testing generated type introspection method:");
        var typesInfo = GeneratedHelloWorld.GetAvailableTypesAndMethods();
        Console.WriteLine("Available types and methods in compilation:");
        Console.WriteLine(typesInfo);
        Console.WriteLine();

        // Demonstrate the type names array
        Console.WriteLine("3. Testing generated type names array:");
        var typeNames = GeneratedHelloWorld.GetTypeNames();
        Console.WriteLine($"Found {typeNames.Length} types:");
        foreach (var typeName in typeNames.Take(5)) // Show first 5
        {
            Console.WriteLine($"  - {typeName}");
        }
        if (typeNames.Length > 5)
        {
            Console.WriteLine($"  ... and {typeNames.Length - 5} more types");
        }
        Console.WriteLine();

        // Use some of the local classes to make them appear in the compilation
        var businessLogic = new SampleBusinessLogic();
        var result = businessLogic.ProcessData("Hello from sample");
        Console.WriteLine($"Sample processing result: {result}");

        var asyncResult = await businessLogic.CalculateAsync(21);
        Console.WriteLine($"Sample async calculation: {asyncResult}");

        businessLogic.LogMessage("Sample app completed successfully");

        var processor = new DataProcessor();
        processor.ProcessItems(new List<string> { "Item1", "Item2", "Item3" });

        Console.WriteLine();
        Console.WriteLine("Demo completed! The source generator automatically created:");
        Console.WriteLine("- GeneratedHelloWorld.SayHello() method");
        Console.WriteLine("- GeneratedHelloWorld.GetAvailableTypesAndMethods() method");
        Console.WriteLine("- GeneratedHelloWorld.GetTypeNames() method");
        Console.WriteLine();
        Console.WriteLine("These methods are available at compile-time without any attributes or configuration!");
    }
}