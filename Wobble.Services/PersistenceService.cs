using System.IO;
using Newtonsoft.Json;
using Wobble.Models;

namespace Wobble.Services
{
    public static class PersistenceService
    {
        private const string WOBBLE_CONFIGURATION_FILE_NAME = "appsettings.json";
        private const string COUNTER_DATA_FILE_NAME = "CounterData.json";
        private const string WOBBLE_POINTS_DATA_FILE_NAME = "WobblePoints.json";

        public static WobbleConfiguration GetWobbleConfiguration()
        {
            return File.Exists(WOBBLE_CONFIGURATION_FILE_NAME)
                ? JsonConvert.DeserializeObject<WobbleConfiguration>(File.ReadAllText(WOBBLE_CONFIGURATION_FILE_NAME))
                : new WobbleConfiguration();
        }

        public static void SaveWobbleConfiguration(WobbleConfiguration wobbleConfiguration)
        {
            File.WriteAllText(WOBBLE_CONFIGURATION_FILE_NAME,
                JsonConvert.SerializeObject(wobbleConfiguration, Formatting.Indented));
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
}