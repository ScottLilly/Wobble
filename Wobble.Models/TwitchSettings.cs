using System.Collections.Generic;

namespace Wobble.Models
{
    public class TwitchSettings
    {
        public string ChannelName { get; set; }
        public string BotAccountName { get; set; }
        public string BotDisplayName { get; set; }
        public bool HandleAlerts { get; set; }
        public List<Command> Commands { get; set; }
        public TimedMessages TimedMessages { get; set; }
    }

    public class Command
    {
        public List<string> TriggerWords { get; set; }
        public List<string> Responses { get; set; }
    }

    public class TimedMessages
    {
        public string IntervalInMinutes { get; set; }
        public List<string> Messages { get; set; }
    }
}