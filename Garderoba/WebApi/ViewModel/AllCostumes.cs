using Garderoba.Common;

namespace Garderoba.WebApi.ViewModel
{
    public class AllCostumes
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public string Area { get; set; }

        public Gender Gender { get; set; }

        public CostumeStatus Status { get; set; }
        public string NecessaryParts { get; set; }
    }
}
