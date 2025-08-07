using Npgsql;

namespace Garderoba.Repository.Common
{
    public interface IPerformanceRepository
    {
        Task<int> GetCostumeCountAsync(Guid choreographyId, int gender);
        Task<List<Guid>> GetCostumeIdsByChoreoIdAsync(Guid choreographyId, int gender);
        Task<List<string>> GetNecessaryPartsListAsync(Guid costumeId);
        Task<(bool AllPartsAvailable, List<string> MissingParts)> CheckIfAllNecessaryPartsInStockWithMissingListAsync(Guid choreographyId);
    }
}
