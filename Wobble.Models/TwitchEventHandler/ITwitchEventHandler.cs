namespace Wobble.Models.TwitchEventHandler;

public interface ITwitchEventHandler
{
    string EventName { get; }
    string Message { get; }
}