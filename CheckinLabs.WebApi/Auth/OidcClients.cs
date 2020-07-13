using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static IdentityServer4.IdentityServerConstants;

namespace CheckinLabs.WebApi.Auth
{
    internal class OidcClients : IClientStore
    {
        private readonly IdSrvConfig oidcConf;
        public OidcClients(IConfiguration cfg)
        {
            oidcConf = cfg.GetSection(nameof(IdSrvConfig)).Get<IdSrvConfig>() ?? new IdSrvConfig();
        }
        public async Task<Client> FindClientByIdAsync(string clientId)
        {
            var cl = new Client()
            {
                ClientId = clientId,
                ClientName = "HostManager",
                AllowedGrantTypes = GrantTypes.ClientCredentials.Union(GrantTypes.Code).ToList(),
                ClientSecrets = new List<Secret> { new Secret("1783FF1F-262A-4F87-AF01-E3532444C486".Sha256()) },
                AllowedScopes = new List<string> { Glob.ApiName
                , StandardScopes.OpenId
                , StandardScopes.Profile
                , StandardScopes.Email
                , StandardScopes.OfflineAccess
                , "name"},
                //AllowedCorsOrigins = { oidcConf.UIAuthority },
                Claims = new List<ClientClaim> {
                    new ClientClaim("version", "1783FF1F-262A-4F87-AF01-E3532444C486"),
                    new ClientClaim("name", clientId)
                },
                RedirectUris = new List<string> { oidcConf.UIRedirectUrl },
                RequirePkce = true,
                RequireClientSecret = false,
                PostLogoutRedirectUris = new List<string> { oidcConf.UIRedirectUrl },
                AllowOfflineAccess = true,
                AlwaysIncludeUserClaimsInIdToken = true,
                UpdateAccessTokenClaimsOnRefresh = true,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                AccessTokenLifetime = 60,
                //IdentityTokenLifetime = 7200
            };
            return await Task.FromResult(cl);
        }
    }
}
