using Garderoba.Model;
using Garderoba.Repository.Common;
using Garderoba.Service.Common;
using Garderoba.WebApi.ViewModel;

namespace Garderoba.Service
{
    public class CostumePartService : ICostumePartService
    {
        private readonly ICostumePartRepository _costumePartRepository;

        public CostumePartService(ICostumePartRepository costumePartRepository)
        {
            _costumePartRepository = costumePartRepository;
        }
    }
}
