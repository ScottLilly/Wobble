using System.Collections.Generic;
using System.Linq;
using Wobble.Core;
using Wobble.Models.ChatConnectors;

namespace Wobble.Models.ChatCommandHandlers;

public class ChoiceMaker : IWobbleCommandHandler
{
    public List<string> CommandTriggers => new() { "choose" };

    public string GetResponse(string botDisplayName, TwitchChatCommandArgs commandArgs)
    {
        string chatterName = commandArgs.ChatterName;
        string args = commandArgs.Argument;

        // Handle null or empty arguments
        if (args == null || string.IsNullOrWhiteSpace(args))
        {
            return $"{chatterName} You must include options to choose from";
        }

        // Get list of arguments
        List<string> optionsArray;
        if (args.Contains(','))
        {
            optionsArray = args.Split(',').ToList();
        }
        else if(args.Contains(' '))
        {
            optionsArray = args.Split(' ').ToList();
        }
        else
        {
            return $"{chatterName} You must enter the choices, separated by commas or spaces";
        }

        // Special message if only one option
        if (optionsArray.Count == 1)
        {
            return $"{chatterName} You must really want {optionsArray[0]}";
        }

        // return random option
        return $"{chatterName} The obvious choice is: {optionsArray.RandomElement()}";
    }
}