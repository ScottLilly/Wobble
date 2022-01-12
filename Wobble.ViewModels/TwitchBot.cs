using System;
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
using Wobble.Models;
using Wobble.Models.ChatCommandHandlers;
using Wobble.Services;
using static System.StringComparison;
using ChatResponse = Wobble.Models.ChatCommandHandlers.ChatResponse;

namespace Wobble.ViewModels
{
    public class TwitchBot : INotifyPropertyChanged
    {
        private readonly TwitchClient _client = new();
        private readonly BotSettings _twitchBotSettings;
        private readonly ConnectionCredentials _credentials;
        private readonly CounterData _counterData;

        private readonly List<IChatCommandHandler> _chatCommandHandlers =
            new List<IChatCommandHandler>();

        private Timer _timedMessagesTimer;

        private readonly TimeSpan _limitedChattersTimeSpan = new(0, 1, 0);

        public event PropertyChangedEventHandler PropertyChanged;

        public TwitchBot(BotSettings twitchBotSettings)
        {
            _twitchBotSettings = twitchBotSettings;
            _counterData = PersistenceService.GetCounterData();

            _credentials =
                new ConnectionCredentials(
                    string.IsNullOrWhiteSpace(_twitchBotSettings.BotAccountName)
                        ? twitchBotSettings.ChannelName
                        : _twitchBotSettings.BotAccountName, _twitchBotSettings.Token, disableUsernameCheck: true);

            _client.OnChannelStateChanged += HandleChannelStateChanged;
            _client.OnChatCommandReceived += HandleChatCommandReceived;
            _client.OnDisconnected += HandleDisconnected;

            if (_twitchBotSettings.HandleHostRaidSubscriptionEvents)
            {
                SubscribeToHostRaidSubscriptionEvents();
            }

            PopulateChatCommandHandlers();

            InitializeTimedMessages();

            Connect();
        }

        public void DisplayCommands()
        {
            DisplayCommandTriggerWords();
        }

        public void ClearChat()
        {
            _client.ClearChat(_twitchBotSettings.ChannelName);
        }

        public void Disconnect()
        {
            _client.Disconnect();
        }

        #region Connection methods

        private void Connect()
        {
            _client.Initialize(_credentials, _twitchBotSettings.ChannelName);
            _client.Connect();
        }

        private void HandleDisconnected(object sender, OnDisconnectedEventArgs e)
        {
            // If disconnected, automatically attempt to reconnect
            Connect();
        }

        private void SubscribeToHostRaidSubscriptionEvents()
        {
            _client.OnBeingHosted += HandleBeingHosted;
            _client.OnGiftedSubscription += HandleGiftedSubscription;
            _client.OnNewSubscriber += HandleNewSubscriber;
            _client.OnRaidNotification += HandleRaidNotification;
            _client.OnReSubscriber += HandleReSubscriber;
        }

        private void UnsubscribeFromHostRaidSubscriptionEvents()
        {
            _client.OnBeingHosted -= HandleBeingHosted;
            _client.OnGiftedSubscription -= HandleGiftedSubscription;
            _client.OnNewSubscriber -= HandleNewSubscriber;
            _client.OnRaidNotification -= HandleRaidNotification;
            _client.OnReSubscriber -= HandleReSubscriber;
        }

        #endregion

        #region Chat mode management methods

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

            IChatCommandHandler command = GetCommand(e.Command.CommandText);

            if (command == null)
            {
                return;
            }

            if (command is CounterResponse)
            {
                SendChatMessage(GetCounterCommandResponse(e.Command.CommandText));
            }
            else
            {
                SendChatMessage(command.GetResponse(_twitchBotSettings.BotDisplayName,
                    e.Command.ChatMessage.DisplayName, e.Command.CommandText, e.Command.ArgumentsAsString));
            }
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

        private void DisplayCommandTriggerWords()
        {
            List<string> triggerWords = new List<string>();

            foreach (IChatCommandHandler chatCommand in _chatCommandHandlers)
            {
                triggerWords.AddRange(chatCommand.CommandTriggers
                    .Select(ct => $"!{ct.ToLower(CultureInfo.InvariantCulture)}"));
            }

            SendChatMessage($"Available commands: {string.Join(", ", triggerWords.OrderBy(ctw => ctw))}");
        }

        private void InitializeTimedMessages()
        {
            if (!(_twitchBotSettings.TimedMessages?.IntervalInMinutes > 0))
            {
                return;
            }

            _timedMessagesTimer =
                new Timer(_twitchBotSettings.TimedMessages.IntervalInMinutes * 60 * 1000);
            _timedMessagesTimer.Elapsed += TimedMessagesTimer_Elapsed;
            _timedMessagesTimer.Enabled = true;
        }

        private void PopulateChatCommandHandlers()
        {
            foreach (ChatResponse reply in _twitchBotSettings.ChatCommands)
            {
                _chatCommandHandlers.Add(reply);
            }

            foreach (CounterResponse reply in _twitchBotSettings.CounterCommands)
            {
                _chatCommandHandlers.Add(reply);
            }

            _chatCommandHandlers.Add(new ChoiceMaker());
            _chatCommandHandlers.Add(new Roller());
            _chatCommandHandlers.Add(new RockPaperScissorsGame());
            _chatCommandHandlers.Add(new Lurk());
            _chatCommandHandlers.Add(new Unlurk());
        }

        private IChatCommandHandler GetCommand(string commandText)
        {
            return _chatCommandHandlers.FirstOrDefault(cc =>
                cc.CommandTriggers.Any(ct => ct.Equals(commandText, InvariantCultureIgnoreCase)));
        }

        private string GetCounterCommandResponse(string counterCommand)
        {
            IChatCommandHandler command = GetCommand(counterCommand);

            if (!_counterData.CommandCounters.Any(cc => cc.Command.Matches(counterCommand)))
            {
                _counterData.CommandCounters.Add(new CommandCounter
                {
                    Command = counterCommand,
                    Count = 0
                });
            }

            CommandCounter commandCounter =
                _counterData.CommandCounters
                    .First(cc => cc.Command.Matches(counterCommand));

            commandCounter.Count++;

            PersistenceService.SaveCounterData(_counterData);

            return command.GetResponse(_twitchBotSettings.BotDisplayName, "",
                counterCommand, commandCounter.Count.ToString("N0"));
        }

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