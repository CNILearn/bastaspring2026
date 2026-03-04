using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace EditorConfigGenerator.Tests;

/// <summary>
/// Integration tests for the EditorConfig source generator
/// </summary>
public class EditorConfigSourceGeneratorTests
{
    [Fact]
    public void Generator_WithDefaultSettings_GeneratesExpectedCode()
    {
        // Arrange
        var source = """
            namespace TestProject;
            
            public class TestClass
            {
                public void TestMethod() { }
            }
            """;

        var editorConfig = new Dictionary<string, string>
        {
            ["custom_generator_namespace"] = "Generated",
            ["custom_generator_class_prefix"] = "Generated",
            ["custom_generator_feature_level"] = "basic"
        };

        // Act
        var result = RunGenerator(source, editorConfig);

        // Assert
        Assert.Single(result.GeneratedTrees);
        var generatedCode = result.GeneratedTrees[0].ToString();
        
        // Check that default configuration is applied
        Assert.Contains("namespace Generated;", generatedCode);
        Assert.Contains("public static class GeneratedConfigurableClass", generatedCode);
        Assert.Contains("Logging enabled: True", generatedCode);
        Assert.Contains("Naming style: pascal_case", generatedCode);
        Assert.Contains("Feature level: basic", generatedCode);
    }

    [Fact]
    public void Generator_WithCustomLoggingDisabled_DoesNotGenerateLogger()
    {
        // Arrange
        var source = """
            namespace TestProject;
            
            public class TestClass { }
            """;

        var editorConfig = new Dictionary<string, string>
        {
            ["custom_generator_enable_logging"] = "false",
            ["custom_generator_namespace"] = "Generated",
            ["custom_generator_class_prefix"] = "Generated"
        };

        // Act
        var result = RunGenerator(source, editorConfig);

        // Assert
        var generatedCode = result.GeneratedTrees[0].ToString();
        
        Assert.Contains("Logging enabled: False", generatedCode);
        Assert.DoesNotContain("public static class GeneratedLogger", generatedCode);
        Assert.Contains("// Logging disabled by editorconfig", generatedCode);
    }

    [Fact]
    public void Generator_WithCustomNamingStyle_GeneratesCorrectValidator()
    {
        // Arrange
        var source = """
            namespace TestProject;
            
            public class TestClass { }
            """;

        var editorConfig = new Dictionary<string, string>
        {
            ["custom_generator_naming_style"] = "snake_case",
            ["custom_generator_namespace"] = "Generated",
            ["custom_generator_class_prefix"] = "Generated"
        };

        // Act
        var result = RunGenerator(source, editorConfig);

        // Assert
        var generatedCode = result.GeneratedTrees[0].ToString();
        
        Assert.Contains("Naming style: snake_case", generatedCode);
        Assert.Contains("snake_case (lowercase with underscores)", generatedCode);
        Assert.Contains(@"^[a-z][a-z0-9_]*$", generatedCode);
    }

    [Fact]
    public void Generator_WithEnterpriseFeatureLevel_GeneratesEnterpriseFeatures()
    {
        // Arrange
        var source = """
            namespace TestProject;
            
            public class TestClass { }
            """;

        var editorConfig = new Dictionary<string, string>
        {
            ["custom_generator_feature_level"] = "enterprise",
            ["custom_generator_namespace"] = "Generated",
            ["custom_generator_class_prefix"] = "Generated"
        };

        // Act
        var result = RunGenerator(source, editorConfig);

        // Assert
        var generatedCode = result.GeneratedTrees[0].ToString();
        
        Assert.Contains("Feature level: enterprise", generatedCode);
        Assert.Contains("public static class GeneratedEnterpriseFeatures", generatedCode);
        Assert.Contains("public static class Metrics", generatedCode);
        Assert.Contains("AsParallel()", generatedCode);
    }

