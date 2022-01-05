using System.Collections.Generic;
using Wobble.Core;

namespace Wobble.Models.ChatCommands
{
    public class ChatCommand : IChatCommand
    {
        private readonly List<string> _responses;

        public List<string> CommandTriggers { get; set; } =
            new List<string>();

        public ChatCommand(List<string> commandTriggers, List<string> responses)
        {
            CommandTriggers = commandTriggers;
            _responses = responses;
        }

        public string GetResult(string botDisplayName, string chatterDisplayName, string commandTriggerWord,
            string arguments = "")
        {
            if (_responses.Count == 0)
            {
                return "";
            }

            return _responses[RandomNumberGenerator.NumberBetween(0, _responses.Count - 1)];
        }
    }
}