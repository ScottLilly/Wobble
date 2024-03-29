﻿using System.Collections.Generic;
using System.Linq;
using Wobble.Core;
using Wobble.Models.ChatConnectors;

namespace Wobble.Models.ChatCommandHandlers;

public class RemoveCommand : IChatCommandHandler
{
    private readonly List<IChatCommandHandler> _chatCommandHandlers;

    public List<string> CommandTriggers => new() { "removecommand" };

    public RemoveCommand(List<IChatCommandHandler> chatCommandHandlers)
    {
        _chatCommandHandlers = chatCommandHandlers;
    }

    public string GetResponse(string botDisplayName, TwitchChatCommandArgs commandArgs)
    {
        if (!commandArgs.IsBroadcaster &&
            !commandArgs.IsModerator &&
            !commandArgs.IsVip)
        {
            return "Additional commands can only be removed by the streamer, mods, or VIPs";
        }

        string triggerWord = commandArgs.Argument.Split(' ')[0];

        var existingCommandHandler =
            _chatCommandHandlers
                .FirstOrDefault(cch => 
                    cch.CommandTriggers.Any(ct => ct.Matches(triggerWord)));

        if (existingCommandHandler == null)
        {
            return $"The command '{triggerWord}' does not exist";
        }

        if (!_chatCommandHandlers.OfType<ChatResponse>()
                .Any(c => 
                    c.IsAdditionalCommand && 
                    c.CommandTriggers.Any(ct => ct.Matches(triggerWord))))
        {
            return "You can only remove additional commands, not base commands";
        }

        _chatCommandHandlers.Remove(existingCommandHandler);

        return $"Removed command '{triggerWord}'";
    }
}