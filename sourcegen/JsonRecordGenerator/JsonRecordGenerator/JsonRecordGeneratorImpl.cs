using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using System.Collections.Immutable;
using System.Text;
using System.Text.Json;

namespace JsonRecordGenerator;

[Generator]
public class JsonRecordGeneratorImpl : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register the JsonRecordAttribute only once
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "JsonRecordAttribute.g.cs", SourceText.From(JsonRecordAttributeSource, Encoding.UTF8)));

        // Find classes with JsonRecordAttribute
        var classesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m != null)
            .Collect();

        // Combine with additional files (JSON files)
        var combinedProvider = classesProvider.Combine(context.AdditionalTextsProvider.Collect());

        // Generate records (not the attribute)
        context.RegisterSourceOutput(combinedProvider, ExecuteRecordGeneration);
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        // Look for classes with attributes
        return node is ClassDeclarationSyntax classDeclaration &&
               classDeclaration.AttributeLists.Count > 0;
    }

    private static ClassInfo? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

        if (classSymbol is null)
            return null;

        // Check for JsonRecordAttribute
        var jsonRecordAttribute = classSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == "JsonRecordAttribute");

        if (jsonRecordAttribute is null)
            return null;

        return new ClassInfo(classSymbol, classDeclaration, jsonRecordAttribute);
    }

    private static void ExecuteRecordGeneration(SourceProductionContext context, (ImmutableArray<ClassInfo?> Left, ImmutableArray<AdditionalText> Right) source)
    {
        var classes = source.Left.Where(c => c != null).Cast<ClassInfo>().ToImmutableArray();
        var additionalFiles = source.Right;
        
        if (classes.IsDefaultOrEmpty)
            return;

        foreach (var classInfo in classes)
        {
            try
            {
                var sourceCode = GenerateRecordFromJson(classInfo, additionalFiles, context);
                if (!string.IsNullOrEmpty(sourceCode))
                {
                    var fileName = $"{classInfo.Symbol.Name}.g.cs";
                    context.AddSource(fileName, SourceText.From(sourceCode, Encoding.UTF8));
                }
            }
            catch (System.Exception ex)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("JRG001", "JSON Record Generation Error",
                        $"Error generating record for {classInfo.Symbol.Name}: {ex.Message}",
                        "JsonRecordGenerator", DiagnosticSeverity.Error, true),
                    classInfo.Syntax.GetLocation()));
            }
        }
    }

    private static string GenerateRecordFromJson(ClassInfo classInfo, ImmutableArray<AdditionalText> additionalFiles, SourceProductionContext context)
    {
        // Get JSON file path from attribute
        var jsonFileName = GetJsonFileName(classInfo.AttributeData);
        if (string.IsNullOrEmpty(jsonFileName))
            return string.Empty;

        // Find the JSON file in additional files
        var jsonFile = additionalFiles.FirstOrDefault(f => 
            Path.GetFileName(f.Path).Equals(jsonFileName, System.StringComparison.OrdinalIgnoreCase) ||
            f.Path.EndsWith(jsonFileName, System.StringComparison.OrdinalIgnoreCase));

        if (jsonFile is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("JRG002", "JSON File Not Found",
                    $"JSON file '{jsonFileName}' not found in additional files",
                    "JsonRecordGenerator", DiagnosticSeverity.Error, true),
                classInfo.Syntax.GetLocation()));
            return string.Empty;
        }

        // Read and parse JSON
        var jsonText = jsonFile.GetText(context.CancellationToken)?.ToString();
        if (string.IsNullOrWhiteSpace(jsonText))
            return string.Empty;

        JsonDocument? jsonDocument = null;
        try
        {
            jsonDocument = JsonDocument.Parse(jsonText!);
        }
        catch (JsonException ex)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("JRG003", "JSON Parse Error",
                    $"Error parsing JSON file '{jsonFileName}': {ex.Message}",
                    "JsonRecordGenerator", DiagnosticSeverity.Error, true),
                classInfo.Syntax.GetLocation()));
            return string.Empty;
        }

        // Generate the record
        var config = GetGenerationConfig(classInfo.AttributeData);
        var recordName = config.RecordName ?? classInfo.Symbol.Name;
        var namespaceName = config.Namespace ?? classInfo.Symbol.ContainingNamespace.ToDisplayString();

        var recordSource = GenerateRecordSource(recordName, jsonDocument.RootElement, namespaceName, config);
        
        jsonDocument.Dispose();
        return recordSource;
    }

    private static string GenerateRecordSource(string recordName, JsonElement jsonElement, string namespaceName, GenerationConfig config)
    {
        var properties = new StringBuilder();
        var nestedRecords = new StringBuilder();

        foreach (var property in jsonElement.EnumerateObject())
        {
            var propertyName = GetPropertyName(property.Name, config.PropertyNamingConvention);
            var (propertyType, nestedRecordDefinition) = GetPropertyType(property.Value, propertyName, config);
            
            properties.AppendLine($"    public {propertyType} {propertyName} {{ get; init; }}");
            
            if (!string.IsNullOrEmpty(nestedRecordDefinition))
            {
                nestedRecords.AppendLine(nestedRecordDefinition);
            }
        }

        var accessModifier = config.AccessModifier ?? "public";
        
        return $$"""
            // <auto-generated/>
            #nullable enable
            using System;
            using System.Collections.Generic;

            namespace {{namespaceName}};

            {{accessModifier}} record class {{recordName}}
            {
            {{properties}}
            }

            {{nestedRecords}}
            """;
    }

    private static (string type, string? nestedRecord) GetPropertyType(JsonElement element, string propertyName, GenerationConfig config)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => ("string?", null),
            JsonValueKind.Number => element.TryGetInt32(out _) ? ("int", null) : ("double", null),
            JsonValueKind.True or JsonValueKind.False => ("bool", null),
            JsonValueKind.Array => HandleArrayType(element, propertyName, config),
            JsonValueKind.Object => HandleObjectType(element, propertyName, config),
            JsonValueKind.Null => ("object?", null),
            _ => ("object?", null)
        };
    }

    private static (string type, string? nestedRecord) HandleArrayType(JsonElement arrayElement, string propertyName, GenerationConfig config)
    {
        if (arrayElement.GetArrayLength() == 0)
            return ("IReadOnlyList<object?>?", null);

        var firstElement = arrayElement.EnumerateArray().FirstOrDefault();
        var (elementType, nestedRecord) = GetPropertyType(firstElement, propertyName, config);
        
        return ($"IReadOnlyList<{elementType}>?", nestedRecord);
    }

    private static (string type, string? nestedRecord) HandleObjectType(JsonElement objectElement, string propertyName, GenerationConfig config)
    {
        var nestedRecordName = GetPropertyName(propertyName, PropertyNamingConvention.PascalCase);
        var nestedRecordSource = GenerateRecordSource(nestedRecordName, objectElement, string.Empty, config);
        
        // Remove namespace declaration for nested records
        var lines = nestedRecordSource.Split('\n');
        var recordStart = System.Array.FindIndex(lines, l => l.Contains("record"));
        var recordEnd = lines.Length;
        
        var cleanedRecord = string.Join("\n", lines.Skip(recordStart).Take(recordEnd - recordStart));
        
        return ($"{nestedRecordName}?", cleanedRecord);
    }

    private static string GetPropertyName(string jsonPropertyName, PropertyNamingConvention convention)
    {
        return convention switch
        {
            PropertyNamingConvention.PascalCase => ToPascalCase(jsonPropertyName),
            PropertyNamingConvention.CamelCase => ToCamelCase(jsonPropertyName),
            PropertyNamingConvention.KeepOriginal => jsonPropertyName,
            _ => ToPascalCase(jsonPropertyName)
        };
    }

    private static string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        
        return char.ToUpperInvariant(input[0]) + input.Substring(1);  // range operator not available in netstandard2.0
    }

    private static string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        
        return char.ToLowerInvariant(input[0]) + input.Substring(1); // range operator not available in netstandard2.0
    }

    private static string? GetJsonFileName(AttributeData attributeData)
    {
        if (attributeData.ConstructorArguments.Length > 0)
        {
            return attributeData.ConstructorArguments[0].Value?.ToString();
        }
        return null;
    }

    private static GenerationConfig GetGenerationConfig(AttributeData attributeData)
    {
        var config = new GenerationConfig();
        
        foreach (var namedArg in attributeData.NamedArguments)
        {
            switch (namedArg.Key)
            {
                case "Namespace":
                    config.Namespace = namedArg.Value.Value?.ToString();
                    break;
                case "RecordName":
                    config.RecordName = namedArg.Value.Value?.ToString();
                    break;
                case "PropertyNamingConvention":
                    if (namedArg.Value.Value is int enumValue)
                    {
                        config.PropertyNamingConvention = (PropertyNamingConvention)enumValue;
                    }
                    break;
                case "AccessModifier":
                    config.AccessModifier = namedArg.Value.Value?.ToString();
                    break;
            }
        }
        
        return config;
    }

    private class ClassInfo
    {
        public INamedTypeSymbol Symbol { get; }
        public ClassDeclarationSyntax Syntax { get; }
        public AttributeData AttributeData { get; }

        public ClassInfo(INamedTypeSymbol symbol, ClassDeclarationSyntax syntax, AttributeData attributeData)
        {
            Symbol = symbol;
            Syntax = syntax;
            AttributeData = attributeData;
        }
    }

    private class GenerationConfig
    {
        public string? Namespace { get; set; }
        public string? RecordName { get; set; }
        public PropertyNamingConvention PropertyNamingConvention { get; set; } = PropertyNamingConvention.PascalCase;
        public string? AccessModifier { get; set; }
    }

    private enum PropertyNamingConvention
    {
        PascalCase = 0,
        CamelCase = 1,
        KeepOriginal = 2
    }

    private const string JsonRecordAttributeSource = @"// <auto-generated/>
#nullable enable
using System;

namespace JsonRecordGenerator.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class JsonRecordAttribute : Attribute
{
    public JsonRecordAttribute(string jsonFileName)
    {
        JsonFileName = jsonFileName;
    }

    public string JsonFileName { get; }
    public string? Namespace { get; set; }
    public string? RecordName { get; set; }
    public PropertyNamingConvention PropertyNamingConvention { get; set; } = PropertyNamingConvention.PascalCase;
    public string? AccessModifier { get; set; }
}

public enum PropertyNamingConvention
{
    PascalCase = 0,
    CamelCase = 1,
    KeepOriginal = 2
}";
}