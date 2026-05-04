using Google.Cloud.Firestore;
using PersonalFinanceTracker.Model;

namespace PersonalFinanceTracker.Repository
{
    public class BrokerageRepository : IBrokerageRepository
    {
        private readonly FirestoreDb _database;

        public BrokerageRepository(FirestoreDb database)
        {
            _database = database;
        }

        public async Task<List<Brokerage>> GetBrokerageAccountsAsync(string username)
        {
            List<Brokerage> accounts = new();

            if (string.IsNullOrWhiteSpace(username))
            {
                return accounts;
            }

            QuerySnapshot snapshot = await GetBrokerageAccountsCollection(username)
                .GetSnapshotAsync();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (!document.Exists)
                {
                    continue;
                }

                Brokerage account = document.ConvertTo<Brokerage>();

                if (account.OwnerUsername == username)
                {
                    accounts.Add(account);
                }
            }

            return accounts;
        }

        public async Task<Brokerage?> GetBrokerageAccountAsync(string username, string brokerageAccountId)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(brokerageAccountId))
            {
                return null;
            }

            DocumentSnapshot document = await GetBrokerageAccountsCollection(username)
                .Document(brokerageAccountId)
                .GetSnapshotAsync();

            if (!document.Exists)
            {
                return null;
            }

            Brokerage account = document.ConvertTo<Brokerage>();

            return account.OwnerUsername == username ? account : null;
        }

        public async Task<bool> AddBrokerageAccountAsync(string username, Brokerage brokerageAccount)
        {
            if (string.IsNullOrWhiteSpace(username) || brokerageAccount == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(brokerageAccount.Id))
            {
                brokerageAccount.Id = Guid.NewGuid().ToString();
            }

            brokerageAccount.OwnerUsername = username;

            foreach (Holding holding in brokerageAccount.Holdings)
            {
                holding.OwnerUsername = username;
            }

            foreach (BrokerageTransaction transaction in brokerageAccount.Transactions)
            {
                transaction.OwnerUsername = username;
            }

            try
            {
                await GetBrokerageAccountsCollection(username)
                    .Document(brokerageAccount.Id)
                    .SetAsync(brokerageAccount);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateBrokerageAccountAsync(string username, Brokerage brokerageAccount)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                brokerageAccount == null ||
                string.IsNullOrWhiteSpace(brokerageAccount.Id))
            {
                return false;
            }

            brokerageAccount.OwnerUsername = username;

            foreach (Holding holding in brokerageAccount.Holdings)
            {
                holding.OwnerUsername = username;
            }

            foreach (BrokerageTransaction transaction in brokerageAccount.Transactions)
            {
                transaction.OwnerUsername = username;
            }

            try
            {
                await GetBrokerageAccountsCollection(username)
                    .Document(brokerageAccount.Id)
                    .SetAsync(brokerageAccount, SetOptions.Overwrite);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteBrokerageAccountAsync(string username, string brokerageAccountId)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(brokerageAccountId))
            {
                return false;
            }

            try
            {
                await GetBrokerageAccountsCollection(username)
                    .Document(brokerageAccountId)
                    .DeleteAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddHoldingAsync(string username, string brokerageAccountId, Holding holding)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(brokerageAccountId) ||
                holding == null ||
                string.IsNullOrWhiteSpace(holding.Ticker))
            {
                return false;
            }

            Brokerage? brokerageAccount = await GetBrokerageAccountAsync(
                username,
                brokerageAccountId);

            if (brokerageAccount == null)
            {
                return false;
            }

            holding.OwnerUsername = username;
            holding.Ticker = holding.Ticker.Trim().ToUpper();

            Holding? existingHolding = brokerageAccount.Holdings
                .FirstOrDefault(existing =>
                    existing.Ticker.Equals(holding.Ticker, StringComparison.OrdinalIgnoreCase));

            if (existingHolding == null)
            {
                brokerageAccount.Holdings.Add(holding);
            }
            else
            {
                decimal oldTotalCost = existingHolding.Shares * existingHolding.AverageCost;
                decimal newTotalCost = holding.Shares * holding.AverageCost;
                decimal combinedShares = existingHolding.Shares + holding.Shares;

                existingHolding.OwnerUsername = username;
                existingHolding.Shares = combinedShares;
                existingHolding.AverageCost = combinedShares == 0
                    ? 0
                    : (oldTotalCost + newTotalCost) / combinedShares;
                existingHolding.CurrentPrice = holding.CurrentPrice;

                if (!string.IsNullOrWhiteSpace(holding.Name))
                {
                    existingHolding.Name = holding.Name;
                }
            }

            return await UpdateBrokerageAccountAsync(username, brokerageAccount);
        }

        public async Task<bool> AddBrokerageTransactionAsync(
            string username,
            string brokerageAccountId,
            BrokerageTransaction transaction)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(brokerageAccountId) ||
                transaction == null)
            {
                return false;
            }

            Brokerage? brokerageAccount = await GetBrokerageAccountAsync(
                username,
                brokerageAccountId);

            if (brokerageAccount == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(transaction.Id))
            {
                transaction.Id = Guid.NewGuid().ToString();
            }

            transaction.OwnerUsername = username;
            transaction.Ticker = transaction.Ticker.Trim().ToUpper();

            brokerageAccount.Transactions.Add(transaction);

            if (transaction.Type == BrokerageTransactionType.Sell)
            {
                decimal totalProceeds = transaction.Quantity * transaction.Price - transaction.Fees;
                brokerageAccount.CashBalance += totalProceeds;
            }
            else if (transaction.Type == BrokerageTransactionType.Dividend)
            {
                brokerageAccount.CashBalance += transaction.Price;
            }

            return await UpdateBrokerageAccountAsync(username, brokerageAccount);
        }

        private CollectionReference GetBrokerageAccountsCollection(string username)
        {
            return _database
                .Collection("users")
                .Document(username)
                .Collection("brokerageAccounts");
        }
    }
}