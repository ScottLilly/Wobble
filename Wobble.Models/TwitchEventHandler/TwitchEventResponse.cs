namespace Wobble.Models.TwitchEventHandler;

public class TwitchEventResponse : ITwitchEventHandler
{
    public string EventName { get; }
    public string Message { get; }

    public TwitchEventResponse(string eventName, string message)
    {
        EventName = eventName;
        Message = message;
    }
}