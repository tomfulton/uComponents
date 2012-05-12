﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using uComponents.Core;
using uComponents.DataTypes.Shared.Extensions;
using uComponents.DataTypes.UrlPicker.Dto;
using umbraco;
using umbraco.cms.businesslogic.datatype;
using umbraco.interfaces;

namespace uComponents.DataTypes.UrlPicker
{
    /// <summary>
    /// The PreValueEditor for the UrlPicker.
    /// </summary>
    public class UrlPickerPreValueEditor : Control, IDataPrevalue
    {
        /// <summary>
        /// The underlying base data-type.
        /// </summary>
        protected readonly BaseDataType DataType;

        /// <summary>
        /// 
        /// </summary>
        protected SortedList PreValues;
        private object m_Locker = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlPickerPreValueEditor"/> class.
        /// </summary>
        /// <param name="dataType">Type of the data.</param>
        public UrlPickerPreValueEditor(BaseDataType dataType)
        {
            this.DataType = dataType;
        }

        #region Child Controls

        /// <summary>
        /// Select one or more modes
        /// </summary>
        protected ListBox ModeSelector;

        /// <summary>
        /// Selects the default mode
        /// </summary>
        protected DropDownList DefaultModeDropDown;

        /// <summary>
        /// Lists the different data formats available
        /// </summary>
        protected DropDownList DataFormatDropDown;

        /// <summary>
        /// 
        /// </summary>
        protected CheckBox EnableTitleCheckbox;

        /// <summary>
        /// 
        /// </summary>
        protected CheckBox EnableNewWindowCheckbox;

        #endregion

        /// <summary>
        /// Lazy loads the prevalues for this data type
        /// </summary>
        /// <returns></returns>
        protected SortedList GetPreValues()
        {
            if (PreValues == null)
            {
                PreValues = umbraco.cms.businesslogic.datatype.PreValues.GetPreValues(DataType.DataTypeDefinitionId);
            }
            return PreValues;
        }

        /// <summary>
        /// Retrieves the prevalues
        /// </summary>
        public UrlPickerSettings Settings
        {
            get
            {
                var settings = new UrlPickerSettings();
                var vals = GetPreValues();

                // Allowed modes:
                if (vals.Count >= 1 && !string.IsNullOrEmpty(((PreValue)vals[0]).Value))
                {
                    var allowedModes = new List<UrlPickerMode>();

                    var selectedModesStrings = ((PreValue)vals[0]).Value.Split('|');

                    foreach (var modeString in selectedModesStrings)
                    {
                        // A crude "Enum.TryParse"
                        if (Enum.IsDefined(typeof(UrlPickerMode), modeString))
                        {
                            allowedModes.Add((UrlPickerMode)Enum.Parse(typeof(UrlPickerMode), modeString, true));
                        }
                    }

                    settings.AllowedModes = allowedModes;
                }

                // Data Format:
                if (vals.Count >= 2)
                {
                    UrlPickerDataFormat? dataFormat = null;

                    var value = ((PreValue)vals[1]).Value;

                    if (!string.IsNullOrEmpty(value))
                    {
                        // Deal with legacy values, "True"/"False"
                        bool legacy_StoreAsCommaDelimited;
                        if (bool.TryParse(((PreValue)vals[1]).Value, out legacy_StoreAsCommaDelimited))
                        {
                            if (legacy_StoreAsCommaDelimited)
                            {
                                dataFormat = UrlPickerDataFormat.Csv;
                            }
                            else
                            {
                                dataFormat = UrlPickerDataFormat.Xml;
                            }
                        }
                        else
                        {
                            // A crude "Enum.TryParse"
                            if (Enum.IsDefined(typeof(UrlPickerDataFormat), value))
                            {
                                dataFormat = (UrlPickerDataFormat)Enum.Parse(typeof(UrlPickerDataFormat), value, true);
                            }
                        }
                    }

                    if (dataFormat.HasValue)
                    {
                        settings.DataFormat = dataFormat.GetValueOrDefault();
                    }
                }

                // Enable Title:
                if (vals.Count >= 3)
                {
                    bool value;
                    if (bool.TryParse(((PreValue)vals[2]).Value, out value))
                    {
                        settings.EnableTitle = value;
                    }
                }

                // Enable New Window:
                if (vals.Count >= 4)
                {
                    bool value;
                    if (bool.TryParse(((PreValue)vals[3]).Value, out value))
                    {
                        settings.EnableNewWindow = value;
                    }
                }

                // Default mode:
                if (vals.Count >= 5)
                {
                    UrlPickerMode? defaultMode = null;

                    var value = ((PreValue)vals[4]).Value;

                    if (!string.IsNullOrEmpty(value) && Enum.IsDefined(typeof(UrlPickerMode), value))
                    {
                        defaultMode = (UrlPickerMode)Enum.Parse(typeof(UrlPickerMode), value, true);
                    }

                    if (defaultMode.HasValue)
                    {
                        settings.DefaultMode = defaultMode.GetValueOrDefault();
                    }
                }

                return settings;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.EnsureChildControls();
            this.AddResourceToClientDependency(Constants.PrevalueEditorCssResourcePath, ClientDependency.Core.ClientDependencyType.Css);
        }

        /// <summary>
        /// Prepopulates the controls with prevalue data
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Fill mode control
            ModeSelector.Items.Clear();
            foreach (UrlPickerMode mode in Enum.GetValues(typeof(UrlPickerMode)))
            {
                ModeSelector.Items.Add(new ListItem(mode.ToString()));

                foreach (var selectedMode in Settings.AllowedModes)
                {
                    var item = ModeSelector.Items.FindByText(selectedMode.ToString());

                    if (item != null)
                    {
                        item.Selected = true;
                    }
                }
            }

            // Fill default mode control
            DefaultModeDropDown.Items.Clear();
            foreach (var mode in Enum.GetValues(typeof(UrlPickerMode)))
            {
                DefaultModeDropDown.Items.Add(new ListItem(mode.ToString()));
            }

            // Set selected default mode
            var selectedDefaultModeItem = DefaultModeDropDown.Items.FindByText(Settings.DefaultMode.ToString());

            if (selectedDefaultModeItem != null)
            {
                DefaultModeDropDown.ClearSelection();
                selectedDefaultModeItem.Selected = true;
            }

            // Fill data format control
            DataFormatDropDown.Items.Clear();
            foreach (UrlPickerDataFormat format in Enum.GetValues(typeof(UrlPickerDataFormat)))
            {
                DataFormatDropDown.Items.Add(new ListItem(format.ToString()));
            }

            // Set selected data format
            var selectedDataFormatItem = DataFormatDropDown.Items.FindByText(Settings.DataFormat.ToString());

            if (selectedDataFormatItem != null)
            {
                DataFormatDropDown.ClearSelection();
                selectedDataFormatItem.Selected = true;
            }

            // Fill other controls
            EnableTitleCheckbox.Checked = Settings.EnableTitle;
            EnableNewWindowCheckbox.Checked = Settings.EnableNewWindow;
        }

