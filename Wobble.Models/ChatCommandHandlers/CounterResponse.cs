using System.Collections.Generic;
using Wobble.Core;
using Wobble.Models.ChatConnectors;

namespace Wobble.Models.ChatCommandHandlers;

public class CounterResponse : IChatCommandHandler
{
    private readonly List<string> _replies;

    public List<string> CommandTriggers { get; }

    public CounterResponse(List<string> commandTriggers, List<string> replies)
    {
        CommandTriggers = commandTriggers;

        _replies = replies;
    }

    public string GetResponse(string botDisplayName, TwitchChatCommandArgs commandArgs)
    {
        return _replies.Count == 0
            ? ""
            : _replies.RandomElement();
    }
}