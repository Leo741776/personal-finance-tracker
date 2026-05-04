using PersonalFinanceTracker.Model;
using PersonalFinanceTracker.Repository;
using System.Security.Cryptography;

namespace PersonalFinanceTracker.Service
{
    public class Authenticator
    {
        private readonly IUserRepository _userRepository;

        public Authenticator(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User?> LoginAsync(string username, string password)
        {
            User? retrievedUser = await _userRepository.GetUserAsync(username);

            if (retrievedUser == null)
            {
                return null;
            }

            string hashedPassword = GenerateHash(password, retrievedUser.PasswordSalt);

            return hashedPassword == retrievedUser.PasswordHash
                ? retrievedUser
                : null;
        }

        public async Task<bool> RegisterAsync(
            string userFirstName,
            string userLastName,
            string username,
            string password)
        {
            User? existingUser = await _userRepository.GetUserAsync(username);

            if (existingUser != null)
            {
                return false;
            }

            string newSalt = GenerateSalt();
            string newPasswordHash = GenerateHash(password, newSalt);

            User newUser = new(
                userFirstName,
                userLastName,
                username,
                newPasswordHash,
                newSalt);

            return await _userRepository.AddUserAsync(newUser);
        }

        private static string GenerateHash(string password, string salt)
        {
            using Rfc2898DeriveBytes pbkdf2 = new(
                password,
                Convert.FromBase64String(salt),
                100_000,
                HashAlgorithmName.SHA256);

            return Convert.ToBase64String(pbkdf2.GetBytes(32));
        }

        private static string GenerateSalt()
        {
            byte[] saltBytes = RandomNumberGenerator.GetBytes(16);
            return Convert.ToBase64String(saltBytes);
        }
    }
}