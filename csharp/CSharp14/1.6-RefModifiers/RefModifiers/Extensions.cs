// Delegates for ref parameter actions
public delegate void RefAction<T>(ref T value);
public delegate void ScopedRefAction<T>(scoped ref T value);

public static class ArrayExtensions
{
    extension(int[] source) // Extension for arrays
    {
        public void ForEach(RefAction<int> action)
        {
            for (int i = 0; i < source.Length; i++)
            {
                action(ref source[i]);
            }
        }
    }
}

public static class SpanExtensions
{
    extension (Span<int> source) // Extension for spans
    {
        public void Process(RefAction<int> action)
        {
            for (int i = 0; i < source.Length; i++)
            {
                action(ref source[i]);
            }
        }
    }
}
