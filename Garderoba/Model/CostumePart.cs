using Garderoba.Common;

namespace Garderoba.Model
{
    public class CostumePart
    {
        public Guid Id { get; set; } 

        public Guid CostumeId { get; set; } 

        public string Region { get; set; }

        public string Name { get; set; }
        public int PartNumber {  get; set; }
        public Gender Gender { get; set; }
        public CostumeStatus Status { get; set; }

        public DateTime? DateCreated { get; set; }

        public Costume Costume { get; set; }
    }
}
