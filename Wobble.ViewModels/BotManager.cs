using System.ComponentModel;
using Wobble.Models;

namespace Wobble.ViewModels
{
    public class BotManager : INotifyPropertyChanged
    {
        private readonly TwitchBot _twitchBot;

        public event PropertyChangedEventHandler PropertyChanged;

        public BotManager(BotSettings botSettings)
        {
            _twitchBot = new TwitchBot(botSettings);

            _twitchBot.Connect();
        }

        public void DisplayCommands()
        {
            _twitchBot.DisplayCommandTriggerWords();
        }

        public void ClearChat()
        {
            _twitchBot.ClearChat();
        }

        public void Disconnect()
        {
            _twitchBot.Disconnect();
        }
    }
}