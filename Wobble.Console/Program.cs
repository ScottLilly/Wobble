using Microsoft.Extensions.Configuration;
using Wobble.Models;
using Wobble.Models.ChatCommandHandlers;
using Wobble.Services;
using Wobble.ViewModels;

Console.WriteLine("Wobble - Twitch chat bot");
Console.WriteLine("Type '!help' to see available commands");

WobbleInstance wobbleInstance = SetupWobbleInstance();

// Wait for user commands
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
        Console.WriteLine("!commands            Show available chat commands in Twitch chat");
        Console.WriteLine("!clear               Clear Twitch chat messages");
        Console.WriteLine("!emoteonlyon         Turn emote-only mode on");
        Console.WriteLine("!emoteonlyoff        Turn emote-only mode off");
        Console.WriteLine("!followeronlyon      Turn follower-only mode on");
        Console.WriteLine("!followeronlyoff     Turn follower-only mode off");
        Console.WriteLine("!subonlyon           Turn sub-only mode on");
        Console.WriteLine("!subonlyoff          Turn sub-only mode off");
        Console.WriteLine("!slowmodeon          Turn slow mode on");
        Console.WriteLine("!slowmodeoff         Turn slow mode off");
        Console.WriteLine("!additionalcommands  Show additional commands");
        //Console.WriteLine("!title <title>      Change the stream title to <title>");
    }
    else
    {
        var commandWords = command.Split(" ");

        switch (commandWords[0].ToLowerInvariant())
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
            case "!title":
                wobbleInstance.SetStreamTitle(string.Join(' ', commandWords.Skip(1)));
                break;
            case "!additionalcommands":
                foreach (ChatResponse additionalCommand in wobbleInstance.AdditionalCommands)
                {
                    Console.WriteLine($"Command: {string.Join(" ", additionalCommand.CommandTriggers)}");
                }
                break;
            case "!say":
                wobbleInstance.Speak(string.Join(' ', commandWords.Skip(1)));
                break;
            default:
                Console.WriteLine($"Unrecognized command: '{command}'");
                break;
        }
    }

} while ( !(command ?? "").Equals("exit", StringComparison.InvariantCultureIgnoreCase));

WobbleInstance SetupWobbleInstance()
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", false, true)
        .AddUserSecrets<Program>(true);

    var configuration = builder.Build();

    string userSecretsToken =
        configuration
            .AsEnumerable()
            .Where(c => c.Key == "TwitchToken")
            .First(c => !string.IsNullOrWhiteSpace(c.Value))
            .Value;

    string azureCognitiveServiceKey =
        configuration
            .AsEnumerable()
            .Where(c => c.Key == "AzureCognitiveServicesKey")
            .First(c => !string.IsNullOrWhiteSpace(c.Value))
            .Value;

    WobbleConfiguration wobbleConfiguration =
        PersistenceService.GetWobbleConfiguration();

    BotSettings botSettings =
        new BotSettings(wobbleConfiguration, userSecretsToken, azureCognitiveServiceKey);

    return new WobbleInstance(botSettings);
}