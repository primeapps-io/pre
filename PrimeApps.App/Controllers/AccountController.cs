﻿using System;
using System.Threading.Tasks;
using System.Net;
using System.Linq;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Entities.Platform;
using Newtonsoft.Json;
using PrimeApps.App.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Enums;

namespace PrimeApps.App.Controllers
{
    [Route("api/account")]
    public class AccountController : Controller
    {
        private IRecordRepository _recordRepository;
        private IApplicationRepository _applicationRepository;
        private IPlatformRepository _platformRepository;
        private IPlatformUserRepository _platformUserRepository;
        private ITenantRepository _tenantRepository;
        private IProfileRepository _profileRepository;
        private IUserRepository _userRepository;
        private IRoleRepository _roleRepository;
        private IDocumentHelper _documentHelper;
        private IConfiguration _configuration;

        public IBackgroundTaskQueue Queue { get; }
        public AccountController(IApplicationRepository applicationRepository, IRecordRepository recordRepository, IPlatformUserRepository platformUserRepository, IPlatformRepository platformRepository, IRoleRepository roleRepository, IProfileRepository profileRepository, IUserRepository userRepository, ITenantRepository tenantRepository, IBackgroundTaskQueue queue, IRecordHelper recordHelper, Warehouse warehouse, IConfiguration configuration, IDocumentHelper documentHelper)
        {
            _applicationRepository = applicationRepository;
            _recordRepository = recordRepository;
            _platformUserRepository = platformUserRepository;
            _tenantRepository = tenantRepository;
            _platformRepository = platformRepository;
            _profileRepository = profileRepository;
            _roleRepository = roleRepository;
            _userRepository = userRepository;
            _documentHelper = documentHelper;
            _configuration = configuration;
            Queue = queue;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody]CreateBindingModels activateBindingModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userExist = true;
            PlatformUser user = await _platformUserRepository.GetWithTenants(activateBindingModel.Email);
            var app = await _applicationRepository.Get(activateBindingModel.AppId);

            if (user != null)
            {
                var appTenant = user.TenantsAsUser.FirstOrDefault(x => x.Tenant.AppId == activateBindingModel.AppId);

                if (appTenant != null)
                {
                    ModelState.AddModelError("", "User is already registered for this app.");
                    return Conflict(ModelState);
                }
            }
            else
            {
                userExist = false;
                user = new PlatformUser
                {
                    Email = activateBindingModel.Email,
                    FirstName = activateBindingModel.FirstName,
                    LastName = activateBindingModel.LastName,
                    Setting = new PlatformUserSetting()
                };

                if (!string.IsNullOrEmpty(activateBindingModel.Culture))
                {
                    user.Setting.Culture = activateBindingModel.Culture;
                    user.Setting.Language = activateBindingModel.Culture.Substring(0, 2);
                    //tenant.Setting.TimeZone =
                    user.Setting.Currency = activateBindingModel.Culture;
                }
                else
                {
                    user.Setting.Culture = app.Setting.Culture;
                    user.Setting.Currency = app.Setting.Currency;
                    user.Setting.Language = app.Setting.Language;
                    user.Setting.TimeZone = app.Setting.TimeZone;

                }

                var result = _platformUserRepository.CreateUser(user).Result;

                if (result == 0)
                {
                    ModelState.AddModelError("", "user not created");
                    return BadRequest(ModelState);
                }

                user = await _platformUserRepository.GetWithTenants(activateBindingModel.Email);
            }

            var tenantId = 0;
            Tenant tenant = null;
            //var tenantId = 2032;
            try
            {

                tenant = new Tenant
                {
                    //Id = tenantId,
                    AppId = activateBindingModel.AppId,
                    Owner = user,
                    UseUserSettings = true,
                    GuidId = Guid.NewGuid(),
                    License = new TenantLicense
                    {
                        UserLicenseCount = 5,
                        ModuleLicenseCount = 2
                    },
                    Setting = new TenantSetting
                    {
                        Culture = app.Setting.Culture,
                        Currency = app.Setting.Currency,
                        Language = app.Setting.Language,
                        TimeZone = app.Setting.TimeZone
                    },
                    CreatedBy = user
                };

                await _tenantRepository.CreateAsync(tenant);
                tenantId = tenant.Id;

                user.TenantsAsOwner.Add(tenant);
                await _platformUserRepository.UpdateAsync(user);

                var tenantUser = new TenantUser
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = $"{user.FirstName} {user.LastName}",
                    IsActive = true,
                    IsSubscriber = false,
                    Culture = user.Setting.Culture,
                    Currency = app.Setting.Currency,
                    CreatedAt = user.CreatedAt,
                    CreatedByEmail = user.Email
                };

                await Postgres.CreateDatabaseWithTemplate(_tenantRepository.DbContext.Database.GetDbConnection().ConnectionString, tenantId, activateBindingModel.AppId);

