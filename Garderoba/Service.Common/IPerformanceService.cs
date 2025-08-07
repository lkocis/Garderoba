using Npgsql;

namespace Garderoba.Service.Common
{
    public interface IPerformanceService
    {
        Task<(bool AllPartsAvailable, List<string> MissingParts)> CheckIfAllNecessaryPartsInStockWithMissingListAsync(Guid choreographyId);
    }
}