        /// <summary>
        /// Creates child controls for this control
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // set-up controls
            ModeSelector = new ListBox
            {
                CssClass = "umbEditorTextField",
                SelectionMode = ListSelectionMode.Multiple
            };
            ModeSelector.ID = ModeSelector.ClientID;
            this.Controls.Add(ModeSelector);

            DefaultModeDropDown = new DropDownList();
            DefaultModeDropDown.ID = DefaultModeDropDown.ClientID;
            this.Controls.Add(DefaultModeDropDown);

            DataFormatDropDown = new DropDownList();
            DataFormatDropDown.ID = DataFormatDropDown.ClientID;
            this.Controls.Add(DataFormatDropDown);

            EnableTitleCheckbox = new CheckBox();
            EnableTitleCheckbox.ID = EnableTitleCheckbox.ClientID;
            this.Controls.Add(EnableTitleCheckbox);

            EnableNewWindowCheckbox = new CheckBox();
            EnableNewWindowCheckbox.ID = EnableNewWindowCheckbox.ClientID;
            this.Controls.Add(EnableNewWindowCheckbox);
        }

        /// <summary>
        /// Sends server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter"/> object, which writes the content to be rendered on the client.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the server control content.</param>
        protected override void Render(HtmlTextWriter writer)
        {
			writer.AddAttribute(HtmlTextWriterAttribute.Class, Constants.ApplicationName);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            // add property fields
            writer.AddPrevalueRow("Allowed modes:", "Select all modes which are allowed for this data type", this.ModeSelector);
            writer.AddPrevalueRow("Default mode:", "Choose a mode which is shown for empty instances of this data type", this.DefaultModeDropDown);
            writer.AddPrevalueRow(
                "Data format:",
                @"You can specify to store the data in a number of formats:  
                  <ul>
                      <li>XML is best for retrieval by XSLT,</li> 
                      <li>CSV is best for retrieval by scripting languages like Python,</li> 
                      <li>JSON is best for retrieval by Javascript (you never know!)</li>
                  </ul>
                  To get at the data easily in .NET, you can use the method 
                  <strong>uComponents.DataTypes.UrlPicker.Dto.UrlPickerState.Deserialize</strong> and pass it the data in any format",
                this.DataFormatDropDown);
            writer.AddPrevalueRow("Allow link title:", "User can specify a title for the link", this.EnableTitleCheckbox);
            writer.AddPrevalueRow("Allow new window:", "User can specify link to open in new window", this.EnableNewWindowCheckbox);

            writer.RenderEndTag();
        }


