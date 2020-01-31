﻿using PrimeApps.Model.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("reports")]
    public class Report : BaseEntity
    {
        [Column("name_en"), MaxLength(100)]
        public string NameEn { get; set; }
        
        [Column("name_tr"), MaxLength(100)]
        public string NameTr { get; set; }

        [Column("report_type")]
        public ReportType ReportType { get; set; }

        [Column("report_feed")]
        public ReportFeed ReportFeed { get; set; }

        [Column("sql_function")]
        public string SqlFunction { get; set; }

        [Column("module_id"), Required, ForeignKey("Module")]
        public int ModuleId { get; set; }

        [Column("user_id"), ForeignKey("User")]
        public int? UserId { get; set; }

        [Column("category_id"), ForeignKey("Category")]
        public int? CategoryId { get; set; }

        [Column("group_field")]
        public string GroupField { get; set; }

        [Column("sort_field")]
        public string SortField { get; set; }

        [Column("sort_direction")]
        public SortDirection SortDirection { get; set; }

        [Column("sharing_type")]
        public ReportSharingType SharingType { get; set; }

        [Column("filter_logic"), MaxLength(200)]
        public string FilterLogic { get; set; }

        [Column("system_type"), DefaultValue(SystemType.Custom)]
        public SystemType SystemType { get; set; }

        public virtual Module Module { get; set; }

        public virtual TenantUser User { get; set; }

        public virtual ReportCategory Category { get; set; }

        public virtual ICollection<ReportField> Fields { get; set; }

        public virtual ICollection<ReportFilter> Filters { get; set; }

        public virtual ICollection<ReportAggregation> Aggregations { get; set; }

        public List<ReportShares> Shares { get; set; }
    }
}