using Garderoba.Repository.Common;
using Garderoba.Service.Common;
using System.Security.Claims;

namespace Garderoba.Service
{
    public class ColaborativeFilteringService : IColaborativeFilteringService
    {
        private readonly IColaborativeFilteringRepository _colaborativeFilteringRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ColaborativeFilteringService(IColaborativeFilteringRepository colaborativeFilteringRepository, IHttpContextAccessor httpContextAccessor)
        {
            _colaborativeFilteringRepository = colaborativeFilteringRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Dictionary<Guid, Dictionary<Guid, int>>> FindUserWithCostumePartsAsync(Guid choreographyId)
        {
            var userIdString = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid currentUserId))
            {
                throw new Exception("Current user ID not found or invalid.");
            }

            return await _colaborativeFilteringRepository.FindUserWithCostumePartsAsync(choreographyId, currentUserId);
        }
    }
}