                _userRepository.CurrentUser = new CurrentUser { TenantId = tenant.Id, UserId = user.Id };
                _profileRepository.CurrentUser = new CurrentUser { TenantId = tenant.Id, UserId = user.Id };
                _roleRepository.CurrentUser = new CurrentUser { TenantId = tenant.Id, UserId = user.Id };
                _recordRepository.CurrentUser = new CurrentUser { TenantId = tenant.Id, UserId = user.Id };

                _profileRepository.TenantId = _roleRepository.TenantId = _userRepository.TenantId = _recordRepository.TenantId = tenantId;

                tenantUser.IsSubscriber = true;
                await _userRepository.CreateAsync(tenantUser);

                var userProfile = await _profileRepository.GetDefaultAdministratorProfileAsync();
                var userRole = await _roleRepository.GetByIdAsync(1);

                tenantUser.Profile = userProfile;
                tenantUser.Role = userRole;


                await _userRepository.UpdateAsync(tenantUser);
                await _recordRepository.UpdateSystemData(user.Id, DateTime.UtcNow, tenant.Setting.Language, activateBindingModel.AppId);


                user.TenantsAsUser.Add(new UserTenant { Tenant = tenant, PlatformUser = user });

                Queue.QueueBackgroundWorkItem(token => _documentHelper.UploadSampleDocuments(tenant.GuidId, activateBindingModel.AppId, tenant.Setting.Language));

                //user.TenantId = user.Id;
                //tenant.License.HasAnalyticsLicense = true;
                await _platformUserRepository.UpdateAsync(user);
                await _tenantRepository.UpdateAsync(tenant);

                await _recordRepository.UpdateSampleData(user);
                //await Cache.ApplicationUser.Add(user.Email, user.Id);
                //await Cache.User.Get(user.Id);

                if (!string.IsNullOrEmpty(activateBindingModel.Token) && (!userExist || !activateBindingModel.EmailConfirmed))
                {
                    var template = _platformRepository.GetAppTemplate(activateBindingModel.AppId, AppTemplateType.Email, "email_confirm", activateBindingModel.Culture.Substring(0, 2));
                    var content = template.Content;

                    content = content.Replace("{:FirstName}", activateBindingModel.FirstName);
                    content = content.Replace("{:LastName}", activateBindingModel.LastName);
                    content = content.Replace("{:Email}", activateBindingModel.Email);
                    content = content.Replace("{:Url}", Request.Scheme + "://" + app.Setting.AuthDomain + "/user/confirm_email?email=" + activateBindingModel.Email + "&token=" + WebUtility.UrlEncode(activateBindingModel.Token));

                    Email notification = new Email(template.Subject, content, _configuration);

                    var senderEmail = template.MailSenderEmail ?? app.Setting.MailSenderEmail;
                    var senderName = template.MailSenderName ?? app.Setting.MailSenderName;

                    notification.AddRecipient(senderEmail);
                    notification.AddToQueue(senderEmail, senderName);

                }

                //TODO Buraya webhook eklenecek. AppSetting üzerindeki TenantCreateWebhook alanı dolu kontrol edilecek doluysa bu url'e post edilecek
                //Queue.QueueBackgroundWorkItem(async token => await _platformWorkflowHelper.Run(OperationType.insert, app));

            }
            catch (Exception ex)
            {
                Postgres.DropDatabase(_tenantRepository.DbContext.Database.GetDbConnection().ConnectionString, tenantId, true);

                await DeactivateUser(tenant);

                throw ex;
            }

            return Ok();
        }
        //return GetErrorResult(confirmResponse);
        //return BadRequestResult();

        [Route("change_password")]
        public async Task<IActionResult> ChangePassword([FromBody]ChangePasswordBindingModel changePasswordBindingModel)
        {
            if (HttpContext.User.FindFirst("email") == null || string.IsNullOrEmpty(HttpContext.User.FindFirst("email").Value))
                return Unauthorized();

            changePasswordBindingModel.Email = HttpContext.User.FindFirst("email").Value;

            var appInfo = await _applicationRepository.Get(Request.Host.Value);
            using (var httpClient = new HttpClient())
            {

                var url = Request.Scheme + "://" + appInfo.Setting.AuthDomain + "/user/change_password";
                httpClient.BaseAddress = new Uri(url);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                var json = JsonConvert.SerializeObject(changePasswordBindingModel);
                var response = await httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                    return BadRequest(response);
            }

            return Ok();
        }

        // POST account/logout
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            var appInfo = await _applicationRepository.Get(Request.Host.Value);

            Response.Cookies.Delete("tenant_id");
            await HttpContext.SignOutAsync();

            return StatusCode(200, new { redirectUrl = Request.Scheme + "://" + appInfo.Setting.AuthDomain + "/Account/Logout" });
        }

        private async Task DeactivateUser(Tenant tenant)
        {
            await _tenantRepository.DeleteAsync(tenant);
        }
    }
}


