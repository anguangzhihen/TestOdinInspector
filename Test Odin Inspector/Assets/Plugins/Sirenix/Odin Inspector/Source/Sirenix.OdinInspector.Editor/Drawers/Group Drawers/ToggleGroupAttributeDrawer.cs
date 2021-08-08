#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ToggleGroupAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Utilities.Editor;
    using UnityEngine;
    using Sirenix.OdinInspector.Editor.ValueResolvers;

    /// <summary>
    /// Draws all properties grouped together with the <see cref="ToggleGroupAttribute"/>
    /// </summary>
    /// <seealso cref="ToggleGroupAttribute"/>
    public class ToggleGroupAttributeDrawer : OdinGroupDrawer<ToggleGroupAttribute>
    {
        private ValueResolver<string> titleHelper;
        private string errorMessage;
        private InspectorProperty toggleProperty;
        private PropertyContext<string> openToggleGlobalContext;

        protected override void Initialize()
        {
            toggleProperty = Property.Children.Get(Attribute.ToggleMemberName);

            if (toggleProperty == null)
            {
                this.errorMessage = "No property or field named " + Attribute.ToggleMemberName + " found. Make sure the property is part of the inspector and the group.";
            }
            else
            {
                this.titleHelper = ValueResolver.GetForString(this.Property, Attribute.ToggleGroupTitle ?? Attribute.GroupName);

                if (this.titleHelper.HasError)
                {
                    this.errorMessage = this.titleHelper.ErrorMessage;
                }
            }

            if (Attribute.CollapseOthersOnExpand)
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
            if (this.errorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.errorMessage);
            }
            else
            {
                if (Attribute.CollapseOthersOnExpand)
                {
                    if (openToggleGlobalContext != null && openToggleGlobalContext.Value != null && openToggleGlobalContext.Value != Property.Path)
                    {
                        this.Property.State.Expanded = false;
                    }
                }

                bool isEnabled = (bool)toggleProperty.ValueEntry.WeakSmartValue;

                string title = this.titleHelper.GetValue();

                bool prev = this.Property.State.Expanded;
                bool visibleBuffer = this.Property.State.Expanded;
                if (SirenixEditorGUI.BeginToggleGroup(UniqueDrawerKey.Create(Property, this), ref isEnabled, ref visibleBuffer, title))
                {
                    for (int i = 0; i < Property.Children.Count; i++)
                    {
                        var child = Property.Children[i];
                        if (child != toggleProperty)
                        {
                            child.Draw(child.Label);
                        }
                    }
                }
                else
                {
                    // OnValueChanged is not fired if property is not drawn.
                    GUIHelper.BeginDrawToNothing();
                    toggleProperty.Draw(toggleProperty.Label);
                    GUIHelper.EndDrawToNothing();
                }
                SirenixEditorGUI.EndToggleGroup();

                this.Property.State.Expanded = visibleBuffer;
                if (openToggleGlobalContext != null && prev != this.Property.State.Expanded && this.Property.State.Expanded)
                {
                    openToggleGlobalContext.Value = Property.Path;
                }

                toggleProperty.ValueEntry.WeakSmartValue = isEnabled;
            }
        }
    }
}
#endif