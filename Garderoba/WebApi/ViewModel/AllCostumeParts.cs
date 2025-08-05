using Garderoba.Common;

namespace Garderoba.WebApi.ViewModel
{
    public class AllCostumeParts
    {
        public string Region { get; set; }

        public string Name { get; set; }
        public int PartNumber { get; set; }
        public CostumeStatus Status { get; set; }
    }
}
