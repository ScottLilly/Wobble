using System.Collections.Generic;
using System.Linq;
using Wobble.Core;

namespace Wobble.Models.ChatCommandHandlers
{
    public class ChoiceMaker : IChatCommandHandler
    {
        public List<string> CommandTriggers =>
            new List<string> { "Choose" };

        public string GetResponse(string botDisplayName, string chatterDisplayName, string commandTriggerWord,
            string arguments)
        {
            // Handle null or empty arguments
            if (arguments == null || string.IsNullOrWhiteSpace(arguments))
            {
                return $"{chatterDisplayName} You must include options to choose from";
            }

            // Get list of arguments
            List<string> optionsArray = arguments.Split(',').ToList();

            // Special message if only one option
            if (optionsArray.Count == 1)
            {
                return $"{chatterDisplayName} You must really want {optionsArray[0]}";
            }

            // return random option
            return $"{chatterDisplayName} The obvious choice is: {optionsArray.RandomElement()}";
        }
    }
}