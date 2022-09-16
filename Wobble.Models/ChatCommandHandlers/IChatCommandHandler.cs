using System.Collections.Generic;
using Wobble.Models.ChatConnectors;

namespace Wobble.Models.ChatCommandHandlers;

public interface IChatCommandHandler
{
    List<string> CommandTriggers { get; }

    string GetResponse(string botDisplayName, TwitchChatCommandArgs commandArgs);
}