    [Fact]
    public void Generator_WithCustomNamespaceAndPrefix_GeneratesCorrectNames()
    {
        // Arrange
        var source = """
            namespace TestProject;
            
            public class TestClass { }
            """;

        var editorConfig = new Dictionary<string, string>
        {
            ["custom_generator_namespace"] = "MyApp.Custom",
            ["custom_generator_class_prefix"] = "Custom"
        };

        // Act
        var result = RunGenerator(source, editorConfig);

        // Assert
        var generatedCode = result.GeneratedTrees[0].ToString();
        
        Assert.Contains("namespace MyApp.Custom;", generatedCode);
        Assert.Contains("public static class CustomConfigurableClass", generatedCode);
        Assert.Contains("public static class CustomLogger", generatedCode);
        Assert.Contains("public static class CustomValidator", generatedCode);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("1", true)]
    [InlineData("0", false)]
    [InlineData("invalid", true)] // default value
    public void Generator_WithVariousLoggingValues_ParsesCorrectly(string configValue, bool expectedLogging)
    {
        // Arrange
        var source = """
            namespace TestProject;
            
            public class TestClass { }
            """;

        var editorConfig = new Dictionary<string, string>
        {
            ["custom_generator_enable_logging"] = configValue,
            ["custom_generator_namespace"] = "Generated",
            ["custom_generator_class_prefix"] = "Generated"
        };

        // Act
        var result = RunGenerator(source, editorConfig);

        // Assert
        var generatedCode = result.GeneratedTrees[0].ToString();
        Assert.Contains($"Logging enabled: {expectedLogging}", generatedCode);
    }

    [Theory]
    [InlineData("pascal_case", "PascalCase")]
    [InlineData("camel_case", "camelCase")]
    [InlineData("snake_case", "snake_case")]
    public void Generator_WithDifferentNamingStyles_GeneratesCorrectDescriptions(string style, string description)
    {
        // Arrange
        var source = """
            namespace TestProject;
            
            public class TestClass { }
            """;

        var editorConfig = new Dictionary<string, string>
        {
            ["custom_generator_naming_style"] = style,
            ["custom_generator_namespace"] = "Generated",
            ["custom_generator_class_prefix"] = "Generated"
        };

        // Act
        var result = RunGenerator(source, editorConfig);

        // Assert
        var generatedCode = result.GeneratedTrees[0].ToString();
        Assert.Contains(description, generatedCode);
    }

    private static GeneratorRunResult RunGenerator(string source, Dictionary<string, string>? editorConfig = null)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .ToArray();

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new EditorConfigSourceGenerator();

        // Create analyzer config options if provided
        var configOptions = editorConfig != null
            ? new TestAnalyzerConfigOptionsProvider(editorConfig)
            : new TestAnalyzerConfigOptionsProvider(new Dictionary<string, string>());

        var driver = CSharpGeneratorDriver.Create(generator)
            .WithUpdatedAnalyzerConfigOptions(configOptions);

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        return new GeneratorRunResult(
            outputCompilation.SyntaxTrees.Skip(1).ToArray(), // Skip original source
            diagnostics.ToArray()
        );
    }

    private record GeneratorRunResult(SyntaxTree[] GeneratedTrees, Diagnostic[] Diagnostics);

    /// <summary>
    /// Test implementation of AnalyzerConfigOptionsProvider for unit testing
    /// </summary>
    private class TestAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
    {
        private readonly TestAnalyzerConfigOptions _globalOptions;

        public TestAnalyzerConfigOptionsProvider(Dictionary<string, string> options)
        {
            _globalOptions = new TestAnalyzerConfigOptions(options);
        }

        public override AnalyzerConfigOptions GlobalOptions => _globalOptions;

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => _globalOptions;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => _globalOptions;
    }

    /// <summary>
    /// Test implementation of AnalyzerConfigOptions for unit testing
    /// </summary>
    private class TestAnalyzerConfigOptions : AnalyzerConfigOptions
    {
        private readonly Dictionary<string, string> _options;

        public TestAnalyzerConfigOptions(Dictionary<string, string> options)
        {
            _options = options;
        }

        public override bool TryGetValue(string key, out string value)
        {
            return _options.TryGetValue(key, out value!);
        }

        public override IEnumerable<string> Keys => _options.Keys;
    }
}