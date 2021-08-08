#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PrimitiveValueConflictDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using System;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Evaluates all strings, enums and primitive types and ensures EditorGUI.showMixedValue is true if there are any value conflicts in the current selection.
    /// </summary>

    [DrawerPriority(0.5, 0, 0)]
    [AllowGUIEnabledForReadonly]
    public sealed class PrimitiveValueConflictDrawer<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems
    {
        /// <summary>
        /// Sets the drawer to only be evaluated on primitive types, strings and enums.
        /// </summary>
        public override bool CanDrawTypeFilter(Type type)
        {
            // Removed type.IsValueType because if a value inside a struct was changed,
            // All values would be overriden when multi-editing.
            return type.IsPrimitive || type == typeof(string) || type.IsEnum /*|| type.IsValueType*/;
        }

        protected override bool CanDrawValueProperty(InspectorProperty property)
        {
            return property.Tree.WeakTargets.Count > 1;
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            // showMixedValue will not be applied to all child properties.
            if (entry.ValueState == PropertyValueState.PrimitiveValueConflict)
            {
                GUI.changed = false;

                EditorGUI.showMixedValue = true;
                this.CallNextDrawer(label);

                if (GUI.changed)
                {
                    // Just to be sure, force the change for all targets
                    for (int i = 0; i < entry.ValueCount; i++)
                    {
                        entry.Values[i] = entry.SmartValue;
                    }
                }

                EditorGUI.showMixedValue = false;
            }
            else
            {
                EditorGUI.showMixedValue = false;
                this.CallNextDrawer(label);
            }
        }

        void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            if (property.ValueEntry.ValueState == PropertyValueState.PrimitiveValueConflict)
            {
                var tree = property.Tree;

                if (typeof(UnityEngine.Object).IsAssignableFrom(tree.TargetType))
                {
                    for (int i = 0; i < tree.WeakTargets.Count; i++)
                    {
                        object value = property.ValueEntry.WeakValues[i];
                        string valueString = value == null ? "null" : value.ToString();
                        string contentString = "Resolve value conflict with.../" + ((UnityEngine.Object)tree.WeakTargets[i]).name + " (" + valueString + ")";

                        genericMenu.AddItem(new GUIContent(contentString), false, () =>
                        {
                            property.Tree.DelayActionUntilRepaint(() =>
                            {
                                for (int j = 0; j < property.ValueEntry.WeakValues.Count; j++)
                                {
                                    property.ValueEntry.WeakValues[j] = value;
                                }
                            });
                        });
                    }
                }
            }
        }
    }
}
#endif