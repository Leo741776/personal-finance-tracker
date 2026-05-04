using PersonalFinanceTracker.Model;

namespace PersonalFinanceTracker.Repository
{
    public interface ISavingsRepository
    {
        Task<List<Savings>> GetSavingsAccountsAsync(string username);

        Task<Savings?> GetSavingsAccountAsync(string username, string savingsAccountId);

        Task<bool> AddSavingsAccountAsync(string username, Savings savingsAccount);

        Task<bool> UpdateSavingsAccountAsync(string username, Savings savingsAccount);

        Task<bool> DeleteSavingsAccountAsync(string username, string savingsAccountId);

        Task<List<SavingsGoal>> GetSavingsGoalsAsync(string username);

        Task<bool> AddSavingsGoalAsync(string username, SavingsGoal goal);

        Task<bool> UpdateSavingsGoalAsync(string username, SavingsGoal goal);

        Task<bool> DeleteSavingsGoalAsync(string username, string goalId);

        Task<bool> AddSavingsTransactionAsync(string username, SavingsTransaction transaction);
    }
}