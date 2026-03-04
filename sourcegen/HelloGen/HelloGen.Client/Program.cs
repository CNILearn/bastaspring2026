using HelloGen.Attributes;

Console.WriteLine();

Console.WriteLine(FooInfo.GetSummary());

[GenerateInfo]
public class Foo
{
    public int X { get; set; }
    public void Bar() { }

    public int Y { get; set; }
}
