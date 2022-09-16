using System;

namespace Wobble.Models.ChatConnectors;

public class TwitchChatCommandArgs : EventArgs
{
    public string ChatterId { get; }
    public string ChatterName { get; }
    public string CommandName { get; }
    public string Argument { get; }
    public bool IsBroadcaster { get; }
    public bool IsModerator { get; }
    public bool IsVip { get; }

    public bool DoesNotHaveArguments =>
        string.IsNullOrWhiteSpace(Argument);

    public TwitchChatCommandArgs(string chatterId, string chatterName,
        string commandName, string argument,
        bool isBroadcaster = false, bool isModerator = false, bool isVip = false)
    {
        ChatterId = chatterId;
        ChatterName = chatterName;
        CommandName = commandName;
        Argument = argument;
        IsBroadcaster = isBroadcaster;
        IsModerator = isModerator;
        IsVip = isVip;
    }
}