using System.Collections.Generic;

namespace Wobble.Models.ChatCommands
{
    public class Lurk : IChatCommand
    {
        public List<string> CommandTriggers =>
            new List<string> { "Lurk" };

        public string GetResponse(string botDisplayName, string chatterDisplayName, 
            string commandTriggerWord, string arguments = "")
        {
            return $"{chatterDisplayName} begins lurking in the shadows.";
        }
    }
}