using System.Collections.Generic;

namespace Wobble.Models;

public class CounterData
{
    public List<CommandCounter> CommandCounters { get; set; } =
        new List<CommandCounter>();
}

public class CommandCounter
{
    public string Command { get; set; }
    public int Count { get; set; }
}