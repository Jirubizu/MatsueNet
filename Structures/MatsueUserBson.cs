using System;
using Newtonsoft.Json;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MatsueNet.Structures
{
    public class MatsueUserBson
    {
        [BsonId]
        public ObjectId CollectionId { get; set; }

        [BsonElement("user_id")]
        public ulong UserId { get; set; }

        [BsonElement("balance")]
        public double Balance { get; set; }
    }
}