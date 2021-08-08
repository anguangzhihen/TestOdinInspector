#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="CompositeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Utilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using System;
    using Sirenix.Utilities;

    /// <summary>
    /// Drawer for composite properties.
    /// </summary>
    [DrawerPriority(0, 0, 0.1)]
    public class CompositeDrawer : OdinDrawer
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
            var property = this.Property;

            if (property.IsTreeRoot)
            {
                label = null;
            }

            if (property.Children.Count == 0)
            {
                if (property.ValueEntry != null)
                {
                    if (label != null)
                    {
                        var rect = EditorGUILayout.GetControlRect();
                        GUI.Label(rect, label);
                    }
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    {
                        if (label != null)
                        {
                            EditorGUILayout.PrefixLabel(label);
                        }
                        SirenixEditorGUI.WarningMessageBox("There is no drawer defined for property " + property.NiceName + " of type " + property.Info.PropertyType + ".");
                    }
                    GUILayout.EndHorizontal();
                }

                return;
            }


            if (label == null)
            {
                for (int i = 0; i < property.Children.Count; i++)
                {
                    var child = property.Children[i];
                    child.Draw(child.Label);
                }
            }
            else
            {
                var tmp = EditorGUIUtility.fieldWidth;
                EditorGUIUtility.fieldWidth = 10;
                var foldoutRect = EditorGUILayout.GetControlRect(false);
                EditorGUIUtility.fieldWidth = tmp;

                if (this.searchFilter != null)
                {
                    Rect rect = GUILayoutUtility.GetLastRect().AddXMin(GUIHelper.BetterLabelWidth).AddY(1);
                    var newTerm = SirenixEditorGUI.SearchField(rect, this.searchFilter.SearchTerm, false, this.searchFieldControlName);

                    if (newTerm != this.searchFilter.SearchTerm)
                    {
                        this.searchFilter.SearchTerm = newTerm;
                        this.Property.Tree.DelayActionUntilRepaint(() =>
                        {
                            if (!string.IsNullOrEmpty(newTerm))
                            {
                                this.Property.State.Expanded = true;
                            }

                            this.searchFilter.UpdateSearch();
                            GUIHelper.RequestRepaint();
                        });
                    }
                }
                this.Property.State.Expanded = SirenixEditorGUI.Foldout(foldoutRect, this.Property.State.Expanded, label);

                if (SirenixEditorGUI.BeginFadeGroup(this, this.Property.State.Expanded))
                {
                    EditorGUI.indentLevel++;

                    if (this.searchFilter != null && this.searchFilter.HasSearchResults)
                    {
                        this.searchFilter.DrawSearchResults();
                    }
                    else
                    {
                        for (int i = 0; i < property.Children.Count; i++)
                        {
                            var child = property.Children[i];
                            child.Draw(child.Label);
                        }
                    }

                    EditorGUI.indentLevel--;
                }
                SirenixEditorGUI.EndFadeGroup();
            }
        }
    }
}
#endif