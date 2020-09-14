using System;
using System.Threading.Tasks;

namespace MatsueNet.Services
{
    public class BalanceService
    {
        public DatabaseService DatabaseService { get; set; }
        public RandomService RandomService { get; set; }

        public BalanceService(DatabaseService databaseService, RandomService randomService)
        {
            DatabaseService = databaseService;
            RandomService = randomService;
        }

        public async Task<double> GetBalance(ulong userId)
        {
            var user = await DatabaseService.LoadRecordsByUserId(userId);

            return user.Balance;
        }

        public async Task AddBalance(ulong userId, double amount)
        {
            var user = await DatabaseService.LoadRecordsByUserId(userId);
            user.Balance += amount;
            await DatabaseService.UpdateUser(user);
        }

        public async Task<bool> SubBalance(ulong userId, double amount)
        {
            var user = await DatabaseService.LoadRecordsByUserId(userId);
            user.Balance -= amount;
            if (user.Balance < 0)
            {
                return false;
            }

            await DatabaseService.UpdateUser(user);
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
            if (RandomService.Chance(50))
            {
                var amount = RandomService.Next(1, 15);
                var user = await DatabaseService.LoadRecordsByUserId(userId);
                if (user != null)
                {
                    user.Balance += (double) amount / 100;
                    user.Balance = Math.Round(user.Balance, 2);
                    await DatabaseService.UpdateUser(user);
                }
            }
        }
    }
}