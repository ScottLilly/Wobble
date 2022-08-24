using System;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Enums;
using Wobble.Core;
using Wobble.Models.CustomEventArgs;
using Wobble.Models.TwitchEventHandler;

namespace Wobble.Models;

public class TwitchPubSubConnector
{
    private readonly TwitchAccount _broadcasterAccount;
    private readonly TwitchPubSub _chatClient = new();
    private string ChannelName => _broadcasterAccount.Name;

    public event EventHandler<LogMessageEventArgs> OnMessageToLog;

    public TwitchPubSubConnector(BotSettings botSettings)
    {
        _broadcasterAccount = botSettings.TwitchBroadcasterAccount;

        var connectionCredentials = 
            new ConnectionCredentials(
                _broadcasterAccount.Name,
                _broadcasterAccount.AuthToken,
                disableUsernameCheck: true);
    }

    public void Start()
    {
        SubscribeToEvents();
        Connect();
    }

    public void Stop()
    {
        RaiseLogMessage("[TwitchPubSubConnector] Stopping");
        UnsubscribeFromEvents();
        _chatClient.Disconnect();
        RaiseLogMessage("[TwitchPubSubConnector] Stopped");
    }

    private void Connect()
    {
        RaiseLogMessage("[TwitchPubSubConnector] Start connecting");
        _chatClient.Connect();
    }

    private void SubscribeToEvents()
    {
        //_twitchEventWatcherClient.OnPubSubServiceConnected +=
        //    OnMonitorTwitchEventsServiceConnected;
        //_twitchEventWatcherClient.OnChannelPointsRewardRedeemed +=
        //    OnChannelPointsRewardRedeemed;
    }

    private void UnsubscribeFromEvents()
    {
        //_twitchChatClient.OnRaidNotification -= HandleRaidNotification;
        //_twitchChatClient.OnBeingHosted -= HandleBeingHosted;
        //_twitchChatClient.OnNewSubscriber -= HandleNewSubscriber;
        //_twitchChatClient.OnReSubscriber -= HandleReSubscriber;
        //_twitchChatClient.OnGiftedSubscription -= HandleGiftedSubscription;
    }

    private void HandleRaidNotification(object sender, OnRaidNotificationArgs e)
    {
        //WriteToChatLog("WobbleBot",
        //    $"Received raid from: {e.RaidNotification.DisplayName}");
        //var eventMessage =
        //    _twitchEventHandlers.FirstOrDefault(t => t.EventName.Matches("Raid"));

        //if (eventMessage == null)
        //{
        //    ChatConnector.SendChatMessage($"{e.RaidNotification.DisplayName}, thank you for raiding!");
        //}
        //else
        //{
        //    SendChatMessage(eventMessage.Message
        //        .Replace("{raiderDisplayName}", e.RaidNotification.DisplayName));
        //}
    }

    private void HandleBeingHosted(object sender, OnBeingHostedArgs e)
    {
        //WriteToChatLog("WobbleBot",
        //    $"Received host from: {e.BeingHostedNotification.HostedByChannel}");
        //var eventMessage =
        //    _twitchEventHandlers.FirstOrDefault(t => t.EventName.Matches("Host"));

        //if (eventMessage == null)
        //{
        //    SendChatMessage($"Thank you for hosting {e.BeingHostedNotification.HostedByChannel}!");
        //}
        //else
        //{
        //    SendChatMessage(eventMessage.Message
        //        .Replace("{hostChannelName}", e.BeingHostedNotification.HostedByChannel));
        //}
    }

    private void HandleNewSubscriber(object sender, OnNewSubscriberArgs e)
    {
        //SendChatMessage(e.Subscriber.SubscriptionPlan == SubscriptionPlan.Prime
        //    ? $"{e.Subscriber.DisplayName}, thank you for subscribing with Prime!"
        //    : $"{e.Subscriber.DisplayName}, thank you for subscribing!");
    }

    private void HandleReSubscriber(object sender, OnReSubscriberArgs e)
    {
        //SendChatMessage(e.ReSubscriber.SubscriptionPlan == SubscriptionPlan.Prime
        //    ? $"{e.ReSubscriber.DisplayName}, thank you for re-subscribing with Prime!"
        //    : $"{e.ReSubscriber.DisplayName}, thank you for re-subscribing!");
    }

    private void HandleGiftedSubscription(object sender, OnGiftedSubscriptionArgs e)
    {
        //SendChatMessage($"Welcome to the channel {e.GiftedSubscription.MsgParamRecipientDisplayName}!");
    }

    private void OnMonitorTwitchEventsServiceConnected(object sender, EventArgs e)
    {
        //string channelId =
        //    ApiHelpers.GetChannelId(_botSettings.TwitchBroadcasterAccount.AuthToken,
        //        _botSettings.TwitchBroadcasterAccount.Name);

        //_twitchEventWatcherClient.ListenToChannelPoints(channelId);
        //_twitchEventWatcherClient.SendTopics(_botSettings.TwitchBroadcasterAccount.AuthToken);

        //LogMessage("Connected to PubSub");
    }

    private void OnChannelPointsRewardRedeemed(object sender,
        TwitchLib.PubSub.Events.OnChannelPointsRewardRedeemedArgs e)
    {
        //Console.WriteLine($"{e.RewardRedeemed.Redemption.Reward.Cost} channel points redeemed ");
    }

    private void RaiseLogMessage(string message,
        Enums.LogMessageLevel level = Enums.LogMessageLevel.Information)
    {
        OnMessageToLog?.Invoke(this, new LogMessageEventArgs(level, message));
    }
}