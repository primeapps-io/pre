"use strict";angular.module("primeapps").controller("ReportsController",["$rootScope","$scope","$location","$filter","ModuleService","ReportsService","blockUI","$stateParams","$state","moment","$mdSidenav","$mdDialog",function(e,t,r,a,n,l,o,s,i,u,c,d){function m(e,t){e.cancel=function(){t.cancel()}}t.openCategoryList=e.buildToggler2("reportmenu"),t.ReportCategory={},t.reportSearch="",t.limits=[10,25,50,100],t.getNumber=function(e){for(var t=[],r=1;e>r;r++)t.push(r);return t},t.newFilter={},window.outerWidth>768?t.multiplePanels=[0,1,2,3,4,5,6,7,8,9]:"";var p=o.instances.get("myBlockUI");l.getAllReports().then(function(e){if(t.Reports=a("filter")(e.data,{category_id:"!!"}),s.id){var r=a("filter")(t.Reports,{id:parseInt(s.id)},!0)[0];t.setReport(r)}else t.Reports.length>0?t.setReport(t.Reports[0]):""}),t.getAllCategory=function(){l.getAllCategory().then(function(e){t.ReportCateogryies=e.data})},t.getAllCategory(),t.categoryDelete=function(e){l.categoryDelete(e).then(function(){t.getAllCategory()})},t.collapseStatus=function(e){return t.reportSearch.length>3?!0:-1!==t.multiplePanels.indexOf(e)?!0:!1},t.changeReport=function(e){t.setReport(e)},t.setReport=function(e){if(e){switch(e.report_type){case"summary":t.currentReport=e,t.setSummary(e);break;case"tabular":t.currentReport=e,t.setReportTable(e);break;case"single":t.currentReport=e,t.setSingleReport(e)}document.getElementById("reportlist").scrollIntoView({block:"end"})}},t.setReportTable=function(r){t.currentTable={request:{},activePage:1,total:0,totalPage:0,activeLimit:t.limits[0],loading:!0},p.start(),t.reportView=r.fields,t.getReportData=l.getReportData(r.module_id,t.reportView);var a={fields:["total_count()"],limit:1,offset:0,filters:l.getFilters(r.filters,e.user)};t.currentTable.module=t.getReportData.module,t.getdateFileds(),n.findRecords(t.currentTable.module.name,a).then(function(a){return a.data[0]?(t.currentTable.total=a.data[0].total_count,t.currentTable.request.fields=[],t.currentTable.request.filters=l.getFilters(r.filters,e.user),t.defaultFilter=angular.copy(t.currentTable.request.filters),t.currentTable.request.sort_direction=r.sort_direction,t.currentTable.request.sort_field=r.sort_field,t.currentTable.request.limit=t.limits[0],t.currentTable.totalPage=Math.ceil(t.currentTable.total%t.currentTable.request.limit>0?t.currentTable.total/t.currentTable.request.limit+1:t.currentTable.total/t.currentTable.request.limit),t.reportView.forEach(function(e){t.currentTable.request.fields.push(e.field)}),void n.findRecords(t.getReportData.module.name,t.currentTable.request).then(function(e){var e=e.data;t.currentTable.displayFileds=t.getReportData.displayFileds,n.getPicklists(t.getReportData.module).then(function(a){t.currentTable.data=n.processRecordMulti(e,t.currentTable.module,a,t.reportView,t.currentTable.module.name),t.aggregations=r.aggregations,t.currentTable.aggregationsFields=l.getAggregationsFields(t.aggregations,t.getReportData.module.name,t.getReportData.displayFileds,r.filters),p.stop()})})):(p.stop(),t.currentTable.total=-1,!1)})},t.setSummary=function(r){p.start(),t.reportSummary={config:{dataEmptyMessage:a("translate")("Dashboard.ChartEmptyMessage")}},t.current={field:"",direction:""},l.getChart(r.id).then(function(n){t.reportSummary=n.data,t.reportSummary.config={dataEmptyMessage:a("translate")("Dashboard.ChartEmptyMessage")},t.reportSummary.chart.showPercentValues="1",t.reportSummary.chart.showPercentInTooltip="0",t.reportSummary.chart.animateClockwise="1",t.reportSummary.chart.enableMultiSlicing="0",t.reportSummary.chart.isHollow="0",t.reportSummary.chart.is2D="0",t.reportSummary.chart.formatNumberScale="0",t.reportSummary.chart.exportEnabled="1",t.reportSummary.chart.exportTargetWindow="_self",t.reportSummary.chart.exportFileName=t.reportSummary.chart["caption_"+e.user.language],t.reportSummary.chart.exportFormats="PNG="+a("translate")("Report.ExportImage")+"|PDF="+a("translate")("Report.ExportPdf")+"|XLS=Export Chart Data",t.reportSummary.chart.xaxisname=t.reportSummary.chart["xaxisname_"+e.user.language],t.reportSummary.chart.yaxisname=t.reportSummary.chart["yaxisname_"+e.user.language],t.reportSummary.chart.caption=t.reportSummary.chart["caption_"+e.user.language],"tr"===t.locale&&(t.reportSummary.chart.decimalSeparator=",",t.reportSummary.chart.thousandSeparator=".");var l=a("filter")(e.modules,{id:r.module_id},!0)[0];if(l){if(r.group_field.indexOf(".")<0){var o=a("filter")(l.fields,{name:r.group_field},!0)[0];t.reportSummary.groupField=o["label_"+e.language]}else{var s=r.group_field.split("."),i=a("filter")(e.modules,{name:s[1]},!0)[0],u=a("filter")(l.fields,{name:s[0]},!0)[0],c=a("filter")(i.fields,{name:s[2]},!0)[0];t.reportSummary.groupField=c["label_"+e.language]+" ("+u["label_"+e.language]+")"}var d;if(t.reportSummary.chart.report_aggregation_field.indexOf(".")<0)d=a("filter")(l.fields,{name:t.reportSummary.chart.report_aggregation_field},!0)[0];else{var m=t.reportSummary.chart.report_aggregation_field.split("."),b=a("filter")(e.modules,{name:m[1]},!0)[0];d=a("filter")(b.fields,{name:m[2]},!0)[0]}if(d&&"currency"===d.data_type&&(t.reportSummary.chart.numberPrefix=e.currencySymbol),!d||"currency"!==d.data_type&&"number_decimal"!==d.data_type||(t.reportSummary.chart.forceDecimals="1"),"users"===o.lookup_type)for(var g=0;g<t.reportSummary.data.length;g++)t.reportSummary.data[g].label=t.getUser(parseInt(t.reportSummary.data[g].label))}p.stop()})},t.getUser=function(t){var r=a("filter")(e.users,{id:t},!0)[0];return r.full_name?r.full_name:t},t.setSingleReport=function(e){p.start(),l.getWidget(e.id).then(function(r){t.reportSingle=r.data,t.reportSingle[0].type=t.reportSingle[0].type?a("translate")("Report."+t.reportSingle[0].type):e.name,p.stop()})},t.table={limitChange:function(e){t.currentTable.request.limit=e,t.currentTable.totalPage=Math.ceil(t.currentTable.total%t.currentTable.request.limit>0?t.currentTable.total/t.currentTable.request.limit+1:t.currentTable.total/t.currentTable.request.limit),this.reset(),this.run()},pageChange:function(e){t.currentTable.request.offset=(e-1)*t.currentTable.request.limit,t.currentTable.activePage=e,this.run()},shortChange:function(e){t.currentTable.request.sort_field===e?t.currentTable.request.sort_direction="desc"==t.currentTable.request.sort_direction?"asc":"desc":(t.currentTable.request.sort_direction="desc",t.currentTable.request.sort_field=e),this.reset(),this.run()},isSortBy:function(e,r){return t.currentTable.request.sort_field==e&&t.currentTable.request.sort_direction==r?!0:!1},reset:function(){t.currentTable.request.offset=0,t.currentTable.activePage=1},filterChange:function(){p.start();var e={fields:["total_count()"],limit:1,offset:0,filters:t.currentTable.request.filters};n.findRecords(t.currentTable.module.name,e).then(function(e){return e.data[0]?(t.currentTable.total=e.data[0].total_count,t.currentTable.totalPage=Math.ceil(t.currentTable.total%t.currentTable.request.limit>0?t.currentTable.total/t.currentTable.request.limit+1:t.currentTable.total/t.currentTable.request.limit),void n.findRecords(t.getReportData.module.name,t.currentTable.request).then(function(e){var e=e.data;t.currentTable.data=n.processRecordMulti(e,t.currentTable.module,null,t.reportView,t.currentTable.module.name),t.currentTable.aggregationsFields=l.getAggregationsFields(t.aggregations,t.getReportData.module.name,t.getReportData.displayFileds,t.currentTable.request.filters),p.stop()})):(p.stop(),t.currentTable.total=-1,!1)})},run:function(){p.start(),n.findRecords(t.currentTable.module.name,t.currentTable.request).then(function(e){var e=e.data;t.currentTable.data=n.processRecordMulti(e,t.currentTable.module,null,t.reportView,t.currentTable.module.name),p.stop()})}},t.shortChange=function(e,r){t.current.reverse=r,t.current.field=e,t.reportSummary.data=a("orderBy")(t.reportSummary.data,t.current.field,t.current.reverse)},t.categoryEditModalOpen=function(e,r,a){t.ReportCategory=r,t.ReportCategory.type=e,t.ReportCategory.user_id=r.user_id?r.user_id:"0",t.ReportCategory={user_id:r.user_id?r.user_id:"0",name:r.name,type:e,id:r.id},t.showAdvanced=function(e){d.show({controller:m,templateUrl:"view/app/reports/common/dialog1.html",parent:angular.element(document.body),targetEvent:e,scope:t,clickOutsideToClose:!0,preserveScope:!0,fullscreen:!1})},t.showAdvanced(a)},t.reportCategoryCreate=function(e,r){return e.$valid?("0"===t.ReportCategory.user_id&&(t.ReportCategory.user_id=""),"create"===t.ReportCategory.type&&l.createCategory(r).then(function(e){t.ReportCateogryies&&(t.ReportCateogryies.push(e.data),t.categoryEditModal.hide())}),void("update"===t.ReportCategory.type&&l.updateCategory(r).then(function(){i.reload(),t.categoryEditModal.hide()}))):!1},t.deleteReport=function(e){l.deleteReport(e).then(function(){i.reload()})},t.getdateFileds=function(){t.dateFields=[],angular.forEach(t.currentTable.module.fields,function(e){("date"===e.data_type||"date_time"===e.data_type)&&t.dateFields.push(e)})},t.dateFiltes=[{name:"lastYear",label:a("translate")("Report.LastYear")},{name:"thisYear",label:a("translate")("Report.ThisYear")},{name:"nextYear",label:a("translate")("Report.NextYear")},{name:"yesterDay",label:a("translate")("Report.YesterDay")},{name:"tomorrow",label:a("translate")("Report.Tomorrow")},{name:"today",label:a("translate")("Report.Today")},{name:"lastWeek",label:a("translate")("Report.LastWeek")},{name:"thisWeek",label:a("translate")("Report.ThisWeek")},{name:"nextWeek",label:a("translate")("Report.NextWeek")},{name:"lastMonth",label:a("translate")("Report.LastMonth")},{name:"thisMonth",label:a("translate")("Report.ThisMonth")},{name:"nextMonth",label:a("translate")("Report.NextMonth")},{name:"nextMonth(3)",label:a("translate")("Report.Next3Month")},{name:"nextMonth(6)",label:a("translate")("Report.Next6Month")},{name:"nextMonth(12)",label:a("translate")("Report.Next12Month")},{name:"prevMonth(3)",label:a("translate")("Report.Prev3Month")},{name:"prevMonth(6)",label:a("translate")("Report.Prev6Month")},{name:"prevMonth(12)",label:a("translate")("Report.Prev12Month")},{name:"lastday(7)",label:a("translate")("Report.Last7Day")},{name:"lastday(30)",label:a("translate")("Report.Last30Day")},{name:"lastday(60)",label:a("translate")("Report.Last60Day")},{name:"lastday(90)",label:a("translate")("Report.Last90Day")},{name:"nextday(7)",label:a("translate")("Report.Next7Day")},{name:"nextday(30)",label:a("translate")("Report.Next30Day")},{name:"nextday(60)",label:a("translate")("Report.Next60Day")},{name:"nextday(90)",label:a("translate")("Report.Next90Day")},{name:"costume",label:a("translate")("Report.Costume")}],t.changeFilter=function(){var e=new Date((new Date).getTime()+864e5);switch(t.newFilter.dateFilter){case"lastYear":t.newFilter.startDate=new Date(e.getFullYear()-1,0,1,0,0,0,0,0),t.newFilter.endDate=new Date(e.getFullYear()-1,12,0,0,0,0,0,0);break;case"thisYear":t.newFilter.startDate=new Date(e.getFullYear(),0,1,0,0,0,0,0),t.newFilter.endDate=new Date(e.getFullYear(),12,0,0,0,0,0,0);break;case"nextYear":t.newFilter.startDate=new Date(e.getFullYear()+1,0,1,0,0,0,0,0),t.newFilter.endDate=new Date(e.getFullYear()+1,12,0,0,0,0,0,0);break;case"yesterDay":t.newFilter.startDate=new Date(e.getFullYear(),e.getMonth(),e.getDate()-2,0,0,0,0,0),t.newFilter.endDate=new Date(e.getFullYear(),e.getMonth(),e.getDate()-2,23,59,0,0,0);break;case"tomorrow":t.newFilter.startDate=new Date(e.getFullYear(),e.getMonth(),e.getDate(),0,0,0,0,0),t.newFilter.endDate=new Date(e.getFullYear(),e.getMonth(),e.getDate(),23,59,0,0,0);break;case"today":t.newFilter.startDate=new Date(e.getFullYear(),e.getMonth(),e.getDate()-1,0,0,0,0,0),t.newFilter.endDate=new Date(e.getFullYear(),e.getMonth(),e.getDate()-1,23,59,0,0,0);break;case"lastWeek":t.newFilter.startDate=u().subtract(1,"weeks").startOf("isoWeek"),t.newFilter.endDate=u().subtract(1,"weeks").endOf("isoWeek");break;case"thisWeek":t.newFilter.startDate=u().startOf("week"),t.newFilter.endDate=u().endOf("week");break;case"nextWeek":t.newFilter.startDate=u().subtract(-1,"weeks").startOf("isoWeek"),t.newFilter.endDate=u().subtract(-1,"weeks").endOf("isoWeek");break;case"thisMonth":t.newFilter.startDate=u().startOf("month"),t.newFilter.endDate=u().endOf("month");break;case"nextMonth":t.newFilter.startDate=u().subtract(-1,"month").startOf("month"),t.newFilter.endDate=u().subtract(-1,"month").endOf("month");break;case"nextMonth(3)":t.newFilter.startDate=u().subtract(-1,"month").startOf("month"),t.newFilter.endDate=u().subtract(-4,"month").endOf("month");break;case"nextMonth(6)":t.newFilter.startDate=u().subtract(-1,"month").startOf("month"),t.newFilter.endDate=u().subtract(-6,"month").endOf("month");break;case"nextMonth(12)":t.newFilter.startDate=u().subtract(-1,"month").startOf("month"),t.newFilter.endDate=u().subtract(-12,"month").endOf("month");break;case"lastMonth":t.newFilter.startDate=u().subtract(1,"month").startOf("month"),t.newFilter.endDate=u().subtract(1,"month").endOf("month");break;case"prevMonth(3)":t.newFilter.startDate=u().subtract(3,"month").startOf("month"),t.newFilter.endDate=u().subtract(1,"month").endOf("month");break;case"prevMonth(6)":t.newFilter.startDate=u().subtract(6,"month").startOf("month"),t.newFilter.endDate=u().subtract(1,"month").endOf("month");break;case"prevMonth(12)":t.newFilter.startDate=u().subtract(12,"month").startOf("month"),t.newFilter.endDate=u().subtract(1,"month").endOf("month");break;case"lastday(7)":t.newFilter.startDate=u().subtract(6,"day"),t.newFilter.endDate=u().subtract(0,"day");break;case"lastday(30)":t.newFilter.startDate=u().subtract(29,"day"),t.newFilter.endDate=u().subtract(0,"day");break;case"lastday(60)":t.newFilter.startDate=u().subtract(59,"day"),t.newFilter.endDate=u().subtract(0,"day");break;case"lastday(90)":t.newFilter.startDate=u().subtract(89,"day"),t.newFilter.endDate=u().subtract(0,"day");break;case"nextday(7)":t.newFilter.startDate=u().subtract(0,"day"),t.newFilter.endDate=u().subtract(-6,"day");break;case"nextday(30)":t.newFilter.startDate=u().subtract(0,"day"),t.newFilter.endDate=u().subtract(-29,"day");break;case"nextday(60)":t.newFilter.startDate=u().subtract(0,"day"),t.newFilter.endDate=u().subtract(-59,"day");break;case"nextday(90)":t.newFilter.startDate=u().subtract(0,"day"),t.newFilter.endDate=u().subtract(-89,"day");break;default:t.newFilter.startDate=null,t.newFilter.endDate=null}},t.setFilter=function(){var e=[{field:t.newFilter.dateField,operator:"greater_equal",value:t.newFilter.startDate},{field:t.newFilter.dateField,operator:"less_equal",value:t.newFilter.endDate}];t.currentTable.request.filters=e.concat(t.defaultFilter),t.table.filterChange()},t.clearFilter=function(){t.currentTable.request.filters=t.defaultFilter,t.table.filterChange(),t.newFilter={}},t.dateChange=function(){"costume"!=t.newFilter.dateFilter&&(t.newFilter.dateFilter="costume")}}]);