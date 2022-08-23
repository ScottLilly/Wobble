using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Wobble.Models;
using Wobble.Models.ChatCommandHandlers;

namespace Wobble.Services;

public static class PersistenceService
{
    private const string WOBBLE_CONFIGURATION_FILE_NAME = "appsettings.json";
    private const string ADDITIONAL_COMMANDS_FILE_NAME = "AdditionalCommands.json";
    private const string COUNTER_DATA_FILE_NAME = "CounterData.json";
    private const string WOBBLE_POINTS_DATA_FILE_NAME = "WobblePoints.json";

    public static WobbleConfiguration GetWobbleConfiguration()
    {
        var text = File.ReadAllText(WOBBLE_CONFIGURATION_FILE_NAME);

        WobbleConfiguration wobbleConfiguration =
            File.Exists(WOBBLE_CONFIGURATION_FILE_NAME)
                ? JsonConvert.DeserializeObject<WobbleConfiguration>(text)
                : new WobbleConfiguration();

        if (File.Exists(ADDITIONAL_COMMANDS_FILE_NAME))
        {
            List<ChatMessage> additionalCommands =
                JsonConvert.DeserializeObject<List<ChatMessage>>(File.ReadAllText(ADDITIONAL_COMMANDS_FILE_NAME));

            if (additionalCommands is {Count: > 0})
            {
                additionalCommands.ForEach(ac => ac.IsAdditionalCommand = true);
                wobbleConfiguration.ChatMessages.AddRange(additionalCommands);
            }
        }

        return wobbleConfiguration;
    }

    public static void SaveWobbleConfiguration(WobbleConfiguration wobbleConfiguration)
    {
        File.WriteAllText(WOBBLE_CONFIGURATION_FILE_NAME,
            JsonConvert.SerializeObject(wobbleConfiguration, Formatting.Indented));
    }

    public static void SaveAdditionalCommands(List<ChatResponse> chatResponses)
    {
        List<ChatMessage> additionalCommands = new List<ChatMessage>();

        foreach (ChatResponse chatResponse in chatResponses)
        {
            additionalCommands.Add(new ChatMessage
            {
                TriggerWords = chatResponse.CommandTriggers,
                Responses = chatResponse.Responses,
                IsAdditionalCommand = true
            });
        }

        File.WriteAllText(ADDITIONAL_COMMANDS_FILE_NAME,
            JsonConvert.SerializeObject(additionalCommands, Formatting.Indented));
    }

    public static CounterData GetCounterData()
    {
        return File.Exists(COUNTER_DATA_FILE_NAME)
            ? JsonConvert.DeserializeObject<CounterData>(File.ReadAllText(COUNTER_DATA_FILE_NAME))
            : new CounterData();
    }

    public static void SaveCounterData(CounterData counterData)
    {
        File.WriteAllText(COUNTER_DATA_FILE_NAME,
            JsonConvert.SerializeObject(counterData, Formatting.Indented));
    }

    public static WobblePointsData GetWobblePointsData()
    {
        return File.Exists(WOBBLE_POINTS_DATA_FILE_NAME)
            ? JsonConvert.DeserializeObject<WobblePointsData>(File.ReadAllText(WOBBLE_POINTS_DATA_FILE_NAME))
            : new WobblePointsData();
    }

    public static void SaveWobblePointsData(WobblePointsData wobblePointsData)
    {
        File.WriteAllText(WOBBLE_POINTS_DATA_FILE_NAME,
            JsonConvert.SerializeObject(wobblePointsData, Formatting.Indented));
    }
}