using System;
using System.Threading.Tasks;

namespace MatsueNet.Services
{
    public class BalanceService
    {
        private readonly DatabaseService _databaseService;
        private readonly RandomService _randomService;

        public BalanceService(DatabaseService databaseService, RandomService randomService)
        {
            _databaseService = databaseService;
            _randomService = randomService;
        }

        public async Task<double> GetBalance(ulong userId)
        {
            var user = await _databaseService.LoadRecordsByUserId(userId);

            return user.Balance;
        }

        public async Task AddBalance(ulong userId, double amount)
        {
            var user = await _databaseService.LoadRecordsByUserId(userId);
            user.Balance += amount;
            await _databaseService.UpdateUser(user);
        }

        public async Task<bool> SubBalance(ulong userId, double amount)
        {
            var user = await _databaseService.LoadRecordsByUserId(userId);
            user.Balance -= amount;
            if (user.Balance < 0)
            {
                return false;
            }

            await _databaseService.UpdateUser(user);
            return true;
        }

        public async Task<bool> Pay(ulong payTo, ulong paying, double amount)
        {
            var result = await SubBalance(paying, amount);
            if (!result)
            {
                return false;
            }

            await AddBalance(payTo, amount);
            return true;
        }

        public async Task MessageCoin(ulong userId)
        {
            if (_randomService.Chance(50))
            {
                var amount = _randomService.Next(1, 15);
                var user = await _databaseService.LoadRecordsByUserId(userId);

                user.Balance += (double) amount / 100;
                user.Balance = Math.Round(user.Balance, 2);

                await _databaseService.UpdateUser(user);
            }
        }
    }
}