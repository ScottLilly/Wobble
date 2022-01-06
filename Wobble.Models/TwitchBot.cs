using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;
using Wobble.Models.ChatCommands;
using static System.StringComparison;
using ChatCommand = Wobble.Models.ChatCommands.ChatCommand;

namespace Wobble.Models
{
    public class TwitchBot : INotifyPropertyChanged
    {
        private readonly TimeSpan _limitedChattersTimeSpan = new(0, 1, 0);

        private readonly TwitchClient _client = new();
        private readonly BotSettings _twitchBotSettings;
        private readonly ConnectionCredentials _credentials;

        private readonly List<IChatCommand> _chatCommands =
            new List<IChatCommand>();

        public enum ChatModes
        {
            Everyone,
            FollowersOnly,
            SubscribersOnly
        }

        public ChatModes ChatMode { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public TwitchBot(BotSettings twitchBotSettings)
        {
            foreach (ChatCommand chatCommand in twitchBotSettings.ChatCommands)
            {
                _chatCommands.Add(chatCommand);
            }

            _chatCommands.Add(new ChoiceMaker());
            _chatCommands.Add(new Roller());
            _chatCommands.Add(new RockPaperScissorsGame());

            _twitchBotSettings = twitchBotSettings;

            _credentials =
                new ConnectionCredentials(
                    string.IsNullOrWhiteSpace(_twitchBotSettings.BotAccountName)
                        ? twitchBotSettings.ChannelName
                        : _twitchBotSettings.BotAccountName, _twitchBotSettings.Token, disableUsernameCheck: true);

            _client.OnChannelStateChanged += HandleChannelStateChanged;
            _client.OnChatCommandReceived += HandleChatCommandReceived;
            _client.OnDisconnected += HandleDisconnected;
            _client.OnMessageReceived += HandleChatMessageReceived;

            if (_twitchBotSettings.HandleAlerts)
            {
                SubscribeToChannelEvents();
            }
        }

        #region Connection methods

        public void Connect()
        {
            _client.Initialize(_credentials, _twitchBotSettings.ChannelName);
            _client.Connect();
        }

        public void Disconnect()
        {
            _client.Disconnect();
        }

        private void HandleDisconnected(object sender, OnDisconnectedEventArgs e)
        {
            // If disconnected, automatically attempt to reconnect
            Connect();
        }

        public void SubscribeToChannelEvents()
        {
            _client.OnBeingHosted += HandleBeingHosted;
            _client.OnGiftedSubscription += HandleGiftedSubscription;
            _client.OnNewSubscriber += HandleNewSubscriber;
            _client.OnRaidNotification += HandleRaidNotification;
            _client.OnReSubscriber += HandleReSubscriber;
            _client.OnUserJoined += HandleUserJoined;
            _client.OnUserLeft += HandleUserLeft;
        }

        public void UnsubscribeFromChannelEvents()
        {
            _client.OnBeingHosted -= HandleBeingHosted;
            _client.OnGiftedSubscription -= HandleGiftedSubscription;
            _client.OnNewSubscriber -= HandleNewSubscriber;
            _client.OnRaidNotification -= HandleRaidNotification;
            _client.OnReSubscriber -= HandleReSubscriber;
            _client.OnUserJoined -= HandleUserJoined;
            _client.OnUserLeft -= HandleUserLeft;
        }

        #endregion

        #region Management methods

        public void EmoteModeOnlyOn()
        {
            _client.EmoteOnlyOn(_twitchBotSettings.ChannelName);
        }

        public void EmoteModeOnlyOff()
        {
            _client.EmoteOnlyOff(_twitchBotSettings.ChannelName);
        }

        public void FollowersOnlyOn()
        {
            _client.FollowersOnlyOn(_twitchBotSettings.ChannelName, _limitedChattersTimeSpan);
        }

        public void FollowersOnlyOff()
        {
            _client.FollowersOnlyOff(_twitchBotSettings.ChannelName);
        }

        public void SubscribersOnlyOn()
        {
            _client.SubscribersOnlyOn(_twitchBotSettings.ChannelName);
        }

        public void SubscribersOnlyOff()
        {
            _client.SubscribersOnlyOff(_twitchBotSettings.ChannelName);
        }

        public void SlowModeOn()
        {
            _client.SlowModeOn(_twitchBotSettings.ChannelName, _limitedChattersTimeSpan);
        }

        public void SlowModeOff()
        {
            _client.SlowModeOff(_twitchBotSettings.ChannelName);
        }

        public void DisplayCommands()
        {
            SendChatMessage($"Available commands: {GetAvailableCommandsList()}");
        }

        public void ClearChat()
        {
            _client.ClearChat(_twitchBotSettings.ChannelName);
        }

        #endregion

        #region Twitch EventHandler methods

        private void HandleBeingHosted(object sender, OnBeingHostedArgs e)
        {
            SendChatMessage($"Thank you for hosting {e.BeingHostedNotification.HostedByChannel}!");
        }

        private void HandleChannelStateChanged(object sender, OnChannelStateChangedArgs e)
        {
        }

        private void HandleChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            if (e.Command.CommandText.StartsWith("command", InvariantCultureIgnoreCase))
            {
                DisplayCommands();
            }
            else
            {
                var command =
                    _chatCommands.FirstOrDefault(cc =>
                        cc.CommandTriggers.Any(ct => ct.Equals(e.Command.CommandText, InvariantCultureIgnoreCase)));

                if (command != null)
                {
                    SendChatMessage(command.GetResult(_twitchBotSettings.BotDisplayName, 
                        e.Command.ChatMessage.DisplayName, e.Command.CommandText, e.Command.ArgumentsAsString));
                }
            }
        }

