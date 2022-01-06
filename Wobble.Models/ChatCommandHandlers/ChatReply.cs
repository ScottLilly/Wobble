using System.Collections.Generic;
using Wobble.Core;

namespace Wobble.Models.ChatCommandHandlers
{
    public class ChatReply : IChatCommandHandler
    {
        private readonly List<string> _replies;

        public List<string> CommandTriggers { get; } =
            new List<string>();

        public ChatReply(List<string> commandTriggers, List<string> replies)
        {
            CommandTriggers = commandTriggers;

            _replies = replies;
        }

        public string GetResponse(string botDisplayName, string chatterDisplayName, string commandTriggerWord,
            string arguments = "")
        {
            if (_replies.Count == 0)
            {
                return "";
            }

            // Don't include chatterDisplayName in response when the chatter is the bot
            return chatterDisplayName.Equals(botDisplayName)
                ? _replies.RandomElement()
                : $"{chatterDisplayName} {_replies.RandomElement()}";
        }
    }
}