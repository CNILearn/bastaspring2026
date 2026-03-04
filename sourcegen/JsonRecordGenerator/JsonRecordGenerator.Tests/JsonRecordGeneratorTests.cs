using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace JsonRecordGenerator.Tests;

public class JsonRecordGeneratorTests
{
    [Fact]
    public void GenerateSimpleRecord_ShouldCreateRecordWithProperties()
    {
        // Arrange
        var source = """
            using JsonRecordGenerator.Attributes;
            
            namespace TestNamespace;
            
            [JsonRecord("test.json")]
            public partial class TestRecord
            {
            }
            """;

        var jsonContent = """
            {
              "name": "John",
              "age": 30,
              "isActive": true
            }
            """;

        // Act
        var result = RunGenerator(source, ("test.json", jsonContent));

        // Assert
        var generatedResult = Assert.Single(result.Results); // ensure exactly one generator result
        Assert.NotEmpty(generatedResult.GeneratedSources);

        var generatedSource = Assert.Single(generatedResult.GeneratedSources.Where(s => s.HintName.Contains("TestRecord")));
        
        var generatedCode = generatedSource.SourceText.ToString();
        Assert.Contains("public record class TestRecord", generatedCode);
        Assert.Contains("public string? Name { get; init; }", generatedCode);
        Assert.Contains("public int Age { get; init; }", generatedCode);
        Assert.Contains("public bool IsActive { get; init; }", generatedCode);
    }

    [Fact]
    public void GenerateWithCustomNaming_ShouldRespectNamingConvention()
    {
        // Arrange
        var source = """
            using JsonRecordGenerator.Attributes;
            
            namespace TestNamespace;
            
            [JsonRecord("test.json", PropertyNamingConvention = PropertyNamingConvention.CamelCase)]
            public partial class TestRecord
            {
            }
            """;

        var jsonContent = """
            {
              "first_name": "John",
              "last_name": "Doe"
            }
            """;

        // Act
        var result = RunGenerator(source, ("test.json", jsonContent));

        // Assert
        var generatedResult = Assert.Single(result.Results);
        var generatedSource = Assert.Single(generatedResult.GeneratedSources.Where(s => s.HintName.Contains("TestRecord")));

        var generatedCode = generatedSource.SourceText.ToString();
        Assert.Contains("public string? first_name { get; init; }", generatedCode);
        Assert.Contains("public string? last_name { get; init; }", generatedCode);
    }

    [Fact]
    public void GenerateWithNestedObject_ShouldCreateNestedRecord()
    {
        // Arrange
        var source = """
            using JsonRecordGenerator.Attributes;
            
            namespace TestNamespace;
            
            [JsonRecord("test.json")]
            public partial class TestRecord
            {
            }
            """;

        var jsonContent = """
            {
              "user": {
                "name": "John",
                "email": "john@example.com"
              },
              "settings": {
                "theme": "dark"
              }
            }
            """;

        // Act
        var result = RunGenerator(source, ("test.json", jsonContent));

        // Assert
        var generatedResult = Assert.Single(result.Results);
        var generatedSource = Assert.Single(generatedResult.GeneratedSources.Where(s => s.HintName.Contains("TestRecord")));

        var generatedCode = generatedSource.SourceText.ToString();
        Assert.Contains("public record class TestRecord", generatedCode);
        Assert.Contains("public User? User { get; init; }", generatedCode);
        Assert.Contains("public Settings? Settings { get; init; }", generatedCode);
        Assert.Contains("record class User", generatedCode);
        Assert.Contains("record class Settings", generatedCode);
    }

    private static GeneratorDriverRunResult RunGenerator(string source, params (string fileName, string content)[] additionalFiles)
    {
        var compilation = CreateCompilation(source);
        var generator = new JsonRecordGeneratorImpl();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        if (additionalFiles.Length > 0)
        {
            var additionalTexts = additionalFiles
                .Select(f => (AdditionalText)new InMemoryAdditionalText(f.fileName, f.content))
                .ToImmutableArray();
            
            driver = driver.AddAdditionalTexts(additionalTexts);
        }

        return driver.RunGenerators(compilation).GetRunResult();
    }

    private static Compilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location),
        };

        return CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private class InMemoryAdditionalText : AdditionalText
    {
        private readonly string _text;

        public InMemoryAdditionalText(string path, string text)
        {
            Path = path;
            _text = text;
        }

        public override string Path { get; }

        public override SourceText GetText(System.Threading.CancellationToken cancellationToken = default)
        {
            return SourceText.From(_text);
        }
    }
}