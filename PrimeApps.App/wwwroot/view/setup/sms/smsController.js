"use strict";angular.module("primeapps").controller("SmsController",["$rootScope","$scope","$filter","SmsService","$mdDialog","mdToast","$stateParams","helper","$state","AppService",function(s,e,t,a,i,o,n,r,m,l){l.checkPermission().then(function(n){if(n&&n.data){var r=JSON.parse(n.data.profile),l=void 0;if(n.data.customProfilePermissions&&(l=JSON.parse(n.data.customProfilePermissions)),!r.HasAdminRights){var S=void 0;l&&(S=l.permissions.indexOf("sms")>-1),S||m.go("app.setup.email")}}s.breadcrumblist=[{title:t("translate")("Layout.Menu.Dashboard"),link:"#/app/dashboard"},{title:t("translate")("Setup.Nav.System"),link:"#/app/setup/general"},{title:t("translate")("Setup.Messaging.SMS.Title")}],e.smsModel=angular.copy(s.system.messaging.SMS)||{},e.loading=!1,e.goUrl=function(s){window.location=s},e.editSMS=function(){e.systemForm.$valid&&(e.loading=!0,a.updateSMSSettings(e.smsModel).then(function(){o.success(t("translate")("Setup.Settings.UpdateSuccess")),s.system.messaging.SMS||(s.system.messaging.SMS={}),s.system.messaging.SMS.provider=e.smsModel.provider,s.system.messaging.SMS.user_name=e.smsModel.user_name,s.system.messaging.SMS.alias=e.smsModel.alias,e.loading=!1})["catch"](function(){e.loading=!1,e.systemForm.$submitted=!1,o.error(t("translate")("Common.Error"))}))},e.resetSMSForm=function(){e.smsModel=angular.copy(s.system.messaging.SMS)},e.removeSMSSettings=function(){a.removeSMSSettings(e.smsModel).then(function(){e.smsModel=null,s.system.messaging.SMS=null})},e.close=function(){i.hide()},e.submitGeneral=function(){return e.systemForm.$valid?void e.editSMS(e.smsModel):void o.error(t("translate")("Module.RequiredError"))}})}]);