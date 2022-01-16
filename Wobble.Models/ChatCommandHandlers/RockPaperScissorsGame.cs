using System.Collections.Generic;
using TwitchLib.Client.Models;
using Wobble.Core;

namespace Wobble.Models.ChatCommandHandlers
{
    public class RockPaperScissorsGame : IWobbleCommandHandler
    {
        public List<string> CommandTriggers =>
            new List<string> { "rock", "paper", "scissors" };

        public string GetResponse(string botDisplayName, ChatCommand chatCommand)
        {
            string botOption = CommandTriggers.RandomElement();

            string botChoiceMessage = $"{botDisplayName} chose {botOption}.";

            string triggerWord = chatCommand.CommandText;
            string chatterName = chatCommand.ChatMessage.DisplayName;

            if (triggerWord.Matches(botOption))
            {
                return $"{botChoiceMessage} {chatterName} tied. :/";
            }

            if ((triggerWord.Matches("rock") && botOption.Matches("scissors")) ||
                (triggerWord.Matches("paper") && botOption.Matches("rock")) ||
                (triggerWord.Matches("scissors") && botOption.Matches("paper")))
            {
                return $"{botChoiceMessage} {chatterName} won! :D";
            }

            return $"{botChoiceMessage} {chatterName} lost. :(";
        }
    }
}