using System.ComponentModel.DataAnnotations;

namespace CheckinLabs.BL.Models
{ 
    public class IdNamedObject : IdObject
    {
        [Required]
        public string Name { get; set; }
    }
}
