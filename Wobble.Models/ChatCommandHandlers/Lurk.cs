using System.Collections.Generic;
using TwitchLib.Client.Models;

namespace Wobble.Models.ChatCommandHandlers
{
    public class Lurk : IWobbleCommandHandler
    {
        public List<string> CommandTriggers =>
            new List<string> { "lurk" };

        public string GetResponse(string botDisplayName, ChatCommand chatCommand)
        {
            return $"{chatCommand.ChatMessage.DisplayName} sees all the bugs that need to be fixed and hides in the break room.";
        }
    }
}