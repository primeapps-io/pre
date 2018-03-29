﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform.Identity;
using PrimeApps.Model.Repositories;

namespace PrimeApps.App.Controllers
{
    [Route("auth"), Authorize]
    public class AuthController : Controller
    {
        private ApplicationSignInManager _signInManager;

        [Route("authorize"), HttpGet]
        public ActionResult Authorize()
        {
            var claims = new ClaimsPrincipal(User).Claims.ToArray();
            var identity = new ClaimsIdentity(claims, "Bearer");
            Authentication.SignIn(identity);

            return new EmptyResult();
        }

        [Route("login"), AllowAnonymous]
        public async Task<ActionResult> Login(string returnUrl, string language = null, string error = null, string success = "")
        {
            var lang = GetLanguage();
            if (language != null)
            {
                lang = language;
                SetLanguae(lang);
            }
            ViewBag.Success = success;
            ViewBag.Lang = lang;
            ViewBag.Error = error;
            ViewBag.ReturnUrl = returnUrl;

            ViewBag.AppInfo = await AuthHelper.GetApplicationInfo(Request, language);

            if (!string.IsNullOrWhiteSpace(ViewBag.AppInfo["language"].Value))
            {
                SetLanguae(ViewBag.AppInfo["language"].Value);
                ViewBag.Lang = ViewBag.AppInfo["language"].Value;
            }

            return View();
        }

        [Route("login"), HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginBindingModel model, string returnUrl)
        {
            var url = new Uri(Request.GetDisplayUrl()).Host;
            var lang = GetLanguage();
            ViewBag.Lang = lang;
            ViewBag.ReturnUrl = returnUrl;

            ViewBag.AppInfo = await AuthHelper.GetApplicationInfo(Request, lang);
            model.Email = model.Email.Replace(@" ", "");

            //TODO: Remove this when remember me feature developed
            model.RememberMe = true;

            PlatformUser user;
            int appId;
            SignInStatus result;
            using (var platformDBContext = new PlatformDBContext())
            using (var platformUserRepository = new PlatformUserRepository(platformDBContext))

            {
                user = await platformUserRepository.Get(model.Email);
                appId = GetAppId(url);
                result = SignInStatus.Failure;

                if (user != null)
                {
                    if (url.Contains("localhost") || url.Contains("mirror.ofisim.com") || url.Contains("staging.ofisim.com") || user.AppId == appId)
                        result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
                    else
                    {

                        var app = platformDBContext.UserApps.FirstOrDefault(x => x.UserId == user.Id && x.AppId == appId);

                        if (app != null)
                        {
                            result = await SignInManager.PasswordSignInAsync(app.Email, model.Password, model.RememberMe, shouldLockout: false);
                        }

                    }
                }
            }

            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                //case SignInStatus.LockedOut:
                //    return View("Lockout");
                //case SignInStatus.RequiresVerification:
                //    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                //case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    ViewBag.Error = "wrongInfo";
                    return View(model);
            }
        }

        [Route("register"), AllowAnonymous]
        public async Task<ActionResult> Register(string returnUrl, string language = null, string error = null, string name = null, string lastname = null, string email = null, bool officeSignIn = false, string appId = null)
        {
            RegisterBindingModel registerBindingModel = null;

            if (name != null || lastname != null || email != null)
            {
                registerBindingModel = new RegisterBindingModel()
                {
                    Email = email,
                    FirstName = HttpUtility.UrlDecode(name),
                    LastName = HttpUtility.UrlDecode(lastname)
                };

                if (email != null)
                    ViewBag.ReadOnly = true;
            }
            var lang = GetLanguage();
            if (language != null)
            {
                lang = language;
                SetLanguae(lang);
            }

            ViewBag.OfficeSignIn = officeSignIn;
            ViewBag.Lang = lang;
            ViewBag.error = error;
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.AppInfo = await AuthHelper.GetApplicationInfo(Request, lang);
            ViewBag.AppId = appId;

            if (!string.IsNullOrWhiteSpace(ViewBag.AppInfo["language"].Value))
            {
                SetLanguae(ViewBag.AppInfo["language"].Value);
                ViewBag.Lang = ViewBag.AppInfo["language"].Value;
            }

            return View(registerBindingModel);
        }

