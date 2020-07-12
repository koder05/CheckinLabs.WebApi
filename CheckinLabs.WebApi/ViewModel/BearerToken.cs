using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckinLabs.WebApi.ViewModel
{
    public class BearerToken
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
    }

}
