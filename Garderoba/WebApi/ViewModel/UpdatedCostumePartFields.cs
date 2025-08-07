using Garderoba.Common;

namespace Garderoba.WebApi.ViewModel
{
    public class UpdatedCostumePartFields
    {
        public Guid Id { get; set; } 
        public string? Region { get; set; }
        public string? Name { get; set; }
        public int? PartNumber { get; set; }
        public CostumeStatus? Status { get; set; }
        public Gender? Gender { get; set; }
    }
}
