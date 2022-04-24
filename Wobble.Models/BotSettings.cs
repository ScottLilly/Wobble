using System.Collections.Generic;
using Wobble.Core;
using Wobble.Models.ChatCommandHandlers;

namespace Wobble.Models;

public class BotSettings
{
    public string ChannelName { get; }
    public string BotAccountName { get; }
    public string BotDisplayName { get; }
    public bool HandleHostRaidSubscriptionEvents { get; }
    public string Token { get; }
    public TimedMessages TimedMessages { get; }

    public List<WobbleCommand> WobbleCommands { get; } =
        new List<WobbleCommand>();
    public List<ChatResponse> ChatCommands { get; } =
        new List<ChatResponse>();
    public List<CounterResponse> CounterCommands { get; } =
        new List<CounterResponse>();

    public BotSettings(WobbleConfiguration wobbleConfiguration, string userSecretsToken)
    {
        ChannelName = wobbleConfiguration.ChannelName;
        BotAccountName = wobbleConfiguration.BotAccountName;
        BotDisplayName = wobbleConfiguration.BotDisplayName;
        HandleHostRaidSubscriptionEvents = wobbleConfiguration.HandleHostRaidSubscriptionEvents;
        TimedMessages = wobbleConfiguration.TimedMessages;

        // Get Twitch token from appsettings.json, if present.
        // Otherwise, this is in development and the token should be in user secrets.
        Token = wobbleConfiguration.TwitchToken.IsNotNullEmptyOrWhiteSpace()
            ? wobbleConfiguration.TwitchToken
            : userSecretsToken;

        WobbleCommands.AddRange(wobbleConfiguration.WobbleCommands);

        foreach (ChatMessage message in wobbleConfiguration.ChatMessages)
        {
            ChatCommands.Add(new ChatResponse(message.TriggerWords, message.Responses));
        }

        foreach (CounterMessage message in wobbleConfiguration.CounterMessages)
        {
            CounterCommands.Add(new CounterResponse(message.TriggerWords, message.Responses));
        }
    }
}