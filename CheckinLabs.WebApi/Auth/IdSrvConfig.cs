using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckinLabs.WebApi.Auth
{
    public class IdSrvConfig
    {
        public string Authority { get; set; } = "http://oidc.softingmsk.ru";
        public string ApiName { get; set; }
        public string UILoginPage { get; set; } = "login.html";
        public string UIErrorPage { get; set; } = "error.html";
        public string UILogoutPage { get; set; } = "logout.html";
        public string UIAuthority { get; set; } = "http://localhost:5004";

        public string UILoginUrl => $"{UIAuthority}/{UILoginPage}";
        public string UILogoutUrl => $"{UIAuthority}/{UILogoutPage}";
        public string UIErrorUrl => $"{UIAuthority}/{UIErrorPage}";
    }
}
