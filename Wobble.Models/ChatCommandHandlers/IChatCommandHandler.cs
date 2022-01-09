using System.Collections.Generic;

namespace Wobble.Models.ChatCommandHandlers
{
    public interface IChatCommandHandler
    {
        List<string> CommandTriggers { get; }

        string GetResponse(string botDisplayName, string chatterDisplayName, string commandTriggerWord,
            string arguments = "");
    }
}