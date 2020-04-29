"use strict";angular.module("primeapps").controller("ProfileController",["$rootScope","$scope","$filter","helper","ProfileService","$localStorage","$mdDialog","mdToast","$state",function(e,t,i,r,a,o,l,n,s){t.hasAdminRight=i("filter")(e.profiles,{id:e.user.profile.id},!0)[0].has_admin_rights,t.hasAdminRight||r.hasCustomProfilePermission("profile")||(n.error(i("translate")("Common.Forbidden")),s.go("app.dashboard")),t.loading=!0,t.profileForForm={id:null,clone:null},t.showDeleteForm=function(r){return r?(t.profileDeleteLabel=i("translate")("Setup.Profiles.ProfileDeleteLabel",{name:r["name_"+e.language]}),void a.getAll().then(function(o){t.profiles=a.getProfiles(o.data,e.workgroup.tenant_id,!1),t.selectedProfile=i("filter")(t.profiles,{id:r.id},!0)[0];for(var n=angular.copy(t.profiles),s=angular.copy(t.selectedProfile),d=0,p=0;p<n.length;p++)if(n[p].id===s.id){d=p;break}n.splice(d,1),t.transferProfiles=n;var f=angular.element(document.body);l.show({parent:f,templateUrl:"view/setup/profiles/profileDelete.html",clickOutsideToClose:!0,scope:t,preserveScope:!0})})):void n.warning(i("translate")("Common.NotFoundRecord"))},t["delete"]=function(r){r||(r=t.transferProfiles[0]),t.profileDeleting=!0,a.remove(t.selectedProfile.id,r.id,e.workgroup.tenant_id).then(function(){t.profileDeleting=!1,n.success(i("translate")("Setup.Profiles.DeleteSuccess")),t.close(),t.grid.dataSource.read()})["catch"](function(){t.profileDeleting=!1})},t.showSideModal=function(r,o){function l(){t.formLoading=!0,a.getAll().then(function(o){if(t.profiles=a.getProfiles(o.data,e.workgroup.tenant_id,!1),t.profilesCopy=angular.copy(t.profiles),r){t.profile=i("filter")(t.profiles,{id:r},!0)[0];var l=i("filter")(t.startPageList,{valueLower:t.profile.start_page},!0)[0];t.profile.PageStart=l,0!=t.profile.parent_id&&(t.profile.parent_id=i("filter")(t.profiles,{id:t.profile.parent_id},!0)[0])}else if(t.profile={},t.profile.tenant_id=e.workgroup.tenant_id,t.profileForForm.clone){var n=i("filter")(t.profiles,{id:t.profileForForm.clone},!0)[0];t.profile=n,delete t.profile.name_tr,delete t.profile.name_en,delete t.profile.user_ids,delete t.profile.description_tr,delete t.profile.description_en,delete t.profile.is_persistent,delete t.profile.created_by_id,delete t.profile.id,delete t.profile.system_type;var l=i("filter")(t.startPageList,{valueLower:t.profile.start_page},!0)[0];t.profile.PageStart=l,t.profile.parent_id=i("filter")(t.profiles,{id:n.parent_id},!0)[0]}else{t.profile.has_admin_rights=!1,t.profile.is_persistent=!1,t.profile.send_email=!1,t.profile.send_sms=!1,t.profile.export_data=!1,t.profile.import_data=!1,t.profile.word_pdf_download=!1,t.profile.smtp_settings=!1,t.profile.dashboard=!0,t.profile.permissions=i("filter")(t.profiles,{is_persistent:!0,has_admin_rights:!0})[0].permissions;var s=i("filter")(t.startPageList,{value:"Dashboard"},!0)[0];t.profile.PageStart=s}t.formLoading=!1})["catch"](function(){t.formLoading=!1})}function s(){var e=!0;return e}t.profileForForm={},t.profileForForm.id=r,t.profileForForm.clone=o;var r=r;t.startPageList=[{value:"Dashboard",valueLower:"dashboard",name:i("translate")("Layout.Menu.Dashboard")}],t.moduleLead=i("filter")(e.modules,{name:"leads"},!0)[0],t.moduleIzinler=i("filter")(e.modules,{name:"izinler"},!0)[0],t.moduleRehber=i("filter")(e.modules,{name:"rehber"},!0)[0],t.moduleRehber&&t.startPageList.push({value:"Home",valueLower:"home",name:i("translate")("Layout.Menu.Homepage")}),l(),t.SetStartPage=function(){var e=t.profile.PageStart.value;if(t.profile[e]=!0,"Newsfeed"===t.profile.PageStart.value){var r=i("filter")(t.profile.Permissions,{Type:3},!0)[0];r.Read=!0}},t.submit=function(r){if(s(),r.$valid){t.profileSubmit=!0;var o=null;t.profile.start_page=t.profile.PageStart.valueLower;var l=i("filter")(t.startPageList,{value:t.profile.PageStart.value},!0)[0];if(t.profile[l.value]=!0,"newsfeed"===t.profile.startpage){var d=i("filter")(t.profile.Permissions,{Type:3},!0)[0];d.Read=!0}t.profile.parent_id=t.profile.parent_id?t.profile.parent_id.id:0,t.profile.id?o=a.update(t.profile):("en"===t.language?(t.profile.name_tr=t.profile.name_en,t.profile.description_tr=t.profile.description_en):(t.profile.name_en=t.profile.name_tr,t.profile.description_en=t.profile.description_tr),o=a.create(t.profile)),o.then(function(){t.profileSubmit=!1,e.closeSide("sideModal"),n.success(i("translate")("Setup.Profiles.SubmitSuccess")),t.grid.dataSource.read(),a.getAll().then(function(t){e.profiles=t.data})})["catch"](function(){t.profileSubmit=!1})}else n.error(i("translate")("Module.RequiredError"))},t.pageStartOptions={dataSource:t.startPageList,dataTextField:"tr"===t.language?"name":"value",dataValueField:"valueLower"},e.buildToggler("sideModal","view/setup/profiles/profileForm.html"),t.formLoading=!0},t.close=function(){l.hide()},t.goUrl2=function(e){var i=window.getSelection();0===i.toString().length&&t.showSideModal(e.id,null)};var d='<md-menu md-position-mode="target-right target"> <md-button class="md-icon-button" aria-label=" " ng-click="$mdMenu.open()"> <i class="fas fa-ellipsis-v"></i></md-button><md-menu-content width="2" class="md-dense"><md-menu-item><md-button ng-disabled="dataItem.is_persistent || dataItem.system_type === \'system\'" ng-click="showSideModal(null, dataItem.id)"><i class="fas fa-copy"></i> '+i("translate")("Common.Copy")+' <span></span></md-button></md-menu-item><md-menu-item><md-button id="deleteButton{{dataItem.id}}" ng-disabled="dataItem.is_persistent || dataItem.system_type === \'system\'" ng-click="showDeleteForm(dataItem)"><i class="fas fa-trash"></i> <span> '+i("translate")("Common.Delete")+"</span></md-button></md-menu-item></md-menu-content></md-menu>",p=function(){t.profileGridOptions={dataSource:{type:"odata-v4",page:1,pageSize:10,serverPaging:!0,serverFiltering:!0,serverSorting:!0,transport:{read:{url:"/api/profile/find",type:"GET",dataType:"json",beforeSend:e.beforeSend()}},schema:{data:"items",total:"count",model:{id:"id",fields:{name:{type:"string"},description:{type:"string"}}}}},scrollable:!1,persistSelection:!0,sortable:!0,noRecords:!0,pageable:{refresh:!0,pageSize:10,pageSizes:[10,25,50,100],buttonCount:5,info:!0},filterable:!0,filter:function(e){if(e.filter)for(var t=0;t<e.filter.filters.length;t++)e.filter.filters[t].ignoreCase=!0},rowTemplate:function(){var e='<tr ng-click="goUrl2(dataItem)">';e+='<td class="hide-on-m2"><span>{{dataItem.name_'+t.language+"}}</span></td>",e+='<td class="hide-on-m2"><span>{{dataItem.description_'+t.language+"}}</span></td>",e+='<td class="show-on-m2">';var i="<div> <strong>{{dataItem.name_"+t.language+"}}</strong></div>";return i+="<div>{{dataItem.description_"+t.language+"}}</div>",e+=i+"</td>",e+='<td ng-click="$event.stopPropagation();"><span>'+d+"</span></td>",e+="</tr>"},altRowTemplate:function(){var e='<tr class="k-alt" ng-click="goUrl2(dataItem)">';e+='<td class="hide-on-m2"><span>{{dataItem.name_'+t.language+"}}</span></td>",e+='<td class="hide-on-m2"><span>{{dataItem.description_'+t.language+"}}</span></td>",e+='<td class="show-on-m2">';var i="<div><strong>{{dataItem.name_"+t.language+"}}</strong></div>";return i+="<div>{{dataItem.description_"+t.language+"}}</div>",e+=i+"</td>",e+='<td ng-click="$event.stopPropagation();"><span>'+d+"</span></td>",e+="</tr>"},columns:[{media:"(min-width: 575px)",field:"Name"+t.language,title:i("translate")("Setup.Profiles.ProfileName")},{media:"(min-width: 575px)",field:"Description"+e.user.language,title:i("translate")("Setup.Profiles.ProfileDescription")},{title:i("translate")("Setup.Nav.Tabs.Profiles"),media:"(max-width: 575px)"},{field:"",title:"",filterable:!1,width:"40px"}]}};angular.element(document).ready(function(){p(),t.loading=!1}),t.transferProfileOptions={dataSource:{transport:{read:function(e){e.success(t.transferProfiles)}}},autoBind:!1,dataTextField:"name_"+t.language,dataValueField:"id"}}]);