using System;

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
    }
}