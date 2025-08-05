using Garderoba.Model;
using Garderoba.Repository;
using Garderoba.Repository.Common;
using Garderoba.Service.Common;
using Garderoba.WebApi.ViewModel;
using System.Security.Claims;

namespace Garderoba.Service
{
    public class CostumeService : ICostumeService
    {
        private readonly ICostumeRepository _costumeRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CostumeService(ICostumeRepository costumeRepository, IHttpContextAccessor httpContextAccessor)    
        {
            _costumeRepository = costumeRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> CreateNewCostumeAsync(Costume costume)
        {
            var userIdString = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                Console.WriteLine("User ID not found in token.");
                return false;
            }

            costume.CreatedByUserId = userId;
            return await _costumeRepository.CreateNewCostumeAsync(costume);
        }

        public async Task<bool> UpdateCostumePartAsync(Guid id, UpdatedCostumePartFields updatedFields)
        {
            return await _costumeRepository.UpdateCostumePartAsync(id, updatedFields);
        }
    }

}
