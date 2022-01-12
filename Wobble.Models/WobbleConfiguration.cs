using System.Collections.Generic;

namespace Wobble.Models
{
    public class WobbleConfiguration
    {
        public string ChannelName { get; set; }
        public string BotAccountName { get; set; }
        public string TwitchToken { get; set; }
        public string BotDisplayName { get; set; }
        public bool HandleHostRaidSubscriptionEvents { get; set; }
        public List<ChatMessage> ChatMessages { get; set; }
        public List<CounterMessage> CounterMessages { get; set; }
        public TimedMessages TimedMessages { get; set; }
    }

    public class ChatMessage
    {
        public List<string> TriggerWords { get; set; }
        public List<string> Responses { get; set; }
    }

    public class CounterMessage
    {
        public List<string> TriggerWords { get; set; }
        public List<string> Responses { get; set; }
    }

    public class TimedMessages
    {
        public int IntervalInMinutes { get; set; }
        public List<string> Messages { get; set; }
    }
}