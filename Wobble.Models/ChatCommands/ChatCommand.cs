using System.Collections.Generic;

namespace Wobble.Models.ChatCommands
{
    public class ChatCommand : IChatCommand
    {
        private readonly string _response;

        public List<string> CommandTriggers { get; set; } =
            new List<string>();

        public ChatCommand(string commandName, string response)
        {
            CommandTriggers.Add(commandName);
            _response = response;
        }

        public string GetResult(string botDisplayName, string chatterDisplayName, string commandTriggerWord,
            string arguments = "")
        {
            return _response;
        }
    }
}