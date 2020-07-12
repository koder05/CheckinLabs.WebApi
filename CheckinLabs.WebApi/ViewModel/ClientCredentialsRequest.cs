using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CheckinLabs.WebApi.ViewModel
{
    public class ClientCredentialsRequest
    {
        [Required]
        public string Scope { get; set; }
        [Required]
        public int ClientId { get; set; }
        [Required]
        public string ClientSecret { get; set; }
    }
}
