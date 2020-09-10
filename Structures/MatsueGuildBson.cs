using System;
using System.Collections.Generic;
using Discord.WebSocket;
using Newtonsoft.Json;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MatsueNet.Structures
{
    public class MatsueGuildBson
    {
        [BsonId]
        public ObjectId CollectionId { get; set; }

        [BsonElement("guild_id")]
        public ulong GuildId { get; set; }

        [BsonElement("music_channel")]
        public ulong? MusicChannelId { get; set; }

        [BsonElement("bot_channel")]
        public ulong? BotChannelId { get; set; }

        [BsonElement("admin_channel")]
        public ulong? AdminChannelId { get; set; }

        [BsonElement("prefix")]
        public string Prefix { get; set; }
    }
}