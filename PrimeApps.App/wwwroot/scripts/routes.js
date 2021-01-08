"use strict";var sectionComponents={},currentSectionComponentsTemplate=[],replaceDynamicValues=function(e){var t=e.split("{appConfigs.");if(t.length>1)for(var o in t)if(t.hasOwnProperty(o)){if(!t[o])continue;var r=t[o].split("}")[0];e=e.replace("{appConfigs."+r+"}",appConfigs[r])}return e},loadSectionComponents=function(e,t,o){if(currentSectionComponentsTemplate=[],account.modules){var r=e("filter")(account.modules,{name:t},!0)[0].id;if(sectionComponents["component"+r])for(var s=sectionComponents["component"+r],l=0;l<s.length;l++){for(var a=angular.fromJson(s[l].content).files,p=0;p<a.length;p++)o.push(replaceDynamicValues(a[p]));currentSectionComponentsTemplate.push(replaceDynamicValues(angular.fromJson(s[l].content).app.templateUrl))}}};angular.module("primeapps").config(["$stateProvider","$urlRouterProvider",function(e,t){if(token&&(window.localStorage.access_token=token),window.localStorage.getItem("access_token")){if(e.state("app",{url:"/app","abstract":!0,templateUrl:"view/app.html",controller:"AppController",resolve:{AppService:"AppService",start:["$rootScope","AppService",function(e,t){return e.user?void 0:t.getMyAccount()}]}}),e.state("app.dashboard",{url:"/dashboard",views:{app:{templateUrl:"view/app/dashboard/dashboard.html",controller:"DashboardController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["scripts/vendor/angular-fusioncharts.js","view/app/dashboard/dashboardService.js","view/app/dashboard/dashboardController.js"])}]}}).state("app.moduleList",{url:"/modules/:type?viewid",views:{app:{templateUrl:"view/app/module/moduleList.html",controller:"ModuleListController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/app/module/moduleListController.js","view/app/email/bulkEMailController.js","view/app/sms/bulkSMSController.js","view/setup/templates/templateService.js","view/app/module/exportDataController.js","scripts/vendor/angular-fusioncharts.js"])}]}}).state("app.record",{url:"/record/:type?stype?id?ptype?pid?rtab?pptype?ppid?prtab?back?clone?revise?many?field?value",views:{app:{templateUrl:"view/app/module/recordDetail.html",controller:"RecordController"}},resolve:{AppService:"AppService",plugins:["$rootScope","$state","$$animateJs","$ocLazyLoad","$filter","$http","config",function(e,t,o,r,s,l,a){var p=["view/app/module/recordController.js","view/app/email/bulkEMailController.js","view/app/email/singleEmailController.js","view/app/sms/singleSMSController.js","view/app/location/locationFormModalController.js","view/setup/templates/templateService.js"];if(window.location.hash.split("/")[3]){var i=window.location.hash.split("/")[3];i.search("/?")>-1&&(i=i.split("?")[0])}if(i){if(account.modules)return loadSectionComponents(s,i,p),navigator.onLine&&googleMapsApiKey&&"your-google-maps-api-key"!==googleMapsApiKey,r.load(p);account.modules||l.get(a.apiUrl+"module/get_all").then(function(e){return e.data?(account.modules=e.data,loadSectionComponents(s,i,p),navigator.onLine&&googleMapsApiKey&&"your-google-maps-api-key"!==googleMapsApiKey,r.load(p)):void 0})}}]}}).state("app.documents",{url:"/documents/:type?id",views:{app:{templateUrl:"view/app/documents/documents.html",controller:"DocumentController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/app/documents/documentController.js"])}]}}).state("app.componentDesign",{url:"/componentdesign",views:{app:{templateUrl:"view/app/componentDesign/componentDesign.html",controller:"ComponentdesignController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/app/componentDesign/componentDesign.js"])}]}}).state("app.gridDesign",{url:"/griddesign",views:{app:{templateUrl:"view/app/componentDesign/gridDesign.html",controller:"GridDesignController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/app/componentDesign/gridDesign.js"])}]}}).state("app.recordDetail",{url:"/recorddetail",views:{app:{templateUrl:"view/app/componentDesign/recordDetail.html",controller:"RecordDetailController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/app/componentDesign/recordDetail.js"])}]}}).state("app.documentSearch",{url:"/documentSearch",views:{app:{templateUrl:"view/app/documents/advDocumentSearch.html",controller:"AdvDocumentSearchController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/app/documents/advDocumentSearchController.js"])}]}}).state("app.import",{url:"/import/:type",views:{app:{templateUrl:"view/app/data/import.html",controller:"ImportController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["scripts/vendor/xlsx.core.min.js","view/app/data/importController.js","view/app/data/importService.js"])}]}}).state("app.importCsv",{url:"/importcsv/:type",views:{app:{templateUrl:"view/app/data/csv/import.html",controller:"ImportController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/app/data/csv/importController.js","view/app/data/csv/importService.js"])}]}}).state("app.analytics",{url:"/analytics?id",views:{app:{templateUrl:"view/app/analytics/analytics.html",controller:"AnalyticsController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/app/analytics/analyticsService.js","view/app/analytics/analyticsController.js"])}]}}),e.state("app.setup",{url:"/setup","abstract":!0,views:{app:{templateUrl:"view/setup/setup.html",controller:"SetupController"}}}).state("app.setup.settings",{url:"/settings?tab",views:{app:{templateUrl:"view/setup/settings/settings.html",controller:"SettingController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/setup/settings/settingController.js","view/setup/settings/settingService.js","view/setup/email/emailService.js"])}]}}).state("app.setup.general",{url:"/general",views:{app:{templateUrl:"view/setup/general/generalSettings.html",controller:"GeneralSettingsController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/setup/general/generalSettingsController.js","view/setup/general/generalSettingsService.js"])}]}}).state("app.setup.organization",{url:"/organization",views:{app:{templateUrl:"view/setup/organization/organization.html",controller:"OrganizationController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/setup/organization/organizationController.js","view/setup/organization/organizationService.js"])}]}}).state("app.setup.notifications",{url:"/notifications",views:{app:{templateUrl:"view/setup/notifications/notifications.html",controller:"NotificationController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/setup/notifications/notificationController.js","view/setup/notifications/notificationService.js"])}]}}).state("app.setup.users",{url:"/users",views:{app:{templateUrl:"view/setup/users/users.html",controller:"UserController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/setup/users/userController.js","view/setup/users/userService.js","view/setup/workgroups/workgroupService.js","view/setup/profiles/profileService.js","view/setup/roles/roleService.js","view/setup/usercustomshares/userCustomShareService.js"])}]}}).state("app.setup.profiles",{url:"/profiles",views:{app:{templateUrl:"view/setup/profiles/profiles.html",controller:"ProfileController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/setup/profiles/profileController.js","view/setup/profiles/profileService.js"])}]}}).state("app.setup.profile",{url:"/profile?id&clone",views:{app:{templateUrl:"view/setup/profiles/profileForm.html",controller:"ProfileFormController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/setup/profiles/profileFormController.js","view/setup/profiles/profileService.js"])}]}}).state("app.setup.roles",{url:"/roles",views:{app:{templateUrl:"view/setup/roles/roles.html",controller:"RoleController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/setup/roles/roleController.js","view/setup/roles/roleFormController.js","view/setup/roles/roleService.js"])}]}}).state("app.setup.role",{url:"/role?id&reportsTo",views:{app:{templateUrl:"view/setup/roles/roleForm.html",controller:"RoleFormController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/setup/roles/roleFormController.js","view/setup/roles/roleService.js"])}]}}).state("app.setup.candidateconvertmap",{url:"/candidateconvertmap",views:{app:{templateUrl:"view/setup/convert/candidateConvertMap.html",controller:"CandidateConvertMapController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/setup/convert/candidateConvertMapController.js","view/setup/convert/convertMapService.js"])}]}}).state("app.setup.import",{url:"/importhistory",views:{app:{templateUrl:"view/setup/importhistory/importHistory.html",controller:"ImportHistoryController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/setup/importhistory/importHistoryController.js","view/setup/importhistory/importHistoryService.js"])}]}}).state("app.setup.sms",{url:"/sms",views:{app:{templateUrl:"view/setup/sms/sms.html",controller:"SmsController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/setup/sms/smsController.js","view/setup/sms/smsService.js"])}]}}).state("app.setup.email",{url:"/email",views:{app:{templateUrl:"view/setup/email/email.html",controller:"EmailController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/setup/email/emailController.js","view/setup/email/emailService.js"])}]}}).state("app.setup.office",{url:"/office",views:{app:{templateUrl:"view/setup/office/office.html",controller:"OfficeController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/setup/office/officeController.js","view/setup/office/officeService.js"])}]}}).state("app.setup.auditlog",{url:"/auditlog",views:{app:{templateUrl:"view/setup/auditlog/auditlogs.html",controller:"AuditLogController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/setup/auditlog/auditLogController.js","view/setup/auditlog/auditLogService.js"])}]}}).state("app.setup.templates",{url:"/templates?tab",views:{app:{templateUrl:"view/setup/templates/templates.html",controller:"TemplateController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/setup/templates/templateService.js","view/setup/templates/templateController.js"])}]}}).state("app.setup.template",{url:"/template?id",views:{app:{templateUrl:"view/setup/templates/templateForm.html",controller:"TemplateFormController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/setup/templates/templateService.js","view/setup/templates/templateFormController.js"])}]}}).state("app.setup.usergroups",{url:"/usergroups",views:{app:{templateUrl:"view/setup/usergroups/userGroups.html",controller:"UserGroupController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/setup/usergroups/userGroupController.js","view/setup/usergroups/userGroupFormController.js","view/setup/usergroups/userGroupService.js"])}]}}).state("app.setup.usercustomshares",{url:"/usercustomshares",views:{app:{templateUrl:"view/setup/usercustomshares/userCustomShares.html",controller:"UserCustomShareController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/setup/usercustomshares/userCustomShareController.js","view/setup/usercustomshares/userCustomShareFormController.js","view/setup/usercustomshares/userCustomShareService.js","view/setup/users/userService.js"])}]}}).state("app.setup.usercustomshare",{url:"/usercustomshare?id&clone",views:{app:{templateUrl:"view/setup/usercustomshares/userCustomShareForm.html",controller:"UserCustomShareFormController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/setup/usercustomshares/userCustomShareFormController.js","view/setup/usercustomshares/userCustomShareService.js"])}]}}).state("app.setup.outlook",{url:"/outlook",views:{app:{templateUrl:"view/setup/outlook/outlook.html",controller:"OutlookController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/setup/outlook/outlookController.js","view/setup/outlook/outlookService.js"])}]}}).state("app.setup.usergroup",{url:"/usergroup?id&clone",views:{app:{templateUrl:"view/setup/usergroups/userGroupForm.html",controller:"UserGroupFormController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/setup/usergroups/userGroupFormController.js","view/setup/usergroups/userGroupService.js"])}]}}).state("app.setup.signalnotification",{url:"/signalnotification",views:{app:{templateUrl:"view/setup/signalnotifications/signalNotifications.html",controller:"SignalNotificationController"}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(["view/setup/signalnotifications/signalNotificationController.js"])}]}}),""!==components){var o=angular.fromJson(components);angular.forEach(o,function(t){if(t.content){var o=[],r=angular.fromJson(t.content);if(1001===t.place)return void(sectionComponents["component"+t.module_id]?sectionComponents.push(t):(sectionComponents["component"+t.module_id]=[],sectionComponents["component"+t.module_id].push(t)));r.app.templateUrl=replaceDynamicValues(r.app.templateUrl);for(var s="t"===r.local?"views/app/"+t.name+"/":blobUrl+"/components/"+("app"===r.level||preview?"app-"+applicationId:"tenant-"+tenantId)+"/"+t.name+"/",l=0;l<r.files.length;l++)r.files[l]=replaceDynamicValues(r.files[l]),o.push(0===r.files[l].lastIndexOf("http",0)?r.files[l]:s+r.files[l]);e.state("app."+t.name,{cache:!1,url:"/"+r.url,views:{app:{templateUrl:function(e){var t="?";for(var o in e)e[o]&&(t+=o+"="+e[o]+"&");t=t.substring(0,t.length-1);var l=0===r.app.templateUrl.lastIndexOf("http",0)?r.app.templateUrl:s+r.app.templateUrl;return t.length>1&&(l+=t),l},controller:r.app.controller}},resolve:{plugins:["$$animateJs","$ocLazyLoad",function(e,t){return t.load(o)}]}})}})}t.otherwise("/app/dashboard")}}]);