using System;
using System.Configuration;
using PrimeApps.App.Models;
using PrimeApps.Model.Repositories.Interfaces;
using System.Threading.Tasks;
using PrimeApps.Model.Context;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Common.Cache;
using Hangfire;
using System.Collections.Generic;
using System.Linq;

namespace PrimeApps.App.Helpers
{
	public class PlatformHelper
	{
		public static string GenerateRandomToken(TokenOptions opts = null)
		{
			if (opts == null) opts = new TokenOptions()
			{
				RequiredLength = 16,
				RequiredUniqueChars = 4,
				RequireDigit = true,
				RequireLowercase = true,
				RequireNonAlphanumeric = true,
				RequireUppercase = true
			};

			string[] randomChars = new[] {
				"ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
				"abcdefghijkmnopqrstuvwxyz",    // lowercase
				"0123456789",                   // digits
				"!@$?_-"                        // non-alphanumeric
			};
			Random rand = new Random(Environment.TickCount);
			List<char> chars = new List<char>();

			if (opts.RequireUppercase)
				chars.Insert(rand.Next(0, chars.Count),
					randomChars[0][rand.Next(0, randomChars[0].Length)]);

			if (opts.RequireLowercase)
				chars.Insert(rand.Next(0, chars.Count),
					randomChars[1][rand.Next(0, randomChars[1].Length)]);

			if (opts.RequireDigit)
				chars.Insert(rand.Next(0, chars.Count),
					randomChars[2][rand.Next(0, randomChars[2].Length)]);

			if (opts.RequireNonAlphanumeric)
				chars.Insert(rand.Next(0, chars.Count),
					randomChars[3][rand.Next(0, randomChars[3].Length)]);

			for (int i = chars.Count; i < opts.RequiredLength
				|| chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
			{
				string rcs = randomChars[rand.Next(0, randomChars.Length)];
				chars.Insert(rand.Next(0, chars.Count),
					rcs[rand.Next(0, rcs.Length)]);
			}

			return new string(chars.ToArray());
		}
	}

	//
	// Summary:
	//     Specifies options for password requirements.
	public class TokenOptions
	{
		//
		// Summary:
		//     Gets or sets the minimum length a password must be. Defaults to 6.
		public int RequiredLength { get; set; }
		//
		// Summary:
		//     Gets or sets the minimum number of unique chars a password must comprised of.
		//     Defaults to 1.
		public int RequiredUniqueChars { get; set; }
		//
		// Summary:
		//     Gets or sets a flag indicating if passwords must contain a non-alphanumeric character.
		//     Defaults to true.
		public bool RequireNonAlphanumeric { get; set; }
		//
		// Summary:
		//     Gets or sets a flag indicating if passwords must contain a lower case ASCII character.
		//     Defaults to true.
		public bool RequireLowercase { get; set; }
		//
		// Summary:
		//     Gets or sets a flag indicating if passwords must contain a upper case ASCII character.
		//     Defaults to true.
		public bool RequireUppercase { get; set; }
		//
		// Summary:
		//     Gets or sets a flag indicating if passwords must contain a digit. Defaults to
		//     true.
		public bool RequireDigit { get; set; }
	}

