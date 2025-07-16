using Garderoba.Model;

namespace Garderoba.Repository.Common
{
    public interface IUserRepository
    {
        Task<bool> CreateUserAsync(User newUser);
        Task<bool> UpdateUserAsync(User newUser);
        Task<User?> ReadUserAsync(Guid id);
        Task<User?> LoginUserAsync(string email, string password);
    }
}
