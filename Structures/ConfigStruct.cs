using Newtonsoft.Json;

namespace MatsueNet.Structures
{
    public struct ConfigStruct
    {
        [JsonProperty("bot_token")]
        public string BotToken { get; set; }

        [JsonProperty("osu_token")]
        public string OsuToken { get; set; }

        [JsonProperty("mongo_db_uri")]
        // ReSharper disable once InconsistentNaming
        public string MongoDBUri { get; set; }

        [JsonProperty("mongo_db_name")]
        // ReSharper disable once InconsistentNaming
        public string MongoDBName { get; set; }
    }
}