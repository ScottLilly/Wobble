using System.Collections.Generic;
using Wobble.Core;
using Wobble.Models.ChatConnectors;

namespace Wobble.Models.ChatCommandHandlers;

public class ChatResponse : IChatCommandHandler
{
    private readonly bool _requiresArgument;
    private readonly string _missingArgumentMessage;

    public List<string> CommandTriggers { get; }
    public List<string> Responses { get; }
    public bool IsAdditionalCommand { get; }

    public ChatResponse(List<string> commandTriggers, List<string> responses,
        bool isAdditionalCommand = false, bool requiresArgument = false, 
        string missingArgumentMessage = "")
    {
        CommandTriggers = commandTriggers;

        Responses = responses;
        IsAdditionalCommand = isAdditionalCommand;
        _requiresArgument = requiresArgument;
        _missingArgumentMessage = missingArgumentMessage;
    }

    public string GetResponse(string botDisplayName, TwitchChatCommandArgs commandArgs)
    {
        if (Responses.Count == 0)
        {
            return "";
        }

        if (_requiresArgument && 
            string.IsNullOrWhiteSpace(commandArgs.Argument))
        {
            return _missingArgumentMessage;
        }

        // Don't include chatterDisplayName in response when the chatter is the bot
        return string.IsNullOrWhiteSpace(commandArgs.ChatterName)
            ? Responses.RandomElement()
            : $"{commandArgs.ChatterName} {Responses.RandomElement()}";
    }
}