using System.Collections.Generic;
using System.Linq;
using Wobble.Core;
using Wobble.Models.ChatConnectors;

namespace Wobble.Models.ChatCommandHandlers;

public class MyPoints : IWobbleCommandHandler
{
    private readonly WobblePointsData _wobblePointsData;

    public List<string> CommandTriggers => new() { "MyPoints" };

    public MyPoints(WobblePointsData wobblePointsData)
    {
        _wobblePointsData = wobblePointsData;
    }

    public string GetResponse(string botDisplayName, TwitchChatCommandArgs commandArgs)
    {
        int points =
            _wobblePointsData.UserPoints
                .FirstOrDefault(up => up.Name.Matches(commandArgs.ChatterId))?.Points ?? 0;

        return $"{commandArgs.ChatterName}, you have {points} {_wobblePointsData.PointsName}";
    }
}