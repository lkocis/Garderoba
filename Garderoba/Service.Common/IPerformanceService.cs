using Garderoba.WebApi.ViewModel;
using Npgsql;

namespace Garderoba.Service.Common
{
    public interface IPerformanceService
    {
        Task<(bool AllPartsAvailable, List<MissingPartsVM> MissingParts)> CheckIfAllNecessaryPartsInStockWithMissingListAsync(Guid choreographyId);
    }
}
