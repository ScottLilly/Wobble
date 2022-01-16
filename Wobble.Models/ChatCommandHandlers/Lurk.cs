using System.Collections.Generic;

namespace Wobble.Models.ChatCommandHandlers
{
    public class Lurk : IWobbleCommandHandler
    {
        public List<string> CommandTriggers =>
            new List<string> { "lurk" };

        public string GetResponse(string botDisplayName, string chatterDisplayName, 
            string commandTriggerWord, string arguments = "")
        {
            return $"{chatterDisplayName} begins lurking in the shadows.";
        }
    }
}