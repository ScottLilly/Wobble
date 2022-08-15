﻿using System;
using System.Collections.Generic;
using Wobble.Core;
using Wobble.Models.ChatCommandHandlers;
using Wobble.Models.TwitchEventHandler;

namespace Wobble.Models;

public class BotSettings
{
    public string ChannelName { get; }
    public string BotAccountName { get; }
    public string BotDisplayName { get; }
    public bool HandleHostRaidSubscriptionEvents { get; }
    public string Token { get; }
    public string AzureCognitiveServicesRegion { get; }
    public string AzureCognitiveServicesKey { get; }
    public string AzureTtsVoiceName { get; }
    public TimedMessages TimedMessages { get; }

    public List<WobbleCommand> WobbleCommands { get; } = new();
    public List<TwitchEventResponse> TwitchEventResponses { get; } = new();
    public List<ChatResponse> ChatCommands { get; } = new();
    public List<CounterResponse> CounterCommands { get; } = new();
    public List<string> AutomatedShoutOuts { get; }

    public BotSettings(WobbleConfiguration wobbleConfiguration, string userSecretsToken,
        string azureCognitiveServicesKey)
    {
        ChannelName = wobbleConfiguration.ChannelName;
        BotAccountName = wobbleConfiguration.BotAccountName;
        BotDisplayName = wobbleConfiguration.BotDisplayName;
        HandleHostRaidSubscriptionEvents = wobbleConfiguration.HandleHostRaidSubscriptionEvents;
        TimedMessages = wobbleConfiguration.TimedMessages;
        AutomatedShoutOuts = wobbleConfiguration.AutomatedShoutOuts;

        // Get Twitch token from appsettings.json, if present.
        // Otherwise, this is in development and the token should be in user secrets.
        Token = wobbleConfiguration.TwitchToken.IsNotNullEmptyOrWhiteSpace()
            ? wobbleConfiguration.TwitchToken
            : userSecretsToken;

        AzureCognitiveServicesRegion = 
            wobbleConfiguration.AzureCognitiveServicesRegion;
        AzureCognitiveServicesKey = 
            wobbleConfiguration.AzureCognitiveServicesKey.IsNotNullEmptyOrWhiteSpace()
            ? wobbleConfiguration.AzureCognitiveServicesKey
            : azureCognitiveServicesKey;
        AzureTtsVoiceName =
            wobbleConfiguration.AzureTtsVoiceName;

        WobbleCommands.AddRange(wobbleConfiguration.WobbleCommands);

        foreach (TwitchEventMessage message in wobbleConfiguration.TwitchEventMessages)
        {
            TwitchEventResponses.Add(new TwitchEventResponse(message.EventType, message.Message));
        }

        foreach (ChatMessage message in wobbleConfiguration.ChatMessages)
        {
            ChatCommands.Add(new ChatResponse(message.TriggerWords, message.Responses,
                message.IsAdditionalCommand, message.RequiresArgument, message.MissingArgumentMessage));
        }

        foreach (CounterMessage message in wobbleConfiguration.CounterMessages)
        {
            CounterCommands.Add(new CounterResponse(message.TriggerWords, message.Responses));
        }
    }
}