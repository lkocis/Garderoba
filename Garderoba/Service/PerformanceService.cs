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

        public async Task<int> GetMenCostumeCountAsync(Guid choreographyId)
        {
            return await _performanceRepository.GetMenCostumeCountAsync(choreographyId);
        }
        public async Task<List<Guid>> GetMaleCostumeIdsByChoreoIdAsync(Guid choreographyId)
        {
            return await _performanceRepository.GetMaleCostumeIdsByChoreoIdAsync(choreographyId);
        }

        public async Task<List<string>> GetNecessaryPartsListAsync(Guid costumeId)
        {
            return await _performanceRepository.GetNecessaryPartsListAsync(costumeId);
        }
        public async Task<bool> CompareNecessaryAndActualPartsAsync(Guid costumeId)
        {
            return await _performanceRepository.CompareNecessaryAndActualPartsAsync(costumeId);
        }
        public async Task<(bool AllPartsAvailable, List<string> MissingParts)> CheckIfAllNecessaryPartsInStockWithMissingListAsync(Guid choreographyId)
        {
            return await _performanceRepository.CheckIfAllNecessaryPartsInStockWithMissingListAsync(choreographyId);
        }
    }
}
