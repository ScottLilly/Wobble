using System.Collections.Generic;
using TwitchLib.Client.Models;

namespace Wobble.Models.ChatCommandHandlers
{
    public class Unlurk : IWobbleCommandHandler
    {
        public List<string> CommandTriggers =>
            new List<string> { "unlurk" };

        public string GetResponse(string botDisplayName, ChatCommand chatCommand)
        {
            return $"{chatCommand.ChatMessage.DisplayName} notices all the bugs are fixed and sneaks back from the break room.";
        }
    }
}