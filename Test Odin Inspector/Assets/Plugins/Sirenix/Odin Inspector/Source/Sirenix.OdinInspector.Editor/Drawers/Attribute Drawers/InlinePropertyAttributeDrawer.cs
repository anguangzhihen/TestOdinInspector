#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InlinePropertyAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using System;
    using Utilities.Editor;
    using UnityEngine;
    using Sirenix.Utilities;
    using UnityEditor;

    /// <summary>
    /// Drawer for the <see cref="InlinePropertyAttribute"/> attribute.
    /// </summary>
    [DrawerPriority(0, 0, 0.11)]
    public class InlinePropertyAttributeDrawer : OdinAttributeDrawer<InlinePropertyAttribute>
    {
        private PropertySearchFilter searchFilter;
        private string searchFieldControlName;

        protected override void Initialize()
        {
            var searchAttr = this.Property.GetAttribute<SearchableAttribute>();

            if (searchAttr != null)
            {
                this.searchFilter = new PropertySearchFilter(this.Property, searchAttr);
                this.searchFieldControlName = "PropertyTreeSearchField_" + Guid.NewGuid().ToString();
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var pushLabelWidth = this.Attribute.LabelWidth > 0;
            if (label == null)
            {
                if (pushLabelWidth) GUIHelper.PushLabelWidth(this.Attribute.LabelWidth);
                this.CallNextDrawer(label);
                if (pushLabelWidth) GUIHelper.PopLabelWidth();
            }
            else
            {
                SirenixEditorGUI.BeginVerticalPropertyLayout(label);
                if (pushLabelWidth) GUIHelper.PushLabelWidth(this.Attribute.LabelWidth);

                if (this.searchFilter != null)
                {
                    var rect = EditorGUILayout.GetControlRect();
                    var newTerm = SirenixEditorGUI.SearchField(rect, this.searchFilter.SearchTerm, false, this.searchFieldControlName);

                    if (newTerm != this.searchFilter.SearchTerm)
                    {
                        this.searchFilter.SearchTerm = newTerm;
                        this.Property.Tree.DelayActionUntilRepaint(() =>
                        {
                            this.searchFilter.UpdateSearch();
                            GUIHelper.RequestRepaint();
                        });
                    }
                }

                if (this.searchFilter != null && this.searchFilter.HasSearchResults)
                {
                    this.searchFilter.DrawSearchResults();
                }
                else
                {
                    for (int i = 0; i < this.Property.Children.Count; i++)
                    {
                        var child = this.Property.Children[i];
                        child.Draw(child.Label);
                    }
                }

                if (pushLabelWidth) GUIHelper.PopLabelWidth();
                GUILayout.Space(2f);
                SirenixEditorGUI.EndVerticalPropertyLayout();
            }
        }
    }
}
#endif