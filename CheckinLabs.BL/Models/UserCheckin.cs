using CheckinLabs.BL.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CheckinLabs.BL.Models
{
    public class UserCheckin : IdObject
    {
        public virtual UserProfile UserProfile { get; set; }
        public string CheckinAddr { get; set; }
        public NotifyChannelType NotifyChannelType { get; set; }
        public UserCheckinType UserCheckinType { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime? NotifyDate { get; set; }
        public string Code { get; set; } = Guid.NewGuid().ToString();
        public string Msg { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? CheckinDate { get; set; }
        [NotMapped]
        public bool OutOfDate => CreateDate.AddDays(3) < DateTime.Now;
    }
}
