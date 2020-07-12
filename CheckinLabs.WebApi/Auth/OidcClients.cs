using IdentityServer4.Models;
using IdentityServer4.Stores;
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
        public async Task<Client> FindClientByIdAsync(string clientId)
        {
            var cl = new Client()
            {
                ClientId = clientId,
                ClientName = "HostManager",
                AllowedGrantTypes = GrantTypes.ClientCredentials.Union(GrantTypes.Code).ToList(),
                ClientSecrets = new List<Secret> { new Secret("1783FF1F-262A-4F87-AF01-E3532444C486".Sha256()) },
                AllowedScopes = new List<string> { "SUO2.Communication"
                , StandardScopes.OpenId
                , StandardScopes.Profile
                , StandardScopes.Email
                , StandardScopes.OfflineAccess
                , "name"},
                AllowedCorsOrigins = { "http://localhost:5004" },
                Claims = new List<ClientClaim> {
                    new ClientClaim("version", "1783FF1F-262A-4F87-AF01-E3532444C486"),
                    new ClientClaim("name", clientId)
                },
                RedirectUris = new List<string> { "http://localhost:5004/spa.html" },
                RequirePkce = true,
                RequireClientSecret = false,
                PostLogoutRedirectUris = new List<string> { "http://localhost:5004/spa.html" },
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
