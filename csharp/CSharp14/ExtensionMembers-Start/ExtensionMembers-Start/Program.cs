string[] names = ["Jack", "Jochen", "Niki", "Sebastian", "Max", "Juan"];

names.Where(n => n.StartsWith("J")).Print();


public static class CollectionExtensions
{
    public static IEnumerable<T> Where<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        foreach (var item in source)
        {
            if (predicate(item))
                yield return item;
        }
    }
    public static void Print<T>(this IEnumerable<T> source)
    {
        foreach (var item in source)
        {
            Console.WriteLine(item);
        }
    }
}
