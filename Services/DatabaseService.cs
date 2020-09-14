using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using MatsueNet.Structures;
using MongoDB.Driver;
using MongoDB.Bson;
using Discord.WebSocket;

namespace MatsueNet.Services
{
    public class DatabaseService
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly Dictionary<ulong, MatsueUserBson> _matsueUserCache = new Dictionary<ulong, MatsueUserBson>();

        private readonly Dictionary<ulong, MatsueGuildBson>
            _matsueGuildCache = new Dictionary<ulong, MatsueGuildBson>();

        public DatabaseService(DiscordShardedClient shardedClient, ConfigService configService)
        {
            var client = new MongoClient(configService.Config.MongoDBUri);
            _mongoDatabase = client.GetDatabase(configService.Config.MongoDBName);

            shardedClient.JoinedGuild += OnJoinedGuild;
            shardedClient.MessageReceived += OnMessageReceived;
            shardedClient.LeftGuild += OnLeftGuild;
        }

        // Loading Records
        public async Task<List<T>> LoadRecords<T>(string table)
        {
            var collection = _mongoDatabase.GetCollection<T>(table);
            return await collection.Find(new BsonDocument()).ToListAsync();
        }

        public async Task<MatsueUserBson> LoadRecordsByUserId(ulong userId)
        {
            if (_matsueUserCache.TryGetValue(userId, out var cacheUser))
            {
                return cacheUser;
            }

            var collection = _mongoDatabase.GetCollection<MatsueUserBson>("users");
            var filter = Builders<MatsueUserBson>.Filter.Eq("user_id", userId);
            var user = await collection.Find(filter).FirstAsync();

            if (user == null) return null;
            _matsueUserCache.Add(user.UserId, user);
            return user;
        }

        public async Task<MatsueGuildBson> LoadRecordsByGuildId(ulong guildId)
        {
            if (_matsueGuildCache.TryGetValue(guildId, out var cacheGuild))
            {
                return cacheGuild;
            }

            var collection = _mongoDatabase.GetCollection<MatsueGuildBson>("guilds");
            var filter = Builders<MatsueGuildBson>.Filter.Eq("guild_id", guildId);
            var guild = await collection.Find(filter).FirstAsync();

            if (guild == null) return null;
            _matsueGuildCache.Add(guild.GuildId, guild);
            return guild;
        }

        // Inserting Records
        public async Task InsertRecord<T>(string table, T record)
        {
            switch (record)
            {
                case MatsueUserBson user:
                    _matsueUserCache.Add(user.UserId, user);
                    break;
                case MatsueGuildBson guild:
                    _matsueGuildCache.Add(guild.GuildId, guild);
                    break;
            }

            var collection = _mongoDatabase.GetCollection<T>(table);
            await collection.InsertOneAsync(record);
        }

        // Updating Records

        public async Task UpdateUser(MatsueUserBson record)
        {
            _matsueUserCache[record.UserId] = record;
            var collection = _mongoDatabase.GetCollection<MatsueUserBson>("users");
            var filter = Builders<MatsueUserBson>.Filter.Eq("user_id", record.UserId);
            await collection.ReplaceOneAsync(filter, record);
        }

        public async Task UpdateGuild(MatsueGuildBson record)
        {
            _matsueGuildCache[record.GuildId] = record;
            var collection = _mongoDatabase.GetCollection<MatsueGuildBson>("guilds");
            var filter = Builders<MatsueGuildBson>.Filter.Eq("guild_id", record.GuildId);
            await collection.ReplaceOneAsync(filter, record);
        }

        // Delete Records

        private async Task DeleteGuildRecord(ulong guildId)
        {
            _matsueGuildCache.Remove(guildId);
            var collection = _mongoDatabase.GetCollection<MatsueGuildBson>("guilds");
            var filter = Builders<MatsueGuildBson>.Filter.Eq("guild_id", guildId);
            await collection.DeleteOneAsync(filter);
        }

        // Events
        private async Task OnMessageReceived(SocketMessage msg)
        {
            if (!(msg is SocketUserMessage message)) return;
            if (message.Author.IsBot) return;
            if (_matsueUserCache.ContainsKey(message.Author.Id)) return;

            var result = await LoadRecordsByUserId(message.Author.Id);
            if (result == null)
            {
                await InsertRecord("users", new MatsueUserBson {UserId = message.Author.Id, Balance = 0});
            }
        }

        private async Task OnJoinedGuild(SocketGuild arg)
        {
            await InsertRecord("guilds",
                new MatsueGuildBson
                {
                    GuildId = arg.Id, MusicChannelId = null, BotChannelId = null, AdminChannelId = null, Prefix = "!"
                });
        }

        private async Task OnLeftGuild(SocketGuild arg)
        {
            await DeleteGuildRecord(arg.Id);
        }
    }
}