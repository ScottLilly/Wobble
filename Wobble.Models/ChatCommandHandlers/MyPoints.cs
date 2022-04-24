using System.Collections.Generic;
using System.Linq;
using TwitchLib.Client.Models;
using Wobble.Core;

namespace Wobble.Models.ChatCommandHandlers;

public class MyPoints : IWobbleCommandHandler
{
    private readonly WobblePointsData _wobblePointsData;

    public List<string> CommandTriggers { get; } =
        new List<string> { "MyPoints" };

    public MyPoints(WobblePointsData wobblePointsData)
    {
        _wobblePointsData = wobblePointsData;
    }

    public string GetResponse(string botDisplayName, ChatCommand chatCommand)
    {
        int points =
            _wobblePointsData.UserPoints
                .FirstOrDefault(up => up.Name.Matches(chatCommand.ChatMessage.UserId))?.Points ?? 0;

        return $"{chatCommand.ChatMessage.DisplayName}, you have {points} {_wobblePointsData.PointsName}";
    }
}