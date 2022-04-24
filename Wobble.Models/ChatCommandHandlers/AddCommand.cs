using System.Collections.Generic;
using System.Linq;
using TwitchLib.Client.Models;

namespace Wobble.Models.ChatCommandHandlers;

public class AddCommand : IChatCommandHandler
{
    private readonly List<IChatCommandHandler> _chatCommandHandlers;

    public List<string> CommandTriggers { get; } =
        new List<string> { "addcommand" };

    public AddCommand(List<IChatCommandHandler> chatCommandHandlers)
    {
        _chatCommandHandlers = chatCommandHandlers;
    }

    public string GetResponse(string botDisplayName, ChatCommand chatCommand)
    {
        if (!chatCommand.ChatMessage.IsBroadcaster &&
            !chatCommand.ChatMessage.IsModerator &&
            !chatCommand.ChatMessage.IsVip)
        {
            return "";
        }

        string triggerWord = chatCommand.ArgumentsAsList[0];
        var response = string.Join(" ", chatCommand.ArgumentsAsList.Skip(1));

        _chatCommandHandlers.Add(
            new ChatResponse(
                new List<string> {triggerWord},
                new List<string> {response}));

        return $"Added command '{triggerWord}' with response '{response}'";
    }
}