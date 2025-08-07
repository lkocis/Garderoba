using Npgsql;

namespace Garderoba.Repository.Common
{
    public interface IPerformanceRepository
    {
        Task<int> GetMenCostumeCountAsync(Guid choreographyId);
        Task<List<Guid>> GetMaleCostumeIdsByChoreoIdAsync(Guid choreographyId);
        Task<List<string>> GetNecessaryPartsListAsync(Guid costumeId);
        Task<bool> CompareNecessaryAndActualPartsAsync(Guid costumeId);
        Task<(bool AllPartsAvailable, List<string> MissingParts)> CheckIfAllNecessaryPartsInStockWithMissingListAsync(Guid choreographyId);
    }
}
