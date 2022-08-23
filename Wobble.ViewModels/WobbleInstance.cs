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
using TwitchLib.PubSub;
using Wobble.Core;
using Wobble.Models;
using Wobble.Models.ChatCommandHandlers;
using Wobble.Models.TwitchEventHandler;
using Wobble.Services;
using static System.StringComparison;

namespace Wobble.ViewModels;

public class WobbleInstance : INotifyPropertyChanged
{
    private const string CHAT_LOG_DIRECTORY = "./ChatLogs";

    private readonly BotSettings _botSettings;

    private readonly TwitchPubSub _twitchEventWatcherClient = new();
    private readonly TwitchClient _twitchChatClient = new();
    private readonly CounterData _counterData;
    private readonly WobblePointsData _wobblePointsData;

    // Credentials used for client that "speaks" in chat
    private readonly ConnectionCredentials _twitchBotCredentials;
    private SpeechService _speechService;

    private readonly List<string> _automatedShoutOutsPerformedFor = new();

    private readonly List<IChatCommandHandler> _chatCommandHandlers = new();

    private readonly List<ITwitchEventHandler> _twitchEventHandlers = new();

    private Timer _timedMessagesTimer;

    private readonly TimeSpan _limitedChattersTimeSpan = new(0, 10, 0);

    public List<ChatResponse> AdditionalCommands =>
        _chatCommandHandlers.OfType<ChatResponse>()
        .Where(c => c.IsAdditionalCommand)
        .ToList();

    public event PropertyChangedEventHandler PropertyChanged;

    public WobbleInstance(BotSettings botSettings)
    {
        _botSettings = botSettings;

        _counterData = PersistenceService.GetCounterData();
        _wobblePointsData = PersistenceService.GetWobblePointsData();

        SetupAzureTtsService();

        var twitchBroadcasterCredentials = new ConnectionCredentials(
            _botSettings.TwitchBroadcasterAccount.Name, 
            _botSettings.TwitchBroadcasterAccount.AuthToken, 
            disableUsernameCheck: true);

        _twitchBotCredentials =
            _botSettings.TwitchBotAccount == null
                ? twitchBroadcasterCredentials
                : new ConnectionCredentials(
                    _botSettings.TwitchBotAccount.Name,
                    _botSettings.TwitchBotAccount.AuthToken,
                    disableUsernameCheck: true);

        _twitchChatClient.OnChatCommandReceived += HandleChatCommandReceived;
        _twitchChatClient.OnMessageReceived += HandleChatMessageReceived;
        _twitchChatClient.OnDisconnected += HandleDisconnected;

        PopulateChatCommandHandlers();

        _twitchEventHandlers.Clear();
        _twitchEventHandlers.AddRange(_botSettings.TwitchEventResponses);
        
        InitializeTimedMessages();

        Connect();

        _twitchEventWatcherClient.OnPubSubServiceConnected += 
            OnMonitorTwitchEventsServiceConnected;
        _twitchEventWatcherClient.OnChannelPointsRewardRedeemed += 
            OnChannelPointsRewardRedeemed;
        _twitchEventWatcherClient.Connect();
    }

    private void SetupAzureTtsService()
    {
        if (_botSettings.AzureTtsAccount.Key.IsNotNullEmptyOrWhiteSpace() &&
            _botSettings.AzureTtsAccount.Region.IsNotNullEmptyOrWhiteSpace())
        {
            _speechService =
                new SpeechService(_botSettings.AzureTtsAccount.Key,
                    _botSettings.AzureTtsAccount.Region,
                    _botSettings.AzureTtsAccount.AzureTtsVoiceName);
        }
    }

    private void OnMonitorTwitchEventsServiceConnected(object sender, EventArgs e)
    {
        string channelId =
            ApiHelpers.GetChannelId(_botSettings.TwitchBroadcasterAccount.AuthToken, 
                _botSettings.TwitchBroadcasterAccount.Name);

        _twitchEventWatcherClient.ListenToChannelPoints(channelId);
        _twitchEventWatcherClient.SendTopics(_botSettings.TwitchBroadcasterAccount.AuthToken);

        LogMessage("Connected to PubSub");
    }

    private void OnChannelPointsRewardRedeemed(object sender,
        TwitchLib.PubSub.Events.OnChannelPointsRewardRedeemedArgs e)
    {
        Console.WriteLine($"{e.RewardRedeemed.Redemption.Reward.Cost} channel points redeemed ");
    }

    public void DisplayCommands()
    {
        DisplayCommandTriggerWords();
    }

    public void ClearChat()
    {
        _twitchChatClient.ClearChat(_botSettings.TwitchBroadcasterAccount.Name);
    }

