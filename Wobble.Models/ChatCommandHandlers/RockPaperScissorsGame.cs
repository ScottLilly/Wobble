using System.Collections.Generic;
using Wobble.Core;

namespace Wobble.Models.ChatCommandHandlers
{
    public class RockPaperScissorsGame : IChatCommandHandler
    {
        public List<string> CommandTriggers =>
            new List<string> { "rock", "paper", "scissors" };

        public string GetResponse(string botDisplayName, string chatterDisplayName,
            string commandTriggerWord, string arguments = "")
        {
            string botOption = CommandTriggers.RandomElement();

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