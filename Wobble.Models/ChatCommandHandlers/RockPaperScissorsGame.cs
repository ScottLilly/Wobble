using System.Collections.Generic;
using Wobble.Core;
using Wobble.Models.ChatConnectors;

namespace Wobble.Models.ChatCommandHandlers;

public class RockPaperScissorsGame : IWobbleCommandHandler
{
    public List<string> CommandTriggers => new() { "rock", "paper", "scissors" };

    public string GetResponse(string botDisplayName, TwitchChatCommandArgs commandArgs)
    {
        string botOption = CommandTriggers.RandomElement();

        string botChoiceMessage = $"{botDisplayName} chose {botOption}.";

        if (commandArgs.CommandName.Matches(botOption))
        {
            return $"{botChoiceMessage} {commandArgs.ChatterName} tied. :/";
        }

        if ((commandArgs.CommandName.Matches("rock") && botOption.Matches("scissors")) ||
            (commandArgs.CommandName.Matches("paper") && botOption.Matches("rock")) ||
            (commandArgs.CommandName.Matches("scissors") && botOption.Matches("paper")))
        {
            return $"{botChoiceMessage} {commandArgs.ChatterName} won! :D";
        }

        return $"{botChoiceMessage} {commandArgs.ChatterName} lost. :(";
    }
}