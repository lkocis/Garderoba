using Garderoba.Repository.Common;
using Garderoba.Service.Common;

namespace Garderoba.Service
{
    public class CostumeService : ICostumeService
    {
        private readonly ICostumeRepository _costumeRepository;

        public CostumeService(ICostumeRepository costumeRepository)
        {
            _costumeRepository = costumeRepository;
        }
    }
}
