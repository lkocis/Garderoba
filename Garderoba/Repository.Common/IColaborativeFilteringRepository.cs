namespace Garderoba.Repository.Common
{
    public interface IColaborativeFilteringRepository
    {
        Task<Dictionary<Guid, Dictionary<Guid, int>>> FindUserWithCostumePartsAsync(Guid choreographyId);
    }
}
