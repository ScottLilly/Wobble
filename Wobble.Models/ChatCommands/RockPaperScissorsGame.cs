using System.Collections.Generic;
using Wobble.Core;

namespace Wobble.Models.ChatCommands
{
    public class RockPaperScissorsGame : IChatCommand
    {
        public List<string> CommandTriggers =>
            new List<string> { "rock", "paper", "scissors" };

        public string GetResponse(string botDisplayName, string chatterDisplayName,
            string commandTriggerWord, string arguments = "")
        {
            string botOption =
                CommandTriggers[RandomNumberGenerator.NumberBetween(0, CommandTriggers.Count - 1)];

            string botChoiceMessage = $"{botDisplayName} chose {botOption}.";

            if (commandTriggerWord.Matches(botOption))
            {
                return $"{botChoiceMessage} {chatterDisplayName} tied. :/";
            }

            if ((commandTriggerWord.Matches("rock") && botOption.Matches("scissors")) ||
                (commandTriggerWord.Matches("paper") && botOption.Matches("rock")) ||
                (commandTriggerWord.Matches("scissors") && botOption.Matches("paper")))
            {
                return $"{botChoiceMessage} {chatterDisplayName} won! :D";
            }

            return $"{botChoiceMessage} {chatterDisplayName} lost. :(";
        }
    }
}