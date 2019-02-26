﻿using System;
using PrimeApps.Model.Context;
using System.Threading.Tasks;
using Npgsql;
using PrimeApps.Model.Helpers.QueryTranslation;
using PrimeApps.Model.Repositories;
using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Model.Helpers;
using Microsoft.Extensions.Caching.Distributed;

namespace PrimeApps.App.Jobs
{
	public class AccountDeactivate
	{
		private IConfiguration _configuration;
		private IServiceScopeFactory _serviceScopeFactory;

		public AccountDeactivate(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
		{
			_configuration = configuration;
			_serviceScopeFactory = serviceScopeFactory;
		}

		public async Task Deactivate()
		{
			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var databaseContext = scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				var platformDatabaseContext = scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
				var distributedCache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
				var cacheHelper = scope.ServiceProvider.GetRequiredService<ICacheHelper>();

				var previewMode = _configuration.GetValue("AppSettings:PreviewMode", string.Empty);
				previewMode = !string.IsNullOrEmpty(previewMode) ? previewMode : "tenant";

				using (var tenantRepository = new TenantRepository(platformDatabaseContext, _configuration, cacheHelper))
				using (var userRepository = new UserRepository(databaseContext, _configuration))
				{
					var tenants = await tenantRepository.GetExpiredTenants();

					foreach (var tenant in tenants)
					{

						userRepository.CurrentUser = new CurrentUser { TenantId = tenant.Id, UserId = 1, PreviewMode = previewMode };

						var users = await userRepository.GetAllAsync();

						foreach (var user in users)
						{
							try
							{
								user.IsActive = false;
								databaseContext.SaveChanges();
							}
							catch (DataException ex)
							{
								if (ex.InnerException is PostgresException)
								{
									var innerEx = (PostgresException)ex.InnerException;

									if (innerEx.SqlState == PostgreSqlStateCodes.DatabaseDoesNotExist)
										continue;
								}

								throw;
							}
						}

						tenant.License.IsDeactivated = true;
						tenant.License.DeactivatedAt = DateTime.UtcNow;

						await tenantRepository.UpdateAsync(tenant);
					}
				}
			}
		}
	}
}