using System.Collections.Generic;

namespace Wobble.Models
{
    interface IChatCommand
    {
        string CommandName { get; set; }
        List<string> CommandTriggers { get; set; }
        string Execute(string arguments);
    }
}