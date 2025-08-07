namespace Garderoba.Model
{
    public class Choreography
    {
        public Guid Id { get; set; } 

        public string Name { get; set; } 

        public string Area { get; set; }

        public int MenCostumeCount { get; set; }

        public int WomenCostumeCount { get; set; }

        public DateTime? DateCreated { get; set; }

        public DateTime? DateUpdated { get; set; }

        public Guid CreatedByUserId { get; set; } 

        public User CreatedByUser { get; set; }
        public List<ChoreographyCostume> ChoreographyCostumes { get; set; }
    }
}
