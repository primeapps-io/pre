﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PrimeApps.App.Models;
using PrimeApps.Model.Common;
using PrimeApps.Model.Common.Report;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.App.Helpers
{
    public static class ReportHelper
    {
        public static async Task<Report> CreateEntity(ReportBindingModel reportModel, IUserRepository userRepository)
        {
            var report = new Report
            {
                Name = reportModel.Name,
                ModuleId = reportModel.ModuleId,
                ReportFeed = ReportFeed.Module,
                ReportType = reportModel.ReportType,
                CategoryId = reportModel.CategoryId,
                GroupField = reportModel.GroupField,
                SortField = reportModel.SortField,
                SortDirection = reportModel.SortDirection,
                SharingType = reportModel.SharingType != ReportSharingType.NotSet ? reportModel.SharingType : ReportSharingType.Me,
                FilterLogic = reportModel.FilterLogic,
                Fields = new List<ReportField>(),
                Filters = new List<ReportFilter>(),
                Aggregations = new List<ReportAggregation>()
            };

            await CreateReportRelations(reportModel, report, userRepository);

            return report;
        }

        public static async Task UpdateEntity(ReportBindingModel reportModel, Report report, IUserRepository userRepository)
        {
            report.Name = reportModel.Name;
            report.ModuleId = reportModel.ModuleId;
            report.ReportType = reportModel.ReportType;
            report.CategoryId = reportModel.CategoryId;
            report.GroupField = reportModel.GroupField;
            report.SortField = reportModel.SortField;
            report.SortDirection = reportModel.SortDirection;
            report.SharingType = reportModel.SharingType != ReportSharingType.NotSet ? reportModel.SharingType : ReportSharingType.Me;
            report.FilterLogic = reportModel.FilterLogic;

            await CreateReportRelations(reportModel, report, userRepository);
        }

        public static ReportViewModel MapToViewModel(Report report)
        {
            var reportViewModel = new ReportViewModel
            {
                Id = report.Id,
                ModuleId = report.ModuleId,
                ReportType = report.ReportType,
                CategoryId = report.CategoryId,
                GroupField = report.GroupField,
                SortField = report.SortField,
                SortDirection = report.SortDirection,
                SharingType = report.SharingType,
                FilterLogic = report.FilterLogic,
                CreatedBy = report.CreatedById,
                Fields = new List<ReportFieldViewModel>(),
                Filters = new List<ReportFilterViewModel>()
            };

            foreach (var reportField in report.Fields)
            {
                var reportFieldViewModel = new ReportFieldViewModel
                {
                    Field = reportField.Field,
                    Order = reportField.Order
                };

                reportViewModel.Fields.Add(reportFieldViewModel);
            }

            foreach (var reportFilter in report.Filters)
            {
                var reportFilterViewModel = new ReportFilterViewModel
                {
                    Field = reportFilter.Field,
                    Operator = reportFilter.Operator,
                    Value = reportFilter.Value,
                    No = reportFilter.No
                };

                reportViewModel.Filters.Add(reportFilterViewModel);
            }

            foreach (var reportAggregation in report.Aggregations)
            {
                var reportAggregationViewModel = new ReportAggregationViewModel
                {
                    Field = reportAggregation.Field,
                    AggregationType = reportAggregation.AggregationType
                };

                reportViewModel.Aggregations.Add(reportAggregationViewModel);
            }

            if (report.Shares != null && report.Shares.Count > 0)
            {
                reportViewModel.Shares = new List<UserBasicViewModel>();

                foreach (var user in report.Shares)
                {
                    reportViewModel.Shares.Add(new UserBasicViewModel { Id = user.Id, FullName = user.FullName });
                }
            }

            return reportViewModel;
        }

    
        public static List<ReportViewModel> MapToViewModel(ICollection<Report> reports)
        {
            return reports.Select(MapToViewModel).ToList();
        }

        public static ReportCategory CreateCategoryEntity(ReportCategoryBindingModel reportCategoryModel)
        {
            var reportCategory = new ReportCategory
            {
                Name = reportCategoryModel.Name,
                Order = reportCategoryModel.Order,
                UserId = reportCategoryModel.UserId
            };

            return reportCategory;
        }

        public static ReportCategory UpdateCategoryEntity(ReportCategoryBindingModel reportCategoryModel, ReportCategory reportCategory)
        {
            reportCategory.Name = reportCategoryModel.Name;
            reportCategory.Order = reportCategoryModel.Order;
            reportCategory.UserId = reportCategoryModel.UserId;

            return reportCategory;
        }

        public static Chart CreateChartEntity(ChartBindingModel chartModel, int reportId)
        {
            var chart = new Chart
            {
                ReportId = reportId,
                ChartType = chartModel.Type,
                Caption = chartModel.Caption,
                SubCaption = chartModel.SubCaption,
                XaxisName = chartModel.XaxisName,
                YaxisName = chartModel.YaxisName,
                Theme = chartModel.Theme != ChartTheme.NotSet ? chartModel.Theme : ChartTheme.Zune
            };

            return chart;
        }

        public static Chart UpdateChartEntity(ChartBindingModel chartModel, Chart chart)
        {
            chart.ChartType = chartModel.Type;
            chart.Caption = chartModel.Caption;
            chart.SubCaption = chartModel.SubCaption;
            chart.XaxisName = chartModel.XaxisName;
            chart.YaxisName = chartModel.YaxisName;
            chart.Theme = chartModel.Theme != ChartTheme.NotSet ? chartModel.Theme : ChartTheme.Zune;

            return chart;
        }

        public static Widget CreateWidgetEntity(WidgetBindingModel widgetModel, int reportId)
        {
            var widget = new Widget
            {
                ReportId = reportId,
                WidgetType = widgetModel.WidgetType,
                Name = widgetModel.Name,
                Color = widgetModel.Color,
                Icon = widgetModel.Icon
            };

            return widget;
        }

        public static Widget UpdateWidgetEntity(WidgetBindingModel widgetModel, Widget widget)
        {
            widget.WidgetType = widgetModel.WidgetType;
            widget.Name = widgetModel.Name;
            widget.Color = widgetModel.Color;
            widget.Icon = widgetModel.Icon;

            return widget;
        }

        public static void Validate(ReportBindingModel report, ModelStateDictionary modelState)
        {
            if (!RecordHelper.ValidateFilterLogic(report.FilterLogic, report.Filters))
                modelState.AddModelError("request._filter_logic", "The field FilterLogic is invalid or has no filters.");

            if (report.ReportType == ReportType.Tabular && (report.Fields == null || report.Fields.Count < 1))
                modelState.AddModelError("request._fields", "Fields cannot be null when report type is tabular");

            if (report.ReportType == ReportType.Summary && (report.Aggregations == null || report.Aggregations.Count < 1))
                modelState.AddModelError("request._aggregations", "Aggregations cannot be null when report type is summary");

            if (report.ReportType == ReportType.Summary && string.IsNullOrWhiteSpace(report.GroupField))
                modelState.AddModelError("request._group_field", "group_field cannot be null when report type is summary");

            if (report.ReportType == ReportType.Summary && report.Chart == null)
                modelState.AddModelError("request._chart", "Chart cannot be null when report type is summary");

            if (report.ReportType == ReportType.Single && (report.Aggregations == null || report.Aggregations.Count < 1))
                modelState.AddModelError("request._aggregations", "Aggregations cannot be null when report type is single");

            if (report.ReportType == ReportType.Single && report.Widget == null)
                modelState.AddModelError("request._widget", "Widget cannot be null when report type is single");
        }

        private static async Task CreateReportRelations(ReportBindingModel reportModel, Report report, IUserRepository userRepository)
        {
            if (reportModel.Fields != null)
            {
                foreach (var reportFieldModel in reportModel.Fields)
                {
                    var reportField = new ReportField
                    {
                        Field = reportFieldModel.Field,
                        Order = reportFieldModel.Order
                    };

                    report.Fields.Add(reportField);
                }
            }

            if (reportModel.Filters != null)
            {
                foreach (var reportFilterModel in reportModel.Filters)
                {
                    var reportFilter = new ReportFilter
                    {
                        Field = reportFilterModel.Field,
                        Operator = reportFilterModel.Operator,
                        Value = reportFilterModel.Value.ToString(),
                        No = reportFilterModel.No
                    };

                    report.Filters.Add(reportFilter);
                }
            }

            if (reportModel.Aggregations != null)
            {
                foreach (var reportAggregationModel in reportModel.Aggregations)
                {
                    var reportAggregation = new ReportAggregation
                    {
                        Field = reportAggregationModel.Field,
                        AggregationType = reportAggregationModel.AggregationType
                    };

                    report.Aggregations.Add(reportAggregation);
                }
            }

            if (reportModel.Shares != null && reportModel.Shares.Count > 0)
            {
                report.Shares = new List<TenantUser>();

                foreach (var userId in reportModel.Shares)
                {
                    var sharedUser = await userRepository.GetById(userId);

                    if (sharedUser != null)
                        report.Shares.Add(sharedUser);
                }
            }
        }
    }
}