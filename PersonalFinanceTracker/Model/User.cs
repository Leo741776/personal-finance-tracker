using Google.Cloud.Firestore;

namespace PersonalFinanceTracker.Model
{
    [FirestoreData]
    public class User
    {
        public User()
        {
        }

        public User(
            string userFirstName,
            string userLastName,
            string username,
            string passwordHash,
            string passwordSalt)
        {
            UserFirstName = userFirstName;
            UserLastName = userLastName;
            Username = username;
            PasswordHash = passwordHash;
            PasswordSalt = passwordSalt;
        }

        [FirestoreProperty]
        public string UserFirstName { get; set; } = string.Empty;

        [FirestoreProperty]
        public string UserLastName { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Username { get; set; } = string.Empty;

        [FirestoreProperty]
        public string PasswordHash { get; set; } = string.Empty;

        [FirestoreProperty]
        public string PasswordSalt { get; set; } = string.Empty;
    }
}