        [Route("register"), HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterBindingModel registerBindingModel, string returnUrl, string campaignCode = null, bool officeSignIn = false, string appId = null)
        {
            var lang = GetLanguage();
            var phone = registerBindingModel.Phone;
            registerBindingModel.Phone = phone.Replace(@"(", "").Replace(@")", "").Replace(@"-", "").Replace(@" ", "");
            registerBindingModel.License = "7673E999-18FB-497F-A958-84DCA43031CC";
            registerBindingModel.Culture = lang == "tr" ? "tr-TR" : "en-US";
            registerBindingModel.Currency = lang == "tr" ? "TRY" : "USD";
            registerBindingModel.CampaignCode = campaignCode;
            registerBindingModel.Email = registerBindingModel.Email.Replace(@" ", "");
            registerBindingModel.OfficeSignIn = officeSignIn;
            registerBindingModel.AppID = appId != null ? Convert.ToInt32(appId) : GetAppId(Request.Host.ToString());
            if (officeSignIn)
                registerBindingModel.Password = Utils.GenerateRandomUnique(8);

            ViewBag.Lang = lang;
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.AppInfo = await AuthHelper.GetApplicationInfo(Request, lang);
            ViewBag.OfficeSignIn = officeSignIn;

            if (!string.IsNullOrWhiteSpace(ViewBag.AppInfo["language"].Value))
            {
                SetLanguae(ViewBag.AppInfo["language"].Value);
                ViewBag.Lang = ViewBag.AppInfo["language"].Value;
            }

            var index = new Uri(Request.GetDisplayUrl()).OriginalString.IndexOf(new Uri(Request.GetDisplayUrl()).PathAndQuery);
            var apiUrl = new Uri(Request.GetDisplayUrl()).OriginalString.Remove(index) + "/api/account/register";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

	            var dataAsString = JsonConvert.SerializeObject(registerBindingModel);
	            var content = new StringContent(dataAsString);
				var response = await client.PostAsync(apiUrl, content);
				//var response = await client.PostAsJsonAsync(apiUrl, registerBindingModel); 
                if (response.IsSuccessStatusCode)
                {
                    if (!officeSignIn)
                        return RedirectToAction("Verify", "Auth",
                            new { ReturnUrl = ViewBag.ReturnUrl, Email = registerBindingModel.Email });

                    var data = response.Content.ReadAsStringAsync().Result;
                    JObject automaticAccountActivationModel = JObject.Parse(data);
                    var token = (string)automaticAccountActivationModel["Token"];
                    var guid = (Guid)automaticAccountActivationModel["GuId"];

                    return RedirectToAction("Activation", "Auth",
                        new { Token = token, Uid = guid, OfficeSignIn = true });
                }
                ViewBag.Error = "User exist";
                registerBindingModel.Phone = phone;
            }
            return View(registerBindingModel);
        }

        [Route("ResetPassword"), AllowAnonymous]
        public async Task<ActionResult> ResetPassword(string returnUrl, string token, Guid uid, string error = null)
        {
            var lang = GetLanguage();
            ViewBag.Lang = lang;
            ViewBag.Error = error;
            ViewBag.Token = WebUtility.UrlEncode(token);
            ViewBag.Uid = uid;
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.AppInfo = await AuthHelper.GetApplicationInfo(Request, lang);
            return View();
        }

        [Route("ResetPassword"), HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordBindingModel resetPasswordBindingModel, string token, int uid)
        {
            var lang = GetLanguage();
            resetPasswordBindingModel.UserId = uid;
            resetPasswordBindingModel.Token = WebUtility.UrlDecode(token);

            ViewBag.Lang = lang;
            ViewBag.ReturnUrl = "/";
            ViewBag.AppInfo = await AuthHelper.GetApplicationInfo(Request, lang);

            var index = new Uri(Request.GetDisplayUrl()).OriginalString.IndexOf(new Uri(Request.GetDisplayUrl()).PathAndQuery);
            var apiUrl = new Uri(Request.GetDisplayUrl()).OriginalString.Remove(index) + "/api/account/reset_password";

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

	            var dataAsString = JsonConvert.SerializeObject(resetPasswordBindingModel);
	            var contentResetPasswordBindingModel = new StringContent(dataAsString);
	            var response = await client.PostAsync(apiUrl, contentResetPasswordBindingModel);

				//var response = await client.PostAsJsonAsync(apiUrl, resetPasswordBindingModel);
                var res = "";

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Login", "Auth", new { ReturnUrl = ViewBag.ReturnUrl, Success = "passwordChanged" });
                }

                using (var content = response.Content)
                {
                    res = content.ReadAsStringAsync().Result;
                }

                var result = JObject.Parse(res);
                var modelState = result["ModelState"];

                ViewBag.Error = modelState[""] != null && modelState[""].HasValues ? "invalidToken" : "notFound";
            }
            return View(resetPasswordBindingModel);
        }

        [Route("ForgotPassword"), AllowAnonymous]
        public async Task<ActionResult> ForgotPassword(string email = null, string language = null, string error = null, string info = null)
        {
            var lang = GetLanguage();
            if (language != null)
            {
                lang = language;
                SetLanguae(lang);
            }
            ViewBag.Lang = lang;
            ViewBag.error = error;
            ViewBag.Info = info;
            ViewBag.ReturnUrl = "/";
            ViewBag.AppInfo = await AuthHelper.GetApplicationInfo(Request, lang);

            if (!string.IsNullOrWhiteSpace(ViewBag.AppInfo["language"].Value))
            {
                SetLanguae(ViewBag.AppInfo["language"].Value);
                ViewBag.Lang = ViewBag.AppInfo["language"].Value;
            }

            return View();
        }

        [Route("ForgotPassword"), HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(string email)
        {
            var lang = GetLanguage();
            var culture = lang == "tr" ? "tr-TR" : "en-US";
            ViewBag.Error = null;

            ViewBag.Lang = lang;
            ViewBag.ReturnUrl = "/";
            ViewBag.AppInfo = await AuthHelper.GetApplicationInfo(Request, lang);

            var index = new Uri(Request.GetDisplayUrl()).OriginalString.IndexOf(new Uri(Request.GetDisplayUrl()).PathAndQuery);
            var apiUrl = new Uri(Request.GetDisplayUrl()).OriginalString.Remove(index) + "/api/account/forgot_password?email=" + email.Replace(@" ", "") + "&culture=" + culture;


            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.GetAsync(apiUrl);
                var res = "";

                if (response.IsSuccessStatusCode)
                {
                    ViewBag.Info = "Success";
                    return View();
                }

                using (var content = response.Content)
                {
                    res = content.ReadAsStringAsync().Result;
                }

                var result = JObject.Parse(res);
                var modelState = result["ModelState"];

                if (modelState["not_found"] != null && modelState["not_found"].HasValues)
                {
                    ViewBag.Error = "notFound";
                }
                else if (modelState["not_activated"] != null && modelState["not_activated"].HasValues)
                {
                    ViewBag.Error = "notActivated";
                }
            }
            return View();
        }

        [Route("Activation"), HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<ActionResult> Activation(string token = "", string uid = null, bool officeSignIn = false)
        {
            var lang = GetLanguage();
            var culture = lang == "tr" ? "tr-TR" : "en-US";

            var index = new Uri(Request.GetDisplayUrl()).OriginalString.IndexOf(new Uri(Request.GetDisplayUrl()).PathAndQuery);
            var apiUrl = new Uri(Request.GetDisplayUrl()).OriginalString.Remove(index) + "/api/account/activate?userId=" + uid + "&token=" + WebUtility.UrlEncode(token) + "&culture=" + culture + "&officeSignIn=" + officeSignIn;
            ViewBag.Lang = lang;
            ViewBag.ReturnUrl = "/";
            ViewBag.AppInfo = await AuthHelper.GetApplicationInfo(Request, lang);

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.GetAsync(apiUrl);
                var res = "";

                if (response.IsSuccessStatusCode)
                {
                    if (officeSignIn)
                        return RedirectToAction("SignInAd", "Auth");

                    return RedirectToAction("Login", "Auth", new { ReturnUrl = ViewBag.ReturnUrl, Success = "accountActivated" });
                }

                using (var content = response.Content)
                {
                    res = content.ReadAsStringAsync().Result;
                }

                var result = JObject.Parse(res);
                var modelState = result["ModelState"];

                if (modelState == null || result["Message"] != null && result["Message"].HasValues)
                {
                    ViewBag.Error = "anErrorOccured";
                }
                else if (modelState[""] != null && modelState[""].HasValues)
                {
                    ViewBag.Error = "invalidToken";
                }
            }
            ViewBag.PostBack = "PostBack";
            return View();
        }

        [Route("Activation"), AllowAnonymous]
        public async Task<ActionResult> Activation(string token = "", string uid = null, string app = null, bool officeSignIn = false)
        {
            var lang = GetLanguage();
            ViewBag.Lang = lang;
            ViewBag.ReturnUrl = "/";
            ViewBag.AppInfo = await AuthHelper.GetApplicationInfo(Request, lang);
            ViewBag.Token = token;
            ViewBag.Uid = uid;
            ViewBag.PostBack = "";
            ViewBag.OfficeSignIn = officeSignIn;
            return View();
        }

        [Route("Verify"), AllowAnonymous]
        public async Task<ActionResult> Verify(string returnUrl, string email, bool resend = false)
        {
            var lang = GetLanguage();
            ViewBag.Lang = lang;
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Email = email.Replace(@" ", "");
            ViewBag.Resend = resend;
            ViewBag.AppInfo = await AuthHelper.GetApplicationInfo(Request, lang);

            if (!resend) return View();

            var culture = lang == "tr" ? "tr-TR" : "en-US";
            var index = new Uri(Request.GetDisplayUrl()).OriginalString.IndexOf(new Uri(Request.GetDisplayUrl()).PathAndQuery);
            var apiUrl = new Uri(Request.GetDisplayUrl()).OriginalString.Remove(index) + "/api/account/resend_activation?email=" + email + "&culture=" + culture;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.GetAsync(apiUrl);
                var res = "";

                if (response.IsSuccessStatusCode)
                {
                    return View();
                }

                using (var content = response.Content)
                {
                    res = content.ReadAsStringAsync().Result;
                }

                var result = JObject.Parse(res);
                foreach (var keyValuePair in result)
                {
                    if (keyValuePair.Value.Type == JTokenType.Object &&
                        keyValuePair.Value[""][0].ToString() == "User has already activated")
                        ViewBag.Error = "alreadyActivated";
                    else if (keyValuePair.Value.Type == JTokenType.Object &&
                             keyValuePair.Value[""][0].ToString() == "User not found")
                    {
                        ViewBag.Error = "userNotFound";
                    }
                    else if (keyValuePair.Value.Type == JTokenType.Object &&
                             keyValuePair.Value[""][0].ToString() == "Email are required")
                    {
                        ViewBag.Error = "emailRequired";
                    }
                    else
                    {
                        ViewBag.Error = "error";
                    }
                }
            }

            return View();
        }

        [Route("logout"), HttpPost, ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Auth");
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [Route("Auth"), AllowAnonymous]
        public void SignInAd()
        {
            if (!Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = "/" }, OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }

        // sign out triggered from the Sign Out gesture in the UI
        // after sign out, it redirects to Post_Logout_Redirect_Uri (as set in Startup.Auth.cs)
        [Route("SignOut"), AllowAnonymous]
        public ActionResult SignOut()
        {
            //HttpContext.GetOwinContext().Authentication.SignOut(
            //OpenIdConnectAuthenticationDefaults.AuthenticationType, CookieAuthenticationDefaults.AuthenticationType);
            Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Auth");
        }

        public void EndSession()
        {
            // If AAD sends a single sign-out message to the app, end the user's session, but don't redirect to AAD for sign out.
            HttpContext.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
        }

        private void SetLanguae(string lang)
        {
            if (string.IsNullOrWhiteSpace(lang))
                lang = "tr";

            var cookieVisitor = new HttpCookie("_lang", lang) { Expires = DateTime.Now.AddYears(20) };
            Response.Cookies.Add(cookieVisitor);
        }

        private string GetLanguage()
        {
            var lang = Request.Cookies["_lang"];
            if (lang != null)
            {
                return lang.Value;
            }

            SetLanguae("tr");
            return "tr";
        }

        public int GetAppId(string url)
        {
            if (url.Contains("kobi.ofisim.com") || url.Contains("kobi-test.ofisim.com"))
            {
                return 2;
            }
            if (url.Contains("asistan.ofisim.com") || url.Contains("asistan-test.ofisim.com"))
            {
                return 3;
            }
            if (url.Contains("ik.ofisim.com") || url.Equals("ik-test.ofisim.com") || url.Contains("ik-dev.ofisim.com"))
            {
                return 4;
            }
            if (url.Contains("cagri.ofisim.com") || url.Contains("cagri-test.ofisim.com"))
            {
                return 5;
            }
            if (url.Contains("crm.ofisim.com") || url.Contains("test.ofisim.com") || url.Contains("dev.ofisim.com"))
            {
                return 1;
            }
            return 1;
        }
    }
}