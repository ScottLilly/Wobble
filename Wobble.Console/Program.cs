using Microsoft.Extensions.Configuration;
using Wobble.Core;
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
        Console.WriteLine("!timer <n [minutes]> <phrase [optional]>");
    }
    else
    {
        var commandWords = command.Split(" ");

        switch (commandWords[0].ToLowerInvariant())
        {
            case "!commands":
                wobbleInstance.DisplayCommandTriggerWords();
                break;
            case "!clear":
                wobbleInstance.ChatConnector.ClearChat();
                break;
            case "!emoteonlyon":
                wobbleInstance.ChatConnector.EmoteModeOnlyOn();
                break;
            case "!emoteonlyoff":
                wobbleInstance.ChatConnector.EmoteModeOnlyOff();
                break;
            case "!followeronlyon":
                wobbleInstance.ChatConnector.FollowersOnlyOn();
                break;
            case "!followeronlyoff":
                wobbleInstance.ChatConnector.FollowersOnlyOff();
                break;
            case "!subonlyon":
                wobbleInstance.ChatConnector.SubscribersOnlyOn();
                break;
            case "!subonlyoff":
                wobbleInstance.ChatConnector.SubscribersOnlyOff();
                break;
            case "!slowmodeon":
                wobbleInstance.ChatConnector.SlowModeOn();
                break;
            case "!slowmodeoff":
                wobbleInstance.ChatConnector.SlowModeOff();
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
            case "!timer":
                if(commandWords.Length > 1)
                {
                    if (int.TryParse(commandWords[1], out int minutes))
                    {
                        wobbleInstance
                            .SetTimedMessage(minutes, 
                            string.Join(' ', commandWords.Skip(2)));
                    }
                }
                break;
            default:
                Console.WriteLine($"Unrecognized command: '{command}'");
                break;
        }
    }

} while ( !(command ?? "").Equals("exit", StringComparison.InvariantCultureIgnoreCase));

WobbleInstance SetupWobbleInstance()
{
    var secrets = new ConfigurationBuilder()
        .AddUserSecrets<Program>(true)
        .Build();

    var secretsAzureAccounts =
        secrets.GetSection("AzureAccounts")
            .Get<List<AzureAccount>>();

    var secretsTwitchAccounts = 
        secrets.GetSection("TwitchAccounts")
            .Get<List<TwitchAccount>>();

    WobbleConfiguration wobbleConfiguration =
        PersistenceService.GetWobbleConfiguration();

    // Populate AzureAccount keys from user secrets, if missing in appsettings.json
    foreach (AzureAccount azureAccount in wobbleConfiguration
                 .AzureAccounts.Where(aa => string.IsNullOrWhiteSpace(aa.Key)))
    {
        azureAccount.Key =
            secretsAzureAccounts
                .FirstOrDefault(saa =>
                    saa.Service.Matches(azureAccount.Service) &&
                    saa.Region.Matches(azureAccount.Region))?.Key ?? "";
    }

    // Populate TwitchAccount keys from user secrets, if missing in appsettings.json
    foreach (TwitchAccount twitchAccount in wobbleConfiguration
                 .TwitchAccounts.Where(ta => string.IsNullOrWhiteSpace(ta.AuthToken)))
    {
        twitchAccount.AuthToken =
            secretsTwitchAccounts
                .FirstOrDefault(sta =>
                    sta.Type.Matches(twitchAccount.Type) &&
                    sta.Name.Matches(twitchAccount.Name))?.AuthToken ?? "";

        twitchAccount.PubSubToken =
            secretsTwitchAccounts
                .FirstOrDefault(sta =>
                    sta.Type.Matches(twitchAccount.Type) &&
                    sta.Name.Matches(twitchAccount.Name))?.PubSubToken ?? "";
    }

    BotSettings botSettings =
        new BotSettings(wobbleConfiguration);

    return new WobbleInstance(botSettings);
}