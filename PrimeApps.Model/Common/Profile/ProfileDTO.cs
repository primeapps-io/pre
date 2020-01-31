﻿using PrimeApps.Model.Enums;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PrimeApps.Model.Common.Profile
{
    [DataContract]
    public class ProfileDTO
    {
        public ProfileDTO()
        {
            Permissions = new List<ProfilePermissionDTO>();
        }
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string NameEn { get; set; }
        [DataMember]
        public string NameTr { get; set; }
        [DataMember]
        public string DescriptionEn { get; set; }
        [DataMember]
        public string DescriptionTr { get; set; }
        [DataMember]
        public bool HasAdminRights { get; set; }
        [DataMember]
        public bool SendEmail { get; set; }
        [DataMember]
        public bool SendSMS { get; set; }
        [DataMember]
        public bool LeadConvert { get; set; }
        [DataMember]
        public bool ImportData { get; set; }
        [DataMember]
        public bool ExportData { get; set; }
        [DataMember]
        public bool WordPdfDownload { get; set; }
        [DataMember]
        public bool DocumentSearch { get; set; }
        [DataMember]
        public bool Tasks { get; set; }
        [DataMember]
        public bool Calendar { get; set; }
        [DataMember]
        public bool Newsfeed { get; set; }
        [DataMember]
        public bool Report { get; set; }
        [DataMember]
        public bool Dashboard { get; set; }
        [DataMember]
        public bool SmtpSettings { get; set; }
        [DataMember]
        public bool Home { get; set; }
	    [DataMember]
	    public bool CollectiveAnnualLeave { get; set; }
		[DataMember]
        public string StartPage { get; set; }
        [DataMember]
        public string SystemCode { get; set; }
        [DataMember]
        public bool IsPersistent { get; set; }
        [DataMember]
        public int ParentId { get; set; }
        [DataMember]
        public IEnumerable<ProfilePermissionDTO> Permissions { get; set; }
        [DataMember]
        public SystemType SystemType { get; set; }
    }
}
