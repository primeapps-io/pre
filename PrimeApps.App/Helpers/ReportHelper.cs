﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.App.Models;
using PrimeApps.Model.Common;
using PrimeApps.Model.Common.Report;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.App.Helpers
{
    public interface IReportHelper
    {
        Report CreateEntity(ReportBindingModel reportModel);
        void UpdateEntity(ReportBindingModel reportModel, Report report);
        ReportViewModel MapToViewModel(Report report);
        List<ReportViewModel> MapToViewModel(ICollection<Report> reports);
        ReportCategory CreateCategoryEntity(ReportCategoryBindingModel reportCategoryModel);
        ReportCategory UpdateCategoryEntity(ReportCategoryBindingModel reportCategoryModel, ReportCategory reportCategory);
        Chart CreateChartEntity(ChartBindingModel chartModel, int reportId);
        Chart UpdateChartEntity(ChartBindingModel chartModel, Chart chart);
        Widget CreateWidgetEntity(WidgetBindingModel widgetModel, int reportId);
        Widget UpdateWidgetEntity(WidgetBindingModel widgetModel, Widget widget);
        void Validate(ReportBindingModel report, ModelStateDictionary modelState, ValidateFilterLogic ValidateFilterLogic);
        void CreateReportRelations(ReportBindingModel reportModel, Report report);
    }

    public class ReportHelper : IReportHelper
    {
        private CurrentUser _currentUser;
        private IHttpContextAccessor _context;
        private IConfiguration _configuration;
        private IServiceScopeFactory _serviceScopeFactory;

        public ReportHelper(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, IHttpContextAccessor context)
        {
            _context = context;
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;
            _currentUser = UserHelper.GetCurrentUser(_context, configuration);
        }

        public Report CreateEntity(ReportBindingModel reportModel)
        {
            var report = new Report
            {
                NameEn = reportModel.NameEn,
                NameTr = reportModel.NameTr,
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

            CreateReportRelations(reportModel, report);

            return report;
        }

        public void UpdateEntity(ReportBindingModel reportModel, Report report)
        {
            report.NameEn = reportModel.NameEn;
            report.NameTr = reportModel.NameTr;
            report.ModuleId = reportModel.ModuleId;
            report.ReportType = reportModel.ReportType;
            report.CategoryId = reportModel.CategoryId;
            report.GroupField = reportModel.GroupField;
            report.SortField = reportModel.SortField;
            report.SortDirection = reportModel.SortDirection;
            report.SharingType = reportModel.SharingType != ReportSharingType.NotSet ? reportModel.SharingType : ReportSharingType.Me;
            report.FilterLogic = reportModel.FilterLogic;

            CreateReportRelations(reportModel, report);
        }

        public ReportViewModel MapToViewModel(Report report)
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
                    reportViewModel.Shares.Add(new UserBasicViewModel { Id = user.UserId, FullName = user.TenantUser.FullName });
                }
            }

            return reportViewModel;
        }

        public List<ReportViewModel> MapToViewModel(ICollection<Report> reports)
        {
            return reports.Select(MapToViewModel).ToList();
        }

        public ReportCategory CreateCategoryEntity(ReportCategoryBindingModel reportCategoryModel)
        {
            var reportCategory = new ReportCategory
            {
                NameEn = reportCategoryModel.NameEn,
                NameTr = reportCategoryModel.NameTr,
                Order = reportCategoryModel.Order,
                UserId = reportCategoryModel.UserId
            };

            return reportCategory;
        }

        public ReportCategory UpdateCategoryEntity(ReportCategoryBindingModel reportCategoryModel, ReportCategory reportCategory)
        {
            reportCategory.NameEn = reportCategoryModel.NameEn;
            reportCategory.NameTr = reportCategoryModel.NameTr;
            reportCategory.Order = reportCategoryModel.Order;
            reportCategory.UserId = reportCategoryModel.UserId;

            return reportCategory;
        }

        public Chart CreateChartEntity(ChartBindingModel chartModel, int reportId)
        {
            var chart = new Chart
            {
                ReportId = reportId,
                ChartType = chartModel.Type,
                CaptionEn = chartModel.CaptionEn,
                CaptionTr = chartModel.CaptionTr,
                SubCaptionEn = chartModel.SubCaptionEn,
                SubCaptionTr = chartModel.SubCaptionTr,
                XaxisNameEn = chartModel.XaxisNameEn,
                XaxisNameTr = chartModel.XaxisNameTr,
                YaxisNameEn = chartModel.YaxisNameEn,
                YaxisNameTr = chartModel.YaxisNameTr,
                Theme = chartModel.Theme != ChartTheme.NotSet ? chartModel.Theme : ChartTheme.Zune
            };

            return chart;
        }

        public Chart UpdateChartEntity(ChartBindingModel chartModel, Chart chart)
        {
            chart.ChartType = chartModel.Type;
            chart.CaptionEn = chartModel.CaptionEn;
            chart.CaptionTr = chartModel.CaptionTr;
            chart.SubCaptionEn = chartModel.SubCaptionEn;
            chart.SubCaptionTr = chartModel.SubCaptionTr;
            chart.XaxisNameEn = chartModel.XaxisNameEn;
            chart.XaxisNameTr = chartModel.XaxisNameTr;
            chart.YaxisNameEn = chartModel.YaxisNameEn;
            chart.YaxisNameTr = chartModel.YaxisNameTr;
            chart.Theme = chartModel.Theme != ChartTheme.NotSet ? chartModel.Theme : ChartTheme.Zune;

            return chart;
        }

        public Widget CreateWidgetEntity(WidgetBindingModel widgetModel, int reportId)
        {
            var widget = new Widget
            {
                ReportId = reportId,
                WidgetType = widgetModel.WidgetType,
                NameEn = widgetModel.NameEn,
                NameTr = widgetModel.NameTr,
                Color = widgetModel.Color,
                Icon = widgetModel.Icon
            };

            return widget;
        }

        public Widget UpdateWidgetEntity(WidgetBindingModel widgetModel, Widget widget)
        {
            widget.WidgetType = widgetModel.WidgetType;
            widget.NameEn = widgetModel.NameEn;
            widget.NameTr = widgetModel.NameTr;
            widget.Color = widgetModel.Color;
            widget.Icon = widgetModel.Icon;

            return widget;
        }

        public void Validate(ReportBindingModel report, ModelStateDictionary modelState, ValidateFilterLogic ValidateFilterLogic)
        {
            if (!ValidateFilterLogic(report.FilterLogic, report.Filters))
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

        public void CreateReportRelations(ReportBindingModel reportModel, Report report)
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
                report.Shares = new List<ReportShares>();

                foreach (var userId in reportModel.Shares)
                {
                    using (var _scope = _serviceScopeFactory.CreateScope())
                    {
                        var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                        using (var _userRepository = new UserRepository(databaseContext, _configuration))
                        {
                            var sharedUser = _userRepository.GetById(userId);
                            _userRepository.CurrentUser = _currentUser;

                            if (sharedUser != null)
                                report.Shares.Add(new ReportShares { TenantUser = sharedUser });
                        }
                    }
                }
            }
        }
    }
}