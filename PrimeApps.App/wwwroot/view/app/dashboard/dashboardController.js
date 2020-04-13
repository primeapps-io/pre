"use strict";function getCode(e,a){var t=new XMLHttpRequest;t.open("GET",e,!1),t.onload=function(){return 200==this.status?a(t.responseText):void 0},t.send({})}var runController=function(code,callback){return angular.isObject(code)?(eval(code),callback(!0)):void getCode(code.url,function(result){return eval(result),callback(result)})};angular.module("primeapps").controller("DashboardController",["$rootScope","$scope","helper","$filter","$cache","DashboardService","ModuleService","$window","$state","$mdDialog","$timeout","HelpService","$sce","$mdSidenav","mdToast",function(e,a,t,r,n,o,s,l,d,i,c,h,u,b,p){if(e.$on("error",function(e,a){p.warning(r("translate")(a))}),a.currentPath.indexOf("/dashboard")<0)if(a.menu){var m=r("filter")(a.menu,{route:"dashboard"},!0)[0];a.openSubMenu(m,a.menu)}else a.$parent.currentPath="/app/dashboard";a.hasPermission=t.hasPermission,a.isFullScreen=!0,a.loading=!0,a.loadingchanges=!1,a.disableSaveBtn=!0,a.showNewDashboardSetting=r("filter")(e.moduleSettings,{key:"new_dashboard"},!0)[0],e.breadcrumblist=[{title:r("translate")("Layout.Menu.Dashboard")}],e.dashboardHelpSide||h.getByType("sidemodal",null,"/app/dashboard").then(function(a){e.dashboardHelpSide=a.data}),l.scrollTo(0,0);var D=e.user.profile.start_page.toLowerCase();if("dashboard"!=D)return void(window.location="#/app/"+D);a.colorPaletOptions={columns:6,palette:["#D24D57","#BE90D4","#5AABE3","#87D37C","#F4D03E","#B8BEC2","#DC3023","#8E44AD","#19B5FE","#25A65B","#FFB61E","#959EA4","#C3272B","#763668","#1F4688","#006442","#CA6924","#4D5C66"]},a.sideModalLeft=function(){e.buildToggler("sideModal","view/app/dashboard/dashboardOrderChange.html",a)},a.showConfirm=function(e,t){var n=i.confirm().title(r("translate")("Common.AreYouSure")).targetEvent(e).ok(r("translate")("Common.Yes")).cancel(r("translate")("Common.No"));i.show(n).then(function(){a.dashletDelete(t)},function(){a.status="You decided to keep your debt."})},a.endHandler=function(e){e.sender;e.sender.draggable.dropped=!0,e.preventDefault(),c(function(){a.$apply(function(){a.dashlets.splice(e.newIndex,0,a.dashlets.splice(e.oldIndex,1)[0])})}),a.disableSaveBtn=!1},e.sortableOptions={placeholder:function(e){return e.clone().addClass("sortable-list-placeholder").text("Drop here")},hint:function(e){return e.clone().addClass("sortable-list-hint")},cursorOffset:{top:-10,left:20}},a.showFullScreen=function(e){a.fullScreenDashlet=a.fullScreenDashlet?null:e},a.openMenu=function(){},a.DashletTypes=[{label:r("translate")("Dashboard.Chart"),name:"chart"},{label:r("translate")("Dashboard.Widget"),name:"widget"}];var f=r("translate")("Dashboard.Column");a.dashletWidths=[{label:"1 "+f,value:3},{label:"2 "+f,value:6},{label:"3 "+f,value:9},{label:"4 "+f,value:12}],a.dashletHeights=[{label:"50 px",value:80},{label:"100 px",value:130},{label:"300 px",value:330},{label:"400 px",value:430},{label:"500 px",value:530},{label:"600 px",value:630}],a.getUser=function(a){var t=r("filter")(e.users,{id:a},!0)[0];return t.full_name?t.full_name:a},a.showComponent=[];var _=[];if(components)for(var g=JSON.parse(components),v=0;v<g.length;v++)_["component-"+g[v].Id]=g[v];a.loadDashboard=function(){var t=function(){a.activeDashboard=n.get("activeDashboard"),o.getDashlets(a.activeDashboard.id).then(function(t){t=t.data;for(var o=0;o<t.length;o++){var s=t[o];if("component"===s.dashlet_type&&(n.get("component-"+s.id)?runController(n.get(a.showComponent[s.id]),function(){a.componetiGoster=!0}):runController(!1,function(e){a.showComponent[s.id]=!0,n.put(n.get("component-"+id),e)})),"chart"===s.dashlet_type){if(s.chart_item.config={dataEmptyMessage:r("translate")("Dashboard.ChartEmptyMessage")},s.chart_item.chart.showPercentValues="1",s.chart_item.chart.showPercentInTooltip="0",s.chart_item.chart.animateClockwise="1",s.chart_item.chart.enableMultiSlicing="0",s.chart_item.chart.isHollow="0",s.chart_item.chart.is2D="0",s.chart_item.chart.formatNumberScale="0",s.chart_item.chart.xaxisname=s.chart_item.chart["xaxisname_"+e.user.language],s.chart_item.chart.yaxisname=s.chart_item.chart["yaxisname_"+e.user.language],s.chart_item.chart.caption=s.chart_item.chart["caption_"+e.user.language],"tr"===a.locale&&(s.chart_item.chart.decimalSeparator=",",s.chart_item.chart.thousandSeparator="."),"chart"===s.dashlet_type&&"owner"===s.chart_item.chart.report_group_field&&s.chart_item.data&&s.chart_item.data.data)for(var l=0;l<s.chart_item.data.data.length;l++)s.chart_item.data.data[l].label=a.getUser(parseInt(s.chart_item.data.data[l].label));var d=r("filter")(e.modules,{id:s.chart_item.chart.report_module_id},!0)[0];if(d){var i;if(s.chart_item.chart.report_aggregation_field.indexOf(".")<0)i=r("filter")(d.fields,{name:s.chart_item.chart.report_aggregation_field},!0)[0];else{var c=s.chart_item.chart.report_aggregation_field.split("."),h=r("filter")(e.modules,{name:c[1]},!0)[0];i=r("filter")(h.fields,{name:c[2]},!0)[0]}i&&"currency"===i.data_type&&(s.chart_item.chart.numberPrefix=e.currencySymbol),!i||"currency"!==i.data_type&&"number_decimal"!==i.data_type||(s.chart_item.chart.forceDecimals="1")}}}a.dashlets=t,n.put("dashlets",t)})["finally"](function(){a.loading=!1})};void 0===a.showNewDashboardSetting||null===a.showNewDashboardSetting||"true"===a.showNewDashboardSetting.value?(a.showNewDashboard=!0,t()):a.getSummaryJsonValue=function(e){var a=angular.fromJson(e);return a.x};for(var l=0;l<e.modules.length;l++)s.getPicklists(e.modules[l]);a.$on("sample-data-removed",function(){t()}),a.widgetDetail=function(e){0!=e.view_id&&(window.location="#/app/modules/"+e.widget_data.modulename+"?viewid="+e.view_id)},a.chartDetail=function(e){e&&(window.location="#/app/reports?id="+e)}};var w=n.get("dashlets"),y=n.get("activeDashboard"),S=n.get("dashboards"),C=n.get("dashboardprofile");w&&(a.loading=!1,a.dashlets=w),y&&S?(a.dashboards=S,a.activeDashboard=y,a.dashboardprofile=C,a.loadDashboard()):o.getDashboards().then(function(t){a.dashboards=t.data,a.activeDashboard=r("filter")(a.dashboards,{sharing_type:"me",user_id:e.user.id},!0)[0],a.activeDashboard||(a.activeDashboard=e.user.profile.has_admin_rights?r("filter")(a.dashboards,{sharing_type:"everybody"},!0)[0]:r("filter")(a.dashboards,{sharing_type:"profile",profile_id:e.user.profile.id},!0)[0],a.activeDashboard||(a.activeDashboard=r("filter")(a.dashboards,{sharing_type:"everybody"},!0)[0])),a.dashboardprofile=[],angular.forEach(e.profiles,function(t){var n=r("filter")(a.dashboards,{sharing_type:"profile",profile_id:t.id},!0)[0];n||(t.name=t["name_"+e.language],a.dashboardprofile.push(t))}),n.put("activeDashboard",a.activeDashboard),n.put("dashboards",a.dashboards),n.put("dashboardprofile",a.dashboardprofile),a.loadDashboard()}),a.changeDashboard=function(e){a.loading=!0,n.put("activeDashboard",e),a.loadDashboard()},a.hide=function(){i.hide()},a.cancel=function(){i.cancel()},a.dashboardformModal=function(e,t){a.currentDashboard=t?angular.copy(t):{};var r=angular.element(document.body);i.show({parent:r,templateUrl:"view/app/dashboard/formModal.html",clickOutsideToClose:!0,targetEvent:e,scope:a,preserveScope:!0})},a.cancelChangeOrder=function(){a.dashlets=a.currentDashlet},a.saveDashboard=function(t,s){if(s.preventDefault(),t.$submitted&&t.$valid){var l={};if(l["description_"+e.user.language]=a.currentDashboard["description_"+e.user.language],l["name_"+e.user.language]=a.currentDashboard["name_"+e.user.language],"en"===e.user.language?(l.name_tr=a.currentDashboard.id?a.currentDashboard.name_tr:a.currentDashboard.name_en,l.description_tr=a.currentDashboard.id?a.currentDashboard.description_tr:a.currentDashboard.description_en):(l.name_en=a.currentDashboard.id?a.currentDashboard.name_en:a.currentDashboard.name_tr,l.description_en=a.currentDashboard.id?a.currentDashboard.description_en:a.currentDashboard.description_tr),e.preview&&"en"===e.user.language?l.name_tr=a.currentDashboard.name_en:l.name_en=a.currentDashboard.name_tr,a.currentDashboard.id)l.id=a.currentDashboard.id,o.updateDashboard(l).then(function(e){for(var t=0;t<a.dashboards.length;t++)a.dashboards[t].id===e.data.id&&(a.dashboards[t]=e.data);i.cancel(),p.success(r("translate")("Dashboard.DashboardSaveSucces"))});else{l.profile_id=a.currentDashboard.profile_id,l.sharing_type=3,a.loading=!0;{angular.copy(a.activeDashboard)}o.createDashbord(l).then(function(){a.hide(),n.remove("dashlets"),n.remove("activeDashboard"),n.remove("dashboards"),n.remove("dashboardprofile"),d.reload(),i.cancel(),p.success(r("translate")("Dashboard.DashboardSaveSucces"))})}}},a.saveDashlet=function(e,t){if(t.preventDefault(),a.validator.validate()){var n={dashlet_type:a.currentDashlet.dashlet_type,dashboard_id:a.activeDashboard.id,y_tile_length:a.currentDashlet.y_tile_length,x_tile_height:a.currentDashlet.x_tile_height};n.name_tr=a.currentDashlet.name_tr,n.name_en=a.currentDashlet.name_en,"chart"===a.currentDashlet.dashlet_type?n.chart_id=a.currentDashlet.board:(n.widget_id=a.currentDashlet.board,n.y_tile_length=3,n.x_tile_height=150,n.view_id=a.currentDashlet.view_id,n.color=a.currentDashlet.color,n.icon=a.currentDashlet.icon),a.hide(),a.loading=!0,a.currentDashlet.id?o.dashletUpdate(a.currentDashlet.id,n).then(function(){a.loadDashboard(),p.success(r("translate")("Dashboard.DashletUpdateSucces"))}):(n.order=a.dashlets?a.dashlets.length:0,o.createDashlet(n).then(function(){a.loadDashboard(),p.success(r("translate")("Dashboard.DashletSaveSucces"))}))}else p.error(r("translate")("Module.RequiredError"))},a.dashletOrderSave=function(){a.loadingchanges=!0,a.sideModalLeft(),o.dashletOrderChange(a.dashlets).then(function(){a.loadDashboard(),a.loadingchanges=!1,p.success(r("translate")("Dashboard.DashletUpdateSucces")),e.closeSide("sideModal")}),a.disableSaveBtn=!0},a.openNewDashlet=function(e,t){a.currentDashlet={},t&&(a.currentDashlet=angular.copy(t),t.chart_item?(a.currentDashlet.name=a.currentDashlet.chart_item.chart.caption,a.currentDashlet.board=a.currentDashlet.chart_item.chart.id):(a.currentDashlet.name=a.currentDashlet.name,a.currentDashlet.board=a.currentDashlet.widget.id,a.currentDashlet.widget.view_id?(a.currentDashlet.dataSource="view",o.getView(a.currentDashlet.widget.view_id).then(function(e){a.currentDashlet.module_id=e.data.module_id,a.setViews(),a.currentDashlet.view_id=a.currentDashlet.widget.view_id,a.currentDashlet.color=a.currentDashlet.widget.color,a.currentDashlet.icon=a.currentDashlet.widget.icon})):a.currentDashlet.dataSource="report"),a.changeDashletType());var r=angular.element(document.body);i.show({parent:r,templateUrl:"view/app/dashboard/formModalDashlet.html",clickOutsideToClose:!0,targetEvent:e,scope:a,preserveScope:!0})},a.changeDashletType=function(){"chart"===a.currentDashlet.dashlet_type.name||"chart"===a.currentDashlet.dashlet_type?o.getCharts().then(function(e){a.boards=e.data,a.boardLabel=a.DashletTypes[0].label}):o.getWidgets().then(function(e){a.boards=e.data,a.boardLabel=r("translate")("Report.Single")})},a.selectModule=function(){a.setViews()},a.setViews=function(){o.getViews(a.currentDashlet.module_id).then(function(e){a.views=e.data})},a.dashletDelete=function(e){o.dashletDelete(e).then(function(){a.loadDashboard(),p.success(r("translate")("Dashboard.DashletDeletedSucces"))})},a.changeDashletMode=function(){a.dashletMode=a.dashletMode===!0?!1:!0},a.changeView=function(){a.currentDashlet.name_en=r("filter")(a.views,{id:a.currentDashlet.view_id},!0)[0].label_en,a.currentDashlet.name_tr=r("filter")(a.views,{id:a.currentDashlet.view_id},!0)[0].label_tr},a.changeBoard=function(){a.currentDashlet.name_en=r("filter")(a.boards,{id:a.currentDashlet.board},!0)[0].name_en,a.currentDashlet.name_tr=r("filter")(a.boards,{id:a.currentDashlet.board},!0)[0].name_tr},"undefined"!=typeof Tawk_API&&(Tawk_API.visitor={name:e.user.full_name,email:e.user.email})}]);