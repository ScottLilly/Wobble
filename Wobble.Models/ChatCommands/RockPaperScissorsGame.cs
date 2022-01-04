using System.Collections.Generic;
using Wobble.Core;

namespace Wobble.Models.ChatCommands
{
    public class RockPaperScissorsGame : IChatCommand
    {
        private readonly string _botDisplayName;

        public string CommandName => "RockPaperScissorsGame";
        public List<string> CommandTriggers =>
            new List<string> { "rock", "paper", "scissors" };

        public RockPaperScissorsGame(string botDisplayName)
        {
            _botDisplayName = botDisplayName;
        }

        public string GetResult(string arguments = "")
        {
            string botOption =
                CommandTriggers[RandomNumberGenerator.NumberBetween(0, CommandTriggers.Count - 1)];

            string botChoiceMessage = $"{_botDisplayName} chose {botOption}.";

            if (arguments.Matches(botOption))
            {
                return $"{botChoiceMessage} You tied. :/";
            }

            if ((arguments.Matches("rock") && botOption.Matches("scissors")) ||
                (arguments.Matches("paper") && botOption.Matches("rock")) ||
                (arguments.Matches("scissors") && botOption.Matches("paper")))
            {
                return $"{botChoiceMessage} You won! :D";
            }

            return $"{botChoiceMessage} You lost. :(";
        }
    }
}