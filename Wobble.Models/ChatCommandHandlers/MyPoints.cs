using System;
using System.Collections.Generic;
using TwitchLib.Client.Models;

namespace Wobble.Models.ChatCommandHandlers
{
    public class MyPoints : IWobbleCommandHandler
    {
        private WobblePointsData _wobblePointsData;

        public List<string> CommandTriggers { get; } =
            new List<string> {"MyPoints"};

        public MyPoints(WobblePointsData wobblePointsData)
        {
            _wobblePointsData = wobblePointsData;
        }

        public string GetResponse(string botDisplayName, ChatCommand chatCommand)
        {
            throw new NotImplementedException();
        }
    }
}
