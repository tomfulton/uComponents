﻿using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.editorControls;
using Umbraco.Core.IO;

namespace uComponents.DataTypes.Notes
{
    /// <summary>
    /// The PreValue Editor for the Notes data-type.
    /// </summary>
	public class NotesPrevalueEditor : uComponents.DataTypes.Shared.PrevalueEditors.AbstractJsonPrevalueEditor
    {
        /// <summary>
        /// The TextBox control for the value of the Notes data-type.
        /// </summary>
        private TextBox Notes;

        /// <summary>
        /// The Checkbox control to define whether to show the label for the Notes data-type.
        /// </summary>
        private CheckBox ShowLabel;

        /// <summary>
        /// Gets the documentation URL.
        /// </summary>
        public override string DocumentationUrl
        {
            get
            {
                return string.Concat(base.DocumentationUrl, "/data-types/notes/");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotesPrevalueEditor"/> class.
        /// </summary>
        /// <param name="dataType">Type of the data.</param>
        public NotesPrevalueEditor(NotesDataType dataType)
            : base(dataType, umbraco.cms.businesslogic.datatype.DBTypes.Ntext)
        { }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.EnsureChildControls();

            this.Page.ClientScript.RegisterClientScriptInclude("TinyMCE", this.ResolveUrl(SystemDirectories.UmbracoClient) + "/tinymce3/tiny_mce_src.js");
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"/> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // get PreValues, load them into the controls.
            var options = this.GetPreValueOptions<NotesOptions>() ?? new NotesOptions(true);

            // set the values
            this.Notes.Text = options.Value;
            this.ShowLabel.Checked = options.ShowLabel;
        }

        /// <summary>
        /// Renders the contents of the control to the specified writer. This method is used primarily by control developers.
        /// </summary>
        /// <param name="writer">A <see cref="T:System.Web.UI.HtmlTextWriter"/> that represents the output stream to render HTML content on the client.</param>
        protected override void RenderContents(HtmlTextWriter writer)
        {
            // add property fields
            writer.AddPrevalueRow("Text to display", this.Notes);
            writer.AddPrevalueRow("Show label?", this.ShowLabel);

            // write out the JavaScript to initialise TinyMCE
            var javascript = string.Concat(@"<script type='text/javascript'> 
tinyMCE.init({
    mode:'exact',
    theme:'advanced',
    umbraco_path:'", IOHelper.ResolveUrl(SystemDirectories.Umbraco), @"',
    elements:'", this.Notes.ClientID, @"',
    language:'en',
    encoding:'xml',
    theme_advanced_buttons1 : 'bold,italic,underline,|,justifyleft,justifycenter,justifyright,formatselect,bullist,numlist,|,outdent,indent,|,link,unlink,anchor,image,|,code,preview',
    theme_advanced_buttons2 : '',
    theme_advanced_buttons3 : '',
    theme_advanced_toolbar_location : 'top',
    theme_advanced_toolbar_align : 'left',
    theme_advanced_resizing : true
});
</script>");
            writer.WriteLine(javascript);
        }

        /// <summary>
        /// Creates child controls for this control
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // set-up child controls
            this.Notes = new TextBox() { ID = "Notes", CssClass = "guiInputText", TextMode = TextBoxMode.MultiLine };
            this.Notes.Style.Add("width", "600px");
            this.Notes.Style.Add("height", "300px");
            this.Notes.EnableViewState = false;

            this.ShowLabel = new CheckBox() { ID = "ShowLabel" };

            // add the child controls
            this.Controls.AddPrevalueControls(this.Notes, this.ShowLabel);
        }

        /// <summary>
        /// Saves the data-type PreValue options.
        /// </summary>
        public override void Save()
        {
            // set the options
            var options = new NotesOptions
            {
                Value = Notes.Text = HttpUtility.HtmlDecode(Notes.Text),// Due to bug in TinyMCE, need to reset Notes.Text to unencoded value
                ShowLabel = ShowLabel.Checked
            };

            // save the options as JSON
            this.SaveAsJson(options);
        }
    }
}