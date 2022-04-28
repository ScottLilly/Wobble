using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
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
using Wobble.Models.TwitchEventHandler;
using Wobble.Services;
using static System.StringComparison;

namespace Wobble.ViewModels
{
    public class WobbleInstance : INotifyPropertyChanged
    {
        private const string CHAT_LOG_DIRECTORY = "./ChatLogs";

        private readonly TwitchClient _client = new();
        private readonly BotSettings _botSettings;
        private readonly CounterData _counterData;
        private readonly WobblePointsData _wobblePointsData;
        private readonly ConnectionCredentials _credentials;

        private readonly List<IChatCommandHandler> _chatCommandHandlers =
            new List<IChatCommandHandler>();

        private readonly List<ITwitchEventHandler> _twitchEventHandlers =
            new List<ITwitchEventHandler>();

        private Timer _timedMessagesTimer;

        private readonly TimeSpan _limitedChattersTimeSpan = new(0, 10, 0);

        public event PropertyChangedEventHandler PropertyChanged;

        public WobbleInstance(BotSettings botSettings)
        {
            _botSettings = botSettings;
            _counterData = PersistenceService.GetCounterData();
            _wobblePointsData = PersistenceService.GetWobblePointsData();

            _credentials =
                new ConnectionCredentials(
                    string.IsNullOrWhiteSpace(_botSettings.BotAccountName)
                        ? botSettings.ChannelName
                        : _botSettings.BotAccountName, _botSettings.Token, disableUsernameCheck: true);

            _client.OnChannelStateChanged += HandleChannelStateChanged;
            _client.OnChatCommandReceived += HandleChatCommandReceived;
            _client.OnMessageReceived += HandleChatMessageReceived;
            _client.OnDisconnected += HandleDisconnected;

            if (_botSettings.HandleHostRaidSubscriptionEvents)
            {
                _client.OnRaidNotification += HandleRaidNotification;
                _client.OnBeingHosted += HandleBeingHosted;
                //_client.OnNewSubscriber += HandleNewSubscriber;
                //_client.OnReSubscriber += HandleReSubscriber;
                //_client.OnGiftedSubscription += HandleGiftedSubscription;
            }

            PopulateChatCommandHandlers();

            _twitchEventHandlers.Clear();
            _twitchEventHandlers.AddRange(_botSettings.TwitchEventResponses);
            
            InitializeTimedMessages();

            Connect();
        }

        public void DisplayCommands()
        {
            DisplayCommandTriggerWords();
        }

        public void ClearChat()
        {
            _client.ClearChat(_botSettings.ChannelName);
        }

        public void Disconnect()
        {
            // TODO: Find a better place to update WobblePointsData file
            PersistenceService.SaveWobblePointsData(_wobblePointsData);
            _client.Disconnect();
        }

        #region Public stream management functions

        public void SetStreamTitle(string title)
        {
            // TODO
        }

        public void EmoteModeOnlyOn()
        {
            _client.EmoteOnlyOn(_botSettings.ChannelName);
        }

        public void EmoteModeOnlyOff()
        {
            _client.EmoteOnlyOff(_botSettings.ChannelName);
        }

        public void FollowersOnlyOn()
        {
            _client.FollowersOnlyOn(_botSettings.ChannelName, _limitedChattersTimeSpan);
        }

        public void FollowersOnlyOff()
        {
            _client.FollowersOnlyOff(_botSettings.ChannelName);
        }

        public void SubscribersOnlyOn()
        {
            _client.SubscribersOnlyOn(_botSettings.ChannelName);
        }

        public void SubscribersOnlyOff()
        {
            _client.SubscribersOnlyOff(_botSettings.ChannelName);
        }

        public void SlowModeOn()
        {
            _client.SlowModeOn(_botSettings.ChannelName, _limitedChattersTimeSpan);
        }

        public void SlowModeOff()
        {
            _client.SlowModeOff(_botSettings.ChannelName);
        }

        #endregion

        #region Private connection functions

        private void Connect()
        {
            _client.Initialize(_credentials, _botSettings.ChannelName);
            _client.Connect();
        }

        private void HandleDisconnected(object sender, OnDisconnectedEventArgs e)
        {
            // If disconnected, automatically attempt to reconnect
            Connect();
        }

        private void UnsubscribeFromHostRaidSubscriptionEvents()
        {
            _client.OnRaidNotification -= HandleRaidNotification;
            _client.OnBeingHosted -= HandleBeingHosted;
            //_client.OnNewSubscriber -= HandleNewSubscriber;
            //_client.OnReSubscriber -= HandleReSubscriber;
            //_client.OnGiftedSubscription -= HandleGiftedSubscription;
        }

        #endregion

        #region Private eventHandler functions

        private void TimedMessagesTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var message =
                _botSettings.TimedMessages.Messages.RandomElement();

