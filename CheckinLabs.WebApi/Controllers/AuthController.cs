using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using CheckinLabs.BL.Models;
using CheckinLabs.BL.Repo;
using CheckinLabs.WebApi.Auth;
using CheckinLabs.WebApi.ViewModel;
using IdentityModel;
using IdentityModel.Client;
using IdentityServer4;
using IdentityServer4.Services;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CheckinLabs.WebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : MyControllerBase
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IIdentityServerInteractionService interaction, ILogger<AuthController> logger)
        {
            _interaction = interaction;
            _logger = logger;
        }
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, [FromServices] IAccountRepo accountRepo)
        {
            try
            {
                var context = await _interaction.GetAuthorizationContextAsync(request.ReturnUrl);
                if (context != null)
                {
                    var userProfile = await accountRepo.GetAccountAsync(request.Username, request.Password);
                    if (userProfile != null && context != null)
                    {
                        var user = new IdentityServerUser(userProfile.User.UID.ToString());
                        user.DisplayName = userProfile.DisplayName;
                        user.AdditionalClaims = new List<Claim>
                        {
                            new Claim(JwtClaimTypes.Name, userProfile.User.Name),
                            new Claim(JwtClaimTypes.Email, userProfile.Email)
                        };

                        await HttpContext.SignInAsync(user);
                        return new JsonResult(new { RedirectUrl = request.ReturnUrl, IsOk = true });
                    }
                }
                return Unauthorized();
            }
            catch (UserAccountException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        [HttpGet]
        [Route("logout")]
        public async Task<IActionResult> Logout(string logoutId)
        {
            var context = await _interaction.GetLogoutContextAsync(logoutId);
            bool showSignoutPrompt = true;
            if (context?.ShowSignoutPrompt == false)
            {
                // it's safe to automatically sign-out
                showSignoutPrompt = false;
            }
            if (User?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await HttpContext.SignOutAsync();
            }
            else
            {
                //always User.Identity.IsAuthenticated == false, so clear all cookies
                HttpContext.Response.Cookies.Append("idsrv", "", new CookieOptions() { Expires = DateTime.Now.AddDays(-1) });
                //foreach (var key in HttpContext.Request.Cookies.Keys)  HttpContext.Response.Cookies.Append(key, "", new CookieOptions() { Expires = DateTime.Now.AddDays(-1) });
            }
            // no external signout supported for now (see \Quickstart\Account\AccountController.cs TriggerExternalSignout)
            return Ok(new
            {
                showSignoutPrompt,
                ClientName = string.IsNullOrEmpty(context?.ClientName) ? context?.ClientId : context?.ClientName,
                context?.PostLogoutRedirectUri,
                context?.SignOutIFrameUrl,
                logoutId
            });
        }
        [HttpGet]
        [Route("error")]
        public async Task<IActionResult> Error(string errorId)
        {
            // retrieve error details from identityserver
            var message = await _interaction.GetErrorContextAsync(errorId);
            if (message != null)
            {
                //if (!_environment.IsDevelopment()) message.ErrorDescription = null;
            }
            return Ok(message);
        }

        [HttpPost]
        [Route("oidc/client-grant")]
        //[ValidateAntiForgeryToken]
        public IActionResult OidcClientGrant([FromBody] ClientCredentialsRequest req, [FromServices] IConfiguration cfg)
        {
            try
            {
                var oidcConf = cfg.GetSection(nameof(IdSrvConfig)).Get<IdSrvConfig>() ?? new IdSrvConfig();
                if (string.IsNullOrEmpty(oidcConf.Authority))
                    throw new Exception("OidcServerAuthority is missed.");
                string access_token = string.Empty;
                using (var web = new WebClient())
                {
                    web.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    var rawToken = web.UploadString($"{oidcConf.Authority}/connect/token", $"grant_type=client_credentials&scope={req.Scope}&client_id={req.ClientId}&client_secret={req.ClientSecret}");
                    var token = Newtonsoft.Json.JsonConvert.DeserializeObject<BearerToken>(rawToken);
                    access_token = token.access_token;
                }
                return Ok(access_token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error OidcClientGrant AuthController");
                return InternalServerError(ex);
            }
        }
        [HttpGet]
        [Route("oidc/config")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> GetOidcConfig([FromQuery] string clientId, [FromServices] IConfiguration cfg)
        {
            try
            {
                var oidcConf = cfg.GetSection(nameof(IdSrvConfig)).Get<IdSrvConfig>() ?? new IdSrvConfig();
                if (string.IsNullOrEmpty(oidcConf.Authority))
                    throw new Exception("OidcServerAuthority is missed.");

                object ret = null;
                using (var client = new System.Net.Http.HttpClient())
                {
                    var disco = await client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
                    {
                        Address = oidcConf.Authority,
                        Policy = {
                            ValidateIssuerName = false,
                            ValidateEndpoints = false,
                        },
                    });
                    if (disco.IsError)
                    {
                        throw new Exception($"OidcServer Discovery error: {disco.Error}");
                    }
                    ret = new { authorization_endpoint = disco.AuthorizeEndpoint, token_endpoint = disco.TokenEndpoint, logout_endpoint = disco.EndSessionEndpoint };
                }

                return Ok(ret);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error GetOidcConfig AuthController");
                return InternalServerError(ex);
            }
        }

    }
}
