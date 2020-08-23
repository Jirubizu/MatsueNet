using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace MatsueNet.Services
{
    public class BalanceService
    {
        private DatabaseService _databaseService;
        private readonly RandomService _randomService;
        
        public BalanceService(DatabaseService databaseService, RandomService randomService, DiscordShardedClient client)
        {
            _databaseService = databaseService;
            _randomService = randomService;
        }

        public async Task<float> GetBalance(ulong userId)
        {
            var user = await _databaseService.LoadRecordsByUserId(userId);
            
            return user.Balance;
        }

        public async Task AddBalance(ulong userId, float amount)
        {
            var user = await _databaseService.LoadRecordsByUserId(userId);
            user.Balance += amount;
            await _databaseService.UpdateUser(user);
        }

        public async Task<bool> SubBalance(ulong userId, float amount)
        {
            var user = await _databaseService.LoadRecordsByUserId(userId);
            user.Balance -= amount;
            if (user.Balance < 0) return false;
            await _databaseService.UpdateUser(user);
            return true;
        }

        public async Task<bool> Pay(ulong payTo, ulong paying, float amount)
        {
            var result = await SubBalance(paying, amount);
            if (!result) return false;
            
            await AddBalance(payTo, amount);
            return true;
        }

        public async Task MessageCoin(ulong userId)
        {
            if (_randomService.Chance())
            {
                var amount = _randomService.Next(1,15);
                var user = await _databaseService.LoadRecordsByUserId(userId);
                
                user.Balance += (float)amount / 100;
                
                await _databaseService.UpdateUser(user);
            }
        }
    }
}