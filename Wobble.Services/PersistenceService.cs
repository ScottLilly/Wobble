using System.IO;
using Newtonsoft.Json;
using Wobble.Models;

namespace Wobble.Services
{
    public static class PersistenceService
    {
        private const string COUNTER_DATA_FILE_NAME = "CounterData.json";

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
    }
}