using IdentityServer4.Models;
using IdentityServer4.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckinLabs.WebApi.Auth
{
    internal class OidcResources : IResourceStore
    {

        public Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            return Task.FromResult(resources.IdentityResources.Where(r => r.UserClaims.Intersect(scopeNames).Count() > 0));
        }

        public Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames)
        {
            return Task.FromResult(resources.ApiScopes.Where(s => scopeNames.Contains(s.Name)));
        }

        public Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            return Task.FromResult(resources.ApiResources.Where(r => r.Scopes.Intersect(scopeNames).Count() > 0));
        }

        public Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> apiResourceNames)
        {
            return Task.FromResult(resources.ApiResources.Where(r => apiResourceNames.Contains(r.Name)));
        }

        public Task<Resources> GetAllResourcesAsync()
        {
            return Task.FromResult(resources);
        }




        private Resources resources = new Resources(
            new IdentityResource[] { new IdentityResources.OpenId(), new IdentityResources.Profile()/*, new IdentityResources.Email()*/ }
            , new ApiResource[] {
                new ApiResource {
                    Name = Glob.ApiName,
                    DisplayName = Glob.ApiName,
                    Description = Glob.ApiName,
                    UserClaims = new List<string> {"role", IdentityModel.JwtClaimTypes.Name, IdentityModel.JwtClaimTypes.Email},
                    ApiSecrets = new List<Secret> {new Secret("scopeSecret".Sha256())},
                    Scopes = new List<string> {
                        Glob.ApiName,"offline_access","profile","email","name","openid"
                    }
                }
            }
            ,new ApiScope[] { 
                new ApiScope { Name= Glob.ApiName }
                , new ApiScope { Name = "offline_access" }
                , new ApiScope { Name = "name" }
                , new ApiScope { Name = "role" }
                , new ApiScope { Name = "email" }
            }
        );
    }
}
