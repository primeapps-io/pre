﻿using System.ComponentModel.DataAnnotations;
using PrimeApps.Model.Enums;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace PrimeApps.Console.Models
{
	public class AppTemplateBindingModel
	{
		[Required]
		public int AppId { get; set; }

		[Required, StringLength(200)]
		public string Name { get; set; }

		[Required, StringLength(200)]
		public string Subject { get; set; }

		[Required]
		public string Content { get; set; }

		[Required]
		public string Language { get; set; }

		public bool Active { get; set; }
		public bool Deleted { get; set; }
		public string Settings { get; set; }

	}
}