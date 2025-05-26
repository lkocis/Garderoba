using Garderoba.Common;

namespace Garderoba.Model
{
    public class Performance
    {
        public Guid Id { get; set; } 

        public Guid ChoreographyId { get; set; } 

        public Guid UserId { get; set; } 

        public DateTime? DatePerformed { get; set; }

        public string Comment { get; set; }

        public CostumeStatus CostumeStatus { get; set; } 

        public DateTime? DateCreated { get; set; }

        public Choreography Choreography { get; set; }

        public User User { get; set; }
    }
}
