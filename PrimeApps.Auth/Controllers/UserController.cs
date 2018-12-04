﻿using IdentityModel;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Auth.Models;
using PrimeApps.Auth.UI;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Linq;
using System.Net;
using User = PrimeApps.Model.Entities.Tenant.TenantUser;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Enums;
using Microsoft.AspNetCore.Http;
using PrimeApps.Model.Entities.Platform;
using Newtonsoft.Json.Linq;
using PrimeApps.Auth.Helpers;
using PrimeApps.Model.Helpers;
using PrimeApps.Auth.DTO;

namespace PrimeApps.Auth.Controllers
{
    [Route("[controller]")]
    [SecurityHeaders]
	public class UserController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private IPlatformRepository _platformRepository;
		private IApplicationRepository _applicationRepository;
        private IUserRepository _userRepository;
        private IPlatformUserRepository _platformUserRepository;
        private IProfileRepository _profileRepository;
        private IRoleRepository _roleRepository;
        private readonly IEventService _events;
        private IConfiguration _configuration;
        public UserController(
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			IEventService events,
			IPlatformRepository platformRepository,
            IPlatformUserRepository platformUserRepository,
            IApplicationRepository applicationRepository,
            IUserRepository userRepository,
            IProfileRepository profileRepository,
            IRoleRepository roleRepository,
            IPlatformWarehouseRepository platformWarehouseRepository,
            IConfiguration configuration)
		{
			_applicationRepository = applicationRepository;
			_userManager = userManager;
			_signInManager = signInManager;
			_platformRepository = platformRepository;
            _userRepository = userRepository;
            _profileRepository = profileRepository;
            _platformUserRepository = platformUserRepository;
            _roleRepository = roleRepository;
            _configuration = configuration;
			_events = events;
		}

		[Route("add_user"), HttpPost, Authorize(AuthenticationSchemes = "Bearer")]
		public async Task<IActionResult> AddUser([FromBody]AddUserBindingModel addUserBindingModel)
		{
			if (!ModelState.IsValid)
			{
				ModelState.AddModelError("", "ModelState is not valid.");
				return BadRequest(ModelState);
			}
            if (User?.Identity.IsAuthenticated == false)
                return Unauthorized();
            
            var email = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
            //_platformUserRepository.CurrentUser = new Model.Helpers.CurrentUser { TenantId = addUserBindingModel.TenantId };
            var currentPlatformUser = await _platformUserRepository.GetWithSettings(email);
            var tenantCheck = currentPlatformUser.TenantsAsUser.SingleOrDefault(x => x.TenantId == addUserBindingModel.TenantId);
 
            if (tenantCheck == null)
                return Unauthorized();

            _userRepository.CurrentUser = new Model.Helpers.CurrentUser { TenantId = addUserBindingModel.TenantId, UserId = currentPlatformUser.Id };

            var currentTenantUser = _userRepository.GetByIdSync(currentPlatformUser.Id);

            if(!currentTenantUser.Profile.HasAdminRights)
                return Unauthorized();

            var checkEmail = await _platformUserRepository.IsEmailAvailable(addUserBindingModel.Email, addUserBindingModel.AppId);

            if (checkEmail == EmailAvailableType.NotAvailable)
                return new StatusCodeResult(StatusCodes.Status409Conflict);

            var tenant = _platformRepository.GetTenant(addUserBindingModel.TenantId);

            if (tenant.TenantUsers.Count >= tenant.License.UserLicenseCount)
                return new StatusCodeResult(StatusCodes.Status402PaymentRequired);

            var randomPassword = Utils.GenerateRandomUnique(8);

            PlatformUser applicationUser = null;
            if (checkEmail != EmailAvailableType.AvailableForApp)
            {
                applicationUser = new PlatformUser
                {
                    Email = addUserBindingModel.Email,
                    FirstName = addUserBindingModel.FirstName,
                    LastName = addUserBindingModel.LastName,
                    Setting = new PlatformUserSetting()
                };

                applicationUser.Setting.Culture = tenant.Setting.Culture;
                applicationUser.Setting.Language = tenant.Setting.Language;
                //tenant.Setting.TimeZone = 
                applicationUser.Setting.Currency = tenant.Setting.Currency;


                var createUserResult = await _platformUserRepository.CreateUser(applicationUser);

                if (createUserResult == 0)
                {
                    ModelState.AddModelError("", "user not created");
                    return BadRequest(ModelState);
                }
            }
            else
            {
                applicationUser = await _platformUserRepository.Get(addUserBindingModel.Email);
            }

            var appInfo = await _applicationRepository.Get(addUserBindingModel.AppId);
            

            var identityUser = await _userManager.FindByNameAsync(addUserBindingModel.Email);
            string token = null;

			//If user already registered check email is confirmed. If not return confirm token with status code.
			if (identityUser != null && !identityUser.EmailConfirmed)
				token = await GetConfirmToken(identityUser);
            else
            {
                var user = new ApplicationUser
                {
                    UserName = addUserBindingModel.Email,
                    Email = addUserBindingModel.Email,
                    NormalizedEmail = addUserBindingModel.Email,
                    NormalizedUserName = !string.IsNullOrEmpty(addUserBindingModel.FirstName) ? addUserBindingModel.FirstName + " " + addUserBindingModel.LastName : ""
                };
                var result = await _userManager.CreateAsync(user, randomPassword);
                if (!result.Succeeded)
                    return BadRequest(result);


                result = _userManager.AddClaimsAsync(user, new Claim[]{
                    new Claim(JwtClaimTypes.Name, !string.IsNullOrEmpty(addUserBindingModel.FirstName) ? addUserBindingModel.FirstName + " " + addUserBindingModel.LastName : ""),
                    new Claim(JwtClaimTypes.GivenName, addUserBindingModel.FirstName),
                    new Claim(JwtClaimTypes.FamilyName, addUserBindingModel.LastName),
                    new Claim(JwtClaimTypes.Email, addUserBindingModel.Email),
                    new Claim(JwtClaimTypes.EmailVerified, "false", ClaimValueTypes.Boolean)
                }).Result;

                identityUser = await _userManager.FindByNameAsync(addUserBindingModel.Email);
                token = await GetConfirmToken(identityUser);

            }
            
            var platformUser = await _platformUserRepository.Get(applicationUser.Id);
            var tenantUser = await _userRepository.GetById(platformUser.Id);

            if (tenantUser == null)
            {
                tenantUser = new User
                {
                    Id = platformUser.Id,
                    Email = addUserBindingModel.Email,
                    FirstName = addUserBindingModel.FirstName,
                    LastName = addUserBindingModel.LastName,
                    FullName = $"{addUserBindingModel.FirstName} {addUserBindingModel.LastName}",
                    Phone = addUserBindingModel.Phone,
                    Picture = "",
                    IsActive = true,
                    IsSubscriber = false,
                    Culture = currentPlatformUser.Setting.Culture,
                    Currency = currentPlatformUser.Setting.Currency.Substring(0, 2),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByEmail = currentPlatformUser.Email
                };

                await _userRepository.CreateAsync(tenantUser);
            }
            else
            {
                randomPassword = "*******";
                tenantUser.IsActive = true;
                await _userRepository.UpdateAsync(tenantUser);
            }

            _profileRepository.CurrentUser = _roleRepository.CurrentUser = new Model.Helpers.CurrentUser { TenantId = addUserBindingModel.TenantId, UserId = currentPlatformUser.Id };
            await _profileRepository.AddUserAsync(platformUser.Id, addUserBindingModel.ProfileId);
            await _roleRepository.AddUserAsync(platformUser.Id, addUserBindingModel.RoleId);

            var currentTenant = _platformRepository.GetTenant(addUserBindingModel.TenantId);

            platformUser.TenantsAsUser.Add(new UserTenant { Tenant = currentTenant, PlatformUser = platformUser });

            await _platformUserRepository.UpdateAsync(platformUser);
            
            var externalLogin = appInfo.Setting.ExternalAuth != null ? JObject.Parse(appInfo.Setting.ExternalAuth) : null;

            if (externalLogin != null)
            {
                var actions = (JArray)externalLogin["actions"];
                var action = actions.Where(x => x["type"] != null && x["type"].ToString() == "register").FirstOrDefault();

                var obj = new JObject
                {
                    ["email"] = addUserBindingModel.Email,
                    ["password"] = randomPassword,
                    ["first_name"] = addUserBindingModel.FirstName,
                    ["last_name"] = addUserBindingModel.LastName,
                    ["full_name"] = addUserBindingModel.FirstName + " " + addUserBindingModel.LastName,
                    ["language"] = addUserBindingModel.Culture
                };

                await ExternalAuthHelper.Register(externalLogin, action, obj);
            }

            return StatusCode(201, new { token = WebUtility.UrlEncode(token), password = randomPassword });
		}
        
