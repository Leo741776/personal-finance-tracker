using Google.Cloud.Firestore;
using PersonalFinanceTracker.Model;
using System.Windows;

namespace PersonalFinanceTracker.Repository
{
    public class SavingsRepository : ISavingsRepository
    {
        private readonly FirestoreDb _database;

        public SavingsRepository(FirestoreDb database)
        {
            _database = database;
        }

        public async Task<List<Savings>> GetSavingsAccountsAsync(string username)
        {
            List<Savings> accounts = new();

            if (string.IsNullOrWhiteSpace(username))
            {
                return accounts;
            }

            QuerySnapshot snapshot = await GetSavingsAccountsCollection(username)
                .GetSnapshotAsync();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (!document.Exists)
                {
                    continue;
                }

                Savings account = document.ConvertTo<Savings>();

                if (account.OwnerUsername == username)
                {
                    accounts.Add(account);
                }
            }

            return accounts;
        }

        public async Task<Savings?> GetSavingsAccountAsync(string username, string savingsAccountId)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(savingsAccountId))
            {
                return null;
            }

            DocumentSnapshot document = await GetSavingsAccountsCollection(username)
                .Document(savingsAccountId)
                .GetSnapshotAsync();

            if (!document.Exists)
            {
                return null;
            }

            Savings account = document.ConvertTo<Savings>();

            return account.OwnerUsername == username ? account : null;
        }

        public async Task<bool> AddSavingsAccountAsync(string username, Savings savingsAccount)
        {
            if (string.IsNullOrWhiteSpace(username) || savingsAccount == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(savingsAccount.Id))
            {
                savingsAccount.Id = Guid.NewGuid().ToString();
            }

            savingsAccount.OwnerUsername = username;

            try
            {
                await GetSavingsAccountsCollection(username)
                    .Document(savingsAccount.Id)
                    .SetAsync(savingsAccount);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public async Task<bool> UpdateSavingsAccountAsync(string username, Savings savingsAccount)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                savingsAccount == null ||
                string.IsNullOrWhiteSpace(savingsAccount.Id))
            {
                return false;
            }

            savingsAccount.OwnerUsername = username;

            try
            {
                await GetSavingsAccountsCollection(username)
                    .Document(savingsAccount.Id)
                    .SetAsync(savingsAccount, SetOptions.Overwrite);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteSavingsAccountAsync(string username, string savingsAccountId)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(savingsAccountId))
            {
                return false;
            }

            try
            {
                await GetSavingsAccountsCollection(username)
                    .Document(savingsAccountId)
                    .DeleteAsync();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public async Task<List<SavingsGoal>> GetSavingsGoalsAsync(string username)
        {
            List<SavingsGoal> goals = new();

            if (string.IsNullOrWhiteSpace(username))
            {
                return goals;
            }

            QuerySnapshot snapshot = await GetSavingsGoalsCollection(username)
                .GetSnapshotAsync();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (!document.Exists)
                {
                    continue;
                }

                SavingsGoal goal = document.ConvertTo<SavingsGoal>();

                if (goal.OwnerUsername == username)
                {
                    goals.Add(goal);
                }
            }

            return goals;
        }

        public async Task<bool> AddSavingsGoalAsync(string username, SavingsGoal goal)
        {
            if (string.IsNullOrWhiteSpace(username) || goal == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(goal.Id))
            {
                goal.Id = Guid.NewGuid().ToString();
            }

            goal.OwnerUsername = username;

            try
            {
                await GetSavingsGoalsCollection(username)
                    .Document(goal.Id)
                    .SetAsync(goal);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public async Task<bool> UpdateSavingsGoalAsync(string username, SavingsGoal goal)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                goal == null ||
                string.IsNullOrWhiteSpace(goal.Id))
            {
                return false;
            }

            goal.OwnerUsername = username;

            try
            {
                await GetSavingsGoalsCollection(username)
                    .Document(goal.Id)
                    .SetAsync(goal, SetOptions.Overwrite);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteSavingsGoalAsync(string username, string goalId)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(goalId))
            {
                return false;
            }

            try
            {
                await GetSavingsGoalsCollection(username)
                    .Document(goalId)
                    .DeleteAsync();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public async Task<bool> AddSavingsTransactionAsync(string username, SavingsTransaction transaction)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                transaction == null ||
                string.IsNullOrWhiteSpace(transaction.ToSavingsAccountId))
            {
                return false;
            }

            Savings? savingsAccount = await GetSavingsAccountAsync(
                username,
                transaction.ToSavingsAccountId);

            if (savingsAccount == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(transaction.Id))
            {
                transaction.Id = Guid.NewGuid().ToString();
            }

            transaction.OwnerUsername = username;

            savingsAccount.Balance += transaction.Amount;
            savingsAccount.OwnerUsername = username;

            try
            {
                await GetSavingsTransactionsCollection(username)
                    .Document(transaction.Id)
                    .SetAsync(transaction);

                await UpdateSavingsAccountAsync(username, savingsAccount);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        private CollectionReference GetSavingsAccountsCollection(string username)
        {
            return _database
                .Collection("users")
                .Document(username)
                .Collection("savingsAccounts");
        }

        private CollectionReference GetSavingsGoalsCollection(string username)
        {
            return _database
                .Collection("users")
                .Document(username)
                .Collection("savingsGoals");
        }

        private CollectionReference GetSavingsTransactionsCollection(string username)
        {
            return _database
                .Collection("users")
                .Document(username)
                .Collection("savingsTransactions");
        }
    }
}