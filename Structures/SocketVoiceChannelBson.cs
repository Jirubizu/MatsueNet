using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MatsueNet.Structures
{
    public class SocketVoiceChannelBson
    {
        [BsonElement("id")]
        public ulong Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("position")]
        public int Position { get; set; }

        [BsonElement("bitrate")]
        public int Bitrate { get; set; }

        [BsonElement("userLimit")]
        public int? UserLimit { get; set; }
        
        [BsonElement("category")]
        public ulong? CategoryId { get; set; }
        
        public List<SocketVoiceChannelBson> TempChannels { get; set; }
        
    }
}