using System;

namespace Wobble.Models.ChatConnectors;

public class TwitchChatMessageArgs : EventArgs
{
    public string ChatterId { get; }
    public string DisplayName { get; }
    public string Message { get; }

    public TwitchChatMessageArgs(string chatterId, string displayName, string message)
    {
        ChatterId = chatterId;
        DisplayName = displayName;
        Message = message;
    }
}