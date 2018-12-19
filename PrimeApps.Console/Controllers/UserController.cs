﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common.Organization;
using PrimeApps.Model.Common.User;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.Console.Controllers
{
    [Route("api/user"), Authorize(AuthenticationSchemes = "Bearer"), ActionFilters.CheckHttpsRequire, ResponseCache(CacheProfileName = "Nocache")]
    public class UserController : BaseController
    {
        private IConfiguration _configuration;
        private IPlatformUserRepository _platformUserRepository;
        private IAppDraftRepository _appDraftRepository;
        private IOrganizationRepository _organizationRepository;
        private ITeamRepository _teamRepository;

        public UserController(IConfiguration configuration, IPlatformUserRepository platformUserRepository, IAppDraftRepository appDraftRepository, IOrganizationRepository organizationRepository, ITeamRepository teamRepository)
        {
            _platformUserRepository = platformUserRepository;
            _appDraftRepository = appDraftRepository;
            _organizationRepository = organizationRepository;
            _teamRepository = teamRepository;
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated || string.IsNullOrWhiteSpace(context.HttpContext.User.FindFirst("email").Value))
                context.Result = new UnauthorizedResult();

            SetContextUser();
        }

        [Route("me"), HttpGet]
        public async Task<IActionResult> Me()
        {
            var user = await _platformUserRepository.GetWithSettings(AppUser.Email);

            var me = new ConsoleUser
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.GetFullName(),
                Phone = user.Setting.Phone
            };

            return Ok(me);
        }

        [Route("apps"), HttpPost]
        public async Task<IActionResult> Apps([FromBody]JObject request)
        {
            var search = "";
            var page = 0;
            var status = AppDraftStatus.NotSet;

            if (request != null)
            {
                if (!request["search"].IsNullOrEmpty())
                    search = request["search"].ToString();

                if (!request["page"].IsNullOrEmpty())
                    page = (int)request["page"];

                if (!request["status"].IsNullOrEmpty())
                    status = (AppDraftStatus)int.Parse(request["status"].ToString());
            }

            var apps = await _appDraftRepository.GetAllByUserId(AppUser.Id, search, page, status);

            return Ok(apps);
        }

        [Route("organizations"), HttpGet]
        public async Task<IActionResult> Organizations()
        {
            var organizations = await _organizationRepository.GetByUserId(AppUser.Id);

            return Ok(organizations);
        }
    }
}
