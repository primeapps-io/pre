"use strict";angular.module("primeapps").factory("ProfileService",["$rootScope","$http","config","$filter","entityTypes",function(e,r,t,i){return{getAll:function(){return r.post(t.apiUrl+"Profile/GetAll",{})},getAllBasic:function(){return r.get(t.apiUrl+"Profile/GetAllBasic")},getProfiles:function(r,t,n){var a=r;return angular.forEach(a,function(r){if(n)r.permissions&&(r.permissions=[]);else{var t=[];angular.forEach(r.permissions,function(r){var n=!0;switch(r.type){case 1:r.EntityTypeName=i("translate")("Layout.Menu.Documents"),r.order=999;break;case 2:r.EntityTypeName=i("translate")("Layout.Menu.Views"),r.order=1e3;break;case 3:r.EntityTypeName=i("translate")("Feed.Label"),r.Order=1001;break;case 0:var a=i("filter")(e.modules,{id:r.module_id},!0)[0];a&&a.order>0?(r.EntityTypeName=e.getLanguageValue(a.languages,"label","plural"),r.order=a.order):n=!1}n&&t.push(r)}),r.permissions=i("orderBy")(t,"Order")}}),a=i("orderBy")(a,["-is_persistent","-has_admin_rights","+name"])},changeUserProfile:function(e,i,n){return r.post(t.apiUrl+"Profile/ChangeUserProfile",{User_ID:e,tenant_id:i,Transfered_Profile_ID:n})},changeUsersProfile:function(e,i,n){return r.post(t.apiUrl+"Profile/bulk_user_profile_update",{User_Id_list:e,tenant_id:i,Transfered_Profile_ID:n})},create:function(e){return r.post(t.apiUrl+"Profile/Create",e)},update:function(e){return r.post(t.apiUrl+"Profile/Update",e)},remove:function(e,i,n){return r.post(t.apiUrl+"Profile/Remove",{removed_profile:{id:e,InstanceID:n},transfer_profile:{id:i,InstanceID:n}})}}}]);