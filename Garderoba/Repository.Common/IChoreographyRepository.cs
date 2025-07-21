using Garderoba.Model;
using Garderoba.WebApi.ViewModel;

namespace Garderoba.Repository.Common
{
    public interface IChoreographyRepository
    {
        Task<bool> CreateNewChoreographyAsync(Choreography choreography);
        Task<bool> DeleteChoreographyByIdAsync(Guid id);
        Task<List<Choreography>> GetAllChoreographiesAsync();
        Task<bool> UpdateChoreographyByIdAsync(Guid id, UpdatedChoreographyFields updatedChoreography);
    }
}
