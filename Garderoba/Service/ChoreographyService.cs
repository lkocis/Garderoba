using Garderoba.Model;
using Garderoba.Repository.Common;
using Garderoba.Service.Common;
using Garderoba.WebApi.ViewModel;
using System.Security.Claims;

namespace Garderoba.Service
{
    public class ChoreographyService : IChoreographyService
    {
        private IChoreographyRepository _choreographyRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChoreographyService(IChoreographyRepository choreographyRepository, IHttpContextAccessor httpContextAccessor)
        {
            _choreographyRepository = choreographyRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> CreateNewChoreographyAsync(Choreography choreography)
        {
            var userIdString = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                Console.WriteLine("User ID not found in token.");
                return false;
            }

            choreography.CreatedByUserId = userId;
            return await _choreographyRepository.CreateNewChoreographyAsync(choreography);
        }

        public async Task<bool> DeleteChoreographyByIdAsync(Guid id)
        {
            return await _choreographyRepository.DeleteChoreographyByIdAsync(id);
        }

        public async Task<List<Choreography>> GetAllChoreographiesAsync()
        {
            return await _choreographyRepository.GetAllChoreographiesAsync();
        }

        public async Task<bool> UpdateChoreographyByIdAsync(Guid id, UpdatedChoreographyFields updatedChoreography)
        {
            return await _choreographyRepository.UpdateChoreographyByIdAsync(id, updatedChoreography);
        }
    }
}
