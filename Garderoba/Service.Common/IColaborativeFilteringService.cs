namespace Garderoba.Service.Common
{
    public interface IColaborativeFilteringService
    {
        Task<Dictionary<Guid, Dictionary<Guid, int>>> FindUserWithCostumePartsAsync(Guid choreographyId);
    }
}
