﻿using System.Web.UI;
using uComponents.Core;
using uComponents.DataTypes.Shared.Extensions;
using umbraco.cms.businesslogic.datatype;

[assembly: WebResource("uComponents.DataTypes.UrlPicker.UrlPickerScripts.js", Constants.MediaTypeNames.Application.JavaScript)]
[assembly: WebResource("uComponents.DataTypes.UrlPicker.UrlPickerStyles.css", Constants.MediaTypeNames.Text.Css, PerformSubstitution = true)]

namespace uComponents.DataTypes.UrlPicker
{
	/// <summary>
	/// Extension methods for the URL picker
	/// </summary>
	public static class UrlPickerExtensions
	{
		/// <summary>
		/// Adds the JS/CSS required for the UrlPicker
		/// </summary>
		/// <param name="ctl"></param>
		public static void AddAllUrlPickerClientDependencies(this Control ctl)
		{
			//get the urls for the embedded resources
			AddCssUrlPickerClientDependencies(ctl);
			AddJsUrlPickerClientDependencies(ctl);
		}

		/// <summary>
		/// Adds the CSS required for the UrlPicker
		/// </summary>
		/// <param name="ctl"></param>
		public static void AddCssUrlPickerClientDependencies(this Control ctl)
		{
			ctl.AddResourceToClientDependency("uComponents.DataTypes.UrlPicker.UrlPickerStyles.css", ClientDependencyType.Css);
		}

		/// <summary>
		/// Adds the JS required for the UrlPicker
		/// </summary>
		/// <param name="ctl"></param>
		public static void AddJsUrlPickerClientDependencies(this Control ctl)
		{
			ctl.AddResourceToClientDependency(typeof(Constants), "uComponents.DataTypes.Shared.Resources.Scripts.json2.js", ClientDependencyType.Javascript);
			ctl.AddResourceToClientDependency(typeof(Constants), "uComponents.DataTypes.Shared.Resources.Scripts.jquery.form.js", ClientDependencyType.Javascript);
			ctl.AddResourceToClientDependency("uComponents.DataTypes.UrlPicker.UrlPickerScripts.js", ClientDependencyType.Javascript);
		}
	}
}