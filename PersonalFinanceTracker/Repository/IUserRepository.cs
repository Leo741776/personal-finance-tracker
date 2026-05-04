using PersonalFinanceTracker.Model;

namespace PersonalFinanceTracker.Repository
{
    public interface IUserRepository
    {
        Task<bool> AddUserAsync(User newUser);

        Task<User?> GetUserAsync(string username);

        Task<bool> UpdateUserAsync(User updatedUser);

        Task<bool> DeleteUserAsync(string username);
    }
}