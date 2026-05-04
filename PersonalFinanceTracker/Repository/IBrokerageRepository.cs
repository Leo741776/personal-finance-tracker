using PersonalFinanceTracker.Model;

namespace PersonalFinanceTracker.Repository
{
    public interface IBrokerageRepository
    {
        Task<List<Brokerage>> GetBrokerageAccountsAsync(string username);

        Task<Brokerage?> GetBrokerageAccountAsync(string username, string brokerageAccountId);

        Task<bool> AddBrokerageAccountAsync(string username, Brokerage brokerageAccount);

        Task<bool> UpdateBrokerageAccountAsync(string username, Brokerage brokerageAccount);

        Task<bool> DeleteBrokerageAccountAsync(string username, string brokerageAccountId);

        Task<bool> AddHoldingAsync(string username, string brokerageAccountId, Holding holding);

        Task<bool> AddBrokerageTransactionAsync(
            string username,
            string brokerageAccountId,
            BrokerageTransaction transaction);
    }
}