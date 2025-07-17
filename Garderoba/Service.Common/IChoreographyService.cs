using Garderoba.Model;

namespace Garderoba.Service.Common
{
    public interface IChoreographyService
    {
        Task<bool> CreateNewChoreographyAsync(Choreography choreography);
    }
}
