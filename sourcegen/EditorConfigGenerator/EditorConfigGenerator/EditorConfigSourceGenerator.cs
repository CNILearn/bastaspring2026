using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using System.Text;

namespace EditorConfigGenerator;

/// <summary>
/// A source generator that demonstrates reading .editorconfig settings
/// and generating code based on those configuration values.
/// </summary>
[Generator]
public class EditorConfigSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // EditorConfig-based generator - single registration
        var configProvider = context.AnalyzerConfigOptionsProvider;
        var compilationProvider = context.CompilationProvider;
        var combinedProvider = compilationProvider.Combine(configProvider);
        
        context.RegisterSourceOutput(combinedProvider, static (spc, combined) => 
        {
            var (compilation, configOptions) = combined;
            ExecuteWithEditorConfig(spc, compilation, configOptions);
        });
    }



    private static void ExecuteWithEditorConfig(SourceProductionContext context, Compilation compilation, AnalyzerConfigOptionsProvider configOptions)
    {
        try
        {
            // Read configuration from .editorconfig
            var config = ReadEditorConfig(configOptions);
            
            // Generate the main code based on configuration
            var sourceCode = GenerateConfigurableCode(config);
            context.AddSource("EditorConfigGenerated.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
        }
        catch
        {
            // If reading EditorConfig fails, generate with default settings
            var defaultConfig = new EditorConfigSettings(
                enableLogging: true,
                namingStyle: "pascal_case", 
                featureLevel: "advanced",
                customNamespace: "EditorConfigGenerator.Sample.Generated",
                classPrefix: "Demo"
            );
            
            var sourceCode = GenerateConfigurableCode(defaultConfig);
            context.AddSource("EditorConfigGenerated.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
        }
    }



    private static EditorConfigSettings ReadEditorConfig(AnalyzerConfigOptionsProvider configOptions)
    {
        // Try to read actual EditorConfig settings, but with safe fallbacks
        try
        {
            var globalOptions = configOptions.GlobalOptions;
            
            // Try to read each setting with fallbacks
            globalOptions.TryGetValue("custom_generator_enable_logging", out var enableLogging);
            globalOptions.TryGetValue("custom_generator_naming_style", out var namingStyle);
            globalOptions.TryGetValue("custom_generator_feature_level", out var featureLevel);
            globalOptions.TryGetValue("custom_generator_namespace", out var customNamespace);
            globalOptions.TryGetValue("custom_generator_class_prefix", out var classPrefix);
            
            // For the sample project, use Demo prefix and correct namespace if custom values aren't found
            var effectiveNamespace = customNamespace ?? "EditorConfigGenerator.Sample.Generated";
            var effectivePrefix = classPrefix ?? "Demo";
            
            return new EditorConfigSettings(
                enableLogging: ParseBool(enableLogging, defaultValue: true),
                namingStyle: namingStyle ?? "pascal_case",
                featureLevel: featureLevel ?? "advanced", // Use advanced for sample
                customNamespace: effectiveNamespace,
                classPrefix: effectivePrefix
            );
        }
        catch
        {
            // If anything fails, return safe defaults for sample project
            return new EditorConfigSettings(
                enableLogging: true,
                namingStyle: "pascal_case",
                featureLevel: "advanced",
                customNamespace: "EditorConfigGenerator.Sample.Generated",
                classPrefix: "Demo"
            );
        }
    }

    private static bool ParseBool(string? value, bool defaultValue)
    {
        if (string.IsNullOrEmpty(value))
            return defaultValue;
            
        return value!.ToLowerInvariant() switch
        {
            "true" => true,
            "false" => false,
            "1" => true,
            "0" => false,
            _ => defaultValue
        };
    }

    private static string GenerateConfigurableCode(EditorConfigSettings config)
    {
        var className = $"{config.ClassPrefix}ConfigurableClass";
        var loggerClass = $"{config.ClassPrefix}Logger";
        var validatorClass = $"{config.ClassPrefix}Validator";
        
        var loggingCode = config.EnableLogging ? GenerateLoggingCode(loggerClass) : "";
        var validationCode = GenerateValidationCode(validatorClass, config);
        var featureCode = GenerateFeatureCode(config);
        
        var processingLog1 = config.EnableLogging 
            ? $"        {loggerClass}.LogInfo($\"Processing data: {{input}}\");"
            : "        // Logging disabled by editorconfig";
            
        var processingLog2 = config.EnableLogging
            ? $"        {loggerClass}.LogInfo($\"Processed result: {{result}}\");"
            : "        // Logging disabled by editorconfig";

        return $@"// <auto-generated/>
// Generated based on .editorconfig settings
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace {config.CustomNamespace};

/// <summary>
/// Auto-generated class based on .editorconfig settings
/// Configuration detected:
/// - Logging enabled: {config.EnableLogging}
/// - Naming style: {config.NamingStyle}
/// - Feature level: {config.FeatureLevel}
/// - Namespace: {config.CustomNamespace}
/// - Class prefix: {config.ClassPrefix}
/// </summary>
public static class {className}
{{
    /// <summary>
    /// Gets the configuration that was used to generate this class
    /// </summary>
    public static string GetConfiguration()
    {{
        return ""EditorConfig Source Generator Configuration:\n"" +
               ""- Logging enabled: {config.EnableLogging}\n"" +
               ""- Naming style: {config.NamingStyle}\n"" +
               ""- Feature level: {config.FeatureLevel}\n"" +
               ""- Namespace: {config.CustomNamespace}\n"" +
               ""- Class prefix: {config.ClassPrefix}\n\n"" +
               ""To customize this generation, add the following to your .editorconfig:\n"" +
               ""custom_generator_enable_logging = true|false\n"" +
               ""custom_generator_naming_style = pascal_case|camel_case|snake_case\n"" +
               ""custom_generator_feature_level = basic|advanced|enterprise\n"" +
               ""custom_generator_namespace = YourNamespace\n"" +
               ""custom_generator_class_prefix = YourPrefix"";
    }}

    /// <summary>
    /// Process data using the configured feature level
    /// </summary>
    public static string ProcessData(string input)
    {{
{processingLog1}
        
        var result = ""{config.FeatureLevel}"" switch
        {{
            ""basic"" => $""[BASIC] {{input}}"",
            ""advanced"" => $""[ADVANCED] {{input.ToUpperInvariant()}}"",
            ""enterprise"" => $""[ENTERPRISE] {{input.ToUpperInvariant().Replace("" "", ""_"")}}"",
            _ => $""[UNKNOWN] {{input}}""
        }};

{processingLog2}
        return result;
    }}

    /// <summary>
    /// Gets the list of features enabled based on configuration
    /// </summary>
    public static string[] GetFeatures()
    {{
        var features = new List<string> {{ ""Core processing"" }};
        {(config.EnableLogging ? "features.Add(\"Logging\");" : "// Logging disabled")}
        features.Add($""Naming validation: {config.NamingStyle}"");
        features.Add($""Feature level: {config.FeatureLevel}"");
        return features.ToArray();
    }}
}}

{loggingCode}

{validationCode}

{featureCode}";
    }

    private static string GenerateLoggingCode(string loggerClass)
    {
        return $@"/// <summary>
/// Auto-generated logger class (enabled by editorconfig setting)
/// </summary>
public static class {loggerClass}
{{
    private static readonly List<string> _logs = new();

    /// <summary>
    /// Log an informational message
    /// </summary>
    public static void LogInfo(string message)
    {{
        var logEntry = $""[INFO] {{DateTime.Now:yyyy-MM-dd HH:mm:ss}} - {{message}}"";
        _logs.Add(logEntry);
        Console.WriteLine(logEntry);
    }}

    /// <summary>
    /// Log an error message
    /// </summary>
    public static void LogError(string message)
    {{
        var logEntry = $""[ERROR] {{DateTime.Now:yyyy-MM-dd HH:mm:ss}} - {{message}}"";
        _logs.Add(logEntry);
        Console.WriteLine(logEntry);
    }}

    /// <summary>
    /// Get all logged messages
    /// </summary>
    public static IReadOnlyList<string> GetLogs() => _logs.AsReadOnly();

    /// <summary>
    /// Clear all logged messages
    /// </summary>
    public static void ClearLogs() => _logs.Clear();
}}";
    }

    private static string GenerateValidationCode(string validatorClass, EditorConfigSettings config)
    {
        var namingPattern = config.NamingStyle switch
        {
            "pascal_case" => @"^[A-Z][a-zA-Z0-9]*$",
            "camel_case" => @"^[a-z][a-zA-Z0-9]*$",
            "snake_case" => @"^[a-z][a-z0-9_]*$",
            _ => @"^[A-Za-z][A-Za-z0-9_]*$"
        };

        var namingDescription = config.NamingStyle switch
        {
            "pascal_case" => "PascalCase (first letter uppercase)",
            "camel_case" => "camelCase (first letter lowercase)",
            "snake_case" => "snake_case (lowercase with underscores)",
            _ => "mixed case"
        };

        return $@"/// <summary>
/// Auto-generated validator class based on naming style: {config.NamingStyle}
/// </summary>
public static class {validatorClass}
{{
    private static readonly Regex _namingPattern = new(@""{namingPattern}"", RegexOptions.Compiled);

    /// <summary>
    /// Validates if a name follows the configured naming convention: {namingDescription}
    /// </summary>
    public static bool IsValidName(string name)
    {{
        if (string.IsNullOrWhiteSpace(name))
            return false;
            
        return _namingPattern.IsMatch(name);
    }}

    /// <summary>
    /// Gets the expected naming style
    /// </summary>
    public static string GetNamingStyle() => ""{config.NamingStyle}"";

    /// <summary>
    /// Gets a description of the naming convention
    /// </summary>
    public static string GetNamingDescription() => ""{namingDescription}"";

    /// <summary>
    /// Converts a name to the configured naming style
    /// </summary>
    public static string ConvertToNamingStyle(string input)
    {{
        if (string.IsNullOrWhiteSpace(input))
            return input;

        return ""{config.NamingStyle}"" switch
        {{
            ""pascal_case"" => ToPascalCase(input),
            ""camel_case"" => ToCamelCase(input),
            ""snake_case"" => ToSnakeCase(input),
            _ => input
        }};
    }}

    private static string ToPascalCase(string input)
    {{
        if (string.IsNullOrEmpty(input)) return input;
        var words = input.Split(new[] {{ ' ', '_', '-' }}, StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(words.Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));
    }}

    private static string ToCamelCase(string input)
    {{
        var pascal = ToPascalCase(input);
        return string.IsNullOrEmpty(pascal) ? pascal : char.ToLower(pascal[0]) + pascal.Substring(1);
    }}

    private static string ToSnakeCase(string input)
    {{
        if (string.IsNullOrEmpty(input)) return input;
        return string.Join(""_"", input.Split(new[] {{ ' ', '-' }}, StringSplitOptions.RemoveEmptyEntries))
                     .ToLower();
    }}
}}";
    }

    private static string GenerateFeatureCode(EditorConfigSettings config)
    {
        return config.FeatureLevel switch
        {
            "advanced" => GenerateAdvancedFeatures(config),
            "enterprise" => GenerateEnterpriseFeatures(config),
            _ => GenerateBasicFeatures(config)
        };
    }

    private static string GenerateBasicFeatures(EditorConfigSettings config)
    {
        return $@"/// <summary>
/// Basic feature set (configured via editorconfig)
/// </summary>
public static class {config.ClassPrefix}BasicFeatures
{{
    /// <summary>
    /// Simple string processing
    /// </summary>
    public static string ProcessText(string input) => $""Basic: {{input}}"";
}}";
    }

    private static string GenerateAdvancedFeatures(EditorConfigSettings config)
    {
        return $@"/// <summary>
/// Advanced feature set (configured via editorconfig)
/// </summary>
public static class {config.ClassPrefix}AdvancedFeatures
{{
    /// <summary>
    /// Advanced string processing with transformations
    /// </summary>
    public static string ProcessText(string input) => $""Advanced: {{input.ToUpperInvariant()}}"";

    /// <summary>
    /// Batch processing capability
    /// </summary>
    public static IEnumerable<string> ProcessBatch(IEnumerable<string> inputs)
    {{
        return inputs.Select(input => ProcessText(input));
    }}
}}";
    }

    private static string GenerateEnterpriseFeatures(EditorConfigSettings config)
    {
        return $@"/// <summary>
/// Enterprise feature set (configured via editorconfig)
/// </summary>
public static class {config.ClassPrefix}EnterpriseFeatures
{{
    /// <summary>
    /// Enterprise-grade string processing with full transformation pipeline
    /// </summary>
    public static string ProcessText(string input) 
    {{
        var transformed = input.ToUpperInvariant().Replace("" "", ""_"");
        return $""Enterprise: {{transformed}}"";
    }}

    /// <summary>
    /// Advanced batch processing with parallel execution
    /// </summary>
    public static IEnumerable<string> ProcessBatch(IEnumerable<string> inputs)
    {{
        return inputs.AsParallel().Select(input => ProcessText(input));
    }}

    /// <summary>
    /// Performance metrics tracking
    /// </summary>
    public static class Metrics
    {{
        private static int _processedCount = 0;
        
        public static void IncrementProcessed() => Interlocked.Increment(ref _processedCount);
        public static int GetProcessedCount() => _processedCount;
        public static void Reset() => _processedCount = 0;
    }}
}}";
    }

    /// <summary>
    /// Class containing settings read from .editorconfig
    /// </summary>
    private class EditorConfigSettings(bool enableLogging, string namingStyle, string featureLevel, string customNamespace, string classPrefix)
    {
        public bool EnableLogging { get; } = enableLogging;
        public string NamingStyle { get; } = namingStyle;
        public string FeatureLevel { get; } = featureLevel;
        public string CustomNamespace { get; } = customNamespace;
        public string ClassPrefix { get; } = classPrefix;
    }
}