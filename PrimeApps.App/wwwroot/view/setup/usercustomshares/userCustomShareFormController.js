"use strict";angular.module("primeapps").controller("UserCustomShareFormController",["$rootScope","$location","$scope","$filter","guidEmpty","blockUI","$state","UserCustomShareService","UserService","helper","mdToast",function(e,r,a,s,t,u,l,n,d,o,i){if(a.$parent.$parent.selectedShare){a.id=a.$parent.$parent.selectedShare.id,a.language=e.language,a.userOwner={},a.lookupUserAndGroup=o.lookupUserAndGroup;var p=[];d.getAllUser().then(function(e){p=e.data,n.getAll().then(function(e){if(a.userCustomShares=e.data,!a.id)for(var r=0;r<e.data.length;r++){var t=e.data[r];if(s("filter")(p,{id:t.user_id},!0)[0]){var u=s("filter")(p,{id:t.user_id},!0)[0].email;p=s("filter")(p,{email:"!"+u},!0)}}})}),a.multiselect=function(){return s("filter")(e.modules,function(e){return 0!==e.order},!0)},a.id&&(a.loadingModal=!0,n.get(a.id).then(function(r){a.userOwner.user=r.data.user_id,a.userOwner.shared_user=r.data.shared_user_id;var t=[],u=[];if(r.data.user_groups_list.length>0&&"{}"!==r.data.user_groups_list[0]&&n.getUserGroups().then(function(e){for(var a=r.data.user_groups.replace("{","").replace("}","").split(","),u=0;u<r.data.user_groups_list.length;u++){var l=s("filter")(e.data,{id:parseInt(a[u])},!0)[0],n={description:"User Group",id:l.id,name:l.name,type:"group"};t.push(n)}}),r.data.users_list.length>0)for(var l=0;l<r.data.users_list.length;l++){var d=s("filter")(e.users,{id:parseInt(r.data.users_list[l])},!0)[0],o={description:d.Email,id:d.id,name:d.FullName,type:"user"};t.push(o)}if(r.data.module_list.length>0)for(var i=0;i<r.data.module_list.length;i++){var p=s("filter")(e.modules,{name:r.data.module_list[i]},!0)[0];u.push(p)}a.userOwner.modules=u,a.userOwner.owners=t,a.loadingModal=!1})),a.setFormValid=function(){a.userOwnerForm.shared.$setValidity("minTags",!0)},a.save=function(){if(a.userOwnerForm.$valid){a.loadingModal=!0;var r=null;if(a.userOwner.owners&&a.userOwner.owners.length){a.shared_users=null,a.shared_user_groups=null;for(var t=0;t<a.userOwner.owners.length;t++){var u=a.userOwner.owners[t];"user"===u.type&&(null===a.shared_users?a.shared_users=u.id:a.shared_users+=","+u.id),"group"===u.type&&(null===a.shared_user_groups?a.shared_user_groups=u.id:a.shared_user_groups+=","+u.id)}}if(a.userOwner.modules&&a.userOwner.modules.length)for(var l=0;l<a.userOwner.modules.length;l++){var d=a.userOwner.modules[l];null===r?r=d.name:r+=","+d.name}var o={user_id:a.userOwner.user,shared_user_id:a.userOwner.shared_user,users:a.shared_users,user_groups:a.shared_user_groups,modules:r};a.id?n.update(a.id,o).then(function(){i.success(s("translate")("Setup.UserCustomShares.EditSuccess")),e.closeSide("sideModal"),a.$parent.$parent.grid.dataSource.read()})["catch"](function(){error(data,status),e.closeSide("sideModal"),a.loadingModal=!1}):n.create(o).then(function(){i.success(s("translate")("Setup.UserCustomShares.SaveSuccess")),a.$parent.$parent.grid.dataSource.read(),e.closeSide("sideModal")})["catch"](function(){error(data,status),a.loadingModal=!1})}else i.error(s("translate")("Module.RequiredError"))},a.usersOptions={dataSource:{transport:{read:function(e){e.success(p)}}},autoBind:!1,filter:"contains",dataValueField:"id",dataTextField:"full_name",optionLabel:s("translate")("Setup.Workflow.ApprovelProcess.SelectUser"),template:"<span>{{dataItem.full_name}} - {{dataItem.email}}</span>",valueTemplate:"<span>{{dataItem.full_name}} - {{dataItem.email}}</span>"},a.modulesOptions={dataSource:s("filter")(e.modules,function(e){return"users"!==e.name&&"profiles"!==e.name&&"roles"!==e.name}),filter:"contains",dataTextField:"languages."+e.globalization.Label+".label.plural",dataValueField:"id"}}}]);