namespace Garderoba.Repository.Common
{
    public interface IColaborativeFilteringRepository
    {
        Task<Dictionary<Guid, List<Guid>>> FindUserWithCostumePartsAsync(Guid choreographyId);
    }
}
