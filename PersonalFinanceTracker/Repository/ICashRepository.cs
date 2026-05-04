using PersonalFinanceTracker.Model;

namespace PersonalFinanceTracker.Repository
{
    public interface ICashRepository
    {
        Task<List<Cash>> GetCashAccountsAsync(string username);

        Task<Cash?> GetCashAccountAsync(string username, string cashAccountId);

        Task<bool> AddCashAccountAsync(string username, Cash cashAccount);

        Task<bool> UpdateCashAccountAsync(string username, Cash cashAccount);

        Task<bool> DeleteCashAccountAsync(string username, string cashAccountId);

        Task<bool> AddCashTransactionAsync(
            string username,
            string cashAccountId,
            CashTransaction transaction);

        Task<MonthlyBudget?> GetMonthlyBudgetAsync(string username, string month);

        Task<bool> SetMonthlyBudgetAsync(string username, MonthlyBudget budget);
    }
}