        #region IDataPrevalue Members

		/// <summary>
		/// Gets the editor.
		/// </summary>
		/// <value>The editor.</value>
		public Control Editor
		{
			get
			{
				return this;
			}
		}

		/// <summary>
		/// Saves this instance.
		/// </summary>
        public void Save()
        {
            if (this.Page.IsValid)
            {
                if (DataFormatDropDown == null ||
                    DefaultModeDropDown == null ||
                    ModeSelector == null ||
                    EnableTitleCheckbox == null ||
                    EnableNewWindowCheckbox == null)
                {
                    return;
                }

                DataType.DBType = DBTypes.Ntext;

                // If none are selected that's no good - you need to have at least one.  So
                // we shall select all of the modes in this case.
                if (!ModeSelector.Items.Cast<ListItem>().Any(x => x.Selected))
                {
                    ModeSelector.Items.Cast<ListItem>().ToList().ForEach(x => x.Selected = true);
                }

                // Get selected items
                var allowedModesDataSet = ModeSelector.Items.Cast<ListItem>()
                    .Where(x => x.Selected)
                    .Select(y => y.Text)
                    .ToList();

                // Generate "AllowedModes" data string to save - this needs to be the same format as the config above
                var allowedModesDataString = string.Join("|", allowedModesDataSet.ToArray());

                // Validate Default Mode, set to the first allowed mode if it's not allowed
                if (!allowedModesDataSet.Contains(DefaultModeDropDown.SelectedItem.Text))
                {
                    // Set selected default mode
                    var selectedDefaultModeItem = DefaultModeDropDown.Items.FindByText(allowedModesDataSet.First());

                    if (selectedDefaultModeItem != null)
                    {
                        DefaultModeDropDown.ClearSelection();
                        selectedDefaultModeItem.Selected = true;
                    }
                }

                // I guess I'd better lock this? or something? should I? better safe than sorry.
                lock (m_Locker)
                {
                    var vals = GetPreValues();

                    // Only save if one or more items were selected - otherwise this control doesn't make sense!
                    if (!string.IsNullOrEmpty(allowedModesDataString))
                    {
                        if (vals.Count >= 1)
                        {
                            //update
                            ((PreValue)vals[0]).Value = allowedModesDataString;
                            ((PreValue)vals[0]).Save();
                        }
                        else
                        {
                            //insert
                            PreValue.MakeNew(DataType.DataTypeDefinitionId, allowedModesDataString);
                        }
                    }

                    if (vals.Count >= 2)
                    {
                        //update
                        ((PreValue)vals[1]).Value = DataFormatDropDown.SelectedItem.Text;
                        ((PreValue)vals[1]).Save();
                    }
                    else
                    {
                        //insert
                        PreValue.MakeNew(DataType.DataTypeDefinitionId, DataFormatDropDown.SelectedItem.Text);
                    }

                    if (vals.Count >= 3)
                    {
                        //update
                        ((PreValue)vals[2]).Value = EnableTitleCheckbox.Checked.ToString();
                        ((PreValue)vals[2]).Save();
                    }
                    else
                    {
                        //insert
                        PreValue.MakeNew(DataType.DataTypeDefinitionId, EnableTitleCheckbox.Checked.ToString());
                    }

                    if (vals.Count >= 4)
                    {
                        //update
                        ((PreValue)vals[3]).Value = EnableNewWindowCheckbox.Checked.ToString();
                        ((PreValue)vals[3]).Save();
                    }
                    else
                    {
                        //insert
                        PreValue.MakeNew(DataType.DataTypeDefinitionId, EnableNewWindowCheckbox.Checked.ToString());
                    }

                    if (vals.Count >= 5)
                    {
                        //update
                        ((PreValue)vals[4]).Value = DefaultModeDropDown.SelectedItem.Text;
                        ((PreValue)vals[4]).Save();
                    }
                    else
                    {
                        //insert
                        PreValue.MakeNew(DataType.DataTypeDefinitionId, DefaultModeDropDown.SelectedItem.Text);
                    }
                }
            }
        }

        #endregion
    }
}