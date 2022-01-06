using System.Collections.Generic;

namespace Wobble.Models.ChatCommands
{
    interface IChatCommand
    {
        List<string> CommandTriggers { get; }
        string GetResponse(string botDisplayName, string chatterDisplayName, string commandTriggerWord,
            string arguments = "");
    }
}