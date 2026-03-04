using System;

namespace EditorConfigGenerator.Sample;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== EditorConfig Source Generator Demo ===");
        Console.WriteLine();

        // Test EditorConfig generator
        try
        {
            Console.WriteLine("📋 Configuration Report:");
            var config = EditorConfigGenerator.Sample.Generated.DemoConfigurableClass.GetConfiguration();
            Console.WriteLine(config);
            Console.WriteLine();

            Console.WriteLine("🔄 Processing Demo:");
            var result = EditorConfigGenerator.Sample.Generated.DemoConfigurableClass.ProcessData("Hello World");
            Console.WriteLine($"Processing result: {result}");
            Console.WriteLine();

            Console.WriteLine("✨ Feature Detection:");
            var features = EditorConfigGenerator.Sample.Generated.DemoConfigurableClass.GetFeatures();
            foreach (var feature in features)
            {
                Console.WriteLine($"  • {feature}");
            }
            Console.WriteLine();

            Console.WriteLine("📝 Naming Validation Demo:");
            var testNames = new[] { "ValidName", "invalidName", "valid_name", "Test-Name" };
            
            foreach (var name in testNames)
            {
                var isValid = EditorConfigGenerator.Sample.Generated.DemoValidator.IsValidName(name);
                var status = isValid ? "✓" : "✗";
                Console.WriteLine($"  {status} '{name}' - {(isValid ? "Valid" : "Invalid")} for {EditorConfigGenerator.Sample.Generated.DemoValidator.GetNamingStyle()}");
            }
            Console.WriteLine();

            Console.WriteLine("🎯 Key Achievements:");
            Console.WriteLine("  • Source generator reads .editorconfig settings");
            Console.WriteLine("  • Generated code varies based on configuration");
            Console.WriteLine("  • Multiple generators can coexist");
            Console.WriteLine("  • Build-time code generation with zero runtime dependencies");
            Console.WriteLine("  • Demonstrates best practices for configurable source generators");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EditorConfig generator failed: {ex.Message}");
        }
    }
}