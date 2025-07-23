using Garderoba.Common;
using System.Reflection;

namespace Garderoba.Model
{
    public class Costume
    {
        public Guid Id { get; set; }
        public string Name { get; set; } 

        public string Area { get; set; }

        public Gender Gender { get; set; } 

        public CostumeStatus Status { get; set; }

        public DateTime? DateCreated { get; set; }

        public DateTime? DateUpdated { get; set; }

        public Guid CreatedByUserId { get; set; } 
        public User CreatedByUser { get; set; }

        public List<CostumePart> Parts { get; set; }
    }
}
