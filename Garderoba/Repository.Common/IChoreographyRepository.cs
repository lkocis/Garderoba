using Garderoba.Model;

namespace Garderoba.Repository.Common
{
    public interface IChoreographyRepository
    {
        Task<bool> CreateNewChoreographyAsync(Choreography choreography);
    }
}