        private void HandleChatMessageReceived(object sender, OnMessageReceivedArgs e)
        {
        }

        private void HandleGiftedSubscription(object sender, OnGiftedSubscriptionArgs e)
        {
            SendChatMessage($"Welcome to the channel {e.GiftedSubscription.MsgParamRecipientDisplayName}!");
        }

        private void HandleNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            SendChatMessage(e.Subscriber.SubscriptionPlan == SubscriptionPlan.Prime
                ? $"{e.Subscriber.DisplayName}, thank you for subscribing with Prime!"
                : $"{e.Subscriber.DisplayName}, thank you for subscribing!");
        }

        private void HandleRaidNotification(object sender, OnRaidNotificationArgs e)
        {
            SendChatMessage($"{e.RaidNotification.DisplayName}, thank you for raiding!");
        }

        private void HandleReSubscriber(object sender, OnReSubscriberArgs e)
        {
            SendChatMessage(e.ReSubscriber.SubscriptionPlan == SubscriptionPlan.Prime
                ? $"{e.ReSubscriber.DisplayName}, thank you for re-subscribing with Prime!"
                : $"{e.ReSubscriber.DisplayName}, thank you for re-subscribing!");
        }

        private void HandleUserJoined(object sender, OnUserJoinedArgs e)
        {
        }

        private void HandleUserLeft(object sender, OnUserLeftArgs e)
        {
        }

        #endregion

        #region Private support methods

        private void SendChatMessage(string message)
        {
            _client.SendMessage(_twitchBotSettings.ChannelName, message);
        }

        private string GetAvailableCommandsList()
        {
            List<string> triggerWords = new List<string>();

            foreach (IChatCommand chatCommand in _chatCommands)
            {
                triggerWords.AddRange(chatCommand.CommandTriggers.Select(ct => $"!{ct}"));
            }

            var availableCommandsList = string.Join(", ",
                    triggerWords.OrderBy(ctw => ctw))
                .ToLower(CultureInfo.InvariantCulture);

            return availableCommandsList;
        }

        #endregion
    }
}