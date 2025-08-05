using Garderoba.Model;
using Garderoba.WebApi.ViewModel;

namespace Garderoba.Repository.Common
{
    public interface ICostumeRepository
    {
        Task<bool> CreateNewCostumeAsync(Costume costume);
        Task<bool> UpdateCostumePartAsync(Guid id, UpdatedCostumePartFields updatedFields);
        Task<bool> AddCostumePartAsync(Guid costumeId, CostumePart newPart);
        Task<bool> DeleteCostumePartAsync(Guid id);
        Task<bool> DeleteCostumeWithPartsAsync(Guid costumeId);
        Task<List<Costume>> GetAllCostumesAsync();
        Task<List<CostumePart>> GetAllCostumePartsAsync(Guid costumeId);
    }
}
