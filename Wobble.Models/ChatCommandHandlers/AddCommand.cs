using System.Collections.Generic;
using System.Linq;
using Wobble.Core;
using Wobble.Models.ChatConnectors;

namespace Wobble.Models.ChatCommandHandlers;

public class AddCommand : IChatCommandHandler
{
    private readonly List<IChatCommandHandler> _chatCommandHandlers;

    public List<string> CommandTriggers => new() { "addcommand" };

    public AddCommand(List<IChatCommandHandler> chatCommandHandlers)
    {
        _chatCommandHandlers = chatCommandHandlers;
    }

    public string GetResponse(string botDisplayName, TwitchChatCommandArgs commandArgs)
    {
        if (!commandArgs.IsBroadcaster &&
            !commandArgs.IsModerator &&
            !commandArgs.IsVip)
        {
            return "Additional commands can only be added by the streamer, mods, or VIPs";
        }

        string triggerWord = commandArgs.Argument.Split(' ')[0];
        var response = string.Join(" ", commandArgs.Argument.Split(' ').Skip(1));

        var existingCommandHandler =
            _chatCommandHandlers.FirstOrDefault(cch => cch.CommandTriggers.Any(ct => ct.Matches(triggerWord)));

        if (existingCommandHandler != null)
        {
            if (commandArgs.IsBroadcaster ||
                commandArgs.IsModerator)
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