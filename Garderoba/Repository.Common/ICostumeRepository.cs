using Garderoba.Model;
using Garderoba.WebApi.ViewModel;

namespace Garderoba.Repository.Common
{
    public interface ICostumeRepository
    {
        Task<bool> CreateNewCostumeAsync(Costume costume, Guid? choreographyId, Guid userId);
        Task<bool> UpdateCostumePartAsync(Guid id, UpdatedCostumePartFields updatedFields);
        Task<bool> AddCostumePartAsync(CostumePart newPart, Guid costumeId);
        Task<bool> DeleteCostumePartAsync(Guid id);
        Task<bool> DeleteCostumeWithPartsAsync(Guid costumeId);
        Task<List<Costume>> GetAllCostumesAsync(Guid userId, Guid choreographyId);
        Task<List<CostumePart>> GetAllCostumePartsAsync(Guid userId, Guid costumeId);
        Task<CostumePart> GetCostumePartByIdAsync(Guid partId);
    }
}
