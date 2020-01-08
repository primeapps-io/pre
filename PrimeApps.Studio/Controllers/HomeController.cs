﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Services;

namespace PrimeApps.Studio.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration _configuration;
        private IBackgroundTaskQueue _queue;
        private IMigrationHelper _migrationHelper;
        private IDataProtector _dataProctector;
        private IKeyManager _keyManager;

        public HomeController(IConfiguration configuration, IBackgroundTaskQueue queue, IMigrationHelper migrationHelper, IDataProtectionProvider protectionProvider, IKeyManager keyManager)
        {
            _configuration = configuration;
            _queue = queue;
            _migrationHelper = migrationHelper;
            
            _dataProctector = protectionProvider.CreateProtector("Home"); 
            _keyManager = keyManager;
        }

        [Authorize]
        public async Task<ActionResult> Index()
        {
            var platformUserRepository = (IPlatformUserRepository)HttpContext.RequestServices.GetService(typeof(IPlatformUserRepository));

            var userId = await platformUserRepository.GetIdByEmail(HttpContext.User.FindFirst("email").Value);
            await SetValues(userId);

            //TODO REMOVE
            var test = _dataProctector.Protect(HttpContext.User.FindFirst("email").Value);
            _keyManager.CreateNewKey(activationDate: DateTimeOffset.Now, expirationDate: DateTimeOffset.Now.AddMonths(1));

            // var result = _dataProctector.Unprotect(test);
            var list = _keyManager.GetAllKeys();
            //TODO REMOVE

            return View();
        }

        [Route("register")]
        public async Task<IActionResult> Register()
        {
            var applicationRepository = (IApplicationRepository)HttpContext.RequestServices.GetService(typeof(IApplicationRepository));

            var appInfo = await applicationRepository.Get(Request.Host.Value);

            return Redirect(Request.Scheme + "://" + appInfo.Setting.AuthDomain + "/Account/Register?ReturnUrl=/connect/authorize/callback?client_id=" + appInfo.Name + "%26redirect_uri=" + Request.Scheme + "%3A%2F%2F" + appInfo.Setting.AppDomain + "%2Fsignin-oidc%26response_type=code%20id_token&scope=openid%20profile%20api1%20email&response_mode=form_post");
        }

        [Authorize, Route("set_gitea_token")]
        public async Task<IActionResult> SetGiteaToken()
        {
            var applicationRepository = (IApplicationRepository)HttpContext.RequestServices.GetService(typeof(IApplicationRepository));

            var appInfo = await applicationRepository.Get(Request.Host.Value);

            return Redirect(Request.Scheme + "://" + appInfo.Setting.AuthDomain + "/Account/Register?ReturnUrl=/connect/authorize/callback?client_id=" + appInfo.Name + "%26redirect_uri=" + Request.Scheme + "%3A%2F%2F" + appInfo.Setting.AppDomain + "%2Fsignin-oidc%26response_type=code%20id_token&scope=openid%20profile%20api1%20email&response_mode=form_post");
        }

        [HttpGet, Route("healthz")]
        public IActionResult Healthz()
        {
            return Ok();
        }

        [HttpGet, Route("migration")]
        public IActionResult Migration()
        {
            var isLocal = Request.Host.Value.Contains("localhost");
            var schema = Request.Scheme;
            _queue.QueueBackgroundWorkItem(token => _migrationHelper.Apply(schema, isLocal));
            return Ok();
        }

        private async Task SetValues(int userId)
        {
            ViewBag.Token = await HttpContext.GetTokenAsync("access_token");
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(ViewBag.Token) as JwtSecurityToken;
            var emailConfirmed = jwtToken?.Claims.FirstOrDefault(claim => claim.Type == "email_confirmed")?.Value;

            if (!string.IsNullOrEmpty(_configuration.GetValue("AppSettings:GiteaEnabled", string.Empty)) && bool.Parse(_configuration.GetValue("AppSettings:GiteaEnabled", string.Empty)))
            {
                var giteaToken = jwtToken?.Claims.FirstOrDefault(claim => claim.Type == "gitea_token")?.Value;
                //var giteaToken = Request.Cookies["gitea_token"];
                //var giteaToken = jwtToken.Claims.First(claim => claim.Type == "gitea_token")?.Value;
                if (giteaToken != null)
                    Response.Cookies.Append("gitea_token", giteaToken);
            }

            var previewUrl = _configuration.GetValue("AppSettings:PreviewUrl", string.Empty);

            if (!string.IsNullOrEmpty(previewUrl))
                ViewBag.PreviewUrl = previewUrl;

            var blobUrl = _configuration.GetValue("AppSettings:StorageUrl", string.Empty);

            if (!string.IsNullOrEmpty(blobUrl))
                ViewBag.BlobUrl = blobUrl;

            var functionUrl = _configuration.GetValue("AppSettings:FunctionUrl", string.Empty);

            if (!string.IsNullOrEmpty(functionUrl))
                ViewBag.FunctionUrl = functionUrl;

            var giteaUrl = _configuration.GetValue("AppSettings:GiteaUrl", string.Empty);

            if (!string.IsNullOrEmpty(giteaUrl))
                ViewBag.GiteaUrl = giteaUrl;

            var useCdnSetting = _configuration.GetValue("AppSettings:UseCdn", string.Empty);
            var useCdn = false;

            if (!string.IsNullOrEmpty(useCdnSetting))
                useCdn = bool.Parse(useCdnSetting);

            if (useCdn)
            {
                var versionDynamic = System.Reflection.Assembly.GetAssembly(typeof(HomeController)).GetName().Version.ToString();
                var versionStatic = ((AssemblyVersionStaticAttribute)System.Reflection.Assembly.GetAssembly(typeof(HomeController)).GetCustomAttributes(typeof(AssemblyVersionStaticAttribute), false)[0]).Version;
                var cdnUrl = _configuration.GetValue("AppSettings:CdnUrl", string.Empty);

                if (!string.IsNullOrEmpty(cdnUrl))
                {
                    ViewBag.CdnUrlDynamic = cdnUrl + "/" + versionDynamic + "/";
                    ViewBag.CdnUrlStatic = cdnUrl + "/" + versionStatic + "/";
                }
            }
            else
            {
                ViewBag.CdnUrlDynamic = "";
                ViewBag.CdnUrlStatic = "";
            }
        }
    }
}