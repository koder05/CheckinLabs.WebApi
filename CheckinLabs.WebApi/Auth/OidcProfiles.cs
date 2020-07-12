using CheckinLabs.BL.Repo;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckinLabs.WebApi.Auth
{
    internal class OidcProfiles : IProfileService
    {
        private readonly IAccountRepo _repo;
        private readonly ILogger<OidcProfiles> _logger;
        public OidcProfiles(IAccountRepo repo, ILogger<OidcProfiles> logger)
        {
            _repo = repo;
            _logger = logger;
        }
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            return Task.CompletedTask;
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            return Task.CompletedTask;
        }
    }
}
