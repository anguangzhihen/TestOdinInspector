#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ToggleAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Utilities.Editor;
    using UnityEngine;

    /// <summary>
    /// Draws properties marked with <see cref="ToggleAttribute"/>.
    /// </summary>
    /// <seealso cref="ToggleAttribute"/>
    public class ToggleAttributeDrawer : OdinAttributeDrawer<ToggleAttribute>
    {
        private InspectorProperty toggleProperty;
        private PropertyContext<string> openToggleGlobalContext;

        protected override void Initialize()
        {
            this.toggleProperty = this.Property.Children.Get(this.Attribute.ToggleMemberName);

            if (this.Attribute.CollapseOthersOnExpand)
            {
                var parent = this.Property.ParentValueProperty;

                while (parent != null && !parent.Info.HasBackingMembers)
                {
                    parent = parent.ParentValueProperty;
                }

                if (parent == null) parent = this.Property.Tree.RootProperty;

                // This works together with other "parallel" ToggleGroupAttributeDrawers and ToggleAttributeDrawers.
                openToggleGlobalContext = parent.Context.GetGlobal<string>("OpenFoldoutToggleGroup", (string)null);
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (this.toggleProperty == null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.Attribute.ToggleMemberName + " is not a member of " + this.Property.NiceName + ".");
            }
            else if (this.toggleProperty.ValueEntry.TypeOfValue != typeof(bool))
            {
                SirenixEditorGUI.ErrorMessageBox(this.Attribute.ToggleMemberName + " on " + this.Property.NiceName + "  must be a boolean.");
            }
            else
            {
                bool isEnabled = (bool)this.toggleProperty.ValueEntry.WeakSmartValue;

                if (this.Attribute.CollapseOthersOnExpand && openToggleGlobalContext != null && openToggleGlobalContext.Value != null && openToggleGlobalContext.Value != Property.Path)
                {
                    this.Property.State.Expanded = false;
                }

                bool prev = this.Property.State.Expanded;
                bool visibleBuffer = this.Property.State.Expanded;
                if (SirenixEditorGUI.BeginToggleGroup(UniqueDrawerKey.Create(this.Property, this), ref isEnabled, ref visibleBuffer, label != null ? label.text : this.Property.NiceName))
                {
                    for (int i = 0; i < this.Property.Children.Count; i++)
                    {
                        var child = this.Property.Children[i];
                        if (child != this.toggleProperty)
                        {
                            child.Draw(child.Label);
                        }
                    }
                }
                SirenixEditorGUI.EndToggleGroup();

                this.Property.State.Expanded = visibleBuffer;
                if (this.openToggleGlobalContext != null && prev != this.Property.State.Expanded && this.Property.State.Expanded)
                {
                    this.openToggleGlobalContext.Value = this.Property.Path;
                }

                this.toggleProperty.ValueEntry.WeakSmartValue = isEnabled;
            }
        }
    }
}
#endif