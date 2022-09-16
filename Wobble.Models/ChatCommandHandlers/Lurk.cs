using System.Collections.Generic;
using Wobble.Core;
using Wobble.Models.ChatConnectors;

namespace Wobble.Models.ChatCommandHandlers;

public class Lurk : IWobbleCommandHandler
{
    private readonly List<string> _lurkers = new();

    public List<string> CommandTriggers => new() { "lurk", "unlurk" };

    public string GetResponse(string botDisplayName, TwitchChatCommandArgs commandArgs)
    {
        if (commandArgs.CommandName.Matches("lurk"))
        {
            if (_lurkers.Contains(commandArgs.ChatterName))
            {
                // Chatter already ran !lurk, without doing an !unlurk
                return $"{commandArgs.ChatterName} gets really worried about the number of bugs and leaves to buy donuts for the rest of chat.";
            }

            _lurkers.Add(commandArgs.ChatterName);

            return $"{commandArgs.ChatterName} sees all the bugs that need to be fixed and sneaks away to the coffee shop.";
        }

        if (commandArgs.CommandName.Matches("unlurk"))
        {
            if (_lurkers.Contains(commandArgs.ChatterName))
            {
                _lurkers.Remove(commandArgs.ChatterName);
            }

            return $"{commandArgs.ChatterName} sees all the bugs are fixed and returns to chat.";
        }

        return "";
    }
}