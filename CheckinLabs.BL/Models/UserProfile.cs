using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CheckinLabs.BL.Models
{
    public class UserProfile 
    {
        [ForeignKey(nameof(User))]
        public int Id { get; set; }
        public User User { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string DisplayName { get; set; }
        public string ManagerName { get; set; }
        public string CompanyName { get; set; }
    }
}
