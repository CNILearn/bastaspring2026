#!/usr/bin/env dotnet
#:sdk Microsoft.NET.Sdk.Web
#:property EnableRequestDelegateGenerator=true

using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Listen on all interfaces to allow access from Windows when running in WSL
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000); // HTTP
});

// Use source-generated JSON context to avoid IL2026/IL3050 trimming warnings
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, BookJsonContext.Default));

var app = builder.Build();

// Sample data using collection expressions
List<Book> books =
[
    new(1, "C# in Depth", "Jon Skeet", 49.99m),
    new(2, "Clean Code", "Robert C. Martin", 39.99m),
    new(3, "Design Patterns", "Gang of Four", 44.99m),
];

app.MapGet("/", () => "Minimal API File-Based App running on .NET 10 🚀");

app.MapGet("/books", () => books);

app.MapGet("/books/{id:int}", (int id) =>
    books.FirstOrDefault(b => b.Id == id) is { } book
        ? Results.Ok(book)
        : Results.NotFound());

app.MapPost("/books", (Book book) =>
{
    if (books.Any(b => b.Id == book.Id))
        return Results.Conflict($"A book with id {book.Id} already exists.");
    books.Add(book);
    return Results.Created($"/books/{book.Id}", book);
});

app.MapDelete("/books/{id:int}", (int id) =>
{
    var book = books.FirstOrDefault(b => b.Id == id);
    if (book is null) return Results.NotFound();
    books.Remove(book);
    return Results.NoContent();
});

app.Run();

// Primary constructor - no boilerplate constructor body needed
record Book(int Id, string Title, string Author, decimal Price);

// Source-generated JSON context eliminates reflection-based serialization warnings
[JsonSerializable(typeof(Book))]
[JsonSerializable(typeof(List<Book>))]
partial class BookJsonContext : JsonSerializerContext { }
