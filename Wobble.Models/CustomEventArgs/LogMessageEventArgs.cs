namespace Wobble.Models.CustomEventArgs;

public class LogMessageEventArgs
{
    public Enums.LogMessageLevel Level { get; }
    public string Message { get; }

    public LogMessageEventArgs(Enums.LogMessageLevel level, string message)
    {
        Level = level;
        Message = message;
    }
}