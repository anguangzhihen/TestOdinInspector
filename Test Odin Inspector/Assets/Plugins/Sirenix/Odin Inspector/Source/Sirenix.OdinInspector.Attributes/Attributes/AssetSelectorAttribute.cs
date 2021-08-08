//-----------------------------------------------------------------------
// <copyright file="AssetSelectorAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;
    using System.Linq;

    /// <summary>
    /// The AssetSelector attribute can be used on all Unity types and will prepend a small button next to the object field that when clicked,
    /// will present the user with a dropdown of assets to select from which can be customized from the attribute.
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class AssetSelectorAttribute : Attribute
    {
        /// <summary>
        /// True by default.
        /// </summary>
        public bool IsUniqueList = true;

        /// <summary>
        /// True by default. If the ValueDropdown attribute is applied to a list, then disabling this,
        /// will render all child elements normally without using the ValueDropdown. The ValueDropdown will
        /// still show up when you click the add button on the list drawer, unless <see cref="DisableListAddButtonBehaviour"/> is true.
        /// </summary>
        public bool DrawDropdownForListElements = true;

        /// <summary>
        /// False by default.
        /// </summary>
        public bool DisableListAddButtonBehaviour;

        /// <summary>
        /// If the ValueDropdown attribute is applied to a list, and <see cref="IsUniqueList"/> is set to true, then enabling this,
        /// will exclude existing values, instead of rendering a checkbox indicating whether the item is already included or not.
        /// </summary>
        public bool ExcludeExistingValuesInList;

        /// <summary>
        /// If the dropdown renders a tree-view, then setting this to true will ensure everything is expanded by default.
        /// </summary>
        public bool ExpandAllMenuItems = true;

        /// <summary>
        /// By default, the dropdown will create a tree view.
        /// </summary>
        public bool FlattenTreeView;

        /// <summary>
        /// Gets or sets the width of the dropdown. Default is zero.
        /// </summary>
        public int DropdownWidth;

        /// <summary>
        /// Gets or sets the height of the dropdown. Default is zero.
        /// </summary>
        public int DropdownHeight;

        /// <summary>
        /// Gets or sets the title for the dropdown. Null by default.
        /// </summary>
        public string DropdownTitle;

        /// <summary>
        /// Specify which folders to search in. Specifying no folders will make it search in your entire project.
        /// Use the <see cref="Paths"/> property for a more clean way of populating this array through attributes.
        /// </summary>
        public string[] SearchInFolders;

        /// <summary>
        /// The filters we should use when calling AssetDatabase.FindAssets.
        /// </summary>
        public string Filter;

        /// <summary>
        /// <para>
        /// Specify which folders to search in. Specifying no folders will make it search in your entire project.
        /// You can decalir multiple paths using '|' as the seperator.
        /// Example: <code>[AssetList(Paths = "Assets/Textures|Assets/Other/Textures")]</code>
        /// </para>
        /// <para>
        /// This property is simply a more clean way of populating the <see cref="SearchInFolders"/> array. 
        /// </para>
        /// </summary>
        public string Paths
        {
            set
            {
                this.SearchInFolders = value.Split('|')
                    .Select(x => x.Trim().Trim('/', '\\'))
                    .ToArray();
            }
            get { return this.SearchInFolders == null ? null : string.Join(",", this.SearchInFolders); }
        }
    }
}