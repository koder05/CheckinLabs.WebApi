using System.ComponentModel.DataAnnotations;

namespace CheckinLabs.BL.Models
{ 
    public class IdObject
    {
        [Required]
        [Key]
        public int Id { get; set; }
    }
}
