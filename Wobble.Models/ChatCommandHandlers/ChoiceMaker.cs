using System.Collections.Generic;
using System.Linq;
using TwitchLib.Client.Models;
using Wobble.Core;

namespace Wobble.Models.ChatCommandHandlers
{
    public class ChoiceMaker : IWobbleCommandHandler
    {
        public List<string> CommandTriggers =>
            new List<string> { "choose" };

        public string GetResponse(string botDisplayName, ChatCommand chatCommand)
        {
            string chatterName = chatCommand.ChatMessage.DisplayName;
            string args = chatCommand.ArgumentsAsString;

            // Handle null or empty arguments
            if (args == null || string.IsNullOrWhiteSpace(args))
            {
                return $"{chatterName} You must include options to choose from";
            }

            // Get list of arguments
            List<string> optionsArray = args.Split(',').ToList();

            // Special message if only one option
            if (optionsArray.Count == 1)
            {
                return $"{chatterName} You must really want {optionsArray[0]}";
            }

            // return random option
            return $"{chatterName} The obvious choice is: {optionsArray.RandomElement()}";
        }
    }
}