    public void Speak(string message)
    {
        _speechService?.SpeakAsync(message);
    }

    public void Disconnect()
    {
        // TODO: Find a better place to update WobblePointsData file
        PersistenceService.SaveWobblePointsData(_wobblePointsData);

        if (_botSettings.HandleHostRaidSubscriptionEvents)
        {
            UnsubscribeFromHostRaidSubscriptionEvents();
        }

        _twitchChatClient.Disconnect();
    }

    #region Public stream management functions

    public void EmoteModeOnlyOn()
    {
        _twitchChatClient.EmoteOnlyOn(_botSettings.TwitchBroadcasterAccount.Name);
    }

    public void EmoteModeOnlyOff()
    {
        _twitchChatClient.EmoteOnlyOff(_botSettings.TwitchBroadcasterAccount.Name);
    }

    public void FollowersOnlyOn()
    {
        _twitchChatClient.FollowersOnlyOn(_botSettings.TwitchBroadcasterAccount.Name,
            _limitedChattersTimeSpan);
    }

    public void FollowersOnlyOff()
    {
        _twitchChatClient.FollowersOnlyOff(_botSettings.TwitchBroadcasterAccount.Name);
    }

    public void SubscribersOnlyOn()
    {
        _twitchChatClient.SubscribersOnlyOn(_botSettings.TwitchBroadcasterAccount.Name);
    }

    public void SubscribersOnlyOff()
    {
        _twitchChatClient.SubscribersOnlyOff(_botSettings.TwitchBroadcasterAccount.Name);
    }

    public void SlowModeOn()
    {
        _twitchChatClient.SlowModeOn(_botSettings.TwitchBroadcasterAccount.Name, 
            _limitedChattersTimeSpan);
    }

    public void SlowModeOff()
    {
        _twitchChatClient.SlowModeOff(_botSettings.TwitchBroadcasterAccount.Name);
    }

    #endregion

    #region Private connection functions

    private void Connect()
    {
        _twitchChatClient.Initialize(_twitchBotCredentials, 
            _botSettings.TwitchBroadcasterAccount.Name);
        _twitchChatClient.Connect();

    }

    private void HandleDisconnected(object sender, OnDisconnectedEventArgs e)
    {
        // If disconnected, automatically attempt to reconnect
        Connect();
    }

    private void UnsubscribeFromHostRaidSubscriptionEvents()
    {
        _twitchChatClient.OnRaidNotification -= HandleRaidNotification;
        _twitchChatClient.OnBeingHosted -= HandleBeingHosted;
        _twitchChatClient.OnNewSubscriber -= HandleNewSubscriber;
        _twitchChatClient.OnReSubscriber -= HandleReSubscriber;
        _twitchChatClient.OnGiftedSubscription -= HandleGiftedSubscription;
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
                SendChatMessage(command.GetResponse(_botSettings.TwitchBotAccount.Name, null));
            }
        }
        else
        {
            SendChatMessage(message);
        }
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
            SendChatMessage(command.GetResponse(_botSettings.TwitchBotAccount.Name, e.Command));

            if (command is AddCommand)
            {
                PersistenceService.SaveAdditionalCommands(AdditionalCommands);
            }
        }
    }

    private void HandleRaidNotification(object sender, OnRaidNotificationArgs e)
    {
        WriteToChatLog("WobbleBot", 
            $"Received raid from: {e.RaidNotification.DisplayName}");

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
        WriteToChatLog("WobbleBot", 
            $"Received host from: {e.BeingHostedNotification.HostedByChannel}");

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

        if (_botSettings.AutomatedShoutOuts.Any(n => n.Matches(e.ChatMessage.DisplayName)) &&
            !_automatedShoutOutsPerformedFor.Contains(e.ChatMessage.DisplayName))
        {
            _automatedShoutOutsPerformedFor.Add(e.ChatMessage.DisplayName);
            SendChatMessage($"!so {e.ChatMessage.DisplayName}");
        }
    }

    private void LogMessage(string message)
    {
        Console.WriteLine($"[Wobble]: {message}");
        WriteToChatLog("Wobble", message);
    }

    private static void WriteToChatLog(string chatterName, string message)
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
        _chatCommandHandlers.Add(new RemoveCommand(_chatCommandHandlers));
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

        return command.GetResponse(_botSettings.TwitchBotAccount.Name, null)
            .Replace("{counter}", commandCounter.Count.ToString("N0"));
    }

    private void SendChatMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        _twitchChatClient.SendMessage(_botSettings.TwitchBroadcasterAccount.Name, message);
        WriteToChatLog(_botSettings.TwitchBotAccount.Name, message);
    }

    #endregion
}