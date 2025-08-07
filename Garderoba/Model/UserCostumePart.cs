namespace Garderoba.Model
{
    public class UserCostumePart
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid CostumePartId { get; set; }
        public CostumePart CostumePart { get; set; }

        public int Quantity { get; set; }  

        public DateTime? DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
    }
}
