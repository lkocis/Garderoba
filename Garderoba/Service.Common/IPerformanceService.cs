using Npgsql;

namespace Garderoba.Service.Common
{
    public interface IPerformanceService
    {
        Task<int> GetMenCostumeCountAsync(Guid choreographyId);
        Task<List<Guid>> GetMaleCostumeIdsByChoreoIdAsync(Guid choreographyId);
        Task<List<string>> GetNecessaryPartsListAsync(Guid costumeId);
        Task<bool> CompareNecessaryAndActualPartsAsync(Guid costumeId);
        Task<(bool AllPartsAvailable, List<string> MissingParts)> CheckIfAllNecessaryPartsInStockWithMissingListAsync(Guid choreographyId);
    }
}
