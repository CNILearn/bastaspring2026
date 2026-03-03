# Readme - Code Samples for Chapter 18, .NET Compiler Platform

This chapter contains these samples:

* Syntax Analysis
    * WPFSyntaxTree (a WPF application where you can load a source file to see the syntax tree)
    * SyntaxQuery (using `CSharpSyntaxTree` to query C# syntax)
    * SyntaxWalker (using `CSharpSyntaxWalker` to check C# source code with the Visitor pattern)
* Semantics
    * SemanticsCompilation (dynamically compile C# code to access symbols)
* Transformation
    * TransformMethods (modify C# code using `CSharpSyntaxTree`)
    * SyntaxRewriter (modify C# code using `CSharpSyntaxRewriter` with the help of `CSharpSyntaxTree` and `CSharpCompilation` using the Visitor pattern)

Some samples of this chapter require the installation of *Visual Studio extension development*. This is a workload with the installation of Visual Studio 2026. You can modify the installation by starting the Visual Studio Installer, select a Visual Studio version, click *Modify*, and add the *Visual Studio extension development* components.
