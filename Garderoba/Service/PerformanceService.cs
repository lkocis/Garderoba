using Garderoba.Model;
using Garderoba.Repository.Common;
using Garderoba.Service.Common;
using Npgsql;

namespace Garderoba.Service
{
    public class PerformanceService : IPerformanceService
    {
        private IPerformanceRepository _performanceRepository;

        public PerformanceService(IPerformanceRepository performanceRepository)
        {
            _performanceRepository = performanceRepository;
        }
        public async Task<(bool AllPartsAvailable, List<string> MissingParts)> CheckIfAllNecessaryPartsInStockWithMissingListAsync(Guid choreographyId)
        {
            return await _performanceRepository.CheckIfAllNecessaryPartsInStockWithMissingListAsync(choreographyId);
        }
    }
}
