using Garderoba.Model;
using Garderoba.WebApi.ViewModel;

namespace Garderoba.Service.Common
{
    public interface ICostumeService
    {
        Task<bool> CreateNewCostumeAsync(Costume costume, Guid? choreographyId);
        Task<bool> UpdateCostumePartAsync(Guid id, UpdatedCostumePartFields updatedFields);
        Task<bool> AddCostumePartAsync(Guid costumeId, CostumePart newPart);
        Task<bool> DeleteCostumePartAsync(Guid id);
        Task<bool> DeleteCostumeWithPartsAsync(Guid costumeId);
        Task<List<Costume>> GetAllCostumesAsync();
        Task<List<CostumePart>> GetAllCostumePartsAsync(Guid costumeId);
    }
}
