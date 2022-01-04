using Wobble.Core;

namespace Wobble.Models
{
    internal class ChoiceMaker
    {
        internal string ChooseFrom(string options)
        {
            // Handle null or empty options
            if (options == null || string.IsNullOrWhiteSpace(options))
            {
                return "You must include some options";
            }

            // Get list of options
            string[] optionsArray = options.Split(',');

            // Special message if only one option
            if (optionsArray.Length == 1)
            {
                return $"You must really want {optionsArray[0]}";
            }

            // return random option
            return $"The obvious choice is: {optionsArray[RandomNumberGenerator.NumberBetween(0, optionsArray.Length - 1)]}";
        }
    }
}