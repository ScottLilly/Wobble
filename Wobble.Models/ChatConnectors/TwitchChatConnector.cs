using System;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;
using Wobble.Models.CustomEventArgs;

namespace Wobble.Models.ChatConnectors;

public class TwitchChatConnector
{
    private readonly TimeSpan _limitedChattersTimeSpan = new(0, 10, 0);
    private readonly BotSettings _botSettings;
    private readonly TwitchClient _chatClient = new();

    private bool _hasConnected;

    private string ChannelName =>
        _botSettings.TwitchBroadcasterAccount.Name;

    public event EventHandler<TwitchChatCommandArgs> OnTwitchChatCommandReceived;
    public event EventHandler<TwitchChatMessageArgs> OnTwitchChatMessageReceived;
    public event EventHandler<LogMessageEventArgs> OnLogMessageRaised;

    public TwitchChatConnector(BotSettings botSettings)
    {
        _botSettings = botSettings;

        var connectionCredentials =
            new ConnectionCredentials(
                _botSettings.TwitchBotAccount.Name,
                _botSettings.TwitchBotAccount.AuthToken,
                disableUsernameCheck: true);

        _chatClient.Initialize(connectionCredentials, ChannelName);
    }

    #region Public functions

    public void Start()
    {
        RaiseLogMessage("Starting");
        SubscribeToEvents();
        Connect();
        RaiseLogMessage("Started");
    }

    public void Stop()
    {
        RaiseLogMessage("Stopping");
        UnsubscribeFromEvents();
        _chatClient.Disconnect();
        RaiseLogMessage("Stopped");
    }

    public void SendMessage(string message)
    {
        _chatClient.SendMessage(ChannelName, message);
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

        RaiseLogMessage("Connection complete");
    }

    private void HandleDisconnection(object sender,
        OnDisconnectedEventArgs e)
    {
        RaiseLogMessage("Disconnected", Enums.LogMessageLevel.Error);

        // Attempt to reconnect
        Connect();
    }

    private void HandleChatMessageReceived(object sender,
        OnMessageReceivedArgs e)
    {
        OnTwitchChatMessageReceived?.Invoke(this, 
            new TwitchChatMessageArgs(
                e.ChatMessage.UserId,
                e.ChatMessage.DisplayName, 
                e.ChatMessage.Message));
    }

    private void HandleChatCommandReceived(object sender,
        OnChatCommandReceivedArgs e)
    {
        OnTwitchChatCommandReceived?.Invoke(this, 
            new TwitchChatCommandArgs(
                e.Command.ChatMessage.UserId,
                e.Command.ChatMessage.DisplayName, 
                e.Command.CommandText,
                e.Command.ArgumentsAsString,
                e.Command.ChatMessage.IsBroadcaster,
                e.Command.ChatMessage.IsModerator,
                e.Command.ChatMessage.IsVip));
    }

    #endregion

    #region Private support functions

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
        RaiseLogMessage("Start connecting");

        if (_hasConnected)
        {
            _chatClient.Reconnect();
        }
        else
        {
            _chatClient.Connect();
        }

        RaiseLogMessage("Connected");
    }

    private void RaiseLogMessage(string message,
        Enums.LogMessageLevel level = Enums.LogMessageLevel.Information)
    {
        OnLogMessageRaised?.Invoke(this, 
            new LogMessageEventArgs(level, $"[TwitchChatConnector] {message}"));
    }

    #endregion
}