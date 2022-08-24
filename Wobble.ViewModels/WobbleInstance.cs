using System;
using System.IO;
using Wobble.Core;
using Wobble.Models;
using Wobble.Models.CustomEventArgs;
using Wobble.Services;

namespace Wobble.ViewModels;

public class WobbleInstance
{
    private const string CHAT_LOG_DIRECTORY = "./ChatLogs";

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

    private void HandleLogMessageRaised(object sender, LogMessageEventArgs e)
    {
        Console.WriteLine(e.Message);

        if (!Directory.Exists(CHAT_LOG_DIRECTORY))
        {
            Directory.CreateDirectory(CHAT_LOG_DIRECTORY);
        }

        string logFileName = 
            Path.Combine(CHAT_LOG_DIRECTORY, $"Wobble-{DateTime.Now:yyyy-MM-dd}.log");

        File.AppendAllText(logFileName,
            $"{DateTime.Now.ToShortTimeString()}: {e.Message}{Environment.NewLine}");
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