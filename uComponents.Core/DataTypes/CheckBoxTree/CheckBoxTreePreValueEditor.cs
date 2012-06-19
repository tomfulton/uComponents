﻿using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.XPath;
using uComponents.Core.uQueryExtensions;
using uComponents.Core.Shared;
using uComponents.Core.Shared.Extensions;
using uComponents.Core.Shared.PrevalueEditors;
using umbraco.cms.businesslogic.datatype;

namespace uComponents.Core.DataTypes.CheckBoxTree
{
	/// <summary>
	/// PreValueEditor for the CheckBoxTree data-type.
	/// </summary>
	public class CheckBoxTreePreValueEditor : AbstractJsonPrevalueEditor
	{
		/// <summary>
		/// Field for the tree start XPath expression.
		/// </summary>
		private TextBox treeStartNodeXPathTextBox = new TextBox();

		/// <summary>
		/// Field for the tree start XPath expression's required validator.
		/// </summary>
		private RequiredFieldValidator treeStartNodeXPathRequiredFieldValidator = new RequiredFieldValidator();

		/// <summary>
		/// Field for the tree start XPath expression's custom validator.
		/// </summary>
		private CustomValidator treeStartNodeXPathCustomValidator = new CustomValidator();

		/// <summary>
		/// Field for the selectable tree node's XPath.
		/// </summary>
		private TextBox selectableTreeNodesXPathTextBox = new TextBox();

		/// <summary>
		/// Field for the selectable tree node XPath expression's custom validator.
		/// </summary>
		private CustomValidator selectableTreeNodesXPathCustomValidator = new CustomValidator();

		/// <summary>
		/// Field for the minimum selection.
		/// </summary>
		private TextBox minSelectionTextBox = new TextBox();

		/// <summary>
		/// Field for the maximum selection.
		/// </summary>
		private TextBox maxSelectionTextBox = new TextBox();

		/// <summary>
		/// Field for the ancestors CheckBox.
		/// </summary>
		private CheckBox selectAncestorsCheckBox = new CheckBox();

		/// <summary>
		/// Field for the descendents CheckBox.
		/// </summary>
		private CheckBox selectDescendentsCheckBox = new CheckBox();

		/// <summary>
		/// Field for the show tree-icons CheckBox.
		/// </summary>
		private CheckBox showTreeIconsCheckBox = new CheckBox();

		/// <summary>
		/// DropDownList for the expand options.
		/// </summary>
		private DropDownList selectExpandOptionDropDownList = new DropDownList();

		/// <summary>
		/// The RadioButtonList for the output formats.
		/// </summary>
		private RadioButtonList selectOutputFormat = new RadioButtonList() { RepeatDirection = RepeatDirection.Vertical, RepeatLayout = RepeatLayout.Flow };

		/// <summary>
		/// Field for the options.
		/// </summary>
		private CheckBoxTreeOptions options = null;

		// TODO: [HR] CheckBox to toggle auto selecting parent if descendents fully selected (like when installing software)

		/// <summary>
		/// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
		}

