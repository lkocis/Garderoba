using Garderoba.Model;

namespace Garderoba.Repository.Common
{
    public interface ICostumeRepository
    {
        Task<bool> CreateNewCostumeAsync(Costume costume);
    }
}
