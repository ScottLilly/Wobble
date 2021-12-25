using System.Collections.ObjectModel;
using System.ComponentModel;
using Wobble.Models;

namespace Wobble.ViewModels
{
    public class BotManager : INotifyPropertyChanged
    {
        private readonly TwitchBot _twitchBot;

        public ObservableCollection<Viewer> Viewers => _twitchBot.Viewers;

        public event PropertyChangedEventHandler PropertyChanged;

        public BotManager(BotSettings botSettings)
        {
            _twitchBot = new TwitchBot(botSettings);

            _twitchBot.Connect();
        }

        public void DisplayCommands()
        {
            _twitchBot.DisplayCommands();
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