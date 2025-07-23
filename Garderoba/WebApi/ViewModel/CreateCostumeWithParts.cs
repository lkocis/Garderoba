using Garderoba.Common;

namespace Garderoba.WebApi.ViewModel
{
    public class CreateCostumeWithParts
    {
        public string Name { get; set; }
        public string Area { get; set; }
        public Gender Gender { get; set; } 
        public CostumeStatus Status { get; set; }
        public List<CostumePartCreation> Parts { get; set; } = new();
    }
}
