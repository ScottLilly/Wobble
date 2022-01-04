using System.Collections.Generic;

namespace Wobble.Models.ChatCommands
{
    public class ChatCommand : IChatCommand
    {
        private readonly string _response;

        public string CommandName { get; set; }

        public List<string> CommandTriggers { get; set; } =
            new List<string>();

        public ChatCommand(string commandName, string response)
        {
            CommandName = commandName;
            CommandTriggers.Add(commandName);
            _response = response;
        }

        public string GetResult(string arguments = "")
        {
            return _response;
        }
    }
}