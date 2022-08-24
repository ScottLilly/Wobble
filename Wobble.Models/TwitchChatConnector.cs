using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;
using Wobble.Core;
using Wobble.Models.ChatCommandHandlers;
using Wobble.Models.CustomEventArgs;

namespace Wobble.Models;

public class TwitchChatConnector
{
    private readonly TimeSpan _limitedChattersTimeSpan = new(0, 10, 0);

    private readonly BotSettings _botSettings;
    private readonly TwitchClient _chatClient = new();
    private readonly List<string> _automatedShoutOutsPerformedFor = new();
    private readonly List<IChatCommandHandler> _chatCommandHandlers = new();

    private Timer _timedMessagesTimer;
    private bool _hasConnected;

    private string ChannelName => 
        _botSettings.TwitchBroadcasterAccount.Name;
    private string BotName => 
        _botSettings.TwitchBotAccount.Name;

    public CounterData CounterData { get; }
    public WobblePointsData WobblePointsData { get; }

    public List<ChatResponse> AdditionalCommands =>
        _chatCommandHandlers.OfType<ChatResponse>()
            .Where(c => c.IsAdditionalCommand)
            .ToList();

    public event EventHandler<LogMessageEventArgs> OnLogMessageRaised;
    public event EventHandler OnCounterDataChanged;
    public event EventHandler OnWobblePointsDataChanged;

    public TwitchChatConnector(BotSettings botSettings, 
        CounterData counterData, WobblePointsData wobblePointsData)
    {
        _botSettings = botSettings;

        CounterData = counterData;
        WobblePointsData = wobblePointsData;

        var connectionCredentials = 
            new ConnectionCredentials(
                _botSettings.TwitchBotAccount.Name,
                _botSettings.TwitchBotAccount.AuthToken,
                disableUsernameCheck: true);

        _chatClient.Initialize(connectionCredentials, ChannelName);

        PopulateChatCommandHandlers();
        InitializeTimedMessages();
    }

    #region Public functions

    public void Start()
    {
        SubscribeToEvents();
        Connect();
    }

    public void Stop()
    {
        RaiseLogMessage("[TwitchChatConnector] Stopping");
        UnsubscribeFromEvents();
        _chatClient.Disconnect();
        RaiseLogMessage("[TwitchChatConnector] Stopped");
    }

    public void DisplayCommandTriggerWords()
    {
        var triggerWords =
            string.Join(", ", 
                _chatCommandHandlers
                    .SelectMany(cch => cch.CommandTriggers)
                    .Select(cch => $"!{cch.ToLowerInvariant()}")
                    .OrderBy(cch => cch));

        SendChatMessage($"Available commands: {triggerWords}");
    }

    #endregion

    #region Public stream management functions

    public void ClearChat()
    {
        _chatClient.ClearChat(ChannelName);
    }

    public void EmoteModeOnlyOn()
    {
        _chatClient.EmoteOnlyOn(ChannelName);
    }

    public void EmoteModeOnlyOff()
    {
        _chatClient.EmoteOnlyOff(ChannelName);
    }

    public void FollowersOnlyOn()
    {
        _chatClient.FollowersOnlyOn(ChannelName, _limitedChattersTimeSpan);
    }

    public void FollowersOnlyOff()
    {
        _chatClient.FollowersOnlyOff(ChannelName);
    }

    public void SubscribersOnlyOn()
    {
        _chatClient.SubscribersOnlyOn(ChannelName);
    }

    public void SubscribersOnlyOff()
    {
        _chatClient.SubscribersOnlyOff(ChannelName);
    }

    public void SlowModeOn()
    {
        _chatClient.SlowModeOn(ChannelName, _limitedChattersTimeSpan);
    }

    public void SlowModeOff()
    {
        _chatClient.SlowModeOff(ChannelName);
    }

    #endregion

    #region Twitch events monitored by WobbleInstance

    private void HandleConnectionCompleted(object sender,
        OnConnectedArgs e)
    {
        _hasConnected = true;

        RaiseLogMessage("[TwitchChatConnector] Connection complete");
    }

    private void HandleDisconnection(object sender,
        OnDisconnectedEventArgs e)
    {
        RaiseLogMessage("[TwitchChatConnector] Disconnected",
            Enums.LogMessageLevel.Error);

        // Attempt to reconnect
        Connect();
    }

