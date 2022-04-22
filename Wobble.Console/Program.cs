// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Wobble.Models;
using Wobble.Services;
using Wobble.ViewModels;

Console.WriteLine("Wobble - Twitch chat bot");
Console.WriteLine("Type '!help' to see available commands");

// Setup
var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddUserSecrets<Program>();

var configuration = builder.Build();

string userSecretsToken =
    configuration.AsEnumerable().First(c => c.Key == "TwitchToken").Value;

WobbleConfiguration wobbleConfiguration =
    PersistenceService.GetWobbleConfiguration();

BotSettings botSettings =
    new BotSettings(wobbleConfiguration, userSecretsToken);

var wobbleInstance = new WobbleInstance(botSettings);

string? command = "";

do
{
    command = Console.ReadLine();

    if (command == null)
    {
        continue;
    }

    if (command.Equals("!help"))
    {
        Console.WriteLine("!commands           Show available chat commands in Twitch chat");
        Console.WriteLine("!clear              Clear Twitch chat messages");
        Console.WriteLine("!emoteonlyon        Turn emote-only mode on");
        Console.WriteLine("!emoteonlyoff       Turn emote-only mode off");
        Console.WriteLine("!followeronlyon     Turn follower-only mode on");
        Console.WriteLine("!followeronlyoff    Turn follower-only mode off");
        Console.WriteLine("!subonlyon          Turn sub-only mode on");
        Console.WriteLine("!subonlyoff         Turn sub-only mode off");
        Console.WriteLine("!slowmodeon         Turn slow mode on");
        Console.WriteLine("!slowmodeoff        Turn slow mode off");
    }
    else
    {
        switch (command.ToLowerInvariant())
        {
            case "!commands":
                wobbleInstance.DisplayCommands();
                break;
            case "!clear":
                wobbleInstance.ClearChat();
                break;
            case "!emoteonlyon":
                wobbleInstance.EmoteModeOnlyOn();
                break;
            case "!emoteonlyoff":
                wobbleInstance.EmoteModeOnlyOff();
                break;
            case "!followeronlyon":
                wobbleInstance.FollowersOnlyOn();
                break;
            case "!followeronlyoff":
                wobbleInstance.FollowersOnlyOff();
                break;
            case "!subonlyon":
                wobbleInstance.SubscribersOnlyOn();
                break;
            case "!subonlyoff":
                wobbleInstance.SubscribersOnlyOff();
                break;
            case "!slowmodeon":
                wobbleInstance.SlowModeOn();
                break;
            case "!slowmodeoff":
                wobbleInstance.SlowModeOff();
                break;
            default:
                Console.WriteLine($"Unrecognized command: '{command}'");
                break;
        }
    }

} while ( !command.Equals("exit", StringComparison.InvariantCultureIgnoreCase));