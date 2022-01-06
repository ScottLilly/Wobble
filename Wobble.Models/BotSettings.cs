using System.Collections.Generic;
using System.Linq;
using Wobble.Models.ChatCommandHandlers;

namespace Wobble.Models
{
    public class BotSettings
    {
        private readonly Dictionary<string, string> _values =
            new Dictionary<string, string>();

        public string ChannelName { get; }
        public string BotAccountName { get; }
        public string BotDisplayName { get; }
        public bool HandleHostRaidSubscriptionEvents { get; }
        public string Token { get; }
        public TimedMessages TimedMessages { get; } 
        public List<ChatCommandHandlers.ChatReply> ChatCommands { get; }

        public BotSettings(TwitchSettings ts, IEnumerable<KeyValuePair<string, string>> configuration)
        {
            ChannelName = ts.ChannelName;
            BotAccountName = ts.BotAccountName;
            BotDisplayName = ts.BotDisplayName;
            HandleHostRaidSubscriptionEvents = ts.HandleHostRaidSubscriptionEvents;
            TimedMessages = ts.TimedMessages;

            Token = configuration.First(c => c.Key == "Twitch:Token").Value;

            ChatCommands = new List<ChatCommandHandlers.ChatReply>();

            foreach (ChatMessage command in ts.ChatMessages)
            {
                ChatCommands.Add(new ChatCommandHandlers.ChatReply(command.TriggerWords, command.Responses));
            }
        }
    }
}