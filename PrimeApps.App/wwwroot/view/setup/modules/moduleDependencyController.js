"use strict";angular.module("primeapps").controller("ModuleDependencyController",["$rootScope","$scope","$filter","$state","$stateParams","helper","$cache","systemRequiredFields","systemReadonlyFields","ModuleSetupService","ModuleService","AppService","mdToast",function(e,n,l,d,i,a,t,c,r,s,p,u,o){var y=l("filter")(e.modules,{name:i.module},!0)[0];if(!y)return o.warning(l("translate")("Common.NotFound")),void d.go("app.dashboard");n.module=angular.copy(y),n.dependencies=l("filter")(s.processDependencies(n.module),{deleted:!1},!0),n.dependenciesState=angular.copy(n.dependencies),n.sections=n.module.sections,n.affectedAreaType="field",p.getPicklists(n.module).then(function(e){n.picklistsModule=angular.copy(e)}),angular.forEach(n.dependencies,function(e){(r.all.indexOf(e.parentField.name)>-1||r[n.module.name]&&r[n.module.name].indexOf(e.parentField.name)>-1)&&(e.hidden=!0)}),n.getPicklist=function(){var e=l("filter")(n.module.fields,{name:n.currentDependency.parent_field},!0)[0],d=l("filter")(n.picklistsModule[e.picklist_id],{inactive:"!true"});return d};var f=function(){function e(e){return c.all.indexOf(e.name)>-1||c[n.module.name]&&c[n.module.name].indexOf(e.name)>-1?!0:!1}n.parentDisplayFields=[],n.parentValueFields=[],n.childValueListFields=[],n.childValueTextFields=[],n.childDisplayFields=[],n.picklistFields=[],angular.forEach(n.module.fields,function(d){if(!e(d)){var i=l("filter")(n.dependencies,{childField:{name:d.name},dependencyType:"display"},!0)[0],a=l("filter")(n.dependencies,{childField:{name:d.name},dependencyType:"value"},!0)[0];if(i||n.childDisplayFields.push(d),!a)switch(d.data_type){case"picklist":n.parentDisplayFields.push(d),n.parentValueFields.push(d),n.childValueListFields.push(d);break;case"multiselect":n.parentDisplayFields.push(d),n.childValueListFields.push(d);break;case"checkbox":n.parentDisplayFields.push(d),n.childValueListFields.push(d);break;case"text_single":case"text_multi":case"number":case"number_decimal":case"currency":case"email":n.childValueTextFields.push(d)}"picklist"===d.data_type&&n.picklistFields.push(d)}})},h=function(){var e={};e.value="display",e.label=l("translate")("Setup.Modules.DependencyTypeDisplay");var d={};d.value="value",d.label=l("translate")("Setup.Modules.DependencyTypeValueChange");var i={};i.value="freeze",i.label="Kayıt Dondurma",n.dependencyTypes=[],n.dependencyTypes.push(e),n.dependencyTypes.push(d),n.dependencyTypes.push(i)},m=function(){var e={};e.value="list_text",e.label=l("translate")("Setup.Modules.ValueChangeTypeStandard");var d={};d.value="list_value",d.label=l("translate")("Setup.Modules.ValueChangeTypeValueMapping");var i={};i.value="list_field",i.label=l("translate")("Setup.Modules.ValueChangeTypeFieldMapping"),n.valueChangeTypes=[],n.valueChangeTypes.push(e),n.valueChangeTypes.push(d),n.valueChangeTypes.push(i)};f(),h(),m(),n.dependencyTypeChanged=function(){"value"===n.currentDependency.dependencyType&&(n.currentDependency.type="list_text"),n.currentDependency.parent_field=null,n.currentDependency.child_field=null,n.currentDependency.child_section=null},n.valueChangeTypeChanged=function(){switch(n.currentDependency.type){case"list_value":n.currentDependency.value_maps={},n.currentDependency.values=[];break;case"list_field":n.currentDependency.field_map={}}},n.affectedAreaTypeChanged=function(){n.currentDependency.child_field=null,n.currentDependency.child_section=null},n.parentValueChanged=function(){n.currentDependency.child_field=null,n.currentDependency.child_section=null,n.currentDependency.value_maps={},n.currentDependency.values=[],n.currentDependency.field_map={}},n.getParentFields=function(){switch(n.currentDependency.dependencyType){case"display":return n.parentDisplayFields;case"freeze":return n.parentDisplayFields;case"value":return"list_value"===n.currentDependency.type?n.picklistFields:n.parentValueFields}},n.getChildFields=function(){switch(n.currentDependency.dependencyType){case"display":return angular.forEach(n.childDisplayFields,function(e){delete e.hidden,(e.name===n.currentDependency.parent_field||e.deleted)&&(e.hidden=!0)}),n.childDisplayFields;case"freeze":return angular.forEach(n.childDisplayFields,function(e){delete e.hidden,(e.name===n.currentDependency.parent_field||e.deleted)&&(e.hidden=!0)}),n.childDisplayFields;case"value":return"list_text"===n.currentDependency.type?(angular.forEach(n.childValueTextFields,function(e){delete e.hidden,e.name===n.currentDependency.parent_field&&(e.hidden=!0)}),n.childValueTextFields):(angular.forEach(n.childValueListFields,function(e){delete e.hidden,e.name===n.currentDependency.parent_field&&(e.hidden=!0)}),n.childValueListFields)}},n.getMappingOptions=function(){var e=l("filter")(n.module.fields,{name:n.currentDependency.parent_field},!0)[0],d=l("filter")(n.module.fields,{name:n.currentDependency.child_field},!0)[0],i=(l("filter")(n.module.sections,{name:n.currentDependency.child_section},!0)[0],l("filter")(n.picklistsModule[e.picklist_id],{inactive:"!true"})),a=l("filter")(n.picklistsModule[d.picklist_id],{inactive:"!true"});return angular.forEach(i,function(e){e.childPicklist=a}),i},n.showFormModal=function(e){if(e){{var d=l("filter")(n.module.fields,{name:e.childField.name},!0)[0];l("filter")(n.module.sections,{name:e.sectionField.name},!0)[0]}n.affectedAreaType=e.child_section?"section":"field";var i=l("filter")(n.childValueListFields,{name:d.name},!0)[0];i||n.childValueListFields.push(d);var a=l("filter")(n.childValueTextFields,{name:d.name},!0)[0];a||n.childValueTextFields.push(d);var t=l("filter")(n.childDisplayFields,{name:d.name},!0)[0];t||n.childDisplayFields.push(d)}else e={},e.dependencyType="display",e.isNew=!0;n.currentDependency=e,n.currentDependency.hasRelationField=!0,n.currentDependencyState=angular.copy(n.currentDependency)},n.saveSingularControlDependencyForm=function(){var e=angular.copy(n.currentDependency),d=!e.isNew,i=l("filter")(n.dependencies,{dependencyType:e.dependencyType},!0);if(d&&1==i.length||i.length<=0)return!0;switch(e.dependencyType){case"display":i.length>0&&("section"==n.affectedAreaType||e.child_section?i=l("filter")(i,{parent_field:e.parent_field,child_section:e.child_section},!0):("field"==n.affectedAreaType||e.child_field)&&(i=l("filter")(i,{parent_field:e.parent_field,child_field:e.child_field},!0)));break;case"value":i.length>0&&(i=l("filter")(i,{parent_field:e.parent_field,child_field:e.child_field,type:e.type},!0));break;case"freeze":i.length>0&&(i=l("filter")(i,{dependencyType:e.dependencyType,parent_field:e.parent_field},!0));break;default:return!0}return d?i.length<=1?!0:!1:i.length>0?!1:!0},n.save=function(d){if(d.$valid){var a=n.saveSingularControlDependencyForm();if(!a)return void o.warning(l("translate")("Setup.Modules.DependencySameData"));n.saving=!0;var c=angular.copy(n.currentDependency);c.isNew&&(delete c.isNew,n.dependencies||(n.dependencies=[]),n.dependencies.push(c));var r=l("filter")(n.module.fields,{name:n.currentDependency.parent_field},!0)[0];s.updateField(r.id,{inline_edit:!1});var y=s.prepareDependency(angular.copy(c),n.module),f=function(){u.getMyAccount(!0).then(function(){n.module=angular.copy(l("filter")(e.modules,{name:i.module},!0)[0]),n.dependencies=l("filter")(s.processDependencies(n.module),{deleted:!1},!0),angular.forEach(n.dependencies,function(e){!e.type||"list_field"!==e.type&&"list_value"!==e.type||t.remove("picklist_"+e.childField.picklist_id)}),o.success(l("translate")("Setup.Modules.DependencySaveSuccess")),n.saving=!1,n.formModal.hide()})},h=function(){n.dependencies=n.dependenciesState,n.formModal&&(n.formModal.hide(),n.saving=!1)};y.id?p.updateModuleDependency(y,n.module.id).then(function(){f()})["catch"](function(){h()}):p.createModuleDependency(y,n.module.id).then(function(){f()})["catch"](function(){h()})}},n["delete"]=function(e){delete e.$$hashKey;var d=angular.copy(n.dependencies),i=a.arrayObjectIndexOf(d,e);d.splice(i,1),p.deleteModuleDependency(e.id).then(function(){u.getMyAccount(!0).then(function(){var d=a.arrayObjectIndexOf(n.dependencies,e);n.dependencies.splice(d,1),!e.type||"list_field"!==e.type&&"list_value"!==e.type||t.remove("picklist_"+e.childField.picklist_id),o.success(l("translate")("Setup.Modules.DependencyDeleteSuccess"))})})["catch"](function(){n.dependencies=n.dependenciesState,n.formModal&&(n.formModal.hide(),n.saving=!1)})},n.cancel=function(){angular.forEach(n.currentDependency,function(e,l){n.currentDependency[l]=n.currentDependencyState[l]}),n.formModal.hide()}}]);