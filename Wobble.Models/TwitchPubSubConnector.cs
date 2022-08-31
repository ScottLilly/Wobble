using System;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;
using Wobble.Core;
using Wobble.Models.CustomEventArgs;

namespace Wobble.Models;

public class TwitchPubSubConnector
{
    private readonly TwitchAccount _broadcasterAccount;
    private readonly TwitchPubSub _pubSubClient;
    private readonly string _channelId;
    private string ChannelName => _broadcasterAccount.Name;

    public event EventHandler<LogMessageEventArgs> OnMessageToLog;

    public TwitchPubSubConnector(BotSettings botSettings)
    {
        _broadcasterAccount = botSettings.TwitchBroadcasterAccount;
        _channelId =
            ApiHelpers.GetChannelId(_broadcasterAccount.AuthToken,
                ChannelName);

        _pubSubClient = new TwitchPubSub();
    }

    public void Start()
    {
        SubscribeToEvents();
        Connect();
    }

    public void Stop()
    {
        RaiseLogMessage("Stopping");
        UnsubscribeFromEvents();
        _pubSubClient.Disconnect();
        RaiseLogMessage("Stopped");
    }

    private void SubscribeToEvents()
    {
        _pubSubClient.OnPubSubServiceConnected += HandlePubSubServiceConnected;
        _pubSubClient.OnStreamUp += HandleStreamUp;
        _pubSubClient.OnStreamDown += HandleStreamDown;
        _pubSubClient.OnBitsReceivedV2 += HandleBitsReceivedV2;
        _pubSubClient.OnChannelSubscription += HandleChannelSubscription;
        _pubSubClient.OnChannelPointsRewardRedeemed += HandleChannelPointsRewardRedeemed;
        _pubSubClient.OnFollow += HandleFollow;
        _pubSubClient.OnHost += HandleHost;
        _pubSubClient.OnListenResponse += HandleListenResponse;
    }

    private void UnsubscribeFromEvents()
    {
        _pubSubClient.OnPubSubServiceConnected -= HandlePubSubServiceConnected;
        _pubSubClient.OnStreamUp -= HandleStreamUp;
        _pubSubClient.OnStreamDown -= HandleStreamDown;
        _pubSubClient.OnBitsReceivedV2 -= HandleBitsReceivedV2;
        _pubSubClient.OnChannelSubscription -= HandleChannelSubscription;
        _pubSubClient.OnChannelPointsRewardRedeemed -= HandleChannelPointsRewardRedeemed;
        _pubSubClient.OnFollow -= HandleFollow;
        _pubSubClient.OnHost -= HandleHost;
        _pubSubClient.OnListenResponse -= HandleListenResponse;
    }

    private void Connect()
    {
        RaiseLogMessage("Start connecting");

        _pubSubClient.ListenToBitsEventsV2(_channelId);
        _pubSubClient.ListenToChannelPoints(_channelId);
        _pubSubClient.ListenToFollows(_channelId);
        _pubSubClient.ListenToSubscriptions(_channelId);
        _pubSubClient.ListenToRaid(_channelId);
        _pubSubClient.SendTopics(_broadcasterAccount.AuthToken);

        _pubSubClient.Connect();

        RaiseLogMessage("Connection completed");
    }

    #region Private TwitchPubSub event handler functions

    private void HandlePubSubServiceConnected(object sender, EventArgs e)
    {
        RaiseLogMessage("Connected");
    }

    private void HandleStreamUp(object sender, OnStreamUpArgs e)
    {
        RaiseLogMessage("Stream up");
    }

    private void HandleStreamDown(object sender, OnStreamDownArgs e)
    {
        RaiseLogMessage("Stream down");
    }

    private void HandleListenResponse(object sender, 
        OnListenResponseArgs e)
    {
        RaiseLogMessage("Stream down");
    }

    private void HandleBitsReceivedV2(object sender, 
        OnBitsReceivedV2Args e)
    {
        RaiseLogMessage("Bits received");
    }

    private void HandleChannelSubscription(object sender, 
        OnChannelSubscriptionArgs e)
    {
        RaiseLogMessage("Channel subscription");
    }

    private void HandleChannelPointsRewardRedeemed(object sender, 
        OnChannelPointsRewardRedeemedArgs e)
    {
        RaiseLogMessage("Channel points redeemed");
    }

    private void HandleFollow(object sender, OnFollowArgs e)
    {
        RaiseLogMessage("Follow");
    }

    private void HandleHost(object sender, OnHostArgs e)
    {
        RaiseLogMessage("Host");
    }

    #endregion

    #region Private support functions

    private void RaiseLogMessage(string message,
        Enums.LogMessageLevel level = Enums.LogMessageLevel.Information)
    {
        OnMessageToLog?.Invoke(this, 
            new LogMessageEventArgs(level, $"[TwitchPubSubConnector] {message}"));
    }

    #endregion
}