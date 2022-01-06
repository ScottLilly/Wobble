﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Timers;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;
using Wobble.Core;
using Wobble.Models.ChatCommands;
using static System.StringComparison;
using ChatMessage = Wobble.Models.ChatCommands.ChatMessage;
using Timer = System.Threading.Timer;

namespace Wobble.Models
{
    public class TwitchBot : INotifyPropertyChanged
    {
        private readonly System.Timers.Timer _timedMessagesTimer;

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
            _twitchBotSettings = twitchBotSettings;

            foreach (ChatMessage chatCommand in _twitchBotSettings.ChatCommands)
            {
                _chatCommands.Add(chatCommand);
            }

            _chatCommands.Add(new ChoiceMaker());
            _chatCommands.Add(new Roller());
            _chatCommands.Add(new RockPaperScissorsGame());
            _chatCommands.Add(new Lurk());
            _chatCommands.Add(new Unlurk());

            _credentials =
                new ConnectionCredentials(
                    string.IsNullOrWhiteSpace(_twitchBotSettings.BotAccountName)
                        ? twitchBotSettings.ChannelName
                        : _twitchBotSettings.BotAccountName, _twitchBotSettings.Token, disableUsernameCheck: true);

            _client.OnChannelStateChanged += HandleChannelStateChanged;
            _client.OnChatCommandReceived += HandleChatCommandReceived;
            _client.OnDisconnected += HandleDisconnected;

            if (_twitchBotSettings.HandleAlerts)
            {
                SubscribeToChannelEvents();
            }

            if (_twitchBotSettings.TimedMessages?.IntervalInMinutes > 0)
            {
                _timedMessagesTimer =
                    new System.Timers.Timer(_twitchBotSettings.TimedMessages.IntervalInMinutes * 60 * 1000);
                _timedMessagesTimer.Elapsed += TimedMessagesTimer_Elapsed;
                _timedMessagesTimer.Enabled = true;
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

        private void SubscribeToChannelEvents()
        {
            _client.OnBeingHosted += HandleBeingHosted;
            _client.OnGiftedSubscription += HandleGiftedSubscription;
            _client.OnNewSubscriber += HandleNewSubscriber;
            _client.OnRaidNotification += HandleRaidNotification;
            _client.OnReSubscriber += HandleReSubscriber;
        }

        private void UnsubscribeFromChannelEvents()
        {
            _client.OnBeingHosted -= HandleBeingHosted;
            _client.OnGiftedSubscription -= HandleGiftedSubscription;
            _client.OnNewSubscriber -= HandleNewSubscriber;
            _client.OnRaidNotification -= HandleRaidNotification;
            _client.OnReSubscriber -= HandleReSubscriber;
        }

        #endregion

        #region Chat channel management methods

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

        public void DisplayCommandTriggerWords()
        {
            List<string> triggerWords = new List<string>();

            foreach (IChatCommand chatCommand in _chatCommands)
            {
                triggerWords.AddRange(chatCommand.CommandTriggers
                    .Select(ct => $"!{ct.ToLower(CultureInfo.InvariantCulture)}"));
            }

            SendChatMessage($"Available commands: {string.Join(", ", triggerWords.OrderBy(ctw => ctw))}");
        }

        public void ClearChat()
        {
            _client.ClearChat(_twitchBotSettings.ChannelName);
        }

        #endregion

        #region EventHandler methods

        private void TimedMessagesTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var message =
                _twitchBotSettings.TimedMessages.Messages.RandomElement();

            if (message.StartsWith("!"))
            {
                if (message.StartsWith("!"))
                {
                    message = message.Substring(1);
                }

                var command = GetCommand(message);

                if (command != null)
                {
                    SendChatMessage(command.GetResponse(_twitchBotSettings.BotDisplayName, "", message));
                }
            }
            else
            {
                SendChatMessage(message);
            }
        }

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
                DisplayCommandTriggerWords();
                return;
            }

            IChatCommand? command = GetCommand(e.Command.CommandText);

            if (command != null)
            {
                SendChatMessage(command.GetResponse(_twitchBotSettings.BotDisplayName, 
                    e.Command.ChatMessage.DisplayName, e.Command.CommandText, e.Command.ArgumentsAsString));
            }
        }

        private IChatCommand GetCommand(string commandText)
        {
            return _chatCommands.FirstOrDefault(cc =>
                cc.CommandTriggers.Any(ct => ct.Equals(commandText, InvariantCultureIgnoreCase)));
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

        #endregion

        #region Private support methods

        private void SendChatMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            _client.SendMessage(_twitchBotSettings.ChannelName, message);
        }

        #endregion
    }
}