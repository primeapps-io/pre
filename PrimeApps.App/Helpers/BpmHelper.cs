﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using PrimeApps.App.Models;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.App.Helpers
{
    public interface IBpmHelper
    {
        Task<BpmWorkflow> CreateEntity(BpmWorkflowBindingModel bpmWorkflowModel, string tenantLanguage);

        Task UpdateEntity(BpmWorkflowBindingModel bpmWorkflowModel, BpmWorkflow bpmWorkflow, string tenantLanguage);

        string ReferenceCreateToForBpmHost(UserItem appUser);
    }
    public class BpmHelper : IBpmHelper
    {
        private CurrentUser _currentUser;
        private IHttpContextAccessor _context;
        private IConfiguration _configuration;
        private IServiceScopeFactory _serviceScopeFactory;

        public BpmHelper(IServiceScopeFactory serviceScopeFactory, IHttpContextAccessor context, IConfiguration configuration)
        {
            _context = context;
            _currentUser = UserHelper.GetCurrentUser(_context);
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;
        }

        public async Task<BpmWorkflow> CreateEntity(BpmWorkflowBindingModel bpmWorkflowModel, string tenantLanguage)
        {
            var bpmWorkflow = new BpmWorkflow
            {
                Name = bpmWorkflowModel.Name,
                Code = bpmWorkflowModel.Code,
                Description = bpmWorkflowModel.Description,
                CategoryId = bpmWorkflowModel.CategoryId,
                StartTime = bpmWorkflowModel.StartTime,
                EndTime = bpmWorkflowModel.EndTime,
                TriggerType = bpmWorkflowModel.TriggerType,
                RecordOperations = bpmWorkflowModel.RecordOperations,
                Frequency = bpmWorkflowModel.Frequency,
                ProcessFilter = bpmWorkflowModel.ProcessFilter,
                ChangedFields = bpmWorkflowModel.ChangedFields,
                CanStartManuel = bpmWorkflowModel.CanStartManuel,
                DefinitionJson = bpmWorkflowModel.DefinitionJson.ToJsonString(),
                DiagramJson = bpmWorkflowModel.DiagramJson,
                ModuleId = bpmWorkflowModel.ModuleId
            };

            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();

                using (var _moduleRepository = new ModuleRepository(databaseContext, _configuration))
                using (var _picklistRepository = new PicklistRepository(databaseContext, _configuration))
                using (var _categoryRepository = new BpmCategoryRepository(databaseContext, _configuration))
                {
                    _moduleRepository.CurrentUser = _picklistRepository.CurrentUser = _categoryRepository.CurrentUser = _currentUser;

                    //workflow's category is pulling
                    if (bpmWorkflow.CategoryId != null && bpmWorkflow.CategoryId > 0)
                    {
                        var category = await _categoryRepository.Get((int)bpmWorkflow.CategoryId);

                        bpmWorkflow.Category = category;
                    }

                    //workflow's Filters is pulling
                    if (bpmWorkflowModel.Filters != null && bpmWorkflowModel.Filters.Count > 0)
                    {
                        var module = await _moduleRepository.GetById(bpmWorkflowModel.ModuleId);
                        var picklistItemsIDs = new List<int>();
                        bpmWorkflowModel.Filters = new List<BpmRecordFilter>();

                        foreach (var filter in bpmWorkflowModel.Filters)
                        {
                            var field = module.Fields.Single(x => x.Name == filter.Field);

                            int tempvalue;
                            if (field.DataType == DataType.Picklist)
                            {
                                if (int.TryParse(filter.Value.ToString(), out tempvalue))
                                {
                                    picklistItemsIDs.Add(int.Parse(filter.Value.ToString()));
                                }
                            }
                            else if (field.DataType == DataType.Multiselect)
                            {
                                var values = filter.Value.ToString().Split(',');

                                foreach (var value in values)
                                {
                                    picklistItemsIDs.Add(int.Parse(value));
                                }
                            }
                        }

                        ICollection<PicklistItem> picklistItems = null;

                        if (picklistItemsIDs.Count > 0)
                            picklistItems = await _picklistRepository.FindItems(picklistItemsIDs);

                        foreach (var filter in bpmWorkflowModel.Filters)
                        {
                            var field = module.Fields.Single(x => x.Name == filter.Field);
                            var value = filter.Value.ToString();

                            if (filter.Operator != Operator.Empty && filter.Operator != Operator.NotEmpty)
                            {
                                if (field.DataType == DataType.Picklist)
                                {
                                    var picklistItem = picklistItems.Single(x => x.Id == int.Parse(filter.Value.ToString()));
                                    value = tenantLanguage == "tr" ? picklistItem.LabelTr : picklistItem.LabelEn;
                                }
                                else if (field.DataType == DataType.Multiselect)
                                {
                                    var picklistLabels = new List<string>();
                                    var values = filter.Value.ToString().Split(',');

                                    foreach (var item in values)
                                    {
                                        var picklistItem = picklistItems.Single(q => q.Id == int.Parse(item));
                                        picklistLabels.Add(tenantLanguage == "tr" ? picklistItem.LabelTr : picklistItem.LabelEn);
                                    }

                                    value = string.Join("|", picklistLabels);
                                }
                                else if (field.DataType == DataType.Tag)
                                { }
                            }

                            var recordFilter = new BpmRecordFilter
                            {
                                Field = filter.Field,
                                Operator = filter.Operator,
                                Value = value,
                                No = filter.No
                            };

                            bpmWorkflow.Filters.Add(recordFilter);
                        }
                    }
                }
            }

            return bpmWorkflow;
        }

        public async Task UpdateEntity(BpmWorkflowBindingModel bpmWorkflowModel, BpmWorkflow bpmWorkflow, string tenantLanguage)
        {
            bpmWorkflow.Name = bpmWorkflowModel.Name;
            bpmWorkflow.Description = bpmWorkflowModel.Description;
            bpmWorkflow.CategoryId = bpmWorkflowModel.CategoryId;
            bpmWorkflow.StartTime = bpmWorkflowModel.StartTime;
            bpmWorkflow.EndTime = bpmWorkflowModel.EndTime;
            bpmWorkflow.TriggerType = bpmWorkflowModel.TriggerType;
            bpmWorkflow.RecordOperations = bpmWorkflowModel.RecordOperations;
            bpmWorkflow.Frequency = bpmWorkflowModel.Frequency;
            bpmWorkflow.ProcessFilter = bpmWorkflowModel.ProcessFilter;
            bpmWorkflow.ChangedFields = bpmWorkflowModel.ChangedFields;
            bpmWorkflow.CanStartManuel = bpmWorkflowModel.CanStartManuel;
            bpmWorkflow.DefinitionJson = bpmWorkflowModel.DefinitionJson.ToJsonString();
            bpmWorkflow.DiagramJson = bpmWorkflowModel.DiagramJson;
            bpmWorkflow.ModuleId = bpmWorkflowModel.ModuleId;

            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();

                using (var _moduleRepository = new ModuleRepository(databaseContext, _configuration))
                using (var _picklistRepository = new PicklistRepository(databaseContext, _configuration))
                using (var _categoryRepository = new BpmCategoryRepository(databaseContext, _configuration))
                {
                    _moduleRepository.CurrentUser = _picklistRepository.CurrentUser = _categoryRepository.CurrentUser = _currentUser;

                    if (bpmWorkflow.CategoryId != null && bpmWorkflow.CategoryId > 0)
                    {
                        var category = await _categoryRepository.Get((int)bpmWorkflow.CategoryId);

                        bpmWorkflow.Category = category;
                    }

                    if (bpmWorkflowModel.Filters != null && bpmWorkflowModel.Filters.Count > 0)
                    {
                        if (bpmWorkflow.Filters == null)
                            bpmWorkflow.Filters = new List<BpmRecordFilter>();

                        var module = await _moduleRepository.GetById(bpmWorkflowModel.ModuleId);
                        var picklistItemIds = new List<int>();

                        foreach (var filter in bpmWorkflowModel.Filters)
                        {
                            if (filter.Operator != Operator.Empty && filter.Operator != Operator.NotEmpty)
                            {
                                var field = module.Fields.Single(q => q.Name == filter.Field);

                                if (field.DataType == DataType.Picklist)
                                {
                                    picklistItemIds.Add(int.Parse(filter.Value.ToString()));
                                }
                                else if (field.DataType == DataType.Multiselect)
                                {
                                    var values = filter.Value.ToString().Split(',');

                                    foreach (var item in values)
                                    {
                                        picklistItemIds.Add(int.Parse(item));
                                    }
                                }
                            }
                        }

                        ICollection<PicklistItem> picklistItems = null;

                        if (picklistItemIds.Count > 0)
                            picklistItems = await _picklistRepository.FindItems(picklistItemIds);

                        foreach (var filter in bpmWorkflowModel.Filters)
                        {
                            var field = module.Fields.Single(x => x.Name == filter.Field);
                            var value = filter.Value.ToString();

                            if (filter.Operator != Operator.Empty && filter.Operator != Operator.NotEmpty)
                            {
                                if (field.DataType == DataType.Picklist)
                                {
                                    var picklistItem = picklistItems.Single(x => x.Id == int.Parse(filter.Value.ToString()));
                                    value = tenantLanguage == "tr" ? picklistItem.LabelTr : picklistItem.LabelEn;
                                }
                                else if (field.DataType == DataType.Multiselect)
                                {
                                    var picklistLabels = new List<string>();

                                    var values = filter.Value.ToString().Split(',');

                                    foreach (var val in values)
                                    {
                                        var picklistItem = picklistItems.Single(x => x.Id == int.Parse(val));
                                        picklistLabels.Add(tenantLanguage == "tr" ? picklistItem.LabelTr : picklistItem.LabelEn);
                                    }

                                    value = string.Join("|", picklistLabels);
                                }
                            }

                            var recordFilter = new BpmRecordFilter
                            {
                                Field = filter.Field,
                                Operator = filter.Operator,
                                Value = value,
                                No = filter.No
                            };

                            bpmWorkflow.Filters.Add(recordFilter);
                        }
                    }
                }
            }


        }

        public async Task Run(OperationType operationType, JObject record, Module module, UserItem appUser, Warehouse warehouse, JObject previousRecord)
        {
            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                //Set warehouse database name
                warehouse.DatabaseName = appUser.WarehouseDatabaseName;

                var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                var platformDatabaseContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
                using (var _BpmWorkflowRepository = new BpmRepository(databaseContext, _configuration))
                using (var _moduleRepository = new ModuleRepository(databaseContext, _configuration))
                using (var _recordRepository = new RecordRepository(databaseContext, _configuration))
                using (var _userRepository = new UserRepository(databaseContext, _configuration))
                using (var _picklistRepository = new PicklistRepository(databaseContext, _configuration))
                {
                    _userRepository.CurrentUser = _picklistRepository.CurrentUser = _BpmWorkflowRepository.CurrentUser = _moduleRepository.CurrentUser = _recordRepository.CurrentUser = _currentUser;
                    var bpmWorkflows = await _BpmWorkflowRepository.GetByModuleId(module.Id);
                    bpmWorkflows = bpmWorkflows.Where(q => q.OperationsArray.Contains(operationType.ToString())).ToList();
                    var culture = CultureInfo.CreateSpecificCulture(appUser.Culture);

                    if (bpmWorkflows.Count < 1)
                        return;

                    var lookupModuleNames = new List<string>();
                    ICollection<Module> lookupModules = null;

                    foreach (var workflow in bpmWorkflows)
                    {
                        if (!string.IsNullOrEmpty(workflow.ChangedFields) && operationType != OperationType.insert)
                        {
                            bool isFieldExist = false;
                            // var relatedField = module.Fields.FirstOrDefault(q => workflow.ChangedFieldsArray.Contains(q.Name));

                            foreach (var property in record)
                            {
                                if (workflow.ChangedFieldsArray.Contains(property.Key))
                                    isFieldExist = true;
                            }

                            if (!isFieldExist /*|| record[workflow.ChangedFields] == previousRecord[workflow.ChangedFields]*/)
                                continue;
                        }

                        foreach (var field in module.Fields)
                        {
                            if (!field.Deleted && field.DataType == DataType.Lookup && field.LookupType != "users" && field.LookupType != "relation" && !lookupModuleNames.Contains(field.LookupType))
                                lookupModuleNames.Add(field.LookupType);
                        }

                        if (lookupModuleNames.Count > 0)
                            lookupModules = await _moduleRepository.GetByNamesBasic(lookupModuleNames);
                        else
                            lookupModules = new List<Module>();

                        lookupModules.Add(Model.Helpers.ModuleHelper.GetFakeUserModule());
                        record = _recordRepository.GetById(module, (int)record["id"], false, lookupModules, true);

                        bool hasProcessFilter = true;
                        if (workflow.ProcessFilter != WorkflowProcessFilter.None)
                        {
                            if (!record["process_id"].IsNullOrEmpty() || (record["process_id"].IsNullOrEmpty() && workflow.ProcessFilter == WorkflowProcessFilter.All))
                            {
                                switch (workflow.ProcessFilter)
                                {
                                    case WorkflowProcessFilter.All:
                                        break;
                                    case WorkflowProcessFilter.Pending:
                                        if ((int)record["process_status"] != 1)
                                            hasProcessFilter = false;
                                        break;
                                    case WorkflowProcessFilter.Approved:
                                        if ((int)record["process_status"] != 2)
                                            hasProcessFilter = false;
                                        break;
                                    case WorkflowProcessFilter.Rejected:
                                        if ((int)record["process_status"] != 3)
                                            hasProcessFilter = false;
                                        break;
                                }
                            }
                            else return;
                        }

                        if (!hasProcessFilter)
                            continue;

                        if ((workflow.Frequency == WorkflowFrequency.NotSet || workflow.Frequency == WorkflowFrequency.OneTime) && operationType != OperationType.delete)
                        {
                            var haslog = await _BpmWorkflowRepository.HasLog(workflow.Id, module.Id, (int)record["id"]);

                            if (haslog)
                                continue;
                        }

                        if (workflow.Filters != null && workflow.Filters.Count > 0)
                        {
                            var filters = workflow.Filters;
                            var mismatchedCount = 0;

                            foreach (var filter in filters)
                            {
                                var filterField = module.Fields.Where(q => q.Name == filter.Field).FirstOrDefault();
                                var filterFieldStr = filter.Field;

                                if (filterField.DataType == DataType.Lookup && !filter.Field.EndsWith(".id")) ;
                                filterFieldStr = filter.Field + ".id";

                                if (filterField == null || record[filterFieldStr] == null)
                                {
                                    mismatchedCount++;
                                    continue;
                                }

                                var filterOperator = filter.Operator;
                                var fieldValueString = record[filterFieldStr].ToString();
                                var filterValueString = filter.Value;
                                double fieldValueNumber;
                                double filterValueNumber;
                                double.TryParse(filterValueString, out fieldValueNumber);
                                double.TryParse(filterValueString, out filterValueNumber);
                                bool fieldValueBoolen;
                                bool filterValueBoolen;
                                var fieldValueBoolenParsed = bool.TryParse(fieldValueString, out fieldValueBoolen);
                                var filterValueBoolenParsed = bool.TryParse(filterValueString, out filterValueBoolen);

                                if (filterField.DataType == DataType.Lookup && filterField.LookupType == "users" && filterValueNumber == 0)
                                    filterValueNumber = appUser.Id;

                                switch (filterOperator)
                                {
                                    case Operator.Is:
                                        if (fieldValueString.Trim().ToLower(culture) != filterValueString.Trim().ToLower(culture))
                                            mismatchedCount++;
                                        break;
                                    case Operator.IsNot:
                                        if (fieldValueString.Trim().ToLower(culture) == filterValueString.Trim().ToLower(culture))
                                            mismatchedCount++;
                                        break;
                                    case Operator.Equals:
                                        if (fieldValueBoolenParsed && filterValueBoolenParsed)
                                        {
                                            if (fieldValueBoolen != filterValueBoolen)
                                                mismatchedCount++;
                                        }
                                        else
                                        {
                                            if (fieldValueNumber != filterValueNumber)
                                                mismatchedCount++;
                                        }
                                        break;
                                    case Operator.NotEqual:
                                        if (fieldValueBoolenParsed && filterValueBoolenParsed)
                                        {
                                            if (fieldValueBoolen == filterValueBoolen)
                                                mismatchedCount++;
                                        }
                                        else
                                        {
                                            if (fieldValueNumber == filterValueNumber)
                                                mismatchedCount++;
                                        }
                                        break;
                                    case Operator.Contains:
                                        if (!(fieldValueString.Contains("|") || filterValueString.Contains("|")))
                                        {
                                            if (!fieldValueString.Trim().ToLower(culture).Contains(filterValueString.Trim().ToLower(culture)))
                                                mismatchedCount++;
                                        }
                                        else
                                        {
                                            var fieldValueStringArray = fieldValueString.Split('|');
                                            var filterValueStringArray = filterValueString.Split('|');

                                            foreach (var filterValueStr in filterValueStringArray)
                                            {
                                                if (!fieldValueStringArray.Contains(filterValueStr))
                                                    mismatchedCount++;
                                            }
                                        }
                                        break;
                                    case Operator.NotContain:
                                        if (!(fieldValueString.Contains("|") || filterValueString.Contains("|")))
                                        {
                                            if (fieldValueString.Trim().ToLower(culture).Contains(filterValueString.Trim().ToLower(culture)))
                                                mismatchedCount++;
                                        }
                                        else
                                        {
                                            var fieldValueStringArray = fieldValueString.Split('|');
                                            var filterValueStringArray = filterValueString.Split('|');

                                            foreach (var filterValueStr in filterValueStringArray)
                                            {
                                                if (fieldValueStringArray.Contains(filterValueStr))
                                                    mismatchedCount++;
                                            }
                                        }
                                        break;
                                    case Operator.StartsWith:
                                        if (!fieldValueString.Trim().ToLower(culture).StartsWith(filterValueString.Trim().ToLower(culture)))
                                            mismatchedCount++;
                                        break;
                                    case Operator.EndsWith:
                                        if (!fieldValueString.Trim().ToLower(culture).EndsWith(filterValueString.Trim().ToLower(culture)))
                                            mismatchedCount++;
                                        break;
                                    case Operator.Empty:
                                        if (!string.IsNullOrWhiteSpace(fieldValueString))
                                            mismatchedCount++;
                                        break;
                                    case Operator.NotEmpty:
                                        if (string.IsNullOrWhiteSpace(fieldValueString))
                                            mismatchedCount++;
                                        break;
                                    case Operator.Greater:
                                        if (filterField.DataType == DataType.Date)
                                        {
                                            if (!record[filter.Field].IsNullOrEmpty())
                                            {
                                                DateTime filterDate = Convert.ToDateTime(filter.Value);
                                                DateTime recordDate = Convert.ToDateTime(record[filter.Field]);

                                                if (recordDate <= filterDate)
                                                {
                                                    mismatchedCount++;
                                                }
                                            }
                                            else
                                            {
                                                mismatchedCount++;
                                            }
                                        }
                                        else if (fieldValueNumber <= filterValueNumber)
                                        {
                                            mismatchedCount++;
                                        }
                                        break;
                                    case Operator.GreaterEqual:
                                        if (filterField.DataType == DataType.Date)
                                        {
                                            if (!record[filter.Field].IsNullOrEmpty())
                                            {
                                                DateTime filterDate = Convert.ToDateTime(filter.Value);
                                                DateTime recordDate = Convert.ToDateTime(record[filter.Field]);

                                                if (recordDate < filterDate)
                                                {
                                                    mismatchedCount++;
                                                }
                                            }
                                            else
                                            {
                                                mismatchedCount++;
                                            }
                                        }
                                        else if (fieldValueNumber < filterValueNumber)
                                        {
                                            mismatchedCount++;
                                        }

                                        break;
                                    case Operator.Less:
                                        if (filterField.DataType == DataType.Date)
                                        {
                                            if (!record[filter.Field].IsNullOrEmpty())
                                            {
                                                DateTime filterDate = Convert.ToDateTime(filter.Value);
                                                DateTime recordDate = Convert.ToDateTime(record[filter.Field]);

                                                if (recordDate >= filterDate)
                                                {
                                                    mismatchedCount++;
                                                }
                                            }
                                            else
                                            {
                                                mismatchedCount++;
                                            }
                                        }
                                        else if (fieldValueNumber >= filterValueNumber)
                                        {
                                            mismatchedCount++;
                                        }
                                        break;
                                    case Operator.LessEqual:
                                        if (filterField.DataType == DataType.Date)
                                        {
                                            if (!record[filter.Field].IsNullOrEmpty())
                                            {
                                                DateTime filterDate = Convert.ToDateTime(filter.Value);
                                                DateTime recordDate = Convert.ToDateTime(record[filter.Field]);

                                                if (recordDate >= filterDate)
                                                {
                                                    mismatchedCount++;
                                                }
                                            }
                                            else
                                            {
                                                mismatchedCount++;
                                            }
                                        }
                                        else if (fieldValueNumber > filterValueNumber)
                                        {
                                            mismatchedCount++;
                                        }

                                        break;
                                }

                            }

                            if (mismatchedCount > 0)
                                continue;
                        }

                        var workflowLog = new BpmWorkflowLog
                        {
                            WorkflowId = workflow.Id,
                            ModuleId = module.Id,
                            RecordId = (int)record["id"]
                        };

                        try
                        {
                            var resultCreateLog = await _BpmWorkflowRepository.CreateLog(workflowLog);

                            //if (resultCreateLog < 1)
                            //    ErrorLog.GetDefault(null).Log(new Error(new Exception("WorkflowLog cannot be created! Object: " + workflowLog.ToJsonString())));
                        }
                        catch (Exception ex)
                        {
                            //ErrorLog.GetDefault(null).Log(new Error(ex));
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  Created to generate Reference data to know which user Tenants to BpmWorkflows
        /// </summary>
        /// <param name="appUser"></param>
        /// <returns></returns>
        public string ReferenceCreateToForBpmHost(UserItem appUser)
        {
            //var reference = appUser.TenantId + "|" + appUser.Id + "|" + appUser.Language;
            var lang = string.IsNullOrEmpty(appUser.Language) ? appUser.TenantLanguage : appUser.Language;

            var tempReference = new Model.Common.Bpm.Reference
            {
                Id = appUser.Id,
                Culture = appUser.Culture,
                Language = lang,
                TenantId = appUser.TenantId,
                TimeZone = appUser.TimeZone
            };
            var reference = JObject.FromObject(tempReference);

            return reference.ToJsonString();
        }
    }

}