using System.Collections.Generic;
using Wobble.Core;

namespace Wobble.Models.ChatCommands
{
    internal class MagicEightBall
    {
        private readonly List<string> _results;

        internal MagicEightBall()
        {
            _results = new List<string>()
            {
                "It is certain",
                "It is decidedly so",
                "Without a doubt",
                "Yes definitely",
                "You may rely on it",
                "As I see it, yes",
                "Most likely",
                "Outlook good",
                "Yes",
                "Signs point to yes",
                "Reply hazy, try again",
                "Ask again later",
                "Better not tell you now",
                "Cannot predict now",
                "Concentrate and ask again",
                "Don't count on it",
                "My reply is no",
                "My sources say no",
                "Outlook not so good",
                "Very doubtful"
            };
        }

        internal string GetResponse()
        {
            int index = RandomNumberGenerator.NumberBetween(0, _results.Count - 1);

            return _results[index];
        }
    }
}