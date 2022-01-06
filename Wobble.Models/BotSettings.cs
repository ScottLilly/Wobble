using System.Collections.Generic;
using System.Linq;
using Wobble.Models.ChatCommands;

namespace Wobble.Models
{
    public class BotSettings
    {
        private readonly Dictionary<string, string> _values =
            new Dictionary<string, string>();

        public string ChannelName { get; }
        public string BotAccountName { get; }
        public string BotDisplayName { get; }
        public bool HandleAlerts { get; }
        public string Token { get; }
        public TimedMessages TimedMessages { get; } 
        public List<ChatMessage> ChatCommands { get; }

        public BotSettings(TwitchSettings ts, IEnumerable<KeyValuePair<string, string>> configuration)
        {
            ChannelName = ts.ChannelName;
            BotAccountName = ts.BotAccountName;
            BotDisplayName = ts.BotDisplayName;
            HandleAlerts = ts.HandleAlerts;
            TimedMessages = ts.TimedMessages;

            Token = configuration.First(c => c.Key == "Twitch:Token").Value;

            ChatCommands = new List<ChatMessage>();

            foreach (Command command in ts.Commands)
            {
                ChatCommands.Add(new ChatMessage(command.TriggerWords, command.Responses));
            }
        }
    }
}