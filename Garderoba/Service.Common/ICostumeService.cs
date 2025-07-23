using Garderoba.Model;

namespace Garderoba.Service.Common
{
    public interface ICostumeService
    {
        Task<bool> CreateNewCostumeAsync(Costume costume);
    }
}
