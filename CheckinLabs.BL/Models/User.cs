using CheckinLabs.BL.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CheckinLabs.BL.Models
{
    public class User : IdNamedObject
    {
        [Required]
        public Guid UID { get; set; } = Guid.NewGuid();
        [Required]
        public byte[] SecretHash { get; set; }
        [Required]
        public byte[] SecretSalt { get; set; }
        [Required]
        public AccountState AccountState { get; set; }
        [Required]
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string CreatedOver { get; set; }
        //public virtual ICollection<AppRole> Roles { get; set; }
        public void SetPassword(string pwd)
        {
            this.SecretSalt = PBKDF2.GenerateSalt();
            this.SecretHash = PBKDF2.Hash(this.Name, pwd, this.SecretSalt);
        }
    }
}
