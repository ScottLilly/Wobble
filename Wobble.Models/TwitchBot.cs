﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;
using static System.StringComparison;

namespace Wobble.Models
{
    public class TwitchBot : INotifyPropertyChanged
    {
        private readonly TimeSpan _limitedChattersTimeSpan = new(0, 10, 0);

        private readonly TwitchClient _client = new();
        private readonly BotSettings _twitchBotSettings;
        private readonly ConnectionCredentials _credentials;

        public enum ChatModes
        {
            Everyone,
            FollowersOnly,
            SubscribersOnly
        }

        public ChatModes ChatMode { get; set; }

        public ObservableCollection<Viewer> Viewers { get; } =
            new ObservableCollection<Viewer>();

        public event PropertyChangedEventHandler PropertyChanged;

        public TwitchBot(BotSettings twitchBotSettings)
        {
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

        public void SendChatMessage(string message)
        {
            _client.SendMessage(_twitchBotSettings.ChannelName, message);
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
                ChatCommand chatCommand =
                    _twitchBotSettings.ChatCommands.FirstOrDefault(cc =>
                        cc.Command.Equals(e.Command.CommandText, InvariantCultureIgnoreCase));

                if (chatCommand != null)
                {
                    SendChatMessage(chatCommand.Text);
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
            Viewers.Add(new Viewer { Name = e.Username });
        }

        private void HandleUserLeft(object sender, OnUserLeftArgs e)
        {
            Viewer viewerToRemove = 
                Viewers.FirstOrDefault(v => v.Name.Equals(e.Username, InvariantCultureIgnoreCase));

            if (viewerToRemove != null)
            {
                Viewers.Remove(viewerToRemove);
            }
        }

        #endregion

        #region Private support methods

        private string GetAvailableCommandsList()
        {
            return string.Join(", ",
                    _twitchBotSettings.ChatCommands.Select(c => $"!{c.Command}"))
                .ToLower(CultureInfo.InvariantCulture);
        }

        #endregion
    }
}