using System.Collections.Generic;
using Wobble.Core;

namespace Wobble.Models.ChatCommands
{
    public class ChatMessage : IChatCommand
    {
        private readonly List<string> _responses;

        public List<string> CommandTriggers { get; } =
            new List<string>();

        public ChatMessage(List<string> commandTriggers, List<string> responses)
        {
            CommandTriggers = commandTriggers;

            _responses = responses;
        }

        public string GetResponse(string botDisplayName, string chatterDisplayName, string commandTriggerWord,
            string arguments = "")
        {
            if (_responses.Count == 0)
            {
                return "";
            }

            // Don't include chatterDisplayName in response when the chatter is the bot
            return chatterDisplayName.Equals(botDisplayName)
                ? _responses.RandomElement()
                : $"{chatterDisplayName} {_responses.RandomElement()}";
        }
    }
}