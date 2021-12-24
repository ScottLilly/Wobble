using System;
using System.Collections.Generic;
using System.Linq;

namespace Wobble.Models
{
    public class BotSettings
    {
        private readonly Dictionary<string, string> _values =
            new Dictionary<string, string>();

        public string ChannelName => _values["Twitch:ChannelName"];
        public string BotAccountName => _values["Twitch:BotAccountName"];
        public string Token => _values["Twitch:Token"];
        public bool HandleAlerts => Convert.ToBoolean(_values["Twitch:HandleAlerts"] ?? "false");

        public List<ChatCommand> ChatCommands { get; }

        public BotSettings(IEnumerable<KeyValuePair<string, string>> configuration)
        {
            ChatCommands = new List<ChatCommand>();

            foreach ((string key, string value) in configuration.Where(c => c.Value != null))
            {
                if (key.StartsWith("Twitch:ChatCommands"))
                {
                    int lastColonIndex = key.LastIndexOf(":");
                    string keywords = key.Substring(lastColonIndex + 1);

                    ChatCommands.Add(new ChatCommand { Command = keywords, Text = value });
                }
                else
                {
                    _values.TryAdd(key, value);
                }
            }
        }
    }
}