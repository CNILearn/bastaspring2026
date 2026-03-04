using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace HelloWorldGenerator.Tests;

public static class TestHelper
{
    public static (string GeneratedSource, string[] Diagnostics) RunGenerator(string source)
    {
        // Create the generator
        var generator = new HelloWorldSourceGenerator();

        // Create the compilation
        var parseOptions = new CSharpParseOptions(LanguageVersion.Latest);
        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);
        PortableExecutableReference[] references =
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Threading.Tasks.Task).Assembly.Location),
        ];

        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.ConsoleApplication));

        // Run the generator
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        // Get the generated source
        var runResult = driver.GetRunResult();
        var generatedSource = "";
        if (runResult.Results.Length > 0 && runResult.Results[0].GeneratedSources.Length > 0)
        {
            generatedSource = runResult.Results[0].GeneratedSources[0].SourceText.ToString();
        }

        var diagnosticMessages = diagnostics.Select(d => d.ToString()).ToArray();
        
        return (generatedSource, diagnosticMessages);
    }
}