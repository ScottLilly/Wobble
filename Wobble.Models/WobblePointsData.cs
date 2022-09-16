using System.Collections.Generic;

namespace Wobble.Models;

public class WobblePointsData
{
    public string PointsName { get; set; } =
        "WobbleBucks";

    public List<UserPoint> UserPoints { get; set; } = new();

    public class UserPoint
    {
        public string Name { get; set; }
        public int Points { get; set; }
    }
}