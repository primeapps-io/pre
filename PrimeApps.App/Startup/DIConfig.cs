﻿using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.App.Services;
using PrimeApps.Model.Context;
using PrimeApps.Model.Helpers;
using System;
using System.Linq;
using System.Reflection;
using PrimeApps.App.Bpm.Steps;
using WarehouseHelper = PrimeApps.App.Jobs.Warehouse;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Storage;

namespace PrimeApps.App
{
    public partial class Startup
    {
        public static void DIRegister(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<TenantDBContext>(options => options.UseNpgsql(configuration.GetConnectionString("TenantDBConnection")));
            services.AddDbContext<PlatformDBContext>(options => options.UseNpgsql(configuration.GetConnectionString("PlatformDBConnection")));
            services.AddScoped(p => new PlatformDBContext(p.GetService<DbContextOptions<PlatformDBContext>>(), configuration));
            services.AddScoped(p => new TenantDBContext(p.GetService<DbContextOptions<TenantDBContext>>()));
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton(configuration);
            services.AddHttpContextAccessor();

            //Register all repositories
            foreach (var assembly in new[] { "PrimeApps.Model" })
            {
                var assemblies = Assembly.Load(assembly);
                var allServices = assemblies.GetTypes().Where(t => t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract && t.GetTypeInfo().Name.EndsWith("Repository")).ToList();

                foreach (var type in allServices)
                {
                    var allInterfaces = type.GetInterfaces().Where(x => x.Name.EndsWith("Repository")).ToList();
                    var mainInterfaces = allInterfaces.Except(allInterfaces.SelectMany(t => t.GetInterfaces()));

                    foreach (var itype in mainInterfaces)
                    {
                        if (allServices.Any(x => x != type && itype.IsAssignableFrom(x)))
                        {
                            throw new Exception("The " + itype.Name + " type has more than one implementations, please change your filter");
                        }

                        services.AddTransient(itype, type);
                    }
                }
            }

            services.TryAddSingleton<IHistoryHelper, HistoryHelper>();

            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

            services.AddScoped<ICacheHelper, CacheHelper>();
            services.AddScoped<Helpers.IRecordHelper, Helpers.RecordHelper>();
            services.AddScoped<Helpers.IAuditLogHelper, Helpers.AuditLogHelper>();
            services.AddScoped<Helpers.IDocumentHelper, Helpers.DocumentHelper>();
            services.AddScoped<Helpers.ICalculationHelper, Helpers.CalculationHelper>();
            services.AddScoped<Helpers.IChangeLogHelper, Helpers.ChangeLogHelper>();
            services.AddScoped<Helpers.IModuleHelper, Helpers.ModuleHelper>();
            services.AddScoped<Helpers.IWorkflowHelper, Helpers.WorkflowHelper>();
            services.AddScoped<Helpers.IProcessHelper, Helpers.ProcessHelper>();
            services.AddScoped<Helpers.IReportHelper, Helpers.ReportHelper>();
            services.AddScoped<Helpers.IPowerBiHelper, Helpers.PowerBiHelper>();
            services.AddScoped<Helpers.IRoleHelper, Helpers.RoleHelper>();
            //services.AddScoped<Helpers.IBpmHelper, Helpers.BpmHelper>();
            services.AddScoped<Helpers.IAnalyticsHelper, Helpers.AnalyticsHelper>();
            services.AddScoped<Helpers.IActionButtonHelper, Helpers.ActionButtonHelper>();
            services.AddScoped<Notifications.INotificationHelper, Notifications.NotificationHelper>();
            services.AddScoped<Notifications.IActivityHelper, Notifications.ActivityHelper>();
            services.AddScoped<Helpers.IFunctionHelper, Helpers.FunctionHelper>();
            services.AddScoped<Helpers.IEnvironmentHelper, Helpers.EnvironmentHelper>();
            services.AddScoped<WarehouseHelper, WarehouseHelper>();
            services.AddScoped<Warehouse, Warehouse>();
            services.AddScoped<Jobs.Email.Email, Jobs.Email.Email>();
            services.AddScoped<Jobs.Messaging.EMail.EMailClient, Jobs.Messaging.EMail.EMailClient>();
            services.AddScoped<Jobs.Messaging.SMS.SMSClient, Jobs.Messaging.SMS.SMSClient>();
            services.AddScoped<Jobs.Reminder.Activity, Jobs.Reminder.Activity>();
            services.AddScoped<Jobs.TrialNotification, Jobs.TrialNotification>();
            services.AddScoped<Jobs.AccountDeactivate, Jobs.AccountDeactivate>();
            services.AddScoped<Jobs.UpdateLeave, Jobs.UpdateLeave>();
            services.AddScoped<Jobs.EmployeeCalculation, Jobs.EmployeeCalculation>();
            services.AddScoped<Jobs.AccountCleanup, Jobs.AccountCleanup>();

            services.AddTransient<IUnifiedStorage, UnifiedStorage>();
            services.AddTransient<ApprovalStep>();
            services.AddTransient<DataCreateStep>();
            services.AddTransient<DataDeleteStep>();
            services.AddTransient<DataReadStep>();
            services.AddTransient<DataUpdateStep>();
            services.AddTransient<EmailStep>();
            services.AddTransient<FormStep>();
            services.AddTransient<NotificationStep>();
            services.AddTransient<SmsStep>();
            services.AddTransient<TaskStep>();
            services.AddTransient<WebhookStep>();
            services.AddTransient<FunctionStep>();
        }
    }
}