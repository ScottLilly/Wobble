using System;
using System.Collections.Generic;
using System.Linq;

namespace Wobble.Core
{
    public static class ExtensionMethods
    {
        public static bool Matches(this string text, string comparisonText)
        {
            if (text == null || comparisonText == null)
            {
                return false;
            }

            return text.Equals(comparisonText, StringComparison.InvariantCultureIgnoreCase);
        }

        public static string RandomElement(this List<string> options)
        {
            if (options == null || !options.Any())
            {
                return default;
            }

            return options[RandomNumberGenerator.NumberBetween(0, options.Count - 1)];
        }
    }
}