using System;
using System.Threading.Tasks;
using Wobble.Core;
using Wobble.Models;
using Wobble.Models.ChatConnectors;
using Wobble.Models.CustomEventArgs;
using Wobble.Services;

namespace Wobble.ViewModels;

public class WobbleInstance
{
    private readonly SpeechService _speechService;

    public TwitchChatConnector ChatConnector { get; }

    public WobbleInstance(BotSettings botSettings)
    {
        var pubSubConnector =
            new TwitchPubSubConnector(botSettings);
        pubSubConnector.OnMessageToLog += HandleLogMessageRaised;
        pubSubConnector.Start();

        ChatConnector =
            new TwitchChatConnector(botSettings,
                PersistenceService.GetCounterData(),
                PersistenceService.GetWobblePointsData());
        ChatConnector.OnLogMessageRaised += HandleLogMessageRaised;
        ChatConnector.OnCounterDataChanged += HandleCounterDataChanged;
        ChatConnector.OnWobblePointsDataChanged += HandleWobblePointsDataChanged;
        ChatConnector.Start();

        if (botSettings.AzureTtsAccount.Key.IsNotNullEmptyOrWhiteSpace() &&
            botSettings.AzureTtsAccount.Region.IsNotNullEmptyOrWhiteSpace())
        {
            _speechService =
                new SpeechService(
                    botSettings.AzureTtsAccount.Key,
                    botSettings.AzureTtsAccount.Region,
                    botSettings.AzureTtsAccount.AzureTtsVoiceName);
        }
    }

    public void Speak(string message)
    {
        _speechService?.SpeakAsync(message);
    }

    public void SetTimedMessage(int minutes, string phrase = "")
    {
        if(minutes <= 0)
        {
            return;
        }

        if(string.IsNullOrWhiteSpace(phrase))
        {
            phrase = minutes == 1
                ? "Attention. 1 minute has passed"
                : $"Attention. {minutes} minutes have passed";
        }

        Task.Delay(TimeSpan.FromMinutes(minutes))
            .ContinueWith(task => Speak(phrase));
    }

    private void HandleLogMessageRaised(object sender, LogMessageEventArgs e)
    {
        Console.WriteLine(e.Message);
        LoggingService.WriteMessage(e.Message);
    }

    private void HandleCounterDataChanged(object sender, EventArgs e)
    {
        PersistenceService.SaveCounterData(ChatConnector.CounterData);
    }

    private void HandleWobblePointsDataChanged(object sender, EventArgs e)
    {
        PersistenceService.SaveWobblePointsData(ChatConnector.WobblePointsData);
    }
}