using Garderoba.Model;
using Garderoba.Repository.Common;
using Garderoba.Service.Common;
using Garderoba.WebApi.ViewModel;

namespace Garderoba.Service
{
    public class UserService : IUserService
    {
        private IUserRepository _userRepository;
        public UserService(IUserRepository userRepository) 
        {
            _userRepository = userRepository;
        }
        public async Task<bool> CreateUserAsync(User newUser)
        {
            return await _userRepository.CreateUserAsync(newUser);
        }
        public async Task<bool> UpdateUserAsync(Guid id, UpdatedUserInfoFields updatedUser)
        {
            return await _userRepository.UpdateUserAsync(id, updatedUser);
        }
        public async Task<User?> ReadUserAsync(Guid id)
        {
            return await _userRepository.ReadUserAsync(id);
        }

        public async Task<User?> LoginUserAsync(string email, string password)
        {
            return await _userRepository.LoginUserAsync(email, password);
        }
    }
}