    private void HandleTimedMessagesTimerElapsed(object sender, 
        ElapsedEventArgs e)
    {
        var message =
            _botSettings.TimedMessages.Messages.RandomElement();

        if (message.StartsWith("!"))
        {
            IChatCommandHandler chatCommandHandler =
                GetChatCommandHandler(message.Substring(1));

            if (chatCommandHandler != null)
            {
                SendChatMessage(chatCommandHandler.GetResponse(BotName, null));
            }
        }
        else
        {
            SendChatMessage(message);
        }
    }

    private void HandleChatMessageReceived(object sender, 
        OnMessageReceivedArgs e)
    {
        RaiseLogMessage($"[{e.ChatMessage.DisplayName}] {e.ChatMessage.Message}");

        if (_botSettings.AutomatedShoutOuts.Any(n => n.Matches(e.ChatMessage.DisplayName)) &&
            !_automatedShoutOutsPerformedFor.Contains(e.ChatMessage.DisplayName))
        {
            _automatedShoutOutsPerformedFor.Add(e.ChatMessage.DisplayName);
            SendChatMessage($"!so {e.ChatMessage.DisplayName}");
        }
    }

    private void HandleChatCommandReceived(object sender, 
        OnChatCommandReceivedArgs e)
    {
        if (e.Command.CommandText.Matches("commands"))
        {
            DisplayCommandTriggerWords();
            return;
        }

        IChatCommandHandler command = 
            GetChatCommandHandler(e.Command.CommandText);

        if (command == null)
        {
            return;
        }

        if (command is CounterResponse)
        {
            SendChatMessage(GetCounterCommandResponse(e.Command.CommandText));
            return;
        }

        SendChatMessage(command.GetResponse(BotName, e.Command));

        if (command is AddCommand)
        {
            OnWobblePointsDataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    #endregion

    #region Private support functions

    private void InitializeTimedMessages()
    {
        if (!(_botSettings.TimedMessages?.IntervalInMinutes > 0))
        {
            return;
        }

        _timedMessagesTimer =
            new Timer(_botSettings.TimedMessages.IntervalInMinutes * 60 * 1000);
        _timedMessagesTimer.Elapsed += HandleTimedMessagesTimerElapsed;
        _timedMessagesTimer.Enabled = true;
    }

    private void SubscribeToEvents()
    {
        _chatClient.OnConnected += HandleConnectionCompleted;
        _chatClient.OnDisconnected += HandleDisconnection;
        _chatClient.OnMessageReceived += HandleChatMessageReceived;
        _chatClient.OnChatCommandReceived += HandleChatCommandReceived;
    }

    private void UnsubscribeFromEvents()
    {
        _chatClient.OnConnected -= HandleConnectionCompleted;
        _chatClient.OnDisconnected -= HandleDisconnection;
        _chatClient.OnMessageReceived -= HandleChatMessageReceived;
        _chatClient.OnChatCommandReceived -= HandleChatCommandReceived;
    }

    private void Connect()
    {
        RaiseLogMessage("[TwitchChatConnector] Start connecting");

        if (_hasConnected)
        {
            _chatClient.Reconnect();
        }
        else
        {
            _chatClient.Connect();
        }
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
            _chatCommandHandlers.Add(new MyPoints(WobblePointsData));
        }

        _chatCommandHandlers.Add(new AddCommand(_chatCommandHandlers));
        _chatCommandHandlers.Add(new RemoveCommand(_chatCommandHandlers));
    }

    private void SendChatMessage(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            RaiseLogMessage(message);

            _chatClient.SendMessage(ChannelName, message);
        }
    }

    private IChatCommandHandler GetChatCommandHandler(string commandText)
    {
        return _chatCommandHandlers.FirstOrDefault(cc =>
            cc.CommandTriggers.Any(ct => ct.Matches(commandText)));
    }

    private string GetCounterCommandResponse(string counterCommand)
    {
        IChatCommandHandler command = GetChatCommandHandler(counterCommand);

        if (!CounterData.CommandCounters.Any(cc => cc.Command.Matches(counterCommand)))
        {
            CounterData.CommandCounters.Add(new CommandCounter
            {
                Command = counterCommand,
                Count = 0
            });
        }

        CommandCounter commandCounter =
            CounterData.CommandCounters
                .First(cc => cc.Command.Matches(counterCommand));

        commandCounter.Count++;

        OnCounterDataChanged?.Invoke(this, EventArgs.Empty);

        return command.GetResponse(_botSettings.TwitchBotAccount.Name, null)
            .Replace("{counter}", commandCounter.Count.ToString("N0"));
    }

    private void RaiseLogMessage(string message, 
        Enums.LogMessageLevel level = Enums.LogMessageLevel.Information)
    {
        OnLogMessageRaised?.Invoke(this, new LogMessageEventArgs(level, message));
    }

    #endregion
}