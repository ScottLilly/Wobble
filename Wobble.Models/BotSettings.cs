using System.Collections.Generic;
using System.Linq;
using Wobble.Models.ChatCommandHandlers;

namespace Wobble.Models
{
    public class BotSettings
    {
        public string ChannelName { get; }
        public string BotAccountName { get; }
        public string BotDisplayName { get; }
        public bool HandleHostRaidSubscriptionEvents { get; }
        public string Token { get; }
        public TimedMessages TimedMessages { get; }
        public List<ChatReply> ChatCommands { get; }

        public BotSettings(TwitchSettings ts, IEnumerable<KeyValuePair<string, string>> configuration)
        {
            ChannelName = ts.ChannelName;
            BotAccountName = ts.BotAccountName;
            BotDisplayName = ts.BotDisplayName;
            HandleHostRaidSubscriptionEvents = ts.HandleHostRaidSubscriptionEvents;
            TimedMessages = ts.TimedMessages;

            // Get Twitch token from appsettings.json - for non-development execution
            Token = string.IsNullOrWhiteSpace(ts.TwitchToken)
                ? configuration.First(c => c.Key == "TwitchToken").Value
                : ts.TwitchToken;

            ChatCommands = new List<ChatReply>();

            foreach (ChatMessage command in ts.ChatMessages)
            {
                ChatCommands.Add(new ChatReply(command.TriggerWords, command.Responses));
            }
        }
    }
}