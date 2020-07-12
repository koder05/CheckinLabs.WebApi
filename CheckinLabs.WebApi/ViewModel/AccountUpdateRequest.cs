using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CheckinLabs.WebApi.ViewModel
{
    public class AccountUpdateRequest
    {
        [Required]
        public string Code { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        [RegularExpression(@"^(?=[^a-z]*[a-z])(?=\D*\d)[^:&.~\s]{5,20}$"
            , ErrorMessage = "Password must: 5-20 characters long; contain at least one lower-case latin letter; contain at least one number; not contain a ':','&','.','~' or a space.")]
        public string UserPassword { get; set; }
    }
}
