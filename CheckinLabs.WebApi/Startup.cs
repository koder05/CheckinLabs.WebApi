using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckinLabs.WebApi.Auth;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace CheckinLabs.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddControllers()
                .AddNewtonsoftJson(options =>
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                );
            services.AddSwaggerGen(swagger =>
            {
                swagger.SwaggerDoc("v1", new OpenApiInfo { Title = Glob.ApiName });
                swagger.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                swagger.OperationFilter<SecurityRequirementsOperationFilter>();
            });
            services.AddSwaggerGenNewtonsoftSupport();

            var oidcConf = Configuration.GetSection(nameof(IdSrvConfig)).Get<IdSrvConfig>() ?? new IdSrvConfig();
            services.AddCors(setup =>
            {
                setup.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                    policy.WithOrigins(oidcConf.UIAuthority);
                    policy.AllowCredentials();
                });
            });
            services
                .AddIdentityServer(opt => {
                    opt.IssuerUri = oidcConf.Authority;
                    opt.UserInteraction.LoginUrl = oidcConf.UILoginUrl;
                    opt.UserInteraction.ErrorUrl = oidcConf.UIErrorUrl;
                    opt.UserInteraction.LogoutUrl = oidcConf.UILogoutUrl;
                    opt.Authentication.CookieAuthenticationScheme = "idsrv";
                    opt.Authentication.CookieLifetime = TimeSpan.FromSeconds(60);
                })
                .AddClientStore<OidcClients>()
                .AddResourceStore<OidcResources>()
                //.AddProfileService<OidcProfiles>()
                .AddDeveloperSigningCredential();
            

            //services.AddAuthentication(opt => { opt.DefaultSignInScheme = "idsrv"; });
            services
                .AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = oidcConf.Authority;
                    options.ApiName = oidcConf.ApiName;
                    options.RequireHttpsMetadata = true;
                    options.NameClaimType = "name";
                });
            services.AddTransient<IdentityServer4.Services.IReturnUrlParser, ReturnUrlParser>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CheckinLabs WebApi");
            });

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseIdentityServer();
            //app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
