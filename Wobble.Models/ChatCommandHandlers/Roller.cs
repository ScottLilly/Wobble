using System;
using System.Collections.Generic;
using TwitchLib.Client.Models;
using Wobble.Core;

namespace Wobble.Models.ChatCommandHandlers;

public class Roller : IWobbleCommandHandler
{
    public List<string> CommandTriggers => new() {"roll"};

    string IChatCommandHandler.GetResponse(string botDisplayName, ChatCommand chatCommand)
    {
        // If empty, default to 1-10
        if (string.IsNullOrWhiteSpace(chatCommand.ArgumentsAsString))
        {
            return $"You rolled a {RngCreator.GetNumberBetween(1, 10)}";
        }

        string[] values =
            chatCommand.ArgumentsAsString.Split('-', StringSplitOptions.RemoveEmptyEntries);

        // If single number, return 1 to argument number
        if (values.Length == 1)
        {
            if (int.TryParse(values[0], out int num))
            {
                return num == 0
                    ? "You rolled NOTHING!"
                    : $"You rolled a {RngCreator.GetNumberBetween(1, num)}";
            }
        }

        // If two numbers, return random from 1st to second
        if (values.Length == 2)
        {
            if (int.TryParse(values[0], out int firstNumber) &&
                int.TryParse(values[1], out int secondNumber))
            {
                int lowest = Math.Min(firstNumber, secondNumber);
                int highest = Math.Max(firstNumber, secondNumber);

                return $"You rolled a {RngCreator.GetNumberBetween(lowest, highest)}";
            }
        }

        // Default message, if no valid arguments were passed
        return "Roll requires a number or range of two numbers";
    }
}