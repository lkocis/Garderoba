namespace Garderoba.Service.Common
{
    public interface IColaborativeFilteringService
    {
        Task<Dictionary<Guid, List<Guid>>> FindUserWithCostumePartsAsync(Guid choreographyId);
    }
}
