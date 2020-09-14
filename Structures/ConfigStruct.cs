using System;
using Newtonsoft.Json;

namespace MatsueNet.Structures
{
    public struct ConfigStruct : IEquatable<ConfigStruct>
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
        
        [JsonProperty("lavalink_hostname")]
        public string LavaLinkHostname { get; set; }
        
        [JsonProperty("lavalink_port")]
        public ushort LavaLinkPort { get; set; }
        
        [JsonProperty("lavalink_password")]
        public string LavaLinkPassword { get; set; }
        
        [JsonProperty("ocrspace_key")]
        public string OCR { get; set; }
        
        public bool Equals(ConfigStruct other)
        {
            throw new NotImplementedException();
        }
    }
}