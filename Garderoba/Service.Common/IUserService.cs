using Garderoba.Model;
using Garderoba.WebApi.ViewModel;

namespace Garderoba.Service.Common
{
    public interface IUserService
    {
        Task<bool> CreateUserAsync(User newUser);
        Task<bool> UpdateUserAsync(Guid id, UpdatedUserInfoFields updatedUser);
        Task<User?> ReadUserAsync(Guid id);
        Task<User?> LoginUserAsync(string email, string password);
    }
}
