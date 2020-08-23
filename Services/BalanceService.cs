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

        public async Task SubBalance(ulong userId, float amount)
        {
            var user = await _databaseService.LoadRecordsByUserId(userId);
            user.Balance -= amount;
            await _databaseService.UpdateUser(user);
        }

        public async Task MessageCoin(ulong userId)
        {
            if (_randomService.Chance())
            {
                var amount = _randomService.NextFloat();
                var user = await _databaseService.LoadRecordsByUserId(userId);

                user.Balance += amount;
                
                await _databaseService.UpdateUser(user);
            }
        }
    }
}