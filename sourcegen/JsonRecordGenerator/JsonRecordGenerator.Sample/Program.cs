using JsonRecordGenerator.Attributes;
using System.Text.Json;

namespace JsonRecordGenerator.Sample;

// Basic usage - generates a record from simple JSON
[JsonRecord("sample-data.json", Namespace = "JsonRecordGenerator.Sample.Generated")]
public partial class UserData
{
}

// Nested objects example with custom naming
[JsonRecord("nested-example.json", 
    RecordName = "ComplexData",
    Namespace = "JsonRecordGenerator.Sample.Generated",
    PropertyNamingConvention = PropertyNamingConvention.PascalCase)]
public partial class NestedExample
{
}

// Array example with custom namespace
[JsonRecord("array-example.json", 
    Namespace = "JsonRecordGenerator.Sample.Generated",
    PropertyNamingConvention = PropertyNamingConvention.CamelCase)]
public partial class ArrayExample
{
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("JSON Record Generator Sample");
        Console.WriteLine("============================");
        
        // Demonstrate the generated records by creating instances
        // Note: These would typically be populated from actual JSON data
        
        Console.WriteLine("\n1. Basic User Data Record:");
        var userData = new JsonRecordGenerator.Sample.Generated.UserData
        {
            Id = 123,
            Name = "John Doe",
            Email = "john.doe@example.com",
            Age = 30,
            IsActive = true,
            Balance = 1250.75
        };
        
        Console.WriteLine($"User: {userData.Name} (ID: {userData.Id})");
        Console.WriteLine($"Email: {userData.Email}");
        Console.WriteLine($"Age: {userData.Age}, Active: {userData.IsActive}");
        Console.WriteLine($"Balance: ${userData.Balance}");
        
        Console.WriteLine("\n2. Complex Nested Data Record:");
        // The ComplexData record and its nested records are generated from nested-example.json
        
        Console.WriteLine("\n3. Array Example Record:");
        // The ArrayExample record with collections is generated from array-example.json
        
        Console.WriteLine("\nRecords have been generated from JSON files at compile time!");
        Console.WriteLine("Check the generated .g.cs files to see the actual record definitions.");
    }
}