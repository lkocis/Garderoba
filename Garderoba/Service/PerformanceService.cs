using Garderoba.Model;
using Garderoba.Repository.Common;
using Garderoba.Service.Common;
using Garderoba.WebApi.ViewModel;
using Npgsql;
using System.Security.Claims;

namespace Garderoba.Service
{
    public class PerformanceService : IPerformanceService
    {
        private IPerformanceRepository _performanceRepository;
        private IHttpContextAccessor _httpContextAccessor;

        public PerformanceService(IPerformanceRepository performanceRepository, IHttpContextAccessor httpContextAccessor)
        {
            _performanceRepository = performanceRepository;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<(bool AllPartsAvailable, List<MissingPartsVM> MissingParts)> CheckIfAllNecessaryPartsInStockWithMissingListAsync(Guid choreographyId)
        {
            var userIdString = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                Console.WriteLine("User ID not found in token.");
            }

            var tokenUserId = userId;

            return await _performanceRepository.CheckIfAllNecessaryPartsInStockWithMissingListAsync(choreographyId, userId);
        }
    }
}