	/*public static class PlatformHelper
    {
       
		public static async Task<Model.Entities.Platform.App> CreateEntity(AppBindingModel appModel, IUserRepository userRepository)
		{
			var app = new Model.Entities.Platform.App
			{
				Name = appModel.Name,
				Description = appModel.Description,
				Logo = appModel.Logo,
				TemplateId = appModel.TemplateId
			};

			return app;
		}

		public static async Task<Model.Entities.Platform.App> UpdateEntity(AppBindingModel appModel, Model.Entities.Platform.App app, IUserRepository userRepository)
		{
			app.Name = appModel.Name;
			app.Description = appModel.Description;
			app.Logo = appModel.Logo;

			return app;
		}

		public static async Task AppAfterCreate(UserItem appUser, Model.Entities.Platform.App appEntity, ApplicationUserManager userManager, IUserRepository userRepository, IProfileRepository profileRepository, IRoleRepository roleRepository, IRecordRepository recordRepository, IPlatformUserRepository platformUserRepository, ITenantRepository tenantRepository, Model.Helpers.Warehouse warehouse)
		{
			PlatformUser user;
			using (PlatformDBContext platformDbContext = new PlatformDBContext())
			{
				using (PlatformUserRepository puRepo = new PlatformUserRepository(platformDbContext))
				{
					user = await puRepo.Get(appUser.Id);
				}
			}

			var addUserRequest = new AddUserBindingModel
			{
				Email = "app_" + appEntity.Id + "_" + user.Email,
				FirstName = user.FirstName,
				LastName = user.LastName,
				ModuleLicenseCount = 100,
				Phone = user.PhoneNumber,
				ProfileId = 1,
				RoleId = 1
			};

			var applicationUser = new PlatformUser
			{
				UserName = addUserRequest.Email,
				Email = addUserRequest.Email,
				PhoneNumber = addUserRequest.Phone,
				FirstName = addUserRequest.FirstName,
				LastName = addUserRequest.LastName,
				Culture = appUser.Culture,
				Currency = appUser.Currency,
				CreatedAt = DateTime.Now,
				//TODO Removed AppId = appEntity.Id
			};
			//TODO Removed
			//var result = await userManager.CreateAsync(applicationUser, ConfigurationManager.AppSettings["PrimeAppsPreviewPassword"]);
			
            if (!result.Succeeded false)
                throw new Exception();

            var tenantUser = await platformUserRepository.Get(applicationUser.Id);

            userRepository.TenantId = tenantUser.Id;
            profileRepository.TenantId = tenantUser.Id;
            roleRepository.TenantId = tenantUser.Id;
            recordRepository.TenantId = tenantUser.Id;

            await Postgres.CreateDatabaseWithTemplate($"tenant{tenantUser.Id}", $"templet{appEntity.TemplateId.Value}");

            Tenant tenant;

            //Here we create new tenant and assign its id as tenant users id. Then we create a new GuidId for GUID ID dependencies of Documents etc.
            tenant = new Tenant
            {
                Id = tenantUser.Id,
                GuidId = new Guid(),
                Title = appEntity.Name,
                Currency = appUser.Currency,
                Owner = tenantUser,
                Logo = appEntity.Logo,
                Language = appUser.TenantLanguage,
                HasSampleData = appEntity.TemplateId.Value != 0,

            };

            await tenantRepository.UpdateAsync(tenant);
            //HostingEnvironment.QueueBackgroundWorkItem(clt => DocumentHelper.UploadSampleDocuments(tenant.GuidId, appEntity.TemplateId.Value, tenant.Language));
			BackgroundJob.Enqueue(() => DocumentHelper.UploadSampleDocuments(tenant.GuidId, appEntity.TemplateId.Value, tenant.Language));

			await platformUserRepository.UpdateAsync(user);

            await UserHelper.AddUser(addUserRequest, user.Culture, user.Currency, appUser.TenantLanguage, appEntity.Id, user.Email, tenant.Id, userManager, userRepository, profileRepository, roleRepository, recordRepository, platformUserRepository, warehouse, applicationUser);
            await recordRepository.UpdateSystemData(tenantUser.Id, DateTime.UtcNow, appUser.TenantLanguage, appEntity.TemplateId.Value);
            await recordRepository.UpdateSampleData(tenantUser);
        }

		//TODO Removed
		public static async Task AddApp(UserItem appUser, int appId, ApplicationUserManager userManager, ITenantRepository tenantRepository, IPlatformUserRepository platformUserRepository, IUserRepository userRepository, IProfileRepository profileRepository, IRoleRepository roleRepository, IRecordRepository recordRepository, Warehouse warehouse)
        {
            var user = await platformUserRepository.Get(appUser.Id);
            char[] emailCharset = { '@' };
            //user.email.Split(emailCharset, StringSplitOptions.None)[0] + "___" + appId + "@ofisim.com";
            var email = "primemultiapps__" + user.Email;
            var addUserRequest = new AddUserBindingModel
            {
                Email = email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ModuleLicenseCount = 100,
                Phone = user.PhoneNumber,
                ProfileId = 1,
                RoleId = 1
            };

            var applicationUser = new PlatformUser
            {
                UserName = addUserRequest.Email,
                Email = addUserRequest.Email,
                PhoneNumber = addUserRequest.Phone,
                FirstName = addUserRequest.FirstName,
                LastName = addUserRequest.LastName,
                Culture = appUser.Culture,
                Currency = appUser.Currency,
                CreatedAt = DateTime.Now,
                //TODO Removed AppId = appId
            };

			//TODO Removed
			//var result = await userManager.CreateAsync(applicationUser, Utils.GenerateRandomUnique(8));

            if (!result.Succeeded false)
                throw new Exception();

            var tenantUser = await platformUserRepository.Get(applicationUser.Id);

            userRepository.TenantId = tenantUser.Id;
            profileRepository.TenantId = tenantUser.Id;
            roleRepository.TenantId = tenantUser.Id;
            recordRepository.TenantId = tenantUser.Id;

            await Postgres.CreateDatabaseWithTemplate($"tenant{tenantUser.Id}", $"app{appId}");


            Tenant tenant;

            //Here we create new tenant and assign its id as tenant users id. Then we create a new GuidId for GUID ID dependencies of Documents etc.
            tenant = new Tenant
            {
                Id = tenantUser.Id,
                GuidId = new Guid(),
                Title = $"{addUserRequest.FirstName} {addUserRequest.LastName}",
                Currency = appUser.Currency,
                Language = appUser.TenantLanguage,
                Owner = tenantUser,
                Logo = "",
                HasSampleData = true
            };

            await tenantRepository.UpdateAsync(tenant);

            //HostingEnvironment.QueueBackgroundWorkItem(clt => DocumentHelper.UploadSampleDocuments(tenant.GuidId, appId, tenant.Language));
			BackgroundJob.Enqueue(() => DocumentHelper.UploadSampleDocuments(tenant.GuidId, appId, tenant.Language));
			//TODO Removed
			await UserHelper.AddUserToNewTenant(addUserRequest, user.Culture, user.Currency, tenant.Language, appId, user.Email, tenantUser.Id, userManager, platformUserRepository, userRepository, profileRepository, roleRepository, recordRepository, warehouse, applicationUser);


            var addUserRequestOld = new AddUserBindingModel
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ModuleLicenseCount = 100,
                Phone = user.PhoneNumber,
                ProfileId = 1,
                RoleId = 1
            };

            var applicationUserOld = new PlatformUser
            {
                Id = user.Id,
                UserName = addUserRequest.Email,
                Email = addUserRequest.Email,
                PhoneNumber = addUserRequest.Phone,
                FirstName = addUserRequest.FirstName,
                LastName = addUserRequest.LastName,
                Culture = appUser.Culture,
                Currency = appUser.Currency,
                CreatedAt = DateTime.Now,
				//TODO Removed AppId = appId
			};
			//TODO Removed
			await UserHelper.AddUserToNewTenant(addUserRequestOld, user.Culture, user.Currency, tenant.Language, appId, user.Email, tenant.Id, userManager, platformUserRepository, userRepository, profileRepository, roleRepository, recordRepository, warehouse, applicationUserOld);
			//TODO Removed
			await UserHelper.AddUserToNewTenant(addUserRequest, user.Culture, user.Currency, tenant.Language, appId, user.Email, tenant.Id, userManager, platformUserRepository, userRepository, profileRepository, roleRepository, recordRepository, warehouse, applicationUser);

            await recordRepository.UpdateSystemData(tenantUser.Id, DateTime.UtcNow, tenant.Language, appId);

            var newUser = await platformUserRepository.Get(tenantUser.Id);


            newUser.PasswordHash = user.PasswordHash;
            await platformUserRepository.UpdateAsync(newUser);

            using (var dbContext = new PlatformDBContext())
            {
				//TODO Removed
				var app = new UserApp
                {
                    UserId = user.Id,
                    TenantId = tenantUser.TenantId.Value,
                    MainTenantId = user.TenantId.Value,
                    Email = email,
                    Active = false,
                    AppId = appId
                };

                dbContext.UserApps.Add(app);
				//dbContext.UserApps.AddOrUpdate(app);
				dbContext.SaveChanges();
            }
        }

		
	}*/

}