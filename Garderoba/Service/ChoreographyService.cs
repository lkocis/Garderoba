using Garderoba.Model;
using Garderoba.Repository.Common;
using Garderoba.Service.Common;

namespace Garderoba.Service
{
    public class ChoreographyService : IChoreographyService
    {
        private IChoreographyRepository _choreographyRepository;

        public ChoreographyService(IChoreographyRepository choreographyRepository)
        {
            _choreographyRepository = choreographyRepository;
        }

        public async Task<bool> CreateNewChoreographyAsync(Choreography choreography)
        {
            return await _choreographyRepository.CreateNewChoreographyAsync(choreography);
        }

    }
}
