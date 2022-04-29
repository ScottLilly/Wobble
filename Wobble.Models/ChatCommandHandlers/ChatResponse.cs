using System.Collections.Generic;
using TwitchLib.Client.Models;
using Wobble.Core;

namespace Wobble.Models.ChatCommandHandlers;

public class ChatResponse : IChatCommandHandler
{
    private readonly bool _requiresArgument;
    private readonly string _missingArgumentMessage;

    public List<string> CommandTriggers { get; }
    public List<string> Responses { get; }
    public bool IsAdditionalCommand { get; }

    public ChatResponse(List<string> commandTriggers, List<string> responses,
        bool isAdditionalCommand = false, bool requiresArgument = false, string missingArgumentMessage = "")
    {
        CommandTriggers = commandTriggers;

        Responses = responses;
        IsAdditionalCommand = isAdditionalCommand;
        _requiresArgument = requiresArgument;
        _missingArgumentMessage = missingArgumentMessage;
    }

    public string GetResponse(string botDisplayName, ChatCommand chatCommand)
    {
        if (Responses.Count == 0)
        {
            return "";
        }

        if (_requiresArgument && string.IsNullOrWhiteSpace(chatCommand.ArgumentsAsString))
        {
            return _missingArgumentMessage;
        }

        // Don't include chatterDisplayName in response when the chatter is the bot
        return chatCommand?.ChatMessage?.DisplayName == null
            ? Responses.RandomElement()
            : $"{chatCommand.ChatMessage.DisplayName} {Responses.RandomElement()}";
    }
}