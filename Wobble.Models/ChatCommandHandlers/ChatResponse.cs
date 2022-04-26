using System;
using System.Collections.Generic;
using TwitchLib.Client.Models;
using Wobble.Core;

namespace Wobble.Models.ChatCommandHandlers;

public class ChatResponse : IChatCommandHandler
{
    private readonly List<string> _replies;
    private readonly bool _requiresArgument;
    private readonly string _missingArgumentMessage;

    public List<string> CommandTriggers { get; }

    public ChatResponse(List<string> commandTriggers, List<string> replies,
        bool requiresArgument = false, string missingArgumentMessage = "")
    {
        CommandTriggers = commandTriggers;

        _replies = replies;
        _requiresArgument = requiresArgument;
        _missingArgumentMessage = missingArgumentMessage;
    }

    public string GetResponse(string botDisplayName, ChatCommand chatCommand)
    {
        if (_replies.Count == 0)
        {
            return "";
        }

        if (_requiresArgument && string.IsNullOrWhiteSpace(chatCommand.ArgumentsAsString))
        {
            return _missingArgumentMessage;
        }

        // Don't include chatterDisplayName in response when the chatter is the bot
        return chatCommand?.ChatMessage?.DisplayName == null
            ? _replies.RandomElement()
            : $"{chatCommand.ChatMessage.DisplayName} {_replies.RandomElement()}";
    }
}