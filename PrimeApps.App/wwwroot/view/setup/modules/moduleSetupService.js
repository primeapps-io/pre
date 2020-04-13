"use strict";angular.module("primeapps").factory("ModuleSetupService",["$rootScope","$http","config","$filter","$q","helper","defaultLabels","$cache","dataTypes","systemFields",function(e,a,l,t,i,n,r,d,o,s){return{getDataTypes:function(){e.dataTypesExtended=angular.copy(o);var a={};a.name="combination",a.label={},a.label.en=r.DataTypeCombinationEn,a.label.tr=r.DataTypeCombinationTr,a.order=14,e.dataTypesExtended.combination=a;var l=[];return angular.forEach(e.dataTypesExtended,function(e){switch(e.name){case"text_single":case"map":case"location":case"rating":case"email":e.maxLength=3;break;case"text_multi":e.maxLength=4;break;case"number":case"number_decimal":case"currency":e.maxLength=2}switch(e.name){case"text_single":e.max=400;break;case"email":e.max=100;break;case"text_multi":e.max=2e3;break;case"number":case"number_decimal":case"currency":e.max=27;break;case"rating":break;case"tag":break;case"json":}l.push(e)}),l},getPicklist:function(e){if(e>=9e5){var t=i.defer();return t.resolve({data:{items:[]}}),t.promise}return a.get(l.apiUrl+"picklist/get/"+e)},getAllModuleProfileSettings:function(){return a.get(l.apiUrl+"module_profile_settings/get_all")},createModuleProfileSetting:function(e){return a.post(l.apiUrl+"module_profile_settings/create",e)},updateModuleProfileSetting:function(e,t){return a.put(l.apiUrl+"module_profile_settings/update/"+e,t)},deleteModuleProfileSetting:function(e){return a["delete"](l.apiUrl+"module_profile_settings/delete/"+e)},getPicklists:function(){return a.get(l.apiUrl+"picklist/get_all")},createPicklist:function(e){return a.post(l.apiUrl+"picklist/create",e)},updatePicklist:function(e){return a.put(l.apiUrl+"picklist/update/"+e.id,e)},processPicklist:function(a){return a.label=a["label_"+e.language],angular.forEach(a.items,function(a){a.label=a["label_"+e.language],a.track=parseInt(a.id+"00000")}),a.items=t("orderBy")(a.items,"order"),a},preparePicklist:function(e){return e.label_en=e.label,e.label_tr=e.label,angular.forEach(e.items,function(e){e.label_en=e.label,e.label_tr=e.label}),e},processModule:function(a){return angular.forEach(a.fields,function(a){a.combination&&(a.data_type="combination",a.dataType=e.dataTypesExtended.combination,a.combinationField1=a.combination.field1,a.combinationField2=a.combination.field2,a.combinationCharacter=a.combination.combination_character)}),a},prepareDefaults:function(a){a.system_type="custom",a.label_en_plural=r.DefaultModuleNameEn,a.label_en_singular=r.DefaultModuleNameEn,a.label_tr_plural=r.DefaultModuleNameTr,a.label_tr_singular=r.DefaultModuleNameTr,a.display=!0,a.sharing="private",a.deleted=!1;var l=[];angular.forEach(e.modules,function(e){l.push(e.order)}),a.order=Math.max.apply(null,l)+1,a.name="custom_module"+a.order,a.sections=[],a.fields=[];var t={};t.system_type="custom",t.column_count=2,t.name="custom_section1",t.label_en=r.DefaultSectionNameEn,t.label_tr=r.DefaultSectionNameTr,t.order=1,t.display_detail=!0,t.deleted=!1,t.columns=[],t.columns.push({no:1}),t.columns.push({no:2}),a.sections.push(t);var i={};i.system_type="custom",i.column_count=2,i.name="custom_section2",i.label_en=r.SystemInfoSectionNameEn,i.label_tr=r.SystemInfoSectionNameTr,i.order=2,i.display_detail=!0,i.deleted=!1,i.columns=[],i.columns.push({no:1}),i.columns.push({no:2}),a.sections.push(i);var n={};n.system_type="system",n.data_type="lookup",n.dataType=e.dataTypesExtended.lookup,n.section="custom_section1",n.section_column=1,n.name="owner",n.label_en="Owner",n.label_tr="Kayıt Sahibi",n.order=1,n.primary=!1,n.display_detail=!0,n.display_list=!0,n.validation={},n.validation.required=!0,n.validation.readonly=!1,n.inline_edit=!0,n.editable=!0,n.show_label=!0,n.lookup_type="users",n.deleted=!1,n.systemReadonly=!0,n.systemRequired=!0,a.fields.push(n);var d={};d.system_type="custom",d.data_type="text_single",d.dataType=e.dataTypesExtended.text_single,d.section="custom_section1",d.section_column=2,d.name="custom_field2",d.label_en=r.DefaultFieldNameEn,d.label_tr=r.DefaultFieldNameTr,d.order=2,d.primary=!0,d.display_detail=!0,d.display_list=!0,d.validation={},d.validation.required=!0,d.validation.readonly=!1,d.inline_edit=!0,d.editable=!0,d.show_label=!0,d.deleted=!1,a.fields.push(d);var o={};o.system_type="system",o.data_type="lookup",o.dataType=e.dataTypesExtended.lookup,o.section="custom_section2",o.section_column=1,o.name="created_by",o.label_en=r.CreatedByFieldEn,o.label_tr=r.CreatedByFieldTr,o.order=3,o.primary=!1,o.display_detail=!0,o.display_list=!0,o.validation={},o.validation.required=!0,o.validation.readonly=!0,o.inline_edit=!1,o.editable=!0,o.show_label=!0,o.lookup_type="users",o.deleted=!1,o.systemReadonly=!0,o.systemRequired=!0,a.fields.push(o);var s={};s.system_type="system",s.data_type="date_time",s.dataType=e.dataTypesExtended.date_time,s.section="custom_section2",s.section_column=1,s.name="created_at",s.label_en=r.CreatedAtFieldEn,s.label_tr=r.CreatedAtFieldTr,s.order=4,s.primary=!1,s.display_detail=!0,s.display_list=!0,s.validation={},s.validation.required=!0,s.validation.readonly=!0,s.inline_edit=!1,s.editable=!0,s.show_label=!0,s.deleted=!1,s.systemReadonly=!0,s.systemRequired=!0,a.fields.push(s);var u={};u.system_type="system",u.data_type="lookup",u.dataType=e.dataTypesExtended.lookup,u.section="custom_section2",u.section_column=2,u.name="updated_by",u.label_en=r.UpdatedByFieldEn,u.label_tr=r.UpdatedByFieldTr,u.order=5,u.primary=!1,u.display_detail=!0,u.display_list=!0,u.validation={},u.validation.required=!0,u.validation.readonly=!0,u.inline_edit=!1,u.editable=!0,u.show_label=!0,u.lookup_type="users",u.deleted=!1,u.systemReadonly=!0,u.systemRequired=!0,a.fields.push(u);var p={};return p.system_type="system",p.data_type="date_time",p.dataType=e.dataTypesExtended.date_time,p.section="custom_section2",p.section_column=2,p.name="updated_at",p.label_en=r.UpdatedAtFieldEn,p.label_tr=r.UpdatedAtFieldTr,p.order=6,p.primary=!1,p.display_detail=!0,p.display_list=!0,p.validation={},p.validation.required=!0,p.validation.readonly=!0,p.inline_edit=!1,p.editable=!0,p.show_label=!0,p.deleted=!1,p.systemReadonly=!0,p.systemRequired=!0,a.fields.push(p),a},getModuleLayout:function(e){var a={};return a.rows=[],e.sections=t("orderBy")(e.sections,"order"),e.fields=t("orderBy")(e.fields,"order"),angular.forEach(e.sections,function(l){if(!l.deleted){var i={};i.section=l,i.order=l.order,i.columns=[],l.columns=t("orderBy")(l.columns,"order"),angular.forEach(l.columns,function(a){var t={};t.column=a,t.cells=[],angular.forEach(e.fields,function(e){if(e.section==l.name&&e.section_column==a.no&&!e.deleted){var n={};n.field=e,n.order=e.order,t.cells.push(n),e.primary&&(i.hasPrimaryField=!0),e.systemRequired&&(i.hasSystemRequiredField=!0)}}),i.columns.push(t)}),a.rows.push(i)}}),a},refreshModule:function(e,a){for(var l=e.rows,t=[],i=[],n=0,r=0;r<l.length;r++){var d=l[r],o=d.section;o.order=r+1,t.push(o),delete d.hasPrimaryField,delete d.hasSystemRequiredField;for(var s=0;s<d.columns.length;s++)for(var u=d.columns[s],p=0;p<u.cells.length;p++){var c=u.cells[p],_=c.field;_.section=o.name,_.section_column=u.column.no,_.order=n+1,n=_.order,i.push(_),_.primary&&(d.hasPrimaryField=!0),_.systemRequired&&(d.hasSystemRequiredField=!0)}}a.sections=t,a.fields=i},prepareModule:function(a,l,i){var r="en"===e.language?"tr":"en";if(a.name.indexOf("custom_module")>-1){a["label_"+r+"_plural"]=a["label_"+e.language+"_plural"],a["label_"+r+"_singular"]=a["label_"+e.language+"_singular"],a.name=n.getSlug(a["label_"+e.language+"_plural"]);for(var d=e.modules.concat(i),o=2;;){var u=a.name.match(/(\D+)?\d/),p=u?u[0].length-1:-1,c=0===p?"n"+a.name:a.name,_=t("filter")(d,{name:c},!0)[0];if(!_)break;if(20>o)a.name=n.getSlug(a["label_"+e.language+"_plural"])+o,o++;else{var m=new Date;a.name=n.getSlug(a["label_"+e.language+"_plural"])+m.getTime()}}}return a["label_"+r+"_plural"]=a["label_"+e.language+"_plural"],a["label_"+r+"_singular"]=a["label_"+e.language+"_singular"],angular.forEach(a.sections,function(l){if(delete l.columns,l.name.indexOf("custom_section")>-1){var i=n.getSlug(l["label_"+e.language]),r=t("filter")(a.fields,{section:l.name},!0);angular.forEach(r,function(e){e.section=i});var d=angular.copy(i),o=t("filter")(a.sections,{name:d},!0)[0];if(o){do{var s;if(o.name.indexOf("_")>-1){var u=o.name.split("_"),p=u[u.length-1];s=p.replace(/\D/g,""),p=p.replace(/[0-9]/g,""),u.pop(),d=u.join("_")+"_"+p}else s=o.name.replace(/\D/g,""),d=o.name.replace(/[0-9]/g,"");var c="";c=s?d+(parseInt(s)+1):d+2,o=t("filter")(a.sections,{name:c},!0)[0],o||(l.name=c)}while(o)}else l.name=i}var _=angular.copy(l.permissions),m=[];angular.forEach(_,function(e){(e.id||"full"!=e.type)&&m.push(e)}),l.permissions=m.length>0?m:void 0}),angular.forEach(a.fields,function(l){if("combination"===l.data_type){l.data_type="text_single",l.combination={};var i=l.combinationField1,r=l.combinationField2;if(i.indexOf("custom_field")>-1){var d=t("filter")(a.fields,{name:i},!0)[0];i=n.getSlug(d["label_"+e.language])}if(r.indexOf("custom_field")>-1){var o=t("filter")(a.fields,{name:r},!0)[0];r=n.getSlug(o["label_"+e.language])}l.combination.field1=i,l.combination.field2=r,l.combination.combination_character=l.combinationCharacter,delete l.combinationCharacter,l.validation||(l.validation={}),l.validation.readonly=!0,l.inline_edit=!1,delete l.combinationField1,delete l.combinationField2}if("number_auto"===l.data_type&&(l.validation||(l.validation={}),l.validation.readonly=!0,l.inline_edit=!1),"related_module"===l.name&&(l.picklist_id=l.picklist_original_id),l.unique_combine&&l.unique_combine.indexOf("custom_field")>-1){var s=t("filter")(a.fields,{name:l.unique_combine},!0)[0];l.unique_combine=n.getSlug(s["label_"+e.language])}if("url"!==l.data_type||l.validation&&l.validation.pattern||(l.validation||(l.validation={}),l.validation.pattern="^(https?|ftp)://.*$"),"location"===l.data_type&&(l.validation||(l.validation={}),l.validation.readonly=!0,l.inline_edit=!1),l.encrypted&&l.encryption_authorized_users&&l.encryption_authorized_users.length>0){for(var u=null,p=0;p<l.encryption_authorized_users.length;p++){var c=l.encryption_authorized_users[p];null===u?u=c.id:u+=","+c.id}l.encryption_authorized_users=u}(!l.encrypted||l.encrypted&&l.encryption_authorized_users.length<1)&&(l.encryption_authorized_users=null,l.encryption_authorized_users_list=null),delete l.show_lock;var _=angular.copy(l.permissions),m=[];angular.forEach(_,function(e){(e.id||"full"!=e.type)&&m.push(e)}),l.permissions=m.length>0?m:void 0}),angular.forEach(a.fields,function(l){if(delete l.dataType,delete l.operators,delete l.systemRequired,delete l.systemReadonly,delete l.valueFormatted,l.name.indexOf("custom_field")>-1){var i=n.getSlug(l["label_"+e.language]);s.indexOf(i)>-1&&(i+="_c");var r=angular.copy(i),d=t("filter")(a.fields,{name:i},!0)[0];if(d){do{var o;if(d.name.indexOf("_")>-1){var u=d.name.split("_"),p=u[u.length-1];o=p.replace(/\D/g,""),p=p.replace(/[0-9]/g,""),u.pop(),r=u.join("_")+"_"+p}else o=d.name.replace(/\D/g,""),r=d.name.replace(/[0-9]/g,"");var c="";c=o?r+(parseInt(o)+1):r+2,d=t("filter")(a.fields,{name:c},!0)[0],d||(l.name=c)}while(d)}else l.name=i}}),delete a.relations,delete a.dependencies,delete a.calculations,a},getDeletedModules:function(){var e=i.defer(),t=d.get("modulesDeleted");return t?e.resolve(t):a.get(l.apiUrl+"module/get_all_deleted").then(function(a){d.put("modulesDeleted",a.data),e.resolve(a.data)}),e.promise},getFields:function(a){var l={};if(l.selectedFields=[],l.availableFields=[],l.allFields=[],!a.relatedModule)return l;var i=angular.copy(a.relatedModule.fields);i=t("filter")(i,{display_list:!0,lookup_type:"!relation"},!0);var n={};n.name="seperator-main",n.label="tr"===e.language?a.relatedModule.label_tr_singular:a.relatedModule.label_en_singular,n.order=0,n.seperator=!0,i.push(n);var r=0;return angular.forEach(i,function(a){if("lookup"===a.data_type&&"relation"!=a.lookup_type){var l=angular.copy(t("filter")(e.modules,{name:a.lookup_type},!0)[0]);if(r+=100,null===l||void 0===l)return;var n={};n.name="seperator-"+l.name,n.order=r,n.seperator=!0,n.label="tr"===e.language?l.label_tr_singular+" ("+a.label_tr+")":l.label_en_singular+" ("+a.label_en+")",i.push(n);var d=angular.copy(l.fields);d=t("filter")(d,{display_list:!0},!0),angular.forEach(d,function(t){"lookup"!==t.data_type&&(t.label="tr"===e.language?t.label_tr:t.label_en,t.labelExt="("+a.label+")",t.name=a.name+"."+l.name+"."+t.name,t.order=parseInt(t.order)+r,i.push(t))})}}),angular.forEach(i,function(e){if(!e.deleted){var n=null;if(a.display_fields){var r=t("filter")(a.display_fields,e.name,!0)[0];r&&(n=t("filter")(i,{name:r},!0)[0])}var d={};if(d.name=e.name,d.label=e.label,d.labelExt=e.labelExt,d.order=e.order,d.lookup_type=e.lookup_type,d.seperator=e.seperator,d.multiline_type=e.multiline_type,n)d.order=n.order,l.selectedFields.push(d);else{var o=t("filter")(i,{primary:!0},!0)[0];e.name!=o.name?l.availableFields.push(d):l.selectedFields.push(d)}l.allFields.push(d)}}),l.selectedFields=t("orderBy")(l.selectedFields,"order"),l.availableFields=t("orderBy")(l.availableFields,"order"),l},deleteFieldsMappings:function(e){return a.post(l.apiUrl+"convert/delete_fields_mappings/",e)},processRelations:function(a){return angular.forEach(a,function(a){var l=t("filter")(e.modules,{name:a.related_module},!0)[0];if(!l||0===l.order)return void(a.deleted=!0);a.relatedModule=l;var i=t("filter")(l.fields,{name:a.relation_field,deleted:!1},!0)[0];return i||"one_to_many"!==a.relation_type?void("one_to_many"===a.relation_type&&(a.relationField=i)):void(a.deleted=!0)}),a},prepareRelation:function(a){a.related_module=a.relatedModule.name,a.relationField&&(a.relation_field=a.relationField.name),delete a.relatedModule,delete a.relationField,delete a.hasRelationField,delete a.type;var l="en"===e.language?"tr":"en";a["label_"+l]||(a["label_"+l+"_plural"]=a["label_"+e.language+"_plural"],a["label_"+l+"_singular"]=a["label_"+e.language+"_singular"])},processDependencies:function(e){e.dependencies||(e.dependencies=[]),e.display_dependencies||(e.display_dependencies=[]);var a=[];return angular.forEach(e.dependencies,function(l){if(l.type=l.dependency_type,l.parentField=t("filter")(e.fields,{name:l.parent_field,deleted:"!true"})[0],l.childField=t("filter")(e.fields,{name:l.child_field,deleted:"!true"})[0],l.sectionField=t("filter")(e.sections,{name:l.child_section,deleted:"!true"})[0],"display"===l.dependency_type){if(l.dependencyType="display",l.name=t("translate")("Setup.Modules.DependencyTypeDisplay"),l.values&&!Array.isArray(l.values)){var i=l.values.split(",");l.values=[],angular.forEach(i,function(e){l.values.push(parseInt(e))})}}else if("freeze"===l.dependency_type){if(l.dependencyType="freeze",l.name=t("translate")("Setup.Modules.DependencyTypeFreeze"),l.values&&!Array.isArray(l.values)){var i=l.values.split(",");l.values=[],angular.forEach(i,function(e){l.values.push(parseInt(e))})}}else if(l.dependencyType="value",l.name=t("translate")("Setup.Modules.DependencyTypeValueChange"),l.field_map_parent&&(l.field_map={},l.field_map.parent_map_field=l.field_map_parent,l.field_map.child_map_field=l.field_map_child),l.value_map&&!l.value_maps){l.value_maps={};var n=l.value_map.split("|");angular.forEach(n,function(e){var a=e.split(";");l.value_maps[a[0]]=a[1].split(",")})}l.parentField&&l.childField&&a.push(l)}),a},prepareDependency:function(e){switch(e.dependencyType){case"display":var a={};return a.id=e.id,a.dependency_type="display",a.parent_field=e.parent_field,a.child_field=e.child_field,a.child_section=e.child_section,a.values=e.values,a;case"freeze":var a={};return a.id=e.id,a.dependency_type="freeze",a.parent_field=e.parent_field,a.child_field=e.child_field,a.child_section=e.child_section,a.values=e.values,a;case"value":var l={};switch(l.id=e.id,l.dependency_type=e.type,l.parent_field=e.parent_field,l.child_field=e.child_field,e.type){case"list_text":l.clear=e.clear;break;case"list_value":l.value_map="",angular.forEach(e.value_maps,function(e,a){l.value_map+=a+";",angular.forEach(e,function(e){l.value_map+=e+","}),l.value_map=l.value_map.slice(0,-1)+"|"}),l.value_map=l.value_map.slice(0,-1);break;case"list_field":case"lookup_list":l.field_map_parent=e.field_map.parent_map_field,l.field_map_child=e.field_map.child_map_field}return l}},createActionButton:function(e){return a.post(l.apiUrl+"action_button/create",e)},updateActionButton:function(e){return a.put(l.apiUrl+"action_button/update/"+e.id,e)},deleteActionButton:function(e){return a["delete"](l.apiUrl+"action_button/delete/"+e)},updateField:function(e,t){return a.put(l.apiUrl+"module/update_field/"+e,t)}}}]),angular.module("primeapps").constant("defaultLabels",{DefaultModuleNameEn:"Module",DefaultModuleNameTr:"Modül",DefaultSectionNameEn:"Section",DefaultSectionNameTr:"Bölüm",SystemInfoSectionNameEn:"System Information",SystemInfoSectionNameTr:"Sistem Bilgisi",DefaultFieldNameEn:"Name",DefaultFieldNameTr:"İsim",UserLookupFieldEn:"User",UserLookupFieldTr:"Kullanıcı",ProfileLookupFieldEn:"Profile",ProfileLookupFieldTr:"Profil",RoleLookupFieldEn:"Role",RoleLookupFieldTr:"Rol",CreatedByFieldEn:"Created by",CreatedByFieldTr:"Oluşturan",UpdatedByFieldEn:"Updated by",UpdatedByFieldTr:"Güncelleyen",CreatedAtFieldEn:"Created at",CreatedAtFieldTr:"Oluşturulma Tarihi",UpdatedAtFieldEn:"Updated at",UpdatedAtFieldTr:"Güncellenme Tarihi",DefaultPicklistItemEn:"Option",DefaultPicklistItemTr:"Seçenek",DataTypeCombinationEn:"Combination",DataTypeCombinationTr:"Birleşim",DataTypeCalculatedEn:"Calculated",DataTypeCalculatedTr:"Hesaplama"});