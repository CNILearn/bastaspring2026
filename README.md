# BASTA! Spring 2026

## C# 14 und Ausblick auf C# 15

10:45 - 11:45, München 1+2

C# 14 ist da! In dieser Session entdecken wir die wichtigsten Neuerungen der Sprache und werfen zugleich einen Blick auf kommende Features in C# 15. Zu den Highlights von C# 14 zählen erweiterte Partial Members (nun auch für Events und Konstruktoren), Interceptors zum gezielten Ersetzen von Methodenaufrufen sowie der Span-Typ als First-Class .NET Type. Mit Field-backed Properties entsteht eine neue Balance zwischen Auto-und Full-Property, während Extension Blocks es ermöglichen, nicht nur Methoden, sondern auch Properties und Operatoren zu erweitern. Zudem zeigen wir, wie moderne C#-Projekte heute ohne Program-Klasse, Main-Methode und sogar ohne klassisches Projektfile auskommen können. Zum Abschluss wagen wir einen Ausblick auf C# 15: Diskutiert werden unter anderem Union Types, Readonly Parameters, mögliche Erweiterungen in LINQ und Expression Trees sowie die tiefere Integration von Async in die Runtime. Diese Session bietet einen kompakten Überblick über die aktuellen Sprachfeatures und einen fundierten Einblick in die Zukunft von C#.

### Code Samples C# 14

- `field`-backed Properties - with WPF and the CommunityToolkit.Mvvm
- `nameof` with unbound generics
- File-based apps with pre-processor directives and Minimal APIs
- Extension members for properties and operators
- Implicit conversions for `Span<T>` and `ReadOnlySpan<T>`
- Ref modifiers

### Code Samples C# 15

- Collection expression constructor aguments
- Runtime async support

## Developing und Debugging Source Generators

9:00 - 10:00, München 1+2

C# Source Generators eröffnen spannende Möglichkeiten, um wiederkehrende Aufgaben zu automatisieren und die Produktivität in der Entwicklung deutlich zu steigern. Doch wie funktionieren sie genau, was ist damit möglich – und worauf sollte man bei der eigenen Implementierung achten? In dieser Session erhalten Sie einen praxisnahen Überblick über die wichtigsten Konzepte von Source Generators. Wir beleuchten typische Einsatzszenarien, zeigen, wie Generatoren sinnvoll in bestehende Projekte integriert werden können, und diskutieren Best Practices für eine saubere Architektur. Ein besonderer Fokus liegt auf der Frage, wie sich Generatoren effizient debuggen und testen lassen, um Stabilität und Wartbarkeit sicherzustellen. Anhand konkreter Beispiele demonstrieren wir, wie Source Generators in realen Projekten eingesetzt werden können – von einfachen Code-Erweiterungen bis hin zu komplexeren Szenarien, die Entwicklungszeit sparen und Fehlerquellen reduzieren. Am Ende der Session wissen Sie, wie Sie das volle Potenzial von C# Source Generators ausschöpfen, welche Stolperfallen es zu vermeiden gilt und wie Sie Ihre eigenen Generatoren so entwickeln, dass sie langfristig Mehrwert für Ihr Team und Ihre Anwendungen bieten.

### The *dotnet new* template

You can use the **dotnet new template** to create a new source generator project:

```bash
dotnet new install CNinnovation.Templates.SourceGenerator

dotnet new sourcegen -n MySourceGenerator
```

### [Code Samples Roslyn](sourcegen/Roslyn/)

- Syntax API
  - Syntax Query
  - Syntax Walker
  - WPF Syntax Tree
  - Syntax Rewriter
- Semantic API
  - Semantics Compilation
- More
  - Transform Methods
  - Property Code Refactoring

### [Code Samples Source Generators](sourcegen)

- BATA! Source Generator
- Practical Source Generator Examples
  - Use attribute to generate code
  - Accessing additional files
  - Basic caching
  - Advanced caching
  - ForAttributeWithMetadataName
- Generate different code based on the C# version used
- Use naming conventions based on editorconfig
- Create C# records from JSON files
- Use partial events for weak events
- Use unsafe accessors to initialize readonly fields
- Use **interceptors** for activities
