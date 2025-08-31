using Garderoba.WebApi.ViewModel;
using Npgsql;

namespace Garderoba.Repository.Common
{
    public interface IPerformanceRepository
    {
        Task<int> GetCostumeCountAsync(Guid choreographyId, int gender);
        Task<List<Guid>> GetCostumeIdsByChoreoIdAsync(Guid choreographyId, int gender, Guid userId);
        Task<List<string>> GetNecessaryPartsListAsync(Guid costumeId);
        Task<List<string>> GetAllNecessaryPartsByChoreoIdAsync(Guid choreographyId);
        Task<(bool AllPartsAvailable, List<MissingPartsVM> MissingParts)> CheckIfAllNecessaryPartsInStockWithMissingListAsync(Guid choreographyId, Guid userId);
    }
}
