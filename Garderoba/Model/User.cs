namespace Garderoba.Model
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } 

        public string Password { get; set; } 

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PhoneNumber { get; set; }

        public string Area { get; set; }

        public string KUDName { get; set; }

        public DateTime? DateCreated { get; set; }

        public DateTime? DateUpdated { get; set; }
    }
}
