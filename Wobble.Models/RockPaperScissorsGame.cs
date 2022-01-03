using System.Collections.Generic;
using Wobble.Core;

namespace Wobble.Models
{
    internal class RockPaperScissorsGame
    {
        internal List<string> Options { get; } =
            new List<string> { "rock", "paper", "scissors" };

        internal string GetGameResult(string botName, string viewerOption)
        {
            string botOption =
                Options[RandomNumberGenerator.NumberBetween(0, Options.Count - 1)];

            string botChoiceMessage = $"{botName} chose {botOption}.";

            if (viewerOption.Matches(botOption))
            {
                return $"{botChoiceMessage} It was a tie";
            }

            if ((viewerOption.Matches("rock") && botOption.Matches("scissors")) ||
                (viewerOption.Matches("paper") && botOption.Matches("rock")) ||
                (viewerOption.Matches("scissors") && botOption.Matches("paper")))
            {
                return $"{botChoiceMessage} You won!";
            }

            return $"{botChoiceMessage} You lost :(";
        }
    }
}