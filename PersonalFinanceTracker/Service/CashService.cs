using PersonalFinanceTracker.Model;
using PersonalFinanceTracker.Repository;

namespace PersonalFinanceTracker.Service
{
    public class CashService
    {
        private readonly ICashRepository _cashRepository;

        public CashService(ICashRepository cashRepository)
        {
            _cashRepository = cashRepository;
        }

        public async Task<Cash?> EnsureDefaultCashAccountAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            List<Cash> accounts = await _cashRepository.GetCashAccountsAsync(username);

            if (accounts.Count > 0)
            {
                return accounts.First();
            }

            Cash defaultAccount = new()
            {
                Id = Guid.NewGuid().ToString(),
                OwnerUsername = username,
                Name = "Checking",
                Balance = 0m,
                PendingBalance = 0m,
                Transactions = new List<CashTransaction>()
            };

            bool created = await _cashRepository.AddCashAccountAsync(username, defaultAccount);

            return created ? defaultAccount : null;
        }

        public async Task<List<Cash>> GetCashAccountsAsync(string username)
        {
            return await _cashRepository.GetCashAccountsAsync(username);
        }

        public async Task<decimal> GetTotalCashBalanceAsync(string username)
        {
            List<Cash> accounts = await _cashRepository.GetCashAccountsAsync(username);

            return accounts.Sum(account => account.Balance);
        }

        public async Task<decimal> GetTotalPendingBalanceAsync(string username)
        {
            List<Cash> accounts = await _cashRepository.GetCashAccountsAsync(username);

            return accounts.Sum(account => account.PendingBalance);
        }

        public async Task<bool> AddCashAccountAsync(
            string username,
            string accountName,
            decimal startingBalance)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(accountName) ||
                startingBalance < 0)
            {
                return false;
            }

            Cash cashAccount = new()
            {
                Id = Guid.NewGuid().ToString(),
                OwnerUsername = username,
                Name = accountName,
                Balance = startingBalance,
                PendingBalance = 0m
            };

            return await _cashRepository.AddCashAccountAsync(username, cashAccount);
        }

        public async Task<bool> AddCashAsync(
            string username,
            string cashAccountId,
            decimal amount,
            string source)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(cashAccountId) ||
                amount <= 0)
            {
                return false;
            }

            CashTransaction transaction = new()
            {
                Id = Guid.NewGuid().ToString(),
                OwnerUsername = username,
                Date = DateTime.UtcNow,
                Amount = amount,
                Type = CashTransactionType.Income,
                Category = TransactionCategory.Other,
                MerchantOrSource = string.IsNullOrWhiteSpace(source) ? "Cash Deposit" : source,
                AccountId = cashAccountId
            };

            return await _cashRepository.AddCashTransactionAsync(
                username,
                cashAccountId,
                transaction);
        }

        public async Task<bool> AddTransactionAsync(
            string username,
            string cashAccountId,
            DateTime date,
            decimal amount,
            CashTransactionType type,
            TransactionCategory category,
            string merchantOrSource)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(cashAccountId) ||
                amount == 0 ||
                string.IsNullOrWhiteSpace(merchantOrSource))
            {
                return false;
            }

            decimal signedAmount =
                type == CashTransactionType.Expense ||
                type == CashTransactionType.TransferOut ||
                type == CashTransactionType.InvestmentIn
                    ? -Math.Abs(amount)
                    : Math.Abs(amount);

            CashTransaction transaction = new()
            {
                Id = Guid.NewGuid().ToString(),
                OwnerUsername = username,
                Date = ToUtcDateTime(date),
                Amount = signedAmount,
                Type = type,
                Category = category,
                MerchantOrSource = merchantOrSource,
                AccountId = cashAccountId
            };

            return await _cashRepository.AddCashTransactionAsync(
                username,
                cashAccountId,
                transaction);
        }

        public async Task<List<CashTransaction>> GetAllTransactionsAsync(string username)
        {
            List<Cash> accounts = await _cashRepository.GetCashAccountsAsync(username);

            return accounts
                .SelectMany(account => account.Transactions)
                .OrderByDescending(transaction => transaction.Date)
                .ToList();
        }

        public async Task<List<CashTransaction>> GetCurrentMonthTransactionsAsync(string username)
        {
            List<CashTransaction> transactions = await GetAllTransactionsAsync(username);
            DateTime now = DateTime.UtcNow;

            return transactions
                .Where(transaction =>
                    transaction.Date.Year == now.Year &&
                    transaction.Date.Month == now.Month)
                .OrderByDescending(transaction => transaction.Date)
                .ToList();
        }

        public async Task<decimal> GetAmountSpentThisMonthAsync(string username)
        {
            List<CashTransaction> transactions = await GetCurrentMonthTransactionsAsync(username);

            return transactions
                .Where(transaction => transaction.Amount < 0)
                .Sum(transaction => Math.Abs(transaction.Amount));
        }

        public async Task<MonthlyBudget> GetOrCreateCurrentMonthBudgetAsync(
            string username,
            decimal defaultLimit = 3000m)
        {
            string month = DateTime.UtcNow.ToString("yyyy-MM");

            MonthlyBudget? existingBudget = await _cashRepository.GetMonthlyBudgetAsync(
                username,
                month);

            if (existingBudget != null)
            {
                return existingBudget;
            }

            decimal spent = await GetAmountSpentThisMonthAsync(username);

            MonthlyBudget newBudget = new()
            {
                OwnerUsername = username,
                Month = month,
                Limit = defaultLimit,
                Spent = spent
            };

            await _cashRepository.SetMonthlyBudgetAsync(username, newBudget);

            return newBudget;
        }

        public async Task<bool> UpdateCurrentMonthBudgetAsync(
            string username,
            decimal newLimit)
        {
            if (string.IsNullOrWhiteSpace(username) || newLimit < 0)
            {
                return false;
            }

            string month = DateTime.UtcNow.ToString("yyyy-MM");
            decimal spent = await GetAmountSpentThisMonthAsync(username);

            MonthlyBudget budget = new()
            {
                OwnerUsername = username,
                Month = month,
                Limit = newLimit,
                Spent = spent
            };

            return await _cashRepository.SetMonthlyBudgetAsync(username, budget);
        }

        public bool IsOverBudget(decimal budgetLimit, decimal amountSpent)
        {
            return amountSpent > budgetLimit;
        }

        public string GetBudgetStatus(decimal budgetLimit, decimal amountSpent)
        {
            return IsOverBudget(budgetLimit, amountSpent)
                ? "Over Budget"
                : "Under Budget";
        }

        public List<decimal> BuildNetCashChangeValues(IEnumerable<CashTransaction> transactions)
        {
            decimal runningTotal = 0m;

            return transactions
                .OrderBy(transaction => transaction.Date)
                .Select(transaction =>
                {
                    runningTotal += transaction.Amount;
                    return runningTotal;
                })
                .ToList();
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