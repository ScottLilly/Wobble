using System.Collections.Generic;
using TwitchLib.Client.Models;

namespace Wobble.Models.ChatCommandHandlers
{
    public interface IChatCommandHandler
    {
        List<string> CommandTriggers { get; }

        string GetResponse(string botDisplayName, ChatCommand chatCommand);
    }
}