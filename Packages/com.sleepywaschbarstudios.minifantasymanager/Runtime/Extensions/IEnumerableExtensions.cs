#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MinifantasyManager.Runtime.Extensions
{
    public static class IEnumerableExtensions
    {
        // Returns default if there is more than 1 value
        public static bool TrySingleOrDefault<T>(this IEnumerable<T> input, out T? value)
        {
            if (input.Any())
            {
                value = input.First();
                return !input.Skip(1).Any();
            }
            else
            {
                value = default;
                return false;
            }
        }
    }
}