		[Route("change_password"), HttpPost]
		public async Task<IActionResult> ChangePassword([FromBody]ChangePasswordViewModel changePasswordViewModel, [FromQuery] string client)
		{
			if (!ModelState.IsValid)
				return Unauthorized();

			var user = await _userManager.FindByEmailAsync(changePasswordViewModel.Email);
			var result = await _userManager.ChangePasswordAsync(user, changePasswordViewModel.OldPassword, changePasswordViewModel.NewPassword);

            var application = await _applicationRepository.GetByName(client);
            var externalLogin = application.Setting.ExternalAuth != null ? JObject.Parse(application.Setting.ExternalAuth) : null;

            if (result.Succeeded && externalLogin != null)
            {
                var actions = (JArray)externalLogin["actions"];
                var action = actions.Where(x => x["type"] != null && x["type"].ToString() == "change_password").FirstOrDefault();

                var obj = new JObject
                {
                    ["email"] = changePasswordViewModel.Email,
                    ["old_password"] = changePasswordViewModel.OldPassword,
                    ["new_password"] = changePasswordViewModel.NewPassword
                };

                ExternalAuthHelper.ChangePassword(externalLogin, action, obj);
            }

            return result.Succeeded ? Ok() : StatusCode(400);
		}

        [HttpPost("verify_user", Name="verify_user")]
        public async Task<bool> VerifyUser([FromBody] ExternalLoginDTO model)
        {
            var application = await _applicationRepository.GetByName(model.client);
            var externalLogin = application.Setting.ExternalAuth != null ? JObject.Parse(application.Setting.ExternalAuth) : null;

            if (externalLogin != null)
            {
                var actions = (JArray)externalLogin["actions"];
                var action = actions.Where(x => x["type"] != null && x["type"].ToString() == "login").FirstOrDefault();

                var obj = new JObject
                {
                    ["email"] = model.email,
                    ["password"] = model.password
                };

                var result = await ExternalAuthHelper.VerifyUser(externalLogin, action, obj);

                return result.IsSuccessStatusCode;
            }

            return false;
        }
        //Helpers
        public async Task<string> GetConfirmToken(ApplicationUser user)
		{
			return await _userManager.GenerateEmailConfirmationTokenAsync(user);
		}

	}
}
