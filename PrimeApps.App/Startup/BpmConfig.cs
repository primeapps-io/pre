﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using PrimeApps.App.Bpm.Workflows;
using PrimeApps.App.Models;
using WorkflowCore.Interface;

namespace PrimeApps.App
{
    public partial class Startup
    {
        public static void BpmConfiguration(IApplicationBuilder app, IConfiguration configuration)
        {
            var enableBpmSetting = configuration.GetValue("AppSettings:EnableBpm", string.Empty);

            if (!string.IsNullOrEmpty(enableBpmSetting))
            {
                var enableBpm = bool.Parse(enableBpmSetting);

                if (!enableBpm)
                    return;
            }
            else
                return;

            var host = app.ApplicationServices.GetService<IWorkflowHost>();

            //Register worflows here if platform needs. Use host.RegisterWorkflow method of Worflow-Core.
            //App's and tenant's worflows should be register on runtime.

            //Register TestWorkflow
            //host.RegisterWorkflow<TestWorkflow, BpmReadDataModel>();

            host.Start();

            Console.WriteLine("Workflow Host started");

            //var record = new JObject { ["id"] = 19281 };//19281:bayan - 19282:bay
            //var data = new BpmReadDataModel { ModuleId = 3, Record = record };
            //var reference = "{ \"Id\": 2472,  \"Culture\": \"tr-TR\", \"TimeZone\": null, \"Language\": \"tr\", \"TenantId\": 2472}";

            //host.StartWorkflow("TestWorkflow", 1, data, reference);
        }
    }
}