using System.Collections.Generic;
using Wobble.Core;

namespace Wobble.Models.ChatCommands
{
    internal class ChoiceMaker : IChatCommand
    {
        public string CommandName => "ChoiceMaker";
        public List<string> CommandTriggers =>
            new List<string> { "Choose" };

        public string GetResult(string arguments)
        {
            // Handle null or empty arguments
            if (arguments == null || string.IsNullOrWhiteSpace(arguments))
            {
                return "You must include some options";
            }

            // Get list of arguments
            string[] optionsArray = arguments.Split(',');

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