﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Common.Role;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.App.Helpers
{
    public interface ICalculationHelper
    {
        Task Calculate(int recordId, Module module, UserItem appUser, Warehouse warehouse, OperationType operationType, BeforeCreateUpdate BeforeCreateUpdate, AfterUpdate AfterUpdate, GetAllFieldsForFindRequest GetAllFieldsForFindRequest, JObject currentRecord = null);
        Task<bool> YillikIzinHesaplama(int userId, int izinTuruId, Warehouse warehouse, int tenantId = 0, bool manuelEkIzin = false);
        Task<bool> DeleteAnnualLeave(int userId, int izinTuruId, JObject record);
        Task<bool> CalculateTimesheet(JArray timesheetItemsRecords, UserItem appUser, Module timesheetItemModule, Module timesheetModule, Warehouse warehouse);
    }

    public class CalculationHelper : ICalculationHelper
    {
        private IServiceScopeFactory _serviceScopeFactory;
        private IHttpContextAccessor _context;
        private IConfiguration _configuration;
        private CurrentUser _currentUser;

        public CalculationHelper(IHttpContextAccessor context, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
            _currentUser = UserHelper.GetCurrentUser(_context, configuration);
        }

        public CalculationHelper(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, CurrentUser currentUser)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;

            _currentUser = currentUser;
        }

        public async Task Calculate(int recordId, Module module, UserItem appUser, Warehouse warehouse, OperationType operationType, BeforeCreateUpdate BeforeCreateUpdate, AfterUpdate AfterUpdate, GetAllFieldsForFindRequest GetAllFieldsForFindRequest, JObject currentRecord)
        {
            try
            {
                using (var _scope = _serviceScopeFactory.CreateScope())
                {
                    var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                    warehouse.DatabaseName = appUser.WarehouseDatabaseName;

                    using (var moduleRepository = new ModuleRepository(databaseContext, _configuration))
                    using (var picklistRepository = new PicklistRepository(databaseContext, _configuration))
                    using (var settingRepository = new SettingRepository(databaseContext, _configuration))
                    using (var userRepository = new UserRepository(databaseContext, _configuration))
                    using (var profileRepository = new ProfileRepository(databaseContext, _configuration, warehouse))
                    using (var recordRepository = new RecordRepository(databaseContext, warehouse, _configuration))
                    using (var tagRepository = new TagRepository(databaseContext, _configuration))
                    using (var roleRepository = new RoleRepository(databaseContext, warehouse, _configuration))
                    using (var templateRepostory = new TemplateRepository(databaseContext, _configuration))
                    using (var userGroupRepository = new UserGroupRepository(databaseContext, _configuration))
                    {
                        moduleRepository.UserId = appUser.TenantId;
                        picklistRepository.UserId = appUser.TenantId;
                        settingRepository.UserId = appUser.TenantId;
                        userRepository.UserId = appUser.TenantId;
                        profileRepository.UserId = appUser.TenantId;
                        recordRepository.UserId = appUser.TenantId;
                        tagRepository.UserId = appUser.TenantId;
                        roleRepository.UserId = appUser.TenantId;
                        templateRepostory.UserId = appUser.TenantId;
                        userGroupRepository.UserId = appUser.TenantId;

                        moduleRepository.CurrentUser = _currentUser;
                        picklistRepository.CurrentUser = _currentUser;
                        settingRepository.CurrentUser = _currentUser;
                        userRepository.CurrentUser = _currentUser;
                        profileRepository.CurrentUser = _currentUser;
                        recordRepository.CurrentUser = _currentUser;
                        tagRepository.CurrentUser = _currentUser;
                        roleRepository.CurrentUser = _currentUser;
                        templateRepostory.CurrentUser = _currentUser;
                        userGroupRepository.CurrentUser = _currentUser;

                        var record = await recordRepository.GetById(module, recordId, true, null, true);
                        var isBranch = await settingRepository.GetByKeyAsync("branch");
                        var isEmployee = await settingRepository.GetByKeyAsync("employee");
                        var title = await settingRepository.GetByKeyAsync("title");
                        /*Branch yapısında modül üzerindeki bazı fieldler dinamik hale getirildi*/
                        var newEpostaFieldName = await settingRepository.GetByKeyAsync("e_posta");
                        int calisanUserId = 0;

                        module = await moduleRepository.GetByIdFullModule(module.Id);

                        if (operationType == OperationType.insert || operationType == OperationType.update)
                        {
                            /*
                             * Lookup field dependency mapping
                             * Ex: Set owner to calisan field.
                             */
                            foreach (var dependency in module.Dependencies)
                            {
                                if (dependency.DependencyType == Model.Enums.DependencyType.LookupField &&
                                    dependency.FieldMapParent != null && dependency.FieldMapChild != null &&
                                    !record[dependency.ParentField].IsNullOrEmpty() && record[dependency.ChildField].IsNullOrEmpty())
                                {
                                    var childField = module.Fields.Where(x => x.Name == dependency.ChildField).FirstOrDefault();
                                    var parentField = module.Fields.Where(x => x.Name == dependency.ParentField).FirstOrDefault();

                                    /*var parentModule = await moduleRepository.GetByNameAsync(parentField.LookupType);
                                    var childModule = await moduleRepository.GetByNameAsync(childField.LookupType);*/

                                    string parentRecordData;

                                    if (parentField.LookupType != "users")
                                    {
                                        var parentRecordRequest = new FindRequest
                                        {
                                            Fields = new List<string> { "id", dependency.FieldMapParent },
                                            Filters = new List<Filter>
                                            {
                                                new Filter { Field = "id", Operator = Operator.Is, Value = (int)record[dependency.ParentField], No = 1 }
                                            },
                                            Limit = 1
                                        };

                                        var parentRecord = (JObject)(await recordRepository.Find(parentField.LookupType, parentRecordRequest, false, false)).FirstOrDefault();
                                        parentRecordData = parentRecord[dependency.ParentField].ToString();
                                    }
                                    else
                                    {
                                        var parentRecord = userRepository.GetById((int)record[dependency.ParentField]);
                                        var parentRecordJson = JObject.Parse(JsonConvert.SerializeObject(parentRecord, Formatting.Indented,
                                            new JsonSerializerSettings
                                            {
                                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                                                ContractResolver = new DefaultContractResolver
                                                {
                                                    NamingStrategy = new SnakeCaseNamingStrategy()
                                                },
                                                Formatting = Formatting.Indented
                                            }));

                                        parentRecordData = parentRecordJson[dependency.FieldMapParent].ToString();
                                    }

                                    var childRecordRequest = new FindRequest
                                    {
                                        Fields = new List<string> { "id" },
                                        Filters = new List<Filter>
                                        {
                                            new Filter { Field = dependency.FieldMapChild, Operator = Operator.Is, Value = parentRecordData, No = 1 }
                                        },
                                        Limit = 1
                                    };

                                    var childRecord = (JObject)(await recordRepository.Find("calisanlar", childRecordRequest, false, false)).FirstOrDefault();
                                    if (childRecord != null)
                                    {
                                        var recordObj = new JObject
                                        {
                                            ["id"] = recordId,
                                            [dependency.ChildField] = childRecord["id"]
                                        };
                                        await recordRepository.Update(recordObj, module, isUtc: false);
                                    }
                                }
                            }
                        }

                        switch (module.Name)
                        {
                            case "branches":
                                if (isBranch != null && isBranch.Value == "t")
                                {
                                    if (!record["parent_branch"].IsNullOrEmpty())
                                    {
                                        var parentBranch = await recordRepository.GetById(module, (int)record["parent_branch"]);

                                        if (operationType == OperationType.insert)
                                        {
                                            /*
                                             * Yeni bir şube eklendiğinde role ağacına da bu şubeyi ekliyoruz.
                                             * Oluşan yeni rolün id sinin branch modulünde ki branch lookup alanına setliyoruz.
                                             */

                                            var branchId = await roleRepository.CreateAsync(new Role()
                                            {
                                                LabelEn = record["name"].ToString(),
                                                LabelTr = record["name"].ToString(),
                                                DescriptionEn = null,
                                                DescriptionTr = null,
                                                Master = false,
                                                OwnersList = new List<string>(),
                                                ReportsToId = (int)parentBranch["branch"],
                                                ShareData = false
                                            }, appUser.TenantLanguage);

                                            record["branch"] = branchId;

                                            await recordRepository.Update(record, module, false, false);
                                        }
                                        else if (operationType == OperationType.update)
                                        {
                                            /*
                                             * Şube güncellendiğinde eğer bağlı olduğu şube değişmişse role ağacın da da bu değişikliği uyguluyoruz.
                                             */
                                            if ((!record["parent_branch"].IsNullOrEmpty() && currentRecord["parent_branch"].IsNullOrEmpty()) || ((int)record["parent_branch"] != (int)currentRecord["parent_branch"]))
                                            {
                                                var role = await roleRepository.GetByIdAsync((int)record["branch"]);
                                                var roleDTO = new RoleDTO()
                                                {
                                                    Id = role.Id,
                                                    LabelEn = role.LabelEn,
                                                    LabelTr = role.LabelTr,
                                                    DescriptionEn = role.DescriptionEn,
                                                    DescriptionTr = role.DescriptionTr,
                                                    ShareData = role.ShareData,
                                                    ReportsTo = (int)parentBranch["branch"]
                                                };

                                                await roleRepository.UpdateAsync(role, roleDTO, appUser.TenantLanguage);
                                            }
                                        }

                                        if (!record["authorities"].IsNullOrEmpty())
                                        {
                                            var role = await roleRepository.GetByIdAsync((int)record["branch"]);
                                            var owners = new List<string>();

                                            var roleToUpdate = new RoleDTO()
                                            {
                                                Id = role.Id,
                                                LabelEn = role.LabelEn,
                                                LabelTr = role.LabelTr,
                                                DescriptionEn = role.DescriptionEn,
                                                DescriptionTr = role.DescriptionTr,
                                                ShareData = role.ShareData,
                                                ReportsTo = role.ReportsToId
                                            };

                                            foreach (var directive in record["authorities"])
                                            {
                                                owners.Add(directive.ToString());
                                                var user = userRepository.GetById((int)directive);
                                                role.Users.Add(user);
                                            }

                                            roleToUpdate.Owners = owners;

                                            if (!string.IsNullOrEmpty(role.Owners))
                                                roleToUpdate.ShareData = true;

                                            await roleRepository.UpdateAsync(role, roleToUpdate, appUser.TenantLanguage);
                                        }
                                    }
                                }

                                break;
                            case "sirket_ici_kariyer":
                                var calisanlarModule = await moduleRepository.GetByName("calisanlar");
                                var lookupModules = new List<Module>();
                                lookupModules.Add(calisanlarModule);
                                lookupModules.Add(Model.Helpers.ModuleHelper.GetFakeUserModule());

                                record = await recordRepository.GetById(module, recordId, true, lookupModules, true);
                                var calisan = await recordRepository.GetById(calisanlarModule, (int)record["personel.id"], true, lookupModules, true);

                                var calisanUpdate = new JObject();
                                calisanUpdate["id"] = calisan["id"];

                                if (!record["gorev_yeri"].IsNullOrEmpty())
                                    calisanUpdate["lokasyon"] = record["gorev_yeri"];

                                if (!record["bolum"].IsNullOrEmpty())
                                    calisanUpdate["departman"] = record["bolum"];

                                if (!record["is_alani"].IsNullOrEmpty())
                                    calisanUpdate["is_alani"] = record["is_alani"];

                                if (!record["unvan"].IsNullOrEmpty())
                                    calisanUpdate["unvan"] = record["unvan"];

                                if (!record["ingilizce_unvan"].IsNullOrEmpty())
                                    calisanUpdate["ingilizce_unvan"] = record["ingilizce_unvan"];

                                if (!record["1yoneticisi.id"].IsNullOrEmpty())
                                    calisanUpdate["yoneticisi"] = record["1yoneticisi.id"];

                                if (!record["2_yoneticisi.id"].IsNullOrEmpty())
                                    calisanUpdate["2_yonetici"] = record["2_yoneticisi.id"];

                                if (!record["direktor.id"].IsNullOrEmpty())
                                    calisanUpdate["direktor"] = record["direktor.id"];

                                if (!record["gmy.id"].IsNullOrEmpty())
                                    calisanUpdate["gmy"] = record["gmy.id"];

                                if (!record["is_alani_yoneticisi_2.id"].IsNullOrEmpty())
                                    calisanUpdate["departman_yoneticisi"] = record["is_alani_yoneticisi_2.id"];

                                await recordRepository.Update(calisanUpdate, calisanlarModule, isUtc: false);

                                AfterUpdate(calisanlarModule, calisanUpdate, calisan, appUser, warehouse, false);

                                string mailSubject;
                                string mailBody;
                                var mailTemplate = await templateRepostory.GetById(52);//Organizasyonel değişiklik bildirimi
                                mailSubject = mailTemplate.Subject;
                                mailBody = mailTemplate.Content;

                                var ccList = new List<string>();
                                ccList.Add("hr@etiya.com");

                                if (!record["1yoneticisi.e_posta"].IsNullOrEmpty() && !ccList.Contains((string)record["1yoneticisi.e_posta"]))
                                    ccList.Add((string)record["1yoneticisi.e_posta"]);

                                if (!record["2_yoneticisi.e_posta"].IsNullOrEmpty() && !ccList.Contains((string)record["2_yoneticisi.e_posta"]))
                                    ccList.Add((string)record["2_yoneticisi.e_posta"]);

                                if (!record["direktor.e_posta"].IsNullOrEmpty() && !ccList.Contains((string)record["direktor.e_posta"]))
                                    ccList.Add((string)record["direktor.e_posta"]);

                                if (!record["gmy.e_posta"].IsNullOrEmpty() && !ccList.Contains((string)record["gmy.e_posta"]))
                                    ccList.Add((string)record["gmy.e_posta"]);

                                if (!calisan["yoneticisi.e_posta"].IsNullOrEmpty() && !ccList.Contains((string)calisan["yoneticisi.e_posta"]))
                                    ccList.Add((string)calisan["yoneticisi.e_posta"]);

                                if (!calisan["2_yonetici.e_posta"].IsNullOrEmpty() && !ccList.Contains((string)calisan["2_yonetici.e_posta"]))
                                    ccList.Add((string)calisan["2_yonetici.e_posta"]);

                                if (!calisan["direktor.e_posta"].IsNullOrEmpty() && !ccList.Contains((string)calisan["direktor.e_posta"]))
                                    ccList.Add((string)calisan["direktor.e_posta"]);

                                if (!calisan["gmy.e_posta"].IsNullOrEmpty() && !ccList.Contains((string)calisan["gmy.e_posta"]))
                                    ccList.Add((string)calisan["gmy.e_posta"]);


                                var externalEmail = new Email(mailSubject, mailBody, _configuration, _serviceScopeFactory);
                                externalEmail.AddRecipient((string)calisan["e_posta"]);
                                await externalEmail.AddToQueue(appUser.TenantId, appUser: appUser, cc: string.Join(",", ccList), moduleId: module.Id, recordId: (int)record["id"], addRecordSummary: false);
                                break;
                            case "ise_alim_talepleri":
                                var iseAlimTalebiModule = await moduleRepository.GetByName("ise_alim_talepleri");
                                var userRequest = new FindRequest { Fields = new List<string> { "email" }, Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)record["owner"], No = 1 } }, Limit = 1 };
                                var userData = await recordRepository.Find("users", userRequest, false, false);
                                var calisanRequest = new FindRequest();
                                var calisanData = new JArray();
                                var calisanObj = new JObject();
                                if (!userData.IsNullOrEmpty())
                                {
                                    calisanRequest = new FindRequest
                                    {
                                        Fields = new List<string> { "yoneticisi.calisanlar.e_posta" },
                                        Filters = new List<Filter>
                                        {
                                            new Filter { Field = "e_posta", Operator = Operator.Is, Value = userData.First()["email"], No = 1 },
                                            new Filter { Field = "calisma_durumu", Operator = Operator.Is, Value = "Aktif", No = 2 }
                                        },
                                        Limit = 1
                                    };

                                    calisanData = await recordRepository.Find("calisanlar", calisanRequest, false, false);
                                    calisanObj = (JObject)calisanData.First();
                                    var moduleCalisan = await moduleRepository.GetByName("calisanlar");
                                    var departmanPicklist = moduleCalisan.Fields.Single(x => x.Name == "departman");
                                    var departmanPicklistItem = await picklistRepository.FindItemByLabel(departmanPicklist.PicklistId.Value, (string)record["bolum"], appUser.TenantLanguage);

                                    if (!calisanObj["yoneticisi.calisanlar.e_posta"].IsNullOrEmpty())
                                    {
                                        record["custom_approver"] = calisanObj["yoneticisi.calisanlar.e_posta"];
                                        int j = 1;
                                        for (var i = 2; i < 6; i++)
                                        {
                                            if (!calisanObj["yoneticisi.calisanlar.e_posta"].IsNullOrEmpty())
                                            {
                                                j++;

                                                calisanRequest = new FindRequest
                                                {
                                                    Fields = new List<string> { "yoneticisi.calisanlar.e_posta" },
                                                    Filters = new List<Filter>
                                                    {
                                                        new Filter { Field = "e_posta", Operator = Operator.Is, Value = (string)calisanObj["yoneticisi.calisanlar.e_posta"], No = 1 },
                                                        new Filter { Field = "calisma_durumu", Operator = Operator.Is, Value = "Aktif", No = 2 }
                                                    },
                                                    Limit = 1
                                                };

                                                calisanData = await recordRepository.Find("calisanlar", calisanRequest, false, false);
                                                calisanObj = (JObject)calisanData.First();

                                                if (!calisanObj["yoneticisi.calisanlar.e_posta"].IsNullOrEmpty())
                                                    record["custom_approver_" + i] = calisanObj["yoneticisi.calisanlar.e_posta"];
                                                else
                                                    j--;
                                            }
                                        }

                                        if (departmanPicklistItem != null && departmanPicklistItem?.Value != "ceo_approve")
                                            record["custom_approver_" + j] = null;
                                    }
                                }

                                await recordRepository.Update(record, iseAlimTalebiModule, isUtc: false);
                                break;
                            case "country_travel_tracking":
                                var countryTravelTracking = await moduleRepository.GetByName("country_travel_tracking");
                                int approverId = 3488;
                                if (!record["shared_users_edit"].IsNullOrEmpty())
                                {
                                    var sharedUsers = (JArray)record["shared_users_edit"];
                                    foreach (var item in sharedUsers.ToList())
                                    {
                                        if ((int)item != approverId)
                                        {
                                            sharedUsers.Add(approverId);
                                            record["shared_users_edit"] = sharedUsers;
                                        }
                                    }
                                }
                                else
                                {
                                    var sharedUsers = new JArray();
                                    sharedUsers.Add(approverId);
                                    record["shared_users_edit"] = sharedUsers;
                                }

                                await recordRepository.Update(record, countryTravelTracking, isUtc: false);
                                break;
                            case "timesheet_item":
                                var timesheetModule = await moduleRepository.GetByName("timesheet");
                                var findRequestFields = await GetAllFieldsForFindRequest("timesheet_item");
                                var findRequestTimesheetItems = new FindRequest { Fields = findRequestFields, Filters = new List<Filter> { new Filter { Field = "related_timesheet", Operator = Operator.Equals, Value = (int)record["related_timesheet"], No = 1 } }, Limit = 9999 };
                                var timesheetItemsRecords = await recordRepository.Find(module.Name, findRequestTimesheetItems, false, false);
                                var statusField = timesheetModule.Fields.Single(x => x.Name == "status");
                                var statusPicklist = await picklistRepository.GetById(statusField.PicklistId.Value);
                                var approvedStatusPicklistItem = statusPicklist.Items.Single(x => x.Value == "approved_second");
                                var approvedStatusPicklistItemLabel = appUser.TenantLanguage == "tr" ? approvedStatusPicklistItem.LabelTr : approvedStatusPicklistItem.LabelEn;
                                var timesheetRecord = await recordRepository.GetById(timesheetModule, (int)record["related_timesheet"]);
                                var approved = true;

                                if ((string)timesheetRecord["status"] == approvedStatusPicklistItemLabel)
                                    return;

                                if (timesheetItemsRecords.Count == 0)
                                    return;

                                if (!timesheetItemsRecords.IsNullOrEmpty() && timesheetItemsRecords.Count > 0)
                                {
                                    foreach (var timesheetItemsRecord in timesheetItemsRecords)
                                    {
                                        if (!approved)
                                            continue;

                                        var statusRecord = timesheetItemsRecord["status"].ToString();
                                        var statusPicklistItem = statusPicklist.Items.Single(x => x.LabelEn == statusRecord);

                                        if (statusPicklistItem.Value != "approved_second")
                                        {
                                            approved = false;
                                            break;
                                        }
                                    }
                                }

                                if (approved)
                                {
                                    var timesheetRecordUpdate = new JObject();
                                    timesheetRecordUpdate["id"] = (int)record["related_timesheet"];
                                    timesheetRecordUpdate["status"] = approvedStatusPicklistItem.Id;
                                    timesheetRecordUpdate["updated_by"] = (int)record["updated_by"];

                                    var modelStateTimesheet = new ModelStateDictionary();
                                    var resultBefore = await BeforeCreateUpdate(timesheetModule, timesheetRecordUpdate, modelStateTimesheet, appUser.TenantLanguage, moduleRepository, picklistRepository, profileRepository, tagRepository, settingRepository, recordRepository);

                                    if (resultBefore != StatusCodes.Status200OK && !modelStateTimesheet.IsValid)
                                    {
                                        ErrorHandler.LogError(new Exception("Timesheet cannot be updated! Object: " + timesheetRecordUpdate + " ModelState: " + modelStateTimesheet.ToJsonString()), "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name + "operation_type:" + operationType + "record_id:" + record["id"].ToString());
                                        return;
                                    }

                                    try
                                    {
                                        var resultUpdate = await recordRepository.Update(timesheetRecordUpdate, timesheetModule, isUtc: false);

                                        if (resultUpdate < 1)
                                        {
                                            ErrorHandler.LogError(new Exception("Timesheet cannot be updated! Object: " + timesheetRecordUpdate), "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name + "operation_type:" + operationType + "record_id:" + record["id"].ToString());
                                            return;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ErrorHandler.LogError(ex, "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name + "operation_type:" + operationType + "record_id:" + record["id"].ToString());
                                        return;
                                    }


                                    var timesheetOwner = userRepository.GetById((int)record["owner"]);
                                    var timesheetInfo = timesheetRecord["year"] + "-" + timesheetRecord["term"];
                                    var timesheetMonth = int.Parse(timesheetRecord["term"].ToString()) - 1;
                                    var body = "<!DOCTYPE html> <html> <head> <meta name=\"viewport\" content=\"width=device-width\"> <meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\"> <title></title> <style type=\"text/css\"> @media only screen and (max-width: 620px) { table[class= body] h1 { font-size: 28px !important; margin-bottom: 10px !important; } table[class=body] p, table[class=body] ul, table[class=body] ol, table[class=body] td, table[class=body] span, table[class=body] a { font-size: 16px !important; } table[class=body] .wrapper, table[class=body] .article { padding: 10px !important; } table[class=body] .content { padding: 0 !important; } table[class=body] .container { padding: 0 !important; width: 100% !important; } table[class=body] .main { border-left-width: 0 !important; border-radius: 0 !important; border-right-width: 0 !important; } table[class=body] .btn table { width: 100% !important; } table[class=body] .btn a { width: 100% !important; } table[class=body] .img-responsive { height: auto !important; max-width: 100% !important; width: auto !important; }} @media all { .ExternalClass { width: 100%; } .ExternalClass, .ExternalClass p, .ExternalClass span, .ExternalClass font, .ExternalClass td, .ExternalClass div { line-height: 100%; } .apple-link a { color: inherit !important; font-family: inherit !important; font-size: inherit !important; font-weight: inherit !important; line-height: inherit !important; text-decoration: none !important; } .btn-primary table td:hover { background-color: #34495e !important; } .btn-primary a:hover { background - color: #34495e !important; border-color: #34495e !important; } } </style> </head> <body class=\"\" style=\"background-color:#f6f6f6;font-family:sans-serif;-webkit-font-smoothing:antialiased;font-size:14px;line-height:1.4;margin:0;padding:0;-ms-text-size-adjust:100%;-webkit-text-size-adjust:100%;\"> <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"body\" style=\"border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;background-color:#f6f6f6;width:100%;\"> <tr> <td style=\"font-family:sans-serif;font-size:14px;vertical-align:top;\">&nbsp;</td> <td class=\"container\" style=\"font-family:sans-serif;font-size:14px;vertical-align:top;display:block;max-width:580px;padding:10px;width:580px;Margin:0 auto !important;\"> <div class=\"content\" style=\"box-sizing:border-box;display:block;Margin:0 auto;max-width:580px;padding:10px;\"> <!-- START CENTERED WHITE CONTAINER --> <table class=\"main\" style=\"border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;background:#fff;border-radius:3px;width:100%;\"> <!-- START MAIN CONTENT AREA --> <tr> <td class=\"wrapper\" style=\"font-family:sans-serif;font-size:14px;vertical-align:top;box-sizing:border-box;padding:20px;\"> <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;width:100%;\"> <tr> <td style=\"font-family:sans-serif;font-size:14px;vertical-align:top;\"> Dear " + timesheetOwner.FullName + ", <br><br>Your timesheet (" + timesheetInfo + ") is approved. <br><br><br><br><table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"btn btn-primary\" style=\"border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;box-sizing:border-box;width:100%;\"> <tbody> <tr> <td align=\"left\" style=\"font-family:sans-serif;font-size:14px;vertical-align:top;padding-bottom:15px;\"> <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;width:100%;width:auto;\"> <tbody> <tr> <td style=\"font-family:sans-serif;font-size:14px;vertical-align:top;background-color:#ffffff;border-radius:5px;text-align:center;background-color:#3498db;\"> <a href=\"https://bee.weglobal.org/#/app/crm/timesheet?month=" + timesheetMonth + "\" target=\"_blank\" style=\"text-decoration:underline;background-color:#ffffff;border:solid 1px #3498db;border-radius:5px;box-sizing:border-box;color:#3498db;cursor:pointer;display:inline-block;font-size:14px;font-weight:bold;margin:0;padding:12px 25px;text-decoration:none;background-color:#3498db;border-color:#3498db;color:#ffffff;\">Go to Your Timesheet</a> </td> </tr> </tbody> </table> </td> </tr> </tbody> </table></td> </tr> </table> </td> </tr> <!-- END MAIN CONTENT AREA --> </table> <!-- START FOOTER --> <div class=\"footer\" style=\"clear:both;padding-top:10px;text-align:center;width:100%;\"> <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;width:100%;\"> <tr> <td class=\"content-block\" style=\"font-family:sans-serif;font-size:14px;vertical-align:top;color:#999999;font-size:12px;text-align:center;\"> <br><span class=\"apple-link\" style=\"color:#999999;font-size:12px;text-align:center;\">Ofisim.com</span> </td> </tr> </table> </div> <!-- END FOOTER --> <!-- END CENTERED WHITE CONTAINER --> </div> </td> <td style=\"font-family:sans-serif;font-size:14px;vertical-align:top;\">&nbsp;</td> </tr> </table> </body> </html>";
                                    var externalEmailTimesheet = new Email("Timesheet (" + timesheetInfo + ") Approved", body, _configuration, _serviceScopeFactory);
                                    externalEmailTimesheet.AddRecipient(timesheetOwner.Email);
                                    externalEmailTimesheet.AddToQueue(appUser: appUser);

                                    await CalculateTimesheet(timesheetItemsRecords, appUser, module, timesheetModule, warehouse);
                                }

                                break;
                            case "human_resources":
                                var findRequestIzinlerCalisanPG = new FindRequest
                                {
                                    Filters = new List<Filter> { new Filter { Field = "yillik_izin", Operator = Operator.Equals, Value = true, No = 1 } },
                                    Limit = 99999,
                                    Offset = 0
                                };

                                var izinlerCalisanPG = (await recordRepository.Find("izin_turleri", findRequestIzinlerCalisanPG, false, false)).First;
                                await YillikIzinHesaplama((int)record["id"], (int)izinlerCalisanPG["id"], warehouse);
                                break;
                            case "calisanlar":

                                calisanUserId = 0;

                                if (!record["kullanici_olustur"].IsNullOrEmpty() && (bool)record["kullanici_olustur"])
                                {
                                    calisanUserId = await ChangeRecordOwner(record["e_posta"].ToString(), record, module, recordRepository, moduleRepository);
                                    if (calisanUserId > 0)
                                        record["owner"] = calisanUserId;
                                }

                                //Yıllık izin ise calculationlar çalıştırılıyor.
                                var findRequestIzinlerCalisan = new FindRequest
                                {
                                    Filters = new List<Filter> { new Filter { Field = "yillik_izin", Operator = Operator.Equals, Value = true, No = 1 } },
                                    Limit = 99999,
                                    Offset = 0
                                };

                                var izinlerCalisan = (await recordRepository.Find("izin_turleri", findRequestIzinlerCalisan, false, false)).First;

                                var rehberModule = await moduleRepository.GetByName("rehber");
                                var calisanModule = await moduleRepository.GetByName("calisanlar");
                                var recordRehber = new JObject();

                                if (operationType == OperationType.update || operationType == OperationType.delete)
                                {
                                    var findRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "calisan_id", Operator = Operator.Equals, Value = (int)record["id"] } } };
                                    var recordsRehber = await recordRepository.Find("rehber", findRequest);

                                    if (recordsRehber.IsNullOrEmpty())
                                    {
                                        if (operationType == OperationType.update)
                                            operationType = OperationType.insert;
                                        else
                                            return;
                                    }
                                    else
                                    {
                                        recordRehber = (JObject)recordsRehber[0];
                                    }
                                }

                                var calismaDurumuField = calisanModule.Fields.First(x => x.Name == "calisma_durumu");
                                var calismaDurumuPicklist = await picklistRepository.GetById(calismaDurumuField.PicklistId.Value);
                                var calismaDurumuPicklistItem = calismaDurumuPicklist.Items.SingleOrDefault(x => x.Value == "active");
                                var calismaDurumu = (string)record["calisma_durumu"];
                                var isActive = !string.IsNullOrEmpty(calismaDurumu) && calismaDurumuPicklistItem != null && calismaDurumu == (appUser.TenantLanguage == "tr" ? calismaDurumuPicklistItem.LabelTr : calismaDurumuPicklistItem.LabelEn);

                                if (operationType != OperationType.delete)
                                {
                                    recordRehber["owner"] = record["owner"];
                                    recordRehber["ad"] = record["ad"];
                                    recordRehber["soyad"] = record["soyad"];
                                    recordRehber["ad_soyad"] = record["ad_soyad"];
                                    recordRehber["cep_telefonu"] = record["cep_telefonu"];
                                    recordRehber["e_posta"] = record["e_posta"];
                                    recordRehber["lokasyon"] = record["lokasyon"];
                                    recordRehber["sube"] = record["sube"];
                                    recordRehber["fotograf"] = record["fotograf"];
                                    recordRehber["departman"] = record["departman"];
                                    recordRehber["unvan"] = record["unvan"];

                                    if (!record["is_telefonu"].IsNullOrEmpty())
                                        recordRehber["is_telefonu"] = record["is_telefonu"];

                                    if (!record["ozel_cep_telefonu"].IsNullOrEmpty())
                                        recordRehber["ozel_cep_telefonu"] = record["ozel_cep_telefonu"];

                                    if (!record["yoneticisi"].IsNullOrEmpty())
                                    {
                                        var findRequestYonetici = new FindRequest { Filters = new List<Filter> { new Filter { Field = "calisan_id", Operator = Operator.Equals, Value = (int)record["yoneticisi"] } } };
                                        var recordsYonetici = await recordRepository.Find("rehber", findRequestYonetici);

                                        if (!recordsYonetici.IsNullOrEmpty())
                                            recordRehber["yoneticisi"] = recordsYonetici[0]["id"];
                                    }

                                    var modelStateRehber = new ModelStateDictionary();
                                    var resultBefore = await BeforeCreateUpdate(rehberModule, recordRehber, modelStateRehber, appUser.TenantLanguage, moduleRepository, picklistRepository, profileRepository, tagRepository, settingRepository, recordRepository, convertPicklists: false);

                                    if (resultBefore != StatusCodes.Status200OK && !modelStateRehber.IsValid)
                                    {
                                        ErrorHandler.LogError(new Exception("Rehber cannot be created or updated! Object: " + recordRehber + " ModelState: " + modelStateRehber.ToJsonString()), "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name + "operation_type:" + operationType + "record_id:" + record["id"].ToString());
                                        return;
                                    }

                                    if (operationType == OperationType.insert && recordRehber["id"].IsNullOrEmpty() && isActive)//create
                                    {
                                        recordRehber["calisan_id"] = record["id"];

                                        try
                                        {
                                            var resultCreate = await recordRepository.Create(recordRehber, rehberModule);

                                            if (resultCreate < 1)
                                                ErrorHandler.LogError(new Exception("Rehber cannot be created! Object: " + recordRehber), "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name + "operation_type:" + operationType + "record_id:" + record["id"].ToString());
                                        }
                                        catch (Exception ex)
                                        {
                                            ErrorHandler.LogError(ex, "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name + "operation_type:" + operationType + "record_id:" + record["id"].ToString());
                                        }
                                    }
                                    else//update
                                    {
                                        if (!isActive)
                                        {
                                            if (!recordRehber["id"].IsNullOrEmpty())
                                            {
                                                try
                                                {
                                                    var resultDelete = await recordRepository.Delete(recordRehber, rehberModule);

                                                    if (resultDelete < 1)
                                                        ErrorHandler.LogError(new Exception("Rehber cannot be deleted! Object: " + recordRehber), "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name + "operation_type:" + operationType + "record_id:" + record["id"].ToString());
                                                }
                                                catch (Exception ex)
                                                {
                                                    ErrorHandler.LogError(ex, "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name + "operation_type:" + operationType + "record_id:" + record["id"].ToString());
                                                }
                                            }
                                        }
                                        else
                                        {
                                            try
                                            {
                                                var resultUpdate = await recordRepository.Update(recordRehber, rehberModule, isUtc: false);

                                                if (resultUpdate < 1)
                                                    ErrorHandler.LogError(new Exception("Rehber cannot be updated! Object: " + recordRehber), "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name + "operation_type:" + operationType + "record_id:" + record["id"].ToString());
                                            }
                                            catch (Exception ex)
                                            {
                                                ErrorHandler.LogError(ex, "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name + "operation_type:" + operationType + "record_id:" + record["id"].ToString());
                                            }
                                        }
                                    }

                                    if (!record["dogum_tarihi"].IsNullOrEmpty())
                                    {
                                        var today = DateTime.Today;
                                        var birthDate = (DateTime)record["dogum_tarihi"];
                                        var age = today.Year - birthDate.Year;

                                        if (birthDate > today.AddYears(-age))
                                            age--;

                                        record["yasi"] = age;
                                    }

                                    var iseBaslamaTarihi2 = calisanModule.Fields.SingleOrDefault(x => x.Name == "ise_baslama_tarihi_2");
                                    if (iseBaslamaTarihi2 != null && !record["ise_baslama_tarihi_2"].IsNullOrEmpty() && record["deneyim_yil"].IsNullOrEmpty())
                                    {
                                        var timespan = DateTime.UtcNow.Subtract((DateTime)record["ise_baslama_tarihi_2"]);
                                        record["deneyim_yil"] = Math.Floor(timespan.TotalDays / 365);

                                        if ((int)record["deneyim_yil"] > 0)
                                        {
                                            record["deneyim_ay"] = Math.Floor(timespan.TotalDays / 30) - ((int)record["deneyim_yil"] * 12);
                                        }
                                        else
                                        {
                                            record["deneyim_ay"] = Math.Floor(timespan.TotalDays / 30);
                                        }

                                        if ((double)record["deneyim_yil"] < 0)
                                            record["deneyim_yil"] = 0;

                                        if ((double)record["deneyim_ay"] < 0)
                                            record["deneyim_ay"] = 0;

                                        var deneyimAyStr = (string)record["deneyim_ay"];

                                        record["toplam_deneyim_firma"] = record["deneyim_yil"] + "." + (deneyimAyStr.Length == 1 ? "0" + deneyimAyStr : deneyimAyStr);
                                        record["toplam_deneyim_firma_yazi"] = record["deneyim_yil"] + " yıl " + deneyimAyStr + " ay";

                                        if (record["onceki_deneyim_yil"].IsNullOrEmpty())
                                            record["onceki_deneyim_yil"] = 0;

                                        if (record["onceki_deneyim_ay"].IsNullOrEmpty())
                                            record["onceki_deneyim_ay"] = 0;

                                        var deneyimYil = (int)record["deneyim_yil"] + (int)record["onceki_deneyim_yil"];
                                        var deneyimAy = (int)record["deneyim_ay"] + (int)record["onceki_deneyim_ay"];

                                        if (deneyimAy > 12)
                                        {
                                            deneyimAy -= 12;
                                            deneyimYil += 1;
                                        }


                                        if (deneyimYil < 0)
                                            deneyimYil = 0;

                                        if (deneyimAy < 0)
                                            deneyimAy = 0;

                                        record["toplam_deneyim"] = deneyimYil + "." + (deneyimAy.ToString().Length == 1 ? "0" + deneyimAy : deneyimAy.ToString());
                                        record["toplam_deneyim_yazi"] = deneyimYil + " yıl " + deneyimAy + " ay";
                                    }

                                    if (!record["dogum_tarihi"].IsNullOrEmpty() || (iseBaslamaTarihi2 != null && !record["ise_baslama_tarihi_2"].IsNullOrEmpty()))
                                        await recordRepository.Update(record, calisanModule, isUtc: false);
                                }
                                else//delete
                                {
                                    try
                                    {
                                        var resultDelete = await recordRepository.Delete(recordRehber, rehberModule);

                                        if (resultDelete < 1)
                                            ErrorHandler.LogError(new Exception("Rehber cannot be deleted! Object: " + recordRehber), "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name + "operation_type:" + operationType + "record_id:" + record["id"].ToString());
                                    }
                                    catch (Exception ex)
                                    {
                                        ErrorHandler.LogError(ex, "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name + "operation_type:" + operationType + "record_id:" + record["id"].ToString());
                                    }
                                }

                                await YillikIzinHesaplama((int)record["id"], (int)izinlerCalisan["id"], warehouse, manuelEkIzin: true);
                                break;
                            case "izinler":
                                if (record["calisan"].IsNullOrEmpty())
                                    return;

                                //Yıllık izin ise calculationlar çalıştırılıyor.
                                var findRequestIzinler = new FindRequest
                                {
                                    Filters = new List<Filter> { new Filter { Field = "yillik_izin", Operator = Operator.Equals, Value = true, No = 1 } },
                                    Limit = 99999,
                                    Offset = 0
                                };

                                var izinTuruModule = await moduleRepository.GetByName("izin_turleri");
                                var izinTuru = await recordRepository.GetById(izinTuruModule, (int)record["izin_turu"], false, profileBasedEnabled: false);
                                var izinler = (await recordRepository.Find("izin_turleri", findRequestIzinler, false)).First;

                                //İzin türüne göre izinler de gün veya saat olduğunu belirtme.
                                if (!izinTuru["saatlik_kullanim_yapilir"].IsNullOrEmpty())
                                {
                                    if (!(bool)izinTuru["saatlik_kullanim_yapilir"])
                                        record["gunsaat"] = "Gün";
                                    else
                                        record["gunsaat"] = "Saat";

                                    var izinlerModule = await moduleRepository.GetByName("izinler");

                                    await recordRepository.Update(record, izinlerModule, isUtc: false);
                                }

                                await YillikIzinHesaplama((int)record["calisan"], (int)izinler["id"], warehouse);
                                break;
                            
                            case var employeModule when isEmployee != null && isEmployee.Value == module.Name:
                                /*
								* Tenant da şube yapısı kullanılıyor mu diye kontrol ediliyor.
								*/
                                calisanUserId = 0;
                                if (isBranch != null && isBranch.Value == "t" && newEpostaFieldName != null)
                                {
                                    calisanUserId = await ChangeRecordOwner(record[newEpostaFieldName.Value].ToString(), record, module, recordRepository, moduleRepository);
                                    if (calisanUserId > 0)
                                        record["owner"] = calisanUserId;

                                    var branchModule = await moduleRepository.GetByName("branches");
                                    var calisanlar = await moduleRepository.GetByName(module.Name);
                                    var branchRecord = new JObject();
                                    var roleId = new int();
                                    var profileId = new int();
                                    var missingSchema = new List<Profile>();
                                    var calisanRecord = await recordRepository.GetById(calisanlar, recordId);

                                    if (!calisanRecord["profile"].IsNullOrEmpty() && !calisanRecord["branch"].IsNullOrEmpty() && title == null)
                                    {
                                        profileId = int.Parse(calisanRecord["profile"].ToString());
                                        branchRecord = await recordRepository.GetById(branchModule, int.Parse(calisanRecord["branch"].ToString()));
                                        roleId = branchRecord != null && !branchRecord["branch"].IsNullOrEmpty() ? (int)branchRecord["branch"] : 0;

                                        List<Profile> profileSchema = new List<Profile>();

                                        /*if (operationType == OperationType.update && (bool)currentRecord["branch_manager"])
                                        {
                                            using (var userRepository = new UserRepository(databaseContext))
                                            {
                                                var user = await userRepository.GetByEmail(record["e_posta"].ToString());
                                                var role = await roleRepository.GetWithCode("branch-" + roleId + "/profile-" + profileId);
                                                await roleRepository.RemoveUserAsync(user.Id, role.Id);
                                            }
                                        }*/

                                        /*
                                        * Eklenen çalışan şube sorumlusu değil ise;
                                        * Çalışan eklerken seçilen profile id si ile profili çekiyoruz.
                                        * Çekilen profil üzerinde ki bilgilerle profil üzerinde ki parent profilleri buluyoruz.
                                        * (Child profiller bu aşamada bizi ilgilendirmiyor sadece parent ları buluyoruz.)
                                        * GetCurrentProfileSchema methoduyla üst profilleri profileSchema objesine dolduruyoruz.
                                        * Profilleri ağacını tutan profileSchema objesini MissingProfileSchema yolluyoruz.
                                        * MissingProfileSchema methodu güncel ağacımız da eksik olan dalları bize dönüyor ve missingSchema objesine atıyoruz.
                                        */
                                        var profile = await profileRepository.GetProfileById(profileId);
                                        await GetCurrentProfileSchema(profile, profileSchema, profileRepository);

                                        missingSchema = await MissingProfileSchema(profileSchema, roleId, roleId, profileRepository, roleRepository, null);
                                    }
                                    else if (title != null && title.Value == "t" && !calisanRecord["sub_branch"].IsNullOrEmpty() && !calisanRecord["branch"].IsNullOrEmpty())
                                    {
                                        var subBranchRecord = await recordRepository.GetById(branchModule, int.Parse(calisanRecord["sub_branch"].ToString()));
                                        //branchRecord = await recordRepository.GetById(branchModule, int.Parse(calisanRecord["branch"].ToString()));
                                        /*Gelen Branch Üst Kırılımlardan Biri olabilir, Bu yüzden mevcut calisanRecord["sub_branch"] üzerinden parent_branches'e erişeceğiz*/
                                        //branchRecord = await recordRepository.GetById(branchModule, (int)subBranchRecord["parent_branch"]);
                                        //branchRecord = (bool)record["branch_manager"] ? branchRecord : subBranchRecord;
                                        branchRecord = subBranchRecord;
                                        roleId = branchRecord != null && !branchRecord["branch"].IsNullOrEmpty() ? (int)branchRecord["branch"] : 0;
                                        missingSchema = await MissingProfileSchema(new List<Profile>() { new Profile() { NameEn = (string)branchRecord["name"], NameTr = (string)branchRecord["name"] } }, roleId, roleId, profileRepository, roleRepository, title);
                                    }
                                    else break;

                                    if (operationType == OperationType.insert)
                                    {
                                        /*
										 * Yeni bir çalışan oluşturulurken.
										 * Çalışan seçilen şubenin sorumlusu ise
										 */
                                        if ((bool)record["branch_manager"] && newEpostaFieldName != null)
                                        {
                                            /*
											 * User ı email yardımıyla çekerek rol unü update ediyoruz.
											 */
                                            var user = await userRepository.GetByEmail(record[newEpostaFieldName.Value].ToString());
                                            await UpdateUserRoleAndProfile(user.Id, profileId, roleId, roleRepository, profileRepository);
                                            await SetAdvanceSharingWithOwners(roleId, (int)branchRecord["id"], branchModule, recordRepository, roleRepository);
                                        }
                                        else
                                        {
                                            if (missingSchema.Count > 0 && newEpostaFieldName != null)
                                            {
                                                var currentUserRoleId = await CreateMissingSchema(missingSchema, roleId, roleRepository, appUser, title);
                                                var user = await userRepository.GetByEmail(record[newEpostaFieldName.Value].ToString());
                                                await UpdateUserRoleAndProfile(user.Id, profileId, (int)currentUserRoleId, roleRepository, profileRepository);
                                                await SetAdvanceSharingWithOwners(roleId, (int)branchRecord["id"], branchModule, recordRepository, roleRepository);
                                            }
                                            else
                                            {
                                                /*
												 * Eğer yeni eklenen çalışan için tüm roller ağaçta zaten mevcut ise
												 * Eklenen çalışanın rolünü direk olarak güncelliyoruz.
												 * Burası Silinecek sistem bunu otomatik yapıyor olması gerekiyor.
												 */
                                                var branchRoleProfile = await roleRepository.GetWithCode("branch-" + roleId + (title == null ? "/profile-" + profileId : "/title"));

                                                if (branchRoleProfile != null && newEpostaFieldName != null)
                                                {
                                                    var user = await userRepository.GetByEmail(record[newEpostaFieldName.Value].ToString());
                                                    await UpdateUserRoleAndProfile(user.Id, profileId, branchRoleProfile.Id, roleRepository, profileRepository);
                                                    await SetAdvanceSharingWithOwners(roleId, (int)branchRecord["id"], branchModule, recordRepository, roleRepository);
                                                }
                                            }
                                        }
                                    }
                                    else if (operationType == OperationType.update)
                                    {
                                        /*
										 * Kayıt update edilirken değişen dataları bulmak için eski ve yeni kayıtı GetDifferences methoduna yolluyoruz.
										 * Eğer branch_manager checkbox ında bir değişiklik varsa
										 * ve true olarak setlenmişse çalışan üzerinde ki seçili olan şubeyi (rolü) user a setliyoruz.
										 */
                                        var differences = GetDifferences(record, currentRecord);

                                        if (!differences["branch_manager"].IsNullOrEmpty() && (bool)differences["branch_manager"])
                                        {
                                            var user = await userRepository.GetByEmail(record[newEpostaFieldName.Value].ToString());
                                            await roleRepository.RemoveUserAsync(user.Id, user.RoleId.Value);
                                            await UpdateUserRoleAndProfile(user.Id, profileId, roleId, roleRepository, profileRepository);
                                            await SetAdvanceSharingWithOwners(roleId, (int)branchRecord["id"], branchModule, recordRepository, roleRepository);
                                        }
                                        else if (!differences["branch"].IsNullOrEmpty() || !differences["sub_branch"].IsNullOrEmpty() || !differences["profile"].IsNullOrEmpty() || !differences["branch_manager"].IsNullOrEmpty())
                                        {
                                            /*
											 * Eğer branch_manager true dan false a çekilmiş ise
											 * Role ağacında ki dallar çalışan üzerinde role ve profil e göre uygun mu diye kontrol edilip eksik varsa bunları oluşturuyoruz.
											 * Role ağacı tamamlandığında çalışan üzerinde role ve profile göre oluşan son rolu ün id sini çalışana setliyoruz.
											 */
                                            if (missingSchema.Count > 0 && !(bool)record["branch_manager"] && newEpostaFieldName != null)
                                            {
                                                var currentUserRoleId = await CreateMissingSchema(missingSchema, roleId, roleRepository, appUser, title);
                                                var user = await userRepository.GetByEmail(record[newEpostaFieldName.Value].ToString());
                                                await roleRepository.RemoveUserAsync(user.Id, user.RoleId.Value);
                                                await UpdateUserRoleAndProfile(user.Id, profileId, (int)currentUserRoleId, roleRepository, profileRepository);
                                                await SetAdvanceSharingWithOwners(roleId, (int)branchRecord["id"], branchModule, recordRepository, roleRepository);
                                            }
                                            else
                                            {
                                                /*
												 * Role ağacında eksik yoksa çalışan üzerinde ki role ve profile id ile birlikte kullanıcının eklenmek istediği rolu ü buluyoruz.
												 * Bulunan role ün id sini çalışana setleyip kaydediyoruz.
												 */
                                                var role = await roleRepository.GetWithCode("branch-" + roleId + (title == null ? "/profile-" + profileId : "/title"));
                                                var user = await userRepository.GetByEmail(record[newEpostaFieldName.Value].ToString());
                                                await roleRepository.RemoveUserAsync(user.Id, user.RoleId.Value);
                                                await UpdateUserRoleAndProfile(user.Id, profileId, (bool)record["branch_manager"] ? roleId : role.Id, roleRepository, profileRepository);
                                                await SetAdvanceSharingWithOwners(roleId, (int)branchRecord["id"], branchModule, recordRepository, roleRepository);
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex, "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name + "operation_type:" + operationType + "record_id:" + recordId);
            }
        }

        public async Task<bool> YillikIzinHesaplama(int userId, int izinTuruId, Warehouse warehouse, int tenantId = 0, bool manuelEkIzin = false)
        {
            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                using (var _moduleRepository = new ModuleRepository(databaseContext, _configuration))
                using (var _recordRepository = new RecordRepository(databaseContext, warehouse, _configuration))
                {
                    if (tenantId > 0)
                        _currentUser.TenantId = tenantId;

                    _moduleRepository.CurrentUser = _recordRepository.CurrentUser = _currentUser;

                    var calisanlarModule = await _moduleRepository.GetByName("calisanlar");
                    if (calisanlarModule == null)
                    {
                        calisanlarModule = await _moduleRepository.GetByName("human_resources");
                        if (calisanlarModule == null)
                            return false;
                    }

                    var calisanId = userId;

                    var calisan = await _recordRepository.GetById(calisanlarModule, calisanId, false, profileBasedEnabled: false);

                    if (calisan == null)
                        return false;

                    var izinTurleri = await _moduleRepository.GetByName("izin_turleri");
                    var iseBaslamaTarihi = (string)calisan["ise_baslama_tarihi"];

                    var bugun = DateTime.UtcNow;

                    if (string.IsNullOrEmpty(iseBaslamaTarihi))
                        return false;

                    var calismayaBasladigiZaman = DateTime.ParseExact(iseBaslamaTarihi, "MM/dd/yyyy h:mm:ss", null);

                    var dayDiff = bugun - calismayaBasladigiZaman;
                    var dayDiffMonth = ((bugun.Year - calismayaBasladigiZaman.Year) * 12) + bugun.Month - calismayaBasladigiZaman.Month;
                    var dayDiffYear = dayDiff.Days / 365;

                    var izinKurali = await _recordRepository.GetById(izinTurleri, izinTuruId, false, profileBasedEnabled: false);

                    var ekIzin = 0.0;
                    var toplamKalanIzin = 0.0;
                    var hakedilenIzin = 0.0;

                    var yasaGoreIzin = 0.0;
                    var kidemeGoreIzin = 0.0;
                    var kullanilanYillikIzin = 0.0;
                    var devredenIzin = 0.0;

                    if (!calisan["sabit_devreden_izin"].IsNullOrEmpty())
                    {
                        devredenIzin = (double)calisan["sabit_devreden_izin"];
                    }

                    if (dayDiffYear > 0)
                    {
                        #region Yıllık izine ek izin süresi tanımlanmışsa tanımlanan ek izin süresini çalışanın profiline tanımlıyoruz.

                        if ((bool)izinKurali["yillik_izine_ek_izin_suresi_ekle"])
                            ekIzin = (double)izinKurali["yillik_izine_ek_izin_suresi_gun"];

                        #endregion

                        #region Yaşa göre asgari izin kuralı.

                        if ((double)izinKurali["yasa_gore_asgari_izin_gun"] != 0)
                        {
                            var dogumYili = (string)calisan["dogum_tarihi"];
                            if (!String.IsNullOrEmpty(dogumYili))
                            {
                                var calisanYasi = (bugun - DateTime.ParseExact(dogumYili, "MM/dd/yyyy h:mm:ss", null)).Days / 365;
                                if (calisanYasi < 18 || calisanYasi > 50)
                                {
                                    yasaGoreIzin = (double)izinKurali["yasa_gore_asgari_izin_gun"];
                                }
                            }
                        }

                        #endregion

                        #region Çalışanın çalıştığı yıl hesaplanarak kıdem izinleri hesaplanıyor.

                        var findRequestKidemeGoreIzinler = new FindRequest
                        {
                            Filters = new List<Filter>
                            {
                                new Filter { Field = "turu", Operator = Operator.Equals, Value = 1, No = 1 },
                                new Filter { Field = "calisilan_yil", Operator = Operator.LessEqual, Value = dayDiffYear }
                            },
                            Limit = 9999,
                            SortDirection = SortDirection.Desc
                        };

                        var kidemIzni = (await _recordRepository.Find("kideme_gore_yillik_izin_artislari", findRequestKidemeGoreIzinler, false, false)).First;


                        if (kidemIzni != null)
                            kidemeGoreIzin = (double)kidemIzni["ek_yillik_izin"];

                        #endregion

                        #region Kıdem ve yaş izinleri kıyaslanarak uygun olan setleniyor.

                        if (kidemeGoreIzin + (double)izinKurali["yillik_izin_hakki_gun"] > yasaGoreIzin)
                            hakedilenIzin = kidemeGoreIzin + (double)izinKurali["yillik_izin_hakki_gun"];
                        else
                            hakedilenIzin = yasaGoreIzin;

                        #endregion

                        #region Ek İzin Süresi Ekleme

                        if (!calisan["ek_izin"].IsNullOrEmpty() && (bool)calisan["ek_izin"] && manuelEkIzin)
                            hakedilenIzin += (int)calisan["ek_izin_suresi"];

                        #endregion
                    }

                    /*#region İlk izin kullanımı hakediş zamanı çalışanın işe giriş tarihinden büyük ise kullanıcının izin hakları sıfır olarak kaydediliyor.
					if ((double)izinKurali["ilk_izin_kullanimi_hakedis_zamani_ay"] != 0 && dayDiffMonth < (int)izinKurali["ilk_izin_kullanimi_hakedis_zamani_ay"])
						return false;
					#endregion*/

                    #region Bu yıl kullandığı toplam izinler hesaplanıyor.

                    var totalUsed = 0.0;

                    var year = DateTime.UtcNow.Year;
                    var ts = new TimeSpan(0, 0, 0);
                    calismayaBasladigiZaman = calismayaBasladigiZaman.Date + ts;

                    if (new DateTime(DateTime.UtcNow.Year, calismayaBasladigiZaman.Month, calismayaBasladigiZaman.Day, 0, 0, 0) > DateTime.UtcNow)
                        year--;

                    Filter filter;
                    if (!izinKurali["izin_hakki_onay_sureci_sonunda_dusulsun"].IsNullOrEmpty() && !(bool)izinKurali["izin_hakki_onay_sureci_sonunda_dusulsun"])
                        filter = new Filter { Field = "process.process_requests.process_status", Operator = Operator.NotEqual, Value = 3, No = 5 };
                    else
                        filter = new Filter { Field = "process.process_requests.process_status", Operator = Operator.Equals, Value = 2, No = 5 };


                    var findRequestIzinler = new FindRequest
                    {
                        Fields = new List<string> { "hesaplanan_alinacak_toplam_izin", "process.process_requests.process_status" },
                        Filters = new List<Filter>
                        {
                            new Filter { Field = "calisan", Operator = Operator.Equals, Value = calisanId, No = 1 },
                            new Filter { Field = "baslangic_tarihi", Operator = Operator.GreaterEqual, Value = new DateTime(year, calismayaBasladigiZaman.Month, calismayaBasladigiZaman.Day, 0, 0, 0).ToString("yyyy-MM-dd h:mm:ss"), No = 2 },
                            new Filter { Field = "izin_turu", Operator = Operator.Equals, Value = izinTuruId, No = 3 },
                            new Filter { Field = "deleted", Operator = Operator.Equals, Value = false, No = 4 },
                            filter
                        },
                        Limit = 9999
                    };

                    var izinlerRecords = await _recordRepository.Find("izinler", findRequestIzinler, false, false);

                    foreach (var izinlerRecord in izinlerRecords)
                    {
                        if (!izinlerRecord["hesaplanan_alinacak_toplam_izin"].IsNullOrEmpty())
                            totalUsed += (double)izinlerRecord["hesaplanan_alinacak_toplam_izin"];
                    }

                    kullanilanYillikIzin = totalUsed;

                    #endregion

                    #region Kullanılan izinler öncelikle devreden düşülüyor sonra kalan izninden düşülüyor.

                    hakedilenIzin = hakedilenIzin + ekIzin;

                    var devredenCounter = devredenIzin - kullanilanYillikIzin;

                    devredenIzin = devredenCounter;

                    if (devredenCounter < 0)
                    {
                        devredenIzin = 0;
                        toplamKalanIzin = hakedilenIzin + devredenCounter;
                    }
                    else
                    {
                        toplamKalanIzin = hakedilenIzin + devredenIzin;
                    }

                    #endregion

                    //var calisanlarModule = await moduleRepository.GetByNameAsync("calisanlar");
                    try
                    {
                        var accountRecordUpdate = new JObject();
                        accountRecordUpdate["id"] = calisanId;
                        accountRecordUpdate["hakedilen_izin"] = hakedilenIzin;
                        accountRecordUpdate["devreden_izin"] = devredenIzin;
                        accountRecordUpdate["kalan_izin_hakki"] = toplamKalanIzin;

                        if (!calisan["ek_izin"].IsNullOrEmpty() && (bool)calisan["ek_izin"] && manuelEkIzin && calisan["ek_izin_atama_tarihi"].IsNullOrEmpty())
                            accountRecordUpdate["ek_izin_atama_tarihi"] = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss");

                        accountRecordUpdate["updated_by"] = (int)calisan["updated_by"];
                        var resultUpdate = await _recordRepository.Update(accountRecordUpdate, calisanlarModule, isUtc: false);

                        if (resultUpdate < 1)
                        {
                            ErrorHandler.LogError(new Exception("Account (IK) cannot be updated! Object: " + accountRecordUpdate), "tenant_id:" + tenantId + "calisanId:" + calisanId);
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.LogError(ex, "tenant_id:" + tenantId + "calisanId:" + calisanId);
                        return false;
                    }

                    return true;
                }
            }
        }

        public async Task<bool> DeleteAnnualLeave(int userId, int izinTuruId, JObject record)
        {
            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                using (var _moduleRepository = new ModuleRepository(databaseContext, _configuration))
                using (var _recordRepository = new RecordRepository(databaseContext, _configuration))
                {
                    _moduleRepository.CurrentUser = _recordRepository.CurrentUser = _currentUser;

                    var calisanlarModule = await _moduleRepository.GetByName("calisanlar");
                    if (calisanlarModule == null)
                    {
                        calisanlarModule = await _moduleRepository.GetByName("human_resources");
                        if (calisanlarModule == null)
                            return false;
                    }

                    var calisanId = userId;
                    var calisan = await _recordRepository.GetById(calisanlarModule, calisanId, false, profileBasedEnabled: false);

                    if (calisan == null)
                        return false;

                    try
                    {
                        var accountRecordUpdate = new JObject();
                        accountRecordUpdate["id"] = calisanId;
                        accountRecordUpdate["kalan_izin_hakki"] = (double)calisan["kalan_izin_hakki"] + (double)record["hesaplanan_alinacak_toplam_izin"];


                        accountRecordUpdate["updated_by"] = (int)calisan["updated_by"];
                        var resultUpdate = await _recordRepository.Update(accountRecordUpdate, calisanlarModule, isUtc: false);

                        if (resultUpdate < 1)
                        {
                            ErrorHandler.LogError(new Exception("Account (IK) cannot be updated! Object: " + accountRecordUpdate), "userId:" + userId + "calisanId:" + calisanId);
                            return false;
                        }

                        return true;
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.LogError(ex, "userId:" + userId + "calisanId:" + calisanId);
                        return false;
                    }
                }
            }
        }

        public async Task<bool> CalculateTimesheet(JArray timesheetItemsRecords, UserItem appUser, Module timesheetItemModule, Module timesheetModule, Warehouse _warehouse)
        {
            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                using (var _moduleRepository = new ModuleRepository(databaseContext, _configuration))
                using (var _recordRepository = new RecordRepository(databaseContext, _warehouse, _configuration))
                using (var _picklistRepository = new PicklistRepository(databaseContext, _configuration))
                {
                    _moduleRepository.CurrentUser = _recordRepository.CurrentUser = _picklistRepository.CurrentUser = _currentUser;

                    var entryTypeField = timesheetItemModule.Fields.Single(x => x.Name == "entry_type");
                    var entryTypePicklist = await _picklistRepository.GetById(entryTypeField.PicklistId.Value);
                    var chargeTypeField = timesheetItemModule.Fields.Single(x => x.Name == "charge_type");
                    var chargeTypePicklist = await _picklistRepository.GetById(chargeTypeField.PicklistId.Value);
                    var placeOfPerformanceField = timesheetItemModule.Fields.Single(x => x.Name == "place_of_performance");
                    var placeOfPerformancePicklist = await _picklistRepository.GetById(placeOfPerformanceField.PicklistId.Value);
                    var timesheetId = 0;
                    var timesheetOwnerEmail = "";
                    var totalDaysWorked = 0.0;
                    var totalDaysWorkedTurkey = 0.0;
                    var totalDaysWorkedNonTurkey = 0.0;
                    var totalDaysWorkedPerDiem = 0.0;
                    var projectTotals = new Dictionary<int, double>();

                    //Calculate timesheet total days
                    foreach (var timesheetItemRecord in timesheetItemsRecords)
                    {
                        timesheetId = (int)timesheetItemRecord["related_timesheet.timesheet.timesheet_id"];
                        timesheetOwnerEmail = (string)timesheetItemRecord["owner.users.email"];

                        if (timesheetItemRecord["entry_type"].IsNullOrEmpty())
                            continue;

                        var entryTypePicklistItem = entryTypePicklist.Items.Single(x => x.LabelEn == timesheetItemRecord["entry_type"].ToString());

                        if (entryTypePicklistItem == null)
                            continue;

                        var value = double.Parse(entryTypePicklistItem.Value);

                        if (entryTypePicklistItem.SystemCode != "per_diem_only")
                        {
                            totalDaysWorked += value;

                            if (!timesheetItemRecord["place_of_performance"].IsNullOrEmpty())
                            {
                                var placeOfPerformancePicklistItem = placeOfPerformancePicklist.Items.Single(x => x.LabelEn == timesheetItemRecord["place_of_performance"].ToString());

                                if (placeOfPerformancePicklistItem.SystemCode == "other")
                                    totalDaysWorkedNonTurkey += value;
                                else
                                    totalDaysWorkedTurkey += value;
                            }

                            if (!timesheetItemRecord["per_diem"].IsNullOrEmpty() && bool.Parse(timesheetItemRecord["per_diem"].ToString()))
                                totalDaysWorkedPerDiem += value;
                        }
                        else
                        {
                            totalDaysWorkedPerDiem += value;
                        }

                        var chargeTypePicklistItem = chargeTypePicklist.Items.Single(x => x.LabelEn == timesheetItemRecord["charge_type"].ToString());

                        if (chargeTypePicklistItem == null)
                            continue;

                        if (chargeTypePicklistItem.Value == "billable")
                        {
                            var projectId = (int)timesheetItemRecord["selected_project.projects.id"];

                            if (projectTotals.ContainsKey(projectId))
                                projectTotals[projectId] += value;
                            else
                                projectTotals.Add(projectId, value);
                        }
                    }

                    //Update timesheet total days
                    var timesheetRecordUpdate = new JObject();
                    timesheetRecordUpdate["id"] = timesheetId;
                    timesheetRecordUpdate["total_days"] = totalDaysWorked;
                    timesheetRecordUpdate["total_days_worked_turkey"] = totalDaysWorkedTurkey;
                    timesheetRecordUpdate["total_days_worked_non_turkey"] = totalDaysWorkedNonTurkey;
                    timesheetRecordUpdate["total_days_perdiem"] = totalDaysWorkedPerDiem;

                    try
                    {
                        var resultUpdate = await _recordRepository.Update(timesheetRecordUpdate, timesheetModule, isUtc: false);

                        if (resultUpdate < 1)
                        {
                            ErrorHandler.LogError(new Exception("Timesheet cannot be updated! Object: " + timesheetRecordUpdate), "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "timesheetId:" + timesheetId);
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.LogError(ex, "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "timesheetId:" + timesheetId);
                        return false;
                    }

                    //Update project teams billable timesheet total days
                    var findRequestExpert = new FindRequest { Filters = new List<Filter> { new Filter { Field = "e_mail1", Operator = Operator.Is, Value = timesheetOwnerEmail, No = 1 } } };
                    var expertRecords = await _recordRepository.Find("experts", findRequestExpert, false, false);

                    if (expertRecords.IsNullOrEmpty() || expertRecords.Count < 1)
                    {
                        //ErrorHandler.LogError(new Exception("Expert not found! FindRequest: " + findRequestExpert.ToJsonString()), "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "timesheetId:" + timesheetId);
                        return false;
                    }

                    var expertRecord = expertRecords[0];
                    var projectTeamModule = await _moduleRepository.GetByName("project_team");

                    foreach (var projectTotal in projectTotals)
                    {
                        var findRequestProjectTeam = new FindRequest
                        {
                            Filters = new List<Filter>
                            {
                                new Filter { Field = "expert", Operator = Operator.Equals, Value = (int)expertRecord["id"], No = 1 },
                                new Filter { Field = "project", Operator = Operator.Equals, Value = projectTotal.Key, No = 2 }
                            }
                        };

                        var projectTeamRecords = await _recordRepository.Find("project_team", findRequestProjectTeam, false, false);

                        if (projectTeamRecords.IsNullOrEmpty() || projectTeamRecords.Count < 1)
                            continue;

                        var projectTeamRecordUpdate = new JObject();
                        projectTeamRecordUpdate["id"] = (int)projectTeamRecords[0]["id"];
                        projectTeamRecordUpdate["timesheet_total_days"] = projectTotal.Value;

                        try
                        {
                            var resultUpdate = await _recordRepository.Update(projectTeamRecordUpdate, projectTeamModule, isUtc: false);

                            if (resultUpdate < 1)
                            {
                                ErrorHandler.LogError(new Exception("ProjectTeam cannot be updated! Object: " + projectTeamRecordUpdate), "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "timesheetId:" + timesheetId);
                                return false;
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorHandler.LogError(ex, "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "timesheetId:" + timesheetId);
                            return false;
                        }
                    }

                    return true;
                }
            }
        }


        public static async Task UpdateUserRoleAndProfile(int userId, int profileId, int roleId, RoleRepository roleRepository, ProfileRepository profileRepository)
        {
            await roleRepository.AddUserAsync(userId, roleId);
            /*Profile olmayabilir, burada title kullanılmış olabilir*/
            if (profileId > 0)
                await profileRepository.AddUserAsync(userId, profileId);
        }

        public static async Task SetAdvanceSharingWithOwners(int roleId, int branchRecordId, Module branchModule, RecordRepository recordRepository, RoleRepository roleRepository)
        {
            var role = await roleRepository.GetByIdAsync(roleId);

            if (role.OwnersList.Count > 0)
            {
                var branch = new JObject
                {
                    ["id"] = branchRecordId,
                    ["shared_users"] = new JArray()
                };
                foreach (var owner in role.OwnersList)
                {
                    ((JArray)branch["shared_users"]).Add(int.Parse(owner));
                }

                await recordRepository.Update(branch, branchModule, false, false);
            }
        }

        public static async Task<int?> CreateMissingSchema(List<Profile> missingSchema, int roleId, RoleRepository roleRepository, UserItem appUser, Setting title)
        {
            int? parentId = null;

            for (var i = missingSchema.Count - 1; i > -1; i--)
            {
                var schemaItem = missingSchema[i];

                if (title == null)
                {
                    /*
					 * Eksik role leri oluşturmak için missingSchema objesi içinde dönüyoruz.
					 * Eğer yeni eklenecek role bi şubeye direk bağlıysa (Ex: Gayrettepe Şubesi > Satış Sorumlusu)
					 * GetWithCode methodu boş dönücektir.
					 * Fakat daha bir şubeye değil de role ağacında ki başka bir profilin altına eklenecek ise (Ex : Satış Sorumlusu > Satış Destek)
					 * O zaman GetWithCode methoduyla Satış sorumlusunun role unu çekiyoruz.
					 * Ve yeni eklenecek dalın bu role e bağlanması için parentId si olarak bu rolun id sini setliyoruz.
					 *
					 */
                    var parent = await roleRepository.GetWithCode("branch-" + roleId + "/profile-" + schemaItem.ParentId);
                    parentId = parent?.Id;

                    /*
					 * Eğer rol ağacına eklenecek yeni profilin parentId si 0 ise bu profile şemasında en üstte olduğu anlamına gelir.
					 * Bu yüzden direk olarak seçilen şube ReportToId olarak setlenerek role ağacına eklenecek profilin direk şubeye bağlanması sağlanır.
					 * missingSchema nın Count u bir den fazla ise role ağacına ilk kayıt eklendikten sonra eklenen kayıtın id si alınır.
					 * Bir sonra ki eklenen kayıt bu yeni eklenen role ün altında olucağı için 2. kayıt eklenirken bir önceki kayıtın id si
					 * 2. kayıta ReportToId olarak atanır.
					 */

                    parentId = await roleRepository.CreateAsync(new Role()
                    {
                        LabelEn = schemaItem.NameEn,
                        LabelTr = schemaItem.NameTr,
                        DescriptionEn = null,
                        DescriptionTr = null,
                        Master = false,
                        OwnersList = new List<string>(),
                        ReportsToId = schemaItem.ParentId == 0 ? roleId : parentId,
                        ShareData = false,
                        SystemCode = "branch-" + roleId + "/profile-" + schemaItem.Id.ToString()
                    }, appUser.TenantLanguage);
                }
                else
                {
                    parentId = await roleRepository.CreateAsync(new Role()
                    {
                        LabelEn = "Standard-" + schemaItem.NameEn,
                        LabelTr = "Standart-" + schemaItem.NameTr,
                        DescriptionEn = null,
                        DescriptionTr = null,
                        Master = false,
                        OwnersList = new List<string>(),
                        ReportsToId = roleId,
                        ShareData = false,
                        SystemCode = "branch-" + roleId + "/title"
                    }, appUser.TenantLanguage);
                }
            }

            return parentId;
        }

        public static JObject GetDifferences(JObject newRecord, JObject oldRecord)
        {
            var differences = new JObject();
            foreach (JProperty property in newRecord.Properties())
            {
                if (!JToken.DeepEquals(newRecord[property.Name], oldRecord[property.Name]))
                {
                    differences[property.Name] = newRecord[property.Name];
                }
            }

            return differences;
        }

        public static async Task GetCurrentProfileSchema(Profile profile, List<Profile> profileSchema, ProfileRepository profileRepository)
        {
            profileSchema.Add(profile);

            if (profile.ParentId != 0)
            {
                var parentProfile = await profileRepository.GetProfileById(profile.ParentId);

                await GetCurrentProfileSchema(parentProfile, profileSchema, profileRepository);
            }
        }

        public static async Task<List<Profile>> MissingProfileSchema(List<Profile> profileSchema, int branchId, int parentId, ProfileRepository profileRepository, RoleRepository roleRepository, Setting title)
        {
            var subBranchs = await roleRepository.GetByReportsToId(parentId);

            if (profileSchema.Count > 0)
            {
                for (var i = 0; i < subBranchs.Count; i++)
                {
                    var subBranch = subBranchs.FirstOrDefault(y => y.SystemCode == "branch-" + branchId + (profileSchema.Count > 0 && title == null ? "/profile-" + profileSchema[profileSchema.Count - 1].Id : "/title"));

                    if (subBranch != null)
                    {
                        profileSchema.Remove(profileSchema[profileSchema.Count - 1]);
                        await MissingProfileSchema(profileSchema, branchId, subBranch.Id, profileRepository, roleRepository, title);
                        break;
                    }
                }
            }

            return profileSchema;
        }

        //Çalışan eklerkenirken oluşan kullanıcı id'sinin recordun ownerına otomatik setlenmesi.
        public static async Task<int> ChangeRecordOwner(string email, JObject calisanRecord, Module calisanModule, RecordRepository recordRepository, ModuleRepository moduleRepository)
        {
            var getUserEmail = new FindRequest
            {
                Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Equals, Value = email, No = 1 } },
                Limit = 1,
                Offset = 0
            };

            var getUser = (await recordRepository.Find("users", getUserEmail, false, false)).FirstOrDefault();

            if (getUser["email"].ToString() == (calisanModule.Name != "calisan" ? email : calisanRecord["e_posta"].ToString()))
            {
                var gelenData = new JObject();
                gelenData["id"] = int.Parse(calisanRecord["id"].ToString());
                gelenData["owner"] = int.Parse(getUser["id"].ToString());

                var resultUpdate = await recordRepository.Update(gelenData, calisanModule);

                if (resultUpdate > 0)
                    return int.Parse(getUser["id"].ToString());
            }

            return 0;
        }
    }
}