            if (message.StartsWith("!"))
            {
                message = message.Substring(1);

                var command = GetCommand(message);

                if (command != null)
                {
                    SendChatMessage(command.GetResponse(_botSettings.BotDisplayName, null));
                }
            }
            else
            {
                SendChatMessage(message);
            }
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
                SendChatMessage(command.GetResponse(_botSettings.BotDisplayName, e.Command));
            }
        }

        private void HandleRaidNotification(object sender, OnRaidNotificationArgs e)
        {
            var eventMessage =
                _twitchEventHandlers.FirstOrDefault(t => t.EventName.Matches("Raid"));

            if (eventMessage == null)
            {
                SendChatMessage($"{e.RaidNotification.DisplayName}, thank you for raiding!");
            }
            else
            {
                SendChatMessage(eventMessage.Message
                    .Replace("{raiderDisplayName}", e.RaidNotification.DisplayName));
            }
        }

        private void HandleBeingHosted(object sender, OnBeingHostedArgs e)
        {
            var eventMessage =
                _twitchEventHandlers.FirstOrDefault(t => t.EventName.Matches("Host"));

            if (eventMessage == null)
            {
                SendChatMessage($"Thank you for hosting {e.BeingHostedNotification.HostedByChannel}!");
            }
            else
            {
                SendChatMessage(eventMessage.Message
                    .Replace("{hostChannelName}", e.BeingHostedNotification.HostedByChannel));
            }
        }

        private void HandleNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            SendChatMessage(e.Subscriber.SubscriptionPlan == SubscriptionPlan.Prime
                ? $"{e.Subscriber.DisplayName}, thank you for subscribing with Prime!"
                : $"{e.Subscriber.DisplayName}, thank you for subscribing!");
        }

        private void HandleReSubscriber(object sender, OnReSubscriberArgs e)
        {
            SendChatMessage(e.ReSubscriber.SubscriptionPlan == SubscriptionPlan.Prime
                ? $"{e.ReSubscriber.DisplayName}, thank you for re-subscribing with Prime!"
                : $"{e.ReSubscriber.DisplayName}, thank you for re-subscribing!");
        }

        private void HandleGiftedSubscription(object sender, OnGiftedSubscriptionArgs e)
        {
            SendChatMessage($"Welcome to the channel {e.GiftedSubscription.MsgParamRecipientDisplayName}!");
        }

        #endregion

        #region Private support functions

        private void HandleChatMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            WriteToChatLog(e.ChatMessage.DisplayName, e.ChatMessage.Message);
        }

        private void WriteToChatLog(string chatterName, string message)
        {
            if (!Directory.Exists(CHAT_LOG_DIRECTORY))
            {
                Directory.CreateDirectory(CHAT_LOG_DIRECTORY);
            }

            File.AppendAllText(
                Path.Combine(CHAT_LOG_DIRECTORY, $"Wobble-{DateTime.Now:yyyy-MM-dd}.log"),
                $"{DateTime.Now.ToShortTimeString()}: {chatterName} - {message}{Environment.NewLine}");
        }

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
            if (!(_botSettings.TimedMessages?.IntervalInMinutes > 0))
            {
                return;
            }

            _timedMessagesTimer =
                new Timer(_botSettings.TimedMessages.IntervalInMinutes * 60 * 1000);
            _timedMessagesTimer.Elapsed += TimedMessagesTimer_Elapsed;
            _timedMessagesTimer.Enabled = true;
        }

        private void PopulateChatCommandHandlers()
        {
            // Add commands populated from appsettings
            _chatCommandHandlers.AddRange(_botSettings.ChatCommands);
            _chatCommandHandlers.AddRange(_botSettings.CounterCommands);

            // TODO: Use Reflection to load IWobbleChatCommands
            // Add other commands
            if (_botSettings.WobbleCommands.FirstOrDefault(
                    c => c.Name.Matches("ChoiceMaker"))?.IsEnabled ?? false)
            {
                _chatCommandHandlers.Add(new ChoiceMaker());
            }
            if (_botSettings.WobbleCommands.FirstOrDefault(
                    c => c.Name.Matches("Roller"))?.IsEnabled ?? false)
            {
                _chatCommandHandlers.Add(new Roller());
            }
            if (_botSettings.WobbleCommands.FirstOrDefault(
                    c => c.Name.Matches("RockPaperScissorsGame"))?.IsEnabled ?? false)
            {
                _chatCommandHandlers.Add(new RockPaperScissorsGame());
            }
            if (_botSettings.WobbleCommands.FirstOrDefault(
                    c => c.Name.Matches("Lurk"))?.IsEnabled ?? false)
            {
                _chatCommandHandlers.Add(new Lurk());
            }
            if (_botSettings.WobbleCommands.FirstOrDefault(
                    c => c.Name.Matches("MyPoints"))?.IsEnabled ?? false)
            {
                _chatCommandHandlers.Add(new MyPoints(_wobblePointsData));
            }

            _chatCommandHandlers.Add(new AddCommand(_chatCommandHandlers));
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

            return command.GetResponse(_botSettings.BotDisplayName, null)
                .Replace("{counter}", commandCounter.Count.ToString("N0"));
        }

        private void SendChatMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            _client.SendMessage(_botSettings.ChannelName, message);
            WriteToChatLog(_botSettings.BotDisplayName, message);
        }

        private void GiveWobblePoints(string userId)
        {
            if (_wobblePointsData.UserPoints.None(up => up.Name.Matches(userId)))
            {
                _wobblePointsData.UserPoints.Add(new WobblePointsData.UserPoint
                {
                    Name = userId,
                    Points = 0
                });
            }

            _wobblePointsData.UserPoints.First(up => up.Name.Matches(userId)).Points += 10;
        }

        #endregion
    }
}