
using BastaSampleGen.Attributes;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
Console.WriteLine(SampleInfo.GetSummary());

[GenerateInfo]
public class Sample
{
    public int X { get; set; }
    public int Y { get; set; }

    public void Foo() { }

    public void Bar() { }
}