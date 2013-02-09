﻿namespace uComponents.DataTypes.DataTypeGrid.Factories
{
    using System.Web.UI;

    using uComponents.DataTypes.DataTypeGrid.Interfaces;
    using uComponents.DataTypes.DataTypeGrid.Model;

    using umbraco;
    using umbraco.cms.businesslogic.media;
    using umbraco.editorControls.mediapicker;

    /// <summary>a
    /// Factory for the <see cref="MemberPickerDataType"/>
    /// </summary>
    [DataTypeFactory(Priority = 0)]
    public class MediaPickerDataTypeFactory : IDataTypeFactory<MemberPickerDataType>
    {
        /// <summary>
        /// Method for customizing the way the <typeparamref name="MemberPickerDataType">datatype</typeparamref> value is displayed in the grid.
        /// </summary>
        /// <remarks>Called when the grid displays the cell value for the specified <typeparamref name="MemberPickerDataType">datatype</typeparamref>.</remarks>
        /// <param name="dataType">The <typeparamref name="MemberPickerDataType">datatype</typeparamref> instance.</param>
        /// <returns>The display value.</returns>
        public string GetDisplayValue(MemberPickerDataType dataType)
        {
            var value = dataType.Data.Value != null ? dataType.Data.Value.ToString() : string.Empty;

            int id;
            int.TryParse(value, out id);

            if (id > 0)
            {
                var m = new Media(id);

                // Return thumbnail if media type is Image
                if (m.ContentType.Alias.Equals("Image"))
                {
                    return string.Format("<a href='editMedia.aspx?id={2}' title='Edit media'><img src='{0}' alt='{1}'/></a>", m.GetImageThumbnailUrl(), m.Text, m.Id);
                }
            }

            return value;
        }

        /// <summary>
        /// Method for getting the backing object for the specified <typeparamref name="MemberPickerDataType">datatype</typeparamref>.
        /// </summary>
        /// <remarks>Called when the method <see cref="GridCell.GetObject{T}()"/> method is called on a <see cref="GridCell"/>.</remarks>
        /// <param name="dataType">The <typeparamref name="MemberPickerDataType">datatype</typeparamref> instance.</param>
        /// <returns>The backing object.</returns>
        public object GetObject(MemberPickerDataType dataType)
        {
            var value = dataType.Data.Value != null ? dataType.Data.Value.ToString() : string.Empty;

            int id;
            int.TryParse(value, out id);

            if (id > 0)
            {
                return new Media(id);
            }

            return default(Media);
        }

        /// <summary>
        /// Method for performing special actions while creating the <typeparamref name="MemberPickerDataType">datatype</typeparamref> editor.
        /// </summary>
        /// <remarks>Called when the grid creates the editor controls for the specified <typeparamref name="MemberPickerDataType">datatype</typeparamref>.</remarks>
        /// <param name="dataType">The <typeparamref name="MemberPickerDataType">datatype</typeparamref> instance.</param>
        /// <param name="container">The editor control container.</param>
        public void Configure(MemberPickerDataType dataType, Control container)
        {
        }

        /// <summary>
        /// Method for executing special actions before saving the editor value to the database.
        /// </summary>
        /// <remarks>Called when the grid is saved for the specified <typeparamref name="MemberPickerDataType">datatype</typeparamref>.</remarks>
        /// <param name="dataType">The <typeparamref name="MemberPickerDataType">datatype</typeparamref> instance.</param>
        public void Save(MemberPickerDataType dataType)
        {
        }
    }
}