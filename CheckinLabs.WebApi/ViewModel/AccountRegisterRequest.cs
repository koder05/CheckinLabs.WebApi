using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CheckinLabs.WebApi.ViewModel
{
    public class AccountRegisterRequest
    {
        [Required]
        public string UIClientId { get; set; }
        [Required]
        public string ManagerName { get; set; }
        [Required]
        public string Company { get; set; }
        [RegularExpression(@"^[0-9a-z]+([-+.'][0-9a-z]+)*@[0-9a-z]+([-.][0-9a-z]+)*\.[0-9a-z]+([-.][0-9a-z]+)*$", ErrorMessage = "invalid email format")]
        public string Email { get; set; }
    }
}
