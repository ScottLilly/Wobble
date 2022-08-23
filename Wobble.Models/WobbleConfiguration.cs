using System.Collections.Generic;

namespace Wobble.Models;

public class WobbleConfiguration
{
    public List<TwitchAccount> TwitchAccounts { get; set; }
    public List<AzureAccount> AzureAccounts { get; set; }
    public bool HandleHostRaidSubscriptionEvents { get; set; }
    public List<WobbleCommand> WobbleCommands { get; set; }
    public List<string> AutomatedShoutOuts { get; set; }
    public List<ChatMessage> ChatMessages { get; set; }
    public List<CounterMessage> CounterMessages { get; set; }
    public List<TwitchEventMessage> TwitchEventMessages { get; set; }
    public TimedMessages TimedMessages { get; set; }
}

public class TwitchAccount
{
    public string Type { get; set; }
    public string Name { get; set; }
    public string AuthToken { get; set; }
}

public class AzureAccount
{
    public string Service { get; set; }
    public string Region { get; set; }
    public string Key { get; set; }
    public string AzureTtsVoiceName { get; set; }
}

public class TimedMessages
{
    public int IntervalInMinutes { get; set; }
    public List<string> Messages { get; set; }
}

public class WobbleCommand
{
    public string Name { get; set; }
    public bool IsEnabled { get; set; }
}

public class ChatMessage
{
    public List<string> TriggerWords { get; set; }
    public List<string> Responses { get; set; }
    public bool IsAdditionalCommand { get; set; }
    public string MissingArgumentMessage { get; set; }
    public bool RequiresArgument { get; set; }
}

public class CounterMessage
{
    public List<string> TriggerWords { get; set; }
    public List<string> Responses { get; set; }
}

public class TwitchEventMessage
{
    public string EventType { get; set; }
    public string Message { get; set; }
}