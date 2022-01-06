using System.Collections.Generic;
using System.Linq;
using Wobble.Core;

namespace Wobble.Models.ChatCommands
{
    internal class ChoiceMaker : IChatCommand
    {
        public List<string> CommandTriggers =>
            new List<string> { "Choose" };

        public string GetResponse(string botDisplayName, string chatterDisplayName, string commandTriggerWord,
            string arguments)
        {
            // Handle null or empty arguments
            if (arguments == null || string.IsNullOrWhiteSpace(arguments))
            {
                return "You must include some options";
            }

            // Get list of arguments
            List<string> optionsArray = arguments.Split(',').ToList();

            // Special message if only one option
            if (optionsArray.Count == 1)
            {
                return $"You must really want {optionsArray[0]}";
            }

            // return random option
            return $"The obvious choice is: {optionsArray.RandomElement()}";
        }
    }
}