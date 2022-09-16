using System;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;
using Wobble.Core;
using Wobble.Models.CustomEventArgs;

namespace Wobble.Models.ChatConnectors;

public class TwitchPubSubConnector
{
    private readonly TwitchAccount _broadcasterAccount;
    private readonly TwitchPubSub _pubSubClient;
    private string ChannelName => _broadcasterAccount.Name;

    public event EventHandler<LogMessageEventArgs> OnMessageToLog;

    public TwitchPubSubConnector(BotSettings botSettings)
    {
        _broadcasterAccount = botSettings.TwitchBroadcasterAccount;

        var channelId =
            ApiHelpers.GetChannelId(_broadcasterAccount.AuthToken, ChannelName);

        _pubSubClient = new TwitchPubSub();

        SubscribeToEvents();

        _pubSubClient.ListenToFollows(channelId);
        _pubSubClient.ListenToSubscriptions(channelId);
        _pubSubClient.ListenToChannelPoints(channelId);
        _pubSubClient.ListenToBitsEventsV2(channelId);
    }

    public void Start()
    {
        RaiseLogMessage("Starting");
        Connect();
        RaiseLogMessage("Started");
    }

    public void Stop()
    {
        RaiseLogMessage("Stopping");
        UnsubscribeFromEvents();
        _pubSubClient.Disconnect();
        RaiseLogMessage("Stopped");
    }

    #region Private TwitchPubSub event handler functions

    private void HandleListenResponse(object sender, OnListenResponseArgs e)
    {
        RaiseLogMessage($"Listen response: {e.Response.Successful} {e.Response.Error}");
    }

    private void HandleServiceConnected(object sender, EventArgs e)
    {
        _pubSubClient.SendTopics(_broadcasterAccount.PubSubToken);

        RaiseLogMessage("Topics sent");
    }

    private void HandleServiceClosed(object sender, EventArgs e)
    {
        RaiseLogMessage("Service closed");
    }

    private void HandleServiceError(object sender, OnPubSubServiceErrorArgs e)
    {
        RaiseLogMessage($"Service error: {e.Exception.Message}");
    }

    private void HandleFollow(object sender, OnFollowArgs e)
    {
        RaiseLogMessage($"Follow: {e.DisplayName}");
    }

    private void HandleSubscription(object sender,
        OnChannelSubscriptionArgs e)
    {
        RaiseLogMessage($"Subscribed: {e.Subscription.Username}");
    }

    private void HandleChannelPointsRewardRedeemed(object sender,
        OnChannelPointsRewardRedeemedArgs e)
    {
        RaiseLogMessage($"Channel points redeemed: {e.RewardRedeemed.Redemption.User.DisplayName} {e.RewardRedeemed.Redemption.Reward.Title} {e.RewardRedeemed.Redemption.Reward.Cost}");
    }

    private void HandleBitsReceivedV2(object sender,
        OnBitsReceivedV2Args e)
    {
        RaiseLogMessage($"Bits received: {e.UserName} {e.BitsUsed}");
    }

    #endregion

    #region Private support functions

    private void SubscribeToEvents()
    {
        _pubSubClient.OnPubSubServiceConnected += HandleServiceConnected;
        _pubSubClient.OnPubSubServiceClosed += HandleServiceClosed;
        _pubSubClient.OnPubSubServiceError += HandleServiceError;

        _pubSubClient.OnListenResponse += HandleListenResponse;

        _pubSubClient.OnFollow += HandleFollow;
        _pubSubClient.OnChannelSubscription += HandleSubscription;
        _pubSubClient.OnChannelPointsRewardRedeemed += HandleChannelPointsRewardRedeemed;
        _pubSubClient.OnBitsReceivedV2 += HandleBitsReceivedV2;
    }

    private void UnsubscribeFromEvents()
    {
        _pubSubClient.OnPubSubServiceConnected -= HandleServiceConnected;
        _pubSubClient.OnPubSubServiceClosed -= HandleServiceClosed;
        _pubSubClient.OnPubSubServiceError -= HandleServiceError;

        _pubSubClient.OnListenResponse -= HandleListenResponse;

        _pubSubClient.OnFollow -= HandleFollow;
        _pubSubClient.OnChannelSubscription -= HandleSubscription;
        _pubSubClient.OnChannelPointsRewardRedeemed -= HandleChannelPointsRewardRedeemed;
        _pubSubClient.OnBitsReceivedV2 -= HandleBitsReceivedV2;
    }

    private void Connect()
    {
        RaiseLogMessage("Start connecting");

        _pubSubClient.Connect();

        RaiseLogMessage("Connection completed");
    }

    private void RaiseLogMessage(string message,
        Enums.LogMessageLevel level = Enums.LogMessageLevel.Information)
    {
        OnMessageToLog?.Invoke(this,
            new LogMessageEventArgs(level, $"[TwitchPubSubConnector] {message}"));
    }

    #endregion
}