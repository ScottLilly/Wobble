using System;
using System.Collections.Generic;
using System.Linq;
using static System.StringComparison;

namespace Wobble.Core;

public static class ExtensionMethods
{
    public static bool IsNotNullEmptyOrWhiteSpace(this string text)
    {
        return !string.IsNullOrWhiteSpace(text);
    }

    public static bool Matches(this string text, string comparisonText)
    {
        if (text == null || comparisonText == null)
        {
            return false;
        }

        return text.Equals(comparisonText, InvariantCultureIgnoreCase);
    }

    public static string RandomElement(this List<string> options)
    {
        if (options == null || options.None())
        {
            return default;
        }

        return options[RngCreator.GetNumberBetween(0, options.Count - 1)];
    }

    public static bool None<T>(this IEnumerable<T> elements, Func<T, bool> func = null)
    {
        return func == null
            ? !elements.Any()
            : !elements.Any(func.Invoke);
    }
}