using Garderoba.Repository;
using Garderoba.Service.Common;

namespace Garderoba.Service
{
    public class ColaborativeFilteringService : IColaborativeFilteringService
    {
        private readonly ColaborativeFilteringRepository _colaborativeFilteringRepository;

        public ColaborativeFilteringService(ColaborativeFilteringRepository colaborativeFilteringRepository)
        {
            _colaborativeFilteringRepository = colaborativeFilteringRepository;
        }

        public async Task<Dictionary<Guid, List<Guid>>> FindUserWithCostumePartsAsync(Guid choreographyId)
        {
            return await _colaborativeFilteringRepository.FindUserWithCostumePartsAsync(choreographyId);
        }
    }
}
