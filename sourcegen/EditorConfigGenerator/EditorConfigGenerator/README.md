# EditorConfig Source Generator

A C# source generator that demonstrates reading `.editorconfig` settings and generating code based on those configuration values. This generator showcases how to use `AnalyzerConfigOptionsProvider` to access EditorConfig properties and influence code generation at build time.

## Features

- **Conditional Code Generation**: Enable or disable features based on EditorConfig settings
- **Naming Convention Enforcement**: Generate validation code based on configured naming styles
- **Feature Level Control**: Generate different code complexity based on feature level settings
- **Customizable Namespaces**: Control the namespace of generated code
- **Runtime Configuration Reporting**: Generated code includes methods to report the configuration used

## EditorConfig Settings

Add these custom properties to your `.editorconfig` file to control code generation:

```ini
# .editorconfig

# Enable or disable logging code generation
custom_generator_enable_logging = true

# Set naming convention style for generated validation
custom_generator_naming_style = pascal_case  # pascal_case, camel_case, snake_case

# Control feature complexity level
custom_generator_feature_level = basic  # basic, advanced, enterprise

# Customize generated namespace
custom_generator_namespace = MyApp.Generated

# Set prefix for generated class names
custom_generator_class_prefix = MyApp
```

## Generated Code Examples

Based on your EditorConfig settings, the generator creates:

### 1. Configuration-Aware Main Class
```csharp
namespace MyApp.Generated;

public static class MyAppConfigurableClass
{
    public static string GetConfiguration() { /* reports current config */ }
    public static string ProcessData(string input) { /* processes based on feature level */ }
}
```

### 2. Conditional Logger (when logging enabled)
```csharp
public static class MyAppLogger
{
    public static void LogInfo(string message) { /* logging implementation */ }
    public static void LogError(string message) { /* error logging */ }
    public static IReadOnlyList<string> GetLogs() { /* retrieve logs */ }
}
```

### 3. Naming Convention Validator
```csharp
public static class MyAppValidator
{
    public static bool IsValidName(string name) { /* validates against naming style */ }
    public static string ConvertToNamingStyle(string input) { /* converts to configured style */ }
}
```

### 4. Feature-Level Specific Code
Depending on `custom_generator_feature_level`:
- **basic**: Simple processing methods
- **advanced**: Additional batch processing and transformations
- **enterprise**: Parallel processing and performance metrics

## Usage

1. **Install the generator** (reference the project or package)
2. **Configure your `.editorconfig`** with the custom properties
3. **Build your project** to generate the code
4. **Use the generated classes** in your application code

Example usage:
```csharp
using MyApp.Generated;

// Use generated functionality
var config = MyAppConfigurableClass.GetConfiguration();
var result = MyAppConfigurableClass.ProcessData("Hello World");

// Use conditional logging (if enabled)
MyAppLogger.LogInfo("Processing started");

// Use naming validation
bool isValid = MyAppValidator.IsValidName("MyVariable");
string converted = MyAppValidator.ConvertToNamingStyle("my variable");
```

## Integration Examples

### Basic Configuration
```ini
# .editorconfig
custom_generator_enable_logging = false
custom_generator_naming_style = camel_case
custom_generator_feature_level = basic
```

### Advanced Configuration
```ini
# .editorconfig
custom_generator_enable_logging = true
custom_generator_naming_style = pascal_case
custom_generator_feature_level = advanced
custom_generator_namespace = MyCompany.Utilities
custom_generator_class_prefix = Auto
```

### Enterprise Configuration
```ini
# .editorconfig
custom_generator_enable_logging = true
custom_generator_naming_style = snake_case
custom_generator_feature_level = enterprise
custom_generator_namespace = Enterprise.Generated
custom_generator_class_prefix = Ent
```

## Benefits

- **Configuration-Driven Development**: Change behavior without modifying source code
- **Build-Time Optimization**: Code generation based on environment/project settings
- **Consistency Enforcement**: Automated naming convention validation
- **Zero Runtime Dependencies**: All configuration is resolved at build time
- **Team Standardization**: Shared EditorConfig ensures consistent code generation across team members

## Implementation Details

- Uses `IIncrementalGenerator` for optimal build performance
- Reads configuration via `AnalyzerConfigOptionsProvider.GlobalOptions`
- Generates different code patterns based on configuration values
- Supports fallback values for all configuration options
- Comprehensive error handling for invalid configuration values

This generator demonstrates best practices for:
- Reading EditorConfig settings in source generators
- Conditional code generation based on configuration
- Creating maintainable and configurable source generators
- Integrating with build systems and IDE experiences