using System.Collections.Generic;
using System.Linq;
using Wobble.Core;
using Wobble.Models.ChatCommandHandlers;
using Wobble.Models.TwitchEventHandler;

namespace Wobble.Models;

public class BotSettings
{
    private readonly WobbleConfiguration _wobbleConfiguration;

    public TwitchAccount TwitchBroadcasterAccount =>
        _wobbleConfiguration.TwitchAccounts.First(a => a.Type.Matches("Broadcaster"));
    public TwitchAccount TwitchBotAccount =>
        _wobbleConfiguration.TwitchAccounts.FirstOrDefault(a => a.Type.Matches("Bot")) ??
        TwitchBroadcasterAccount;
    public List<AzureAccount> AzureAccounts =>
        _wobbleConfiguration.AzureAccounts;
    public AzureAccount AzureTtsAccount =>
        AzureAccounts.FirstOrDefault(aa => aa.Service.Matches("CognitiveServicesSpeech"));
    public bool HandleHostRaidSubscriptionEvents =>
        _wobbleConfiguration.HandleHostRaidSubscriptionEvents;
    public List<string> AutomatedShoutOuts =>
        _wobbleConfiguration.AutomatedShoutOuts;
    public TimedMessages TimedMessages =>
        _wobbleConfiguration.TimedMessages;
    public List<WobbleCommand> WobbleCommands =>
        _wobbleConfiguration.WobbleCommands;
    public List<TwitchEventResponse> TwitchEventResponses { get; } = new();
    public List<ChatResponse> ChatCommands { get; } = new();
    public List<CounterResponse> CounterCommands { get; } = new();

    public BotSettings(WobbleConfiguration wobbleConfiguration)
    {
        _wobbleConfiguration = wobbleConfiguration;

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