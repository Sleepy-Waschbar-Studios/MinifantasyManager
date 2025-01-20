using System.Text.RegularExpressions;

namespace MinifantasyManager.Runtime.Extensions
{
    public static class StringExtensions
    {
        // Can't use standard string.replace due to .netstandard2.0 bug.
        public static string ReplaceCaseInsensitive(this string input, string replace, string replacement)
        {
            return Regex.Replace(input, Regex.Escape(replace), replacement, RegexOptions.IgnoreCase);
        }
    }
}
