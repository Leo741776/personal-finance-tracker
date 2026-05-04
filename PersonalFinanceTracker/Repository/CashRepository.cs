using Google.Cloud.Firestore;
using PersonalFinanceTracker.Model;
using System.Windows;

namespace PersonalFinanceTracker.Repository
{
    public class CashRepository : ICashRepository
    {
        private readonly FirestoreDb _database;

        public CashRepository(FirestoreDb database)
        {
            _database = database;
        }

        public async Task<List<Cash>> GetCashAccountsAsync(string username)
        {
            List<Cash> accounts = new();

            if (string.IsNullOrWhiteSpace(username))
            {
                return accounts;
            }

            QuerySnapshot snapshot = await GetCashAccountsCollection(username)
                .GetSnapshotAsync();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (!document.Exists)
                {
                    continue;
                }

                Cash account = document.ConvertTo<Cash>();

                if (account.OwnerUsername == username)
                {
                    accounts.Add(account);
                }
            }

            return accounts;
        }

        public async Task<Cash?> GetCashAccountAsync(string username, string cashAccountId)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(cashAccountId))
            {
                return null;
            }

            DocumentSnapshot document = await GetCashAccountsCollection(username)
                .Document(cashAccountId)
                .GetSnapshotAsync();

            if (!document.Exists)
            {
                return null;
            }

            Cash account = document.ConvertTo<Cash>();

            return account.OwnerUsername == username ? account : null;
        }

        public async Task<bool> AddCashAccountAsync(string username, Cash cashAccount)
        {
            if (cashAccount == null || string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(cashAccount.Id))
            {
                cashAccount.Id = Guid.NewGuid().ToString();
            }

            cashAccount.OwnerUsername = username;

            foreach (CashTransaction transaction in cashAccount.Transactions)
            {
                transaction.OwnerUsername = username;
                transaction.AccountId = cashAccount.Id;
            }

            try
            {
                await GetCashAccountsCollection(username)
                    .Document(cashAccount.Id)
                    .SetAsync(cashAccount);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public async Task<bool> UpdateCashAccountAsync(string username, Cash cashAccount)
        {
            if (cashAccount == null ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(cashAccount.Id))
            {
                return false;
            }

            cashAccount.OwnerUsername = username;

            foreach (CashTransaction transaction in cashAccount.Transactions)
            {
                transaction.OwnerUsername = username;
                transaction.AccountId = cashAccount.Id;
            }

            try
            {
                await GetCashAccountsCollection(username)
                    .Document(cashAccount.Id)
                    .SetAsync(cashAccount, SetOptions.Overwrite);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteCashAccountAsync(string username, string cashAccountId)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(cashAccountId))
            {
                return false;
            }

            try
            {
                await GetCashAccountsCollection(username)
                    .Document(cashAccountId)
                    .DeleteAsync();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public async Task<bool> AddCashTransactionAsync(
            string username,
            string cashAccountId,
            CashTransaction transaction)
        {
            if (transaction == null ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(cashAccountId))
            {
                return false;
            }

            Cash? account = await GetCashAccountAsync(username, cashAccountId);

            if (account == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(transaction.Id))
            {
                transaction.Id = Guid.NewGuid().ToString();
            }

            transaction.OwnerUsername = username;
            transaction.AccountId = cashAccountId;

            account.Transactions.Add(transaction);
            account.Balance += transaction.Amount;

            return await UpdateCashAccountAsync(username, account);
        }

        public async Task<MonthlyBudget?> GetMonthlyBudgetAsync(string username, string month)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(month))
            {
                return null;
            }

            DocumentSnapshot document = await GetMonthlyBudgetsCollection(username)
                .Document(month)
                .GetSnapshotAsync();

            if (!document.Exists)
            {
                return null;
            }

            MonthlyBudget budget = document.ConvertTo<MonthlyBudget>();

            return budget.OwnerUsername == username ? budget : null;
        }

        public async Task<bool> SetMonthlyBudgetAsync(string username, MonthlyBudget budget)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                budget == null ||
                string.IsNullOrWhiteSpace(budget.Month))
            {
                return false;
            }

            budget.OwnerUsername = username;

            try
            {
                await GetMonthlyBudgetsCollection(username)
                    .Document(budget.Month)
                    .SetAsync(budget, SetOptions.Overwrite);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        private CollectionReference GetCashAccountsCollection(string username)
        {
            return _database
                .Collection("users")
                .Document(username)
                .Collection("cashAccounts");
        }

        private CollectionReference GetMonthlyBudgetsCollection(string username)
        {
            return _database
                .Collection("users")
                .Document(username)
                .Collection("monthlyBudgets");
        }
    }
}