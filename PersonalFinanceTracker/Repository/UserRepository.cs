using Google.Cloud.Firestore;
using PersonalFinanceTracker.Model;

namespace PersonalFinanceTracker.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly FirestoreDb _database;

        public UserRepository(FirestoreDb database)
        {
            _database = database;
        }

        public async Task<bool> AddUserAsync(User newUser)
        {
            if (newUser == null)
            {
                return false;
            }

            try
            {
                await _database
                    .Collection("users")
                    .Document(newUser.Username)
                    .SetAsync(newUser);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<User?> GetUserAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            DocumentSnapshot document = await _database
                .Collection("users")
                .Document(username)
                .GetSnapshotAsync();

            if (!document.Exists)
            {
                return null;
            }

            return document.ConvertTo<User>();
        }

        public async Task<bool> UpdateUserAsync(User updatedUser)
        {
            if (updatedUser == null)
            {
                return false;
            }

            try
            {
                await _database
                    .Collection("users")
                    .Document(updatedUser.Username)
                    .SetAsync(updatedUser, SetOptions.Overwrite);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            try
            {
                await _database
                    .Collection("users")
                    .Document(username)
                    .DeleteAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}