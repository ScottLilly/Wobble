using System.Collections.Generic;
using TwitchLib.Client.Models;
using Wobble.Core;

namespace Wobble.Models.ChatCommandHandlers;

public class Lurk : IWobbleCommandHandler
{
    private readonly List<string> _lurkers = new();

    public List<string> CommandTriggers => new() { "lurk", "unlurk" };

    public string GetResponse(string botDisplayName, ChatCommand chatCommand)
    {
        string triggerWord = chatCommand.CommandText;
        string chatterName = chatCommand.ChatMessage.DisplayName;

        if (triggerWord.Matches("lurk"))
        {
            if (_lurkers.Contains(chatterName))
            {
                // Chatter already ran !lurk, without doing an !unlurk
                return $"{chatterName} gets really worried about the number of bugs and leaves to buy donuts for the rest of chat.";
            }

            _lurkers.Add(chatterName);

            return $"{chatterName} sees all the bugs that need to be fixed and sneaks away to the coffee shop.";
        }

        if (triggerWord.Matches("unlurk"))
        {
            if (_lurkers.Contains(chatterName))
            {
                _lurkers.Remove(chatterName);
            }

            return $"{chatterName} sees all the bugs are fixed and returns to chat.";
        }

        return "";
    }
}