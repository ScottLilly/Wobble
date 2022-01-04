using System.Collections.Generic;

namespace Wobble.Models.ChatCommands
{
    interface IChatCommand
    {
        string CommandName { get; }
        List<string> CommandTriggers { get; }
        string GetResult(string arguments = "");
    }
}