using Garderoba.Repository.Common;
using Garderoba.Service.Common;

namespace Garderoba.Service
{
    public class ColaborativeFilteringService : IColaborativeFilteringService
    {
        private readonly IColaborativeFilteringRepository _colaborativeFilteringRepository;

        public ColaborativeFilteringService(IColaborativeFilteringRepository colaborativeFilteringRepository)
        {
            _colaborativeFilteringRepository = colaborativeFilteringRepository;
        }

        public async Task<Dictionary<Guid, Dictionary<Guid, int>>> FindUserWithCostumePartsAsync(Guid choreographyId)
        {
            return await _colaborativeFilteringRepository.FindUserWithCostumePartsAsync(choreographyId);
        }
    }
}
