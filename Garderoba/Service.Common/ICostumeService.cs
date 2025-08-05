using Garderoba.Model;
using Garderoba.WebApi.ViewModel;

namespace Garderoba.Service.Common
{
    public interface ICostumeService
    {
        Task<bool> CreateNewCostumeAsync(Costume costume);
        Task<bool> UpdateCostumePartAsync(Guid id, UpdatedCostumePartFields updatedFields);
    }
}
