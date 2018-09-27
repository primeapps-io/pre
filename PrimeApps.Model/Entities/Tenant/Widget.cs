﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("widgets")]
    public class Widget : BaseEntity
    {
        [Column("widget_type"), Required]
        public WidgetType WidgetType { get; set; }

        [Column("name"), MaxLength(200), Required]
        public string Name { get; set; }

        //For external widget for future usage
        [Column("load_url"), MaxLength(100)]
        public string LoadUrl { get; set; }

        [Column("color"), MaxLength(30)]
        public string Color { get; set; }

        [Column("icon"), MaxLength(30)]
        public string Icon { get; set; }

        [Column("report_id"), ForeignKey("Report")]
        public int? ReportId { get; set; }

        [Column("view_id"), ForeignKey("View")]
        public int? ViewId { get; set; }

        public virtual Report Report { get; set; }

        public virtual View View { get; set; }
    }
}