		/// <summary>
		/// Gets the options.
		/// </summary>
		/// <value>The options.</value>
		internal CheckBoxTreeOptions Options
		{
			get
			{
				if (this.options == null)
				{
					// Deserialize any stored settings for this PreValueEditor instance
					this.options = this.GetPreValueOptions<CheckBoxTreeOptions>();

					// If still null, ie, object couldn't be de-serialized from PreValue[0] string value
					if (this.options == null)
					{
						// Create a new Options data object with the default values
						this.options = new CheckBoxTreeOptions(true);
					}
				}
				return this.options;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CheckBoxTreePreValueEditor"/> class.
		/// </summary>
		/// <param name="dataType">Type of the data.</param>
		public CheckBoxTreePreValueEditor(BaseDataType dataType)
			: base(dataType, DBTypes.Ntext)
		{
		}

		/// <summary>
		/// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
		/// </summary>
		protected override void CreateChildControls()
		{
			this.treeStartNodeXPathTextBox.ID = "treeNodesXPathTexdtBox";
			this.treeStartNodeXPathTextBox.CssClass = "umbEditorTextField";

			this.treeStartNodeXPathRequiredFieldValidator.ControlToValidate = this.treeStartNodeXPathTextBox.ID;
			this.treeStartNodeXPathRequiredFieldValidator.Display = ValidatorDisplay.Dynamic;
			this.treeStartNodeXPathRequiredFieldValidator.ErrorMessage = " Required";

			this.treeStartNodeXPathCustomValidator.ControlToValidate = this.treeStartNodeXPathTextBox.ID;
			this.treeStartNodeXPathCustomValidator.Display = ValidatorDisplay.Dynamic;
			this.treeStartNodeXPathCustomValidator.ServerValidate += new ServerValidateEventHandler(this.XPathCustomValidator_ServerValidate);

			this.selectableTreeNodesXPathTextBox.ID = "selectableTreeNodesTextBox";
			this.selectableTreeNodesXPathTextBox.CssClass = "umbEditorTextField";

			this.selectableTreeNodesXPathCustomValidator.ControlToValidate = this.selectableTreeNodesXPathTextBox.ID;
			this.selectableTreeNodesXPathCustomValidator.Display = ValidatorDisplay.Dynamic;
			this.selectableTreeNodesXPathCustomValidator.ServerValidate += new ServerValidateEventHandler(this.XPathCustomValidator_ServerValidate);

			var expandOptions = new[]
			{
				new ListItem("Collapse All", "0"),
				new ListItem("Expand All", "1"),
				new ListItem("Expand Selected", "2"),
			};
			this.selectExpandOptionDropDownList.DataSource = expandOptions;
			this.selectExpandOptionDropDownList.DataBind();

			this.selectOutputFormat.DataSource = Enum.GetValues(typeof(Settings.OutputFormat));
			this.selectOutputFormat.DataBind();

			this.Controls.AddPrevalueControls(
				this.treeStartNodeXPathTextBox,
				this.treeStartNodeXPathRequiredFieldValidator,
				this.treeStartNodeXPathCustomValidator,
				this.selectableTreeNodesXPathTextBox,
				this.selectableTreeNodesXPathCustomValidator,
				this.minSelectionTextBox,
				this.maxSelectionTextBox,
				this.selectAncestorsCheckBox,
				this.selectDescendentsCheckBox,
				this.showTreeIconsCheckBox,
				this.selectExpandOptionDropDownList,
				this.selectOutputFormat);
		}

		/// <summary>
		/// Handles the ServerValidate event of the XPathCustomValidator control.
		/// </summary>
		/// <param name="source">The source of the event.</param>
		/// <param name="args">The <see cref="System.Web.UI.WebControls.ServerValidateEventArgs"/> instance containing the event data.</param>
		private void XPathCustomValidator_ServerValidate(object source, ServerValidateEventArgs args)
		{
			string xPath = args.Value;
			bool isValid = false;

			try
			{
				if (uQuery.GetNodesByXPath(xPath).Count >= 0)
				{
					isValid = true;
				}
			}
			catch (XPathException)
			{
				((CustomValidator)source).ErrorMessage = " Syntax error in XPath expression";
			}

			args.IsValid = isValid;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Web.UI.Control.Load"/> event.
		/// </summary>
		/// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!this.Page.IsPostBack)
			{
				this.treeStartNodeXPathTextBox.Text = this.Options.StartTreeNodeXPath;
				this.selectableTreeNodesXPathTextBox.Text = this.Options.SelectableTreeNodesXPath;
				this.minSelectionTextBox.Text = this.Options.MinSelection.ToString();
				this.maxSelectionTextBox.Text = this.Options.MaxSelection.ToString();
				this.selectAncestorsCheckBox.Checked = this.Options.SelectAncestors;
				this.selectDescendentsCheckBox.Checked = this.Options.SelectDescendents;
				this.showTreeIconsCheckBox.Checked = this.Options.ShowTreeIcons;
				this.selectExpandOptionDropDownList.SelectedIndex = (int)this.Options.ExpandOption;
				this.selectOutputFormat.SelectedIndex = (int)this.Options.OutputFormat;
			}
		}

		/// <summary>
		/// Saves the pre value data to Umbraco
		/// </summary>
		public override void Save()
		{
			if (this.Page.IsValid)
			{
				this.Options.SelectAncestors = this.selectAncestorsCheckBox.Checked;
				this.Options.SelectDescendents = this.selectDescendentsCheckBox.Checked;
				this.Options.StartTreeNodeXPath = this.treeStartNodeXPathTextBox.Text;
				this.Options.SelectableTreeNodesXPath = this.selectableTreeNodesXPathTextBox.Text;
				
				int minSelection;
				if (int.TryParse(this.minSelectionTextBox.Text, out minSelection))
				{
					this.Options.MinSelection = minSelection;
				}

				int maxSelection;
				if (int.TryParse(this.maxSelectionTextBox.Text, out maxSelection))
				{
					this.Options.MaxSelection = maxSelection;
				}

				this.Options.ShowTreeIcons = this.showTreeIconsCheckBox.Checked;
				this.Options.ExpandOption = (CheckBoxTreeOptions.ExpandOptions)this.selectExpandOptionDropDownList.SelectedIndex;
				this.Options.OutputFormat = (Settings.OutputFormat)this.selectOutputFormat.SelectedIndex;

				this.SaveAsJson(this.Options);  // Serialize to Umbraco database field
			}
		}

		/// <summary>
		/// Renders the contents of the control to the specified writer. This method is used primarily by control developers.
		/// </summary>
		/// <param name="writer">A <see cref="T:System.Web.UI.HtmlTextWriter"/> that represents the output stream to render HTML content on the client.</param>
		protected override void RenderContents(HtmlTextWriter writer)
		{
			writer.AddPrevalueRow("XPath Start Node", "first matched node will be the root of the tree", this.treeStartNodeXPathTextBox, this.treeStartNodeXPathRequiredFieldValidator, this.treeStartNodeXPathCustomValidator);
			writer.AddPrevalueRow("XPath Filter", "not required - only matched nodes will have checkboxes", this.selectableTreeNodesXPathTextBox, this.selectableTreeNodesXPathCustomValidator);
			writer.AddPrevalueRow("Min Selection", "0 = no limit", this.minSelectionTextBox);
			writer.AddPrevalueRow("Max Selection", "0 = no limit", this.maxSelectionTextBox);
			writer.AddPrevalueRow("Select Ancestors", "automatically select all ancestors of checked nodes", this.selectAncestorsCheckBox);
			//writer.AddPrevalueRow("Select Descendents", this.selectDescendentsCheckBox);
			writer.AddPrevalueRow("Show Tree Icons", "(doesn't yet work with sprites)", this.showTreeIconsCheckBox);
			writer.AddPrevalueRow("Expand Options", "on load, select whether to collapse all, expand all or only selected branches.", this.selectExpandOptionDropDownList);
			writer.AddPrevalueRow("Output Format", this.selectOutputFormat);
		}
	}
}