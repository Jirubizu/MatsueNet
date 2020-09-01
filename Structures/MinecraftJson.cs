using System;
using Newtonsoft.Json;

namespace MatsueNet.Structures
{
    public struct MinecraftNamesJson : IEquatable<MinecraftNamesJson>
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty(PropertyName = "changedToAt")]
        public double ChangedToInMs { get; set; }

        public bool Equals(MinecraftNamesJson other)
        {
            throw new NotImplementedException();
        }
    }

    public struct MinecraftUserJson : IEquatable<MinecraftUserJson>
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("id")]
        public string Uuid { get; set; }

        public bool Equals(MinecraftUserJson other)
        {
            throw new NotImplementedException();
        }
    }
}