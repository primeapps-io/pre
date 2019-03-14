﻿using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using PrimeApps.Auth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using IdentityModel;

namespace PrimeApps.Auth
{
    public class CustomProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private IHttpContextAccessor _context;

        public CustomProfileService(UserManager<ApplicationUser> userManager, IHttpContextAccessor context)
        {
            _userManager = userManager;
            _context = context;
        }


        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject.GetSubjectId();
            //var giteaToken = context.Subject.FindFirst("gitea_token");

            if (context.Subject.FindFirst("amr").Value != "external")
            {
                var user = _userManager.FindByIdAsync(context.Subject.GetSubjectId()).Result;
                var claims = _userManager.GetClaimsAsync(user).Result;

                var giteaToken = claims.FirstOrDefault(x => x.Type == "gitea_token")?.Value;
                
                context.IssuedClaims.Add(new Claim("gitea_token", giteaToken));
                context.IssuedClaims.Add(new Claim("email", user.Email));
                context.IssuedClaims.Add(new Claim("email_confirmed", user.EmailConfirmed.ToString()));
                context.IssuedClaims.Add(new Claim("external_login", "false"));

                /*context.AddRequestedClaims(claims);*/
            }
            else
            {
                //var c = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
                //var result = context.HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
                context.IssuedClaims.Add(new Claim("email", context.Subject.FindFirst("email").Value));
                context.IssuedClaims.Add(new Claim("email_confirmed", "true"));
                context.IssuedClaims.Add(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/tenantId", context.Subject.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/tenantId")));
                context.IssuedClaims.Add(new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", context.Subject.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")));
                context.IssuedClaims.Add(new Claim(ClaimTypes.NameIdentifier, context.Subject.FindFirstValue(ClaimTypes.NameIdentifier)));
                context.IssuedClaims.Add(new Claim("idp", context.Subject.FindFirstValue("idp")));
                context.IssuedClaims.Add(new Claim("external_login", "true"));
                //context.IssuedClaims.Add(new Claim("validated_code", context.Subject.FindFirstValue("validated_code")));
            }
            return Task.CompletedTask;
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = _userManager.FindByIdAsync(context.Subject.GetSubjectId()).Result;
            context.IsActive = user != null;
            return Task.FromResult(user != null);
        }
    }
}