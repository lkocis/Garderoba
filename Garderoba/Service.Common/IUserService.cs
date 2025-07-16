using Garderoba.Model;

namespace Garderoba.Service.Common
{
    public interface IUserService
    {
        Task<bool> CreateUserAsync(User newUser);
        Task<bool> UpdateUserAsync(User newUser);
        Task<User?> ReadUserAsync(Guid id);
        Task<User?> LoginUserAsync(string email, string password);
    }
}
