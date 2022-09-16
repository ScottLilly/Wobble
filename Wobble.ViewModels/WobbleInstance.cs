using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Wobble.Core;
using Wobble.Models;
using Wobble.Models.ChatCommandHandlers;
using Wobble.Models.ChatConnectors;
using Wobble.Models.CustomEventArgs;
using Wobble.Services;

namespace Wobble.ViewModels;

public class WobbleInstance
{
    private readonly BotSettings _botSettings;
    private readonly SpeechService _speechService;
    private readonly List<string> _automatedShoutOutsPerformedFor = new();
    private readonly List<IChatCommandHandler> _chatCommandHandlers = new();
    private readonly CounterData _counterData;
    private readonly WobblePointsData _wobblePointsData;

    private Timer _timedMessagesTimer;

    public TwitchChatConnector ChatConnector { get; }

    public List<ChatResponse> AdditionalCommands =>
        _chatCommandHandlers.OfType<ChatResponse>()
            .Where(c => c.IsAdditionalCommand)
            .ToList();

    public WobbleInstance(BotSettings botSettings)
    {
        _botSettings = botSettings;

        // Start TwitchPubSubConnector
        var pubSubConnector =
            new TwitchPubSubConnector(botSettings);
        pubSubConnector.OnLogMessageRaised += HandleLogMessageRaised;
        pubSubConnector.Start();

        // Start TwitchChatConnector
        ChatConnector =
            new TwitchChatConnector(botSettings);
        ChatConnector.OnTwitchChatCommandReceived += HandleTwitchChatCommandReceived;
        ChatConnector.OnTwitchChatMessageReceived += HandleTwitchChatMessageReceived;
        ChatConnector.OnLogMessageRaised += HandleLogMessageRaised;
        ChatConnector.Start();

        // Start Azure TextToSpeech service
        if (botSettings.AzureTtsAccount.Key.IsNotNullEmptyOrWhiteSpace() &&
            botSettings.AzureTtsAccount.Region.IsNotNullEmptyOrWhiteSpace())
        {
            _speechService =
                new SpeechService(
                    botSettings.AzureTtsAccount.Key,
                    botSettings.AzureTtsAccount.Region,
                    botSettings.AzureTtsAccount.AzureTtsVoiceName);
        }

        // Load Wobble data
        _counterData = PersistenceService.GetCounterData();
        _wobblePointsData = PersistenceService.GetWobblePointsData();

        PopulateChatCommandHandlers();

        InitializeTimedMessages();
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

    #region Private eventhandlers

    private void HandleLogMessageRaised(object sender, LogMessageEventArgs e)
    {
        Console.WriteLine(e.Message);
        LoggingService.LogMessage(e.Message);
    }

    private void HandleTwitchChatMessageReceived(object sender, TwitchChatMessageArgs e)
    {
        LoggingService.LogMessage($"[{e.DisplayName}] {e.Message}");

        if (_botSettings.AutomatedShoutOuts.None(n => n.Matches(e.DisplayName)) ||
            _automatedShoutOutsPerformedFor.Contains(e.DisplayName))
        {
            return;
        }

        _automatedShoutOutsPerformedFor.Add(e.DisplayName);

        SendChatMessage($"!so {e.DisplayName}");
    }

    private void HandleTwitchChatCommandReceived(object sender, TwitchChatCommandArgs e)
    {
        if (e.CommandName.Matches("commands"))
        {
            DisplayCommandTriggerWords();
            return;
        }

        IChatCommandHandler command =
            GetChatCommandHandler(e.CommandName);

        if (command == null)
        {
            return;
        }

        if (command is CounterResponse)
        {
            SendChatMessage(GetCounterCommandResponse(e.CommandName));
            return;
        }

        string response = 
            command.GetResponse(_botSettings.TwitchBotAccount.Name, e);

        SendChatMessage(response);

        if (command is AddCommand)
        {
            PersistenceService.SaveAdditionalCommands(AdditionalCommands);
        }
    }

    #endregion

    #region Private support functions

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
            _chatCommandHandlers.Add(new MyPoints(_wobblePointsData));
        }

        _chatCommandHandlers.Add(new AddCommand(_chatCommandHandlers));
        _chatCommandHandlers.Add(new RemoveCommand(_chatCommandHandlers));
    }

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
                var response = 
                    chatCommandHandler.GetResponse(_botSettings.TwitchBotAccount.Name, new TwitchChatCommandArgs("", "", chatCommandHandler.CommandTriggers.FirstOrDefault(), ""));

                SendChatMessage(response);
            }
        }
        else
        {
            SendChatMessage(message);
        }
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

    private void SendChatMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        ChatConnector.SendMessage(message);
        LoggingService.LogMessage(message);
    }

    private IChatCommandHandler GetChatCommandHandler(string commandText)
    {
        return _chatCommandHandlers
            .FirstOrDefault(cc =>
                cc.CommandTriggers.Any(ct => ct.Matches(commandText)));
    }

    private string GetCounterCommandResponse(string counterCommand)
    {
        IChatCommandHandler command = GetChatCommandHandler(counterCommand);

        if (!_counterData.CommandCounters.Any(cc => cc.Command.Matches(counterCommand)))
        {
            _counterData.CommandCounters.Add(new CommandCounter
            {
                Command = counterCommand,
                Count = 0
            });
        }

        CommandCounter commandCounter =
            _counterData.CommandCounters
                .First(cc => cc.Command.Matches(counterCommand));

        commandCounter.Count++;

        PersistenceService.SaveCounterData(_counterData);

        return command.GetResponse(_botSettings.TwitchBotAccount.Name,
                new TwitchChatCommandArgs("", "", counterCommand, ""))
            .Replace("{counter}", commandCounter.Count.ToString("N0"));
    }

    #endregion
}