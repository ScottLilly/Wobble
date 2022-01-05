using System.Collections.Generic;

namespace Wobble.Models.ChatCommands
{
    interface IChatCommand
    {
        List<string> CommandTriggers { get; }
        string GetResult(string arguments = "");
    }
}