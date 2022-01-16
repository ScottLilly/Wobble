using System.Collections.Generic;

namespace Wobble.Models.ChatCommandHandlers
{
    public class Unlurk : IWobbleCommandHandler
    {
        public List<string> CommandTriggers =>
            new List<string> { "unlurk" };

        public string GetResponse(string botDisplayName, string chatterDisplayName,
            string commandTriggerWord, string arguments = "")
        {
            return $"{chatterDisplayName} sneaks back into chat from the shadows.";
        }
    }
}