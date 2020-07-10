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

        public DatabaseService(DiscordShardedClient shardedClient, ConfigService configService)
        {
            var client = new MongoClient(configService.Config.MongoDB);
            _mongoDatabase = client.GetDatabase("matsue");

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
            var collection = _mongoDatabase.GetCollection<MatsueUserBson>("users");
            var filter = Builders<MatsueUserBson>.Filter.Eq("user_id", userId);
            try
            {
                return await collection.Find(filter).FirstAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<MatsueGuildBson> LoadRecordsByGuildId(ulong guildId)
        {
            var collection = _mongoDatabase.GetCollection<MatsueGuildBson>("guilds");
            var filter = Builders<MatsueGuildBson>.Filter.Eq("guild_id", guildId);
            return await collection.Find(filter).FirstAsync();
        }

        // Inserting Records
        public async Task InsertRecord<T>(string table, T record)
        {
            var collection = _mongoDatabase.GetCollection<T>(table);
            await collection.InsertOneAsync(record);
        }

        // Updating Records

        public async Task UpdateUser(MatsueUserBson record)
        {
            var collection = _mongoDatabase.GetCollection<MatsueUserBson>("users");
            var filter = Builders<MatsueUserBson>.Filter.Eq("user_id", record.UserId);
            await collection.ReplaceOneAsync(filter, record);
        }

        public async Task UpdateGuild(MatsueGuildBson record)
        {
            var collection = _mongoDatabase.GetCollection<MatsueGuildBson>("guilds");
            var filter = Builders<MatsueGuildBson>.Filter.Eq("guild_id", record.GuildId);
            await collection.ReplaceOneAsync(filter, record);
        }

        // Delete Records

        public async Task DeleteGuildRecord(ulong guildId)
        {
            var collection = _mongoDatabase.GetCollection<MatsueGuildBson>("guilds");
            var filter = Builders<MatsueGuildBson>.Filter.Eq("guild_id", guildId);
            await collection.DeleteOneAsync(filter);
        }

        // Events

        private async Task OnMessageReceived(SocketMessage msg)
        {
            if (!(msg is SocketUserMessage message)) return;

            if (message.Author.IsBot) return;

            var result = await LoadRecordsByUserId(message.Author.Id);
            if (result == null)
            {
                await InsertRecord("users", new MatsueUserBson { UserId = message.Author.Id, Balance = 0, Married = false, MarriedTo = null });
            }
        }

        private async Task OnJoinedGuild(SocketGuild arg)
        {
            await InsertRecord("guilds", new MatsueGuildBson { GuildId = arg.Id, MusicChannelId = null, BotChannelId = null, AdminChannelId = null, Prefix = "!" });
        }

        private async Task OnLeftGuild(SocketGuild arg)
        {
            await DeleteGuildRecord(arg.Id);
        }
    }
}