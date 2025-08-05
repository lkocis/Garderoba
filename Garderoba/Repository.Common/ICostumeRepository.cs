using Garderoba.Model;
using Garderoba.WebApi.ViewModel;

namespace Garderoba.Repository.Common
{
    public interface ICostumeRepository
    {
        Task<bool> CreateNewCostumeAsync(Costume costume);
        Task<bool> UpdateCostumePartAsync(Guid id, UpdatedCostumePartFields updatedFields);
    }
}
