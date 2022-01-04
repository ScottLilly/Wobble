using System.Collections.Generic;
using Wobble.Core;

namespace Wobble.Models
{
    internal class RockPaperScissorsGame
    {
        private readonly string _botDisplayName;

        internal List<string> Options { get; } =
            new List<string> { "rock", "paper", "scissors" };

        internal RockPaperScissorsGame(string botDisplayName)
        {
            _botDisplayName = botDisplayName;
        }

        internal string GetGameResult(string viewerOption)
        {
            string botOption =
                Options[RandomNumberGenerator.NumberBetween(0, Options.Count - 1)];

            string botChoiceMessage = $"{_botDisplayName} chose {botOption}.";

            if (viewerOption.Matches(botOption))
            {
                return $"{botChoiceMessage} You tied. :/";
            }

            if ((viewerOption.Matches("rock") && botOption.Matches("scissors")) ||
                (viewerOption.Matches("paper") && botOption.Matches("rock")) ||
                (viewerOption.Matches("scissors") && botOption.Matches("paper")))
            {
                return $"{botChoiceMessage} You won! :D";
            }

            return $"{botChoiceMessage} You lost. :(";
        }
    }
}