﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Studio;

namespace PrimeApps.Model.Repositories
{
    public class AppDraftRepository : RepositoryBaseStudio, IAppDraftRepository
    {
        public AppDraftRepository(StudioDBContext dbContext, IConfiguration configuration)
            : base(dbContext, configuration)
        {
        }

        public List<int> GetAppIdsByOrganizationId(int organizationId)
        {
            return DbContext.Apps
                .Where(x => x.OrganizationId == organizationId && !x.Deleted)
                .Select(x => x.Id)
                .ToList();
        }

        public async Task<AppDraft> Get(string name)
        {
            return await DbContext.Apps
                .Include(x => x.Setting)
                .Where(x => x.Name == name && !x.Deleted)
                .FirstOrDefaultAsync();
        }

        public async Task<AppDraft> Get(int id)
        {
            return await DbContext.Apps
                .Where(x => x.Id == id && !x.Deleted)
                //.Include(x => x.Organization)
                .Include(x => x.Setting)
                .FirstOrDefaultAsync();
        }

        public async Task<int> Create(AppDraft app)
        {
            DbContext.Apps.Add(app);
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(AppDraft app)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<AppDraftSetting> GetAuthTheme(int id)
        {
            return await DbContext.AppSettings
                .Where(x => x.AppId == id).FirstOrDefaultAsync();
        }

        public async Task<int> UpdateAuthTheme(int id, JObject model)
        {
            var appSettings = DbContext.AppSettings.Where(x => x.AppId == id).FirstOrDefault();
            var jsonData = JsonConvert.SerializeObject(model);
            appSettings.AuthTheme = jsonData;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<AppDraftSetting> GetAppTheme(int id)
        {
            return await DbContext.AppSettings
                .Where(x => x.AppId == id).FirstOrDefaultAsync();
        }

        public async Task<int> UpdateAppTheme(int id, JObject model)
        {
            var appSettings = DbContext.AppSettings.Where(x => x.AppId == id).FirstOrDefault();
            var jsonData = JsonConvert.SerializeObject(model);
            appSettings.AppTheme = jsonData;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Delete(AppDraft app)
        {
            app.Deleted = true;
            return await DbContext.SaveChangesAsync();
        }

        public async Task<List<int>> GetByTeamId(int id)
        {
            return await DbContext.AppCollaborators
                .Where(x => x.TeamId == id && !x.Deleted)
                .Select(x => x.AppId)
                .ToListAsync();
        }

        public async Task<List<AppDraft>> GetByOrganizationId(int userId, int organizationId, string search = "", int page = 0, PublishStatus status = PublishStatus.NotSet)
        {
            var teamIds = await DbContext.TeamUsers.Where(x => x.UserId == userId && !x.Team.Deleted).Select(x => x.TeamId).ToListAsync();

            var appCollabrator = await DbContext.AppCollaborators
                .Where(x => !x.Deleted && x.AppDraft.OrganizationId == organizationId && (x.UserId == userId || (x.Team != null && teamIds.Contains((int)x.TeamId))) && (!string.IsNullOrEmpty(search) ? x.AppDraft.Label.Contains(search) : true) && (status != PublishStatus.NotSet ? x.AppDraft.Status == status : true))
                .Select(x => new AppDraft
                {
                    Id = x.AppDraft.Id,
                    OrganizationId = x.AppDraft.OrganizationId,
                    Name = x.AppDraft.Name,
                    Label = x.AppDraft.Label,
                    Description = x.AppDraft.Description,
                    Logo = x.AppDraft.Logo,
                    TempletId = x.AppDraft.TempletId,
                    Status = x.AppDraft.Status
                })
                .Skip(50 * page)
                .Take(50)
                .Distinct()
                .ToListAsync();

            return appCollabrator;
        }

        public async Task<List<AppDraft>> GetAllByUserId(int userId, string search = "", int page = 0, PublishStatus status = PublishStatus.NotSet)
        {
            var teamIds = await DbContext.TeamUsers.Where(x => x.UserId == userId && !x.Team.Deleted).Select(x => x.TeamId).ToListAsync();

            var appCollabrator = await DbContext.AppCollaborators
                .Where(x => !x.Deleted && (x.UserId == userId || (x.Team != null && teamIds.Contains((int)x.TeamId))) && (string.IsNullOrEmpty(search) || x.AppDraft.Label.ToLower().Contains(search.ToLower())) && (status == PublishStatus.NotSet || x.AppDraft.Status == status))
                .Select(x => x.AppDraft)
                .Skip(50 * page)
                .Take(50)
                .Distinct()
                .ToListAsync();

            return appCollabrator;
        }

        public async Task<List<AppCollaborator>> GetAppCollaborators(int appId)
        {
            return await DbContext.AppCollaborators
                .Where(x => x.AppId == appId && !x.Deleted)
                .ToListAsync();
        }
    }
}