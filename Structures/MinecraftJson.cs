using Newtonsoft.Json;

namespace MatsueNet.Structures
{
    public struct MinecraftNamesJson
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty(PropertyName = "changedToAt")]
        public double ChangedToInMs { get; set; }
    }

    public struct MinecraftUserJson
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("id")]
        public string Uuid { get; set; }
    }
}