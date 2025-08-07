namespace Garderoba.Model
{
    public class ChoreographyCostume
    {
        public Guid Id { get; set; }

        public Guid ChoreographyId { get; set; }
        public Choreography Choreography { get; set; }

        public Guid CostumeId { get; set; }
        public Costume Costume { get; set; }
    }
    
}
