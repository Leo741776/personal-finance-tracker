using PersonalFinanceTracker.Model;
using PersonalFinanceTracker.Repository;

namespace PersonalFinanceTracker.Service
{
    public class SavingsService
    {
        private readonly ISavingsRepository _savingsRepository;

        public SavingsService(ISavingsRepository savingsRepository)
        {
            _savingsRepository = savingsRepository;
        }

        public async Task<Savings?> EnsureDefaultSavingsAccountAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            List<Savings> accounts = await _savingsRepository.GetSavingsAccountsAsync(username);

            if (accounts.Count > 0)
            {
                return accounts.First();
            }

            Savings defaultAccount = new()
            {
                Id = Guid.NewGuid().ToString(),
                OwnerUsername = username,
                Name = "Primary Savings",
                Balance = 0m,
                InterestRate = 4.25,
                LinkedGoalId = string.Empty
            };

            bool created = await _savingsRepository.AddSavingsAccountAsync(username, defaultAccount);

            return created ? defaultAccount : null;
        }

        public async Task<List<Savings>> GetSavingsAccountsAsync(string username)
        {
            return await _savingsRepository.GetSavingsAccountsAsync(username);
        }

        public async Task<decimal> GetTotalSavingsBalanceAsync(string username)
        {
            List<Savings> accounts = await _savingsRepository.GetSavingsAccountsAsync(username);

            return accounts.Sum(account => account.Balance);
        }

        public async Task<double> GetAverageApyAsync(string username)
        {
            List<Savings> accounts = await _savingsRepository.GetSavingsAccountsAsync(username);

            if (accounts.Count == 0)
            {
                return 0;
            }

            return accounts.Average(account => account.InterestRate);
        }

        public async Task<bool> AddSavingsAccountAsync(
            string username,
            string accountName,
            decimal startingBalance,
            double interestRate)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(accountName) ||
                startingBalance < 0 ||
                interestRate < 0)
            {
                return false;
            }

            Savings savingsAccount = new()
            {
                Id = Guid.NewGuid().ToString(),
                OwnerUsername = username,
                Name = accountName,
                Balance = startingBalance,
                InterestRate = interestRate,
                LinkedGoalId = string.Empty
            };

            return await _savingsRepository.AddSavingsAccountAsync(username, savingsAccount);
        }

        public async Task<bool> DepositAsync(
            string username,
            string savingsAccountId,
            decimal amount,
            string fromAccountId = "")
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(savingsAccountId) ||
                amount <= 0)
            {
                return false;
            }

            SavingsTransaction transaction = new()
            {
                Id = Guid.NewGuid().ToString(),
                OwnerUsername = username,
                Date = DateTime.UtcNow,
                Amount = amount,
                FromAccountId = fromAccountId,
                ToSavingsAccountId = savingsAccountId
            };

            return await _savingsRepository.AddSavingsTransactionAsync(username, transaction);
        }

        public async Task<bool> UpdateApyAsync(
            string username,
            string savingsAccountId,
            double newApy)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(savingsAccountId) ||
                newApy < 0)
            {
                return false;
            }

            Savings? account = await _savingsRepository.GetSavingsAccountAsync(
                username,
                savingsAccountId);

            if (account == null)
            {
                return false;
            }

            account.OwnerUsername = username;
            account.InterestRate = newApy;

            return await _savingsRepository.UpdateSavingsAccountAsync(username, account);
        }

        public async Task<bool> AddSavingsGoalAsync(
            string username,
            string goalName,
            decimal targetAmount,
            DateTime? targetDate)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(goalName) ||
                targetAmount <= 0)
            {
                return false;
            }

            SavingsGoal goal = new()
            {
                Id = Guid.NewGuid().ToString(),
                OwnerUsername = username,
                Name = goalName,
                TargetAmount = targetAmount,
                CurrentAmount = 0m,
                TargetDate = targetDate.HasValue
                    ? ToUtcDateTime(targetDate.Value)
                    : null
            };

            return await _savingsRepository.AddSavingsGoalAsync(username, goal);
        }

        public decimal CalculateMonthlySavingsRate(
            decimal savingsThisMonth,
            decimal incomeThisMonth)
        {
            if (incomeThisMonth <= 0)
            {
                return 0;
            }

            return savingsThisMonth / incomeThisMonth * 100;
        }

        public double CalculateGrowthRate(decimal oldBalance, decimal newBalance)
        {
            if (oldBalance <= 0)
            {
                return 0;
            }

            return (double)((newBalance - oldBalance) / oldBalance * 100);
        }

        private static DateTime ToUtcDateTime(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Utc)
            {
                return dateTime;
            }

            if (dateTime.Kind == DateTimeKind.Local)
            {
                return dateTime.ToUniversalTime();
            }

            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }
    }
}