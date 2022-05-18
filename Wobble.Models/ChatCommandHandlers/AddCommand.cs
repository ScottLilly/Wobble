using System.Collections.Generic;
using System.Linq;
using TwitchLib.Client.Models;
using Wobble.Core;

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
            return "Additional commands can only be added by the streamer, mods, or VIPs";
        }

        string triggerWord = chatCommand.ArgumentsAsList[0];
        var response = string.Join(" ", chatCommand.ArgumentsAsList.Skip(1));

        var existingCommandHandler =
            _chatCommandHandlers.FirstOrDefault(cch => cch.CommandTriggers.Any(ct => ct.Matches(triggerWord)));

        if (existingCommandHandler != null)
        {
            if (chatCommand.ChatMessage.IsBroadcaster ||
                chatCommand.ChatMessage.IsModerator)
            {
                _chatCommandHandlers.Remove(existingCommandHandler);
            }
            else
            {
                return $"There is an existing command for '{triggerWord}'. Command was not changed.";
            }
        }

        _chatCommandHandlers.Add(
            new ChatResponse(
                new List<string> {triggerWord},
                new List<string> {response},
                true));

        return $"Added command '{triggerWord}' with response '{response}'";
    }
}