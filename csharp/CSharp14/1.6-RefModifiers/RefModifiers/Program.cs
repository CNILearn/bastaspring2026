// Before C# 14 — must specify full type
// Action<int[]> doubleAll = (ref int item) => item *= 2; // Error!

// C# 14 — modifiers without types in lambdas
int[] data = [1, 2, 3, 4, 5];

// ref parameter in lambda — modifies in place
data.ForEach((ref item) => item *= 2);
// data is now [2, 4, 6, 8, 10]

// With scoped modifier for safety (can't escape the lambda)
Span<int> span = [10, 20, 30];
span.Process((scoped ref item) => item += 1);

Console.WriteLine();