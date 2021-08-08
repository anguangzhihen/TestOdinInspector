#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PrimitiveCompositeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Property drawer for primitive composite properties.
    /// </summary>
    public abstract class PrimitiveCompositeDrawer<T> : OdinValueDrawer<T>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            bool conflict = false;
            int childCount = entry.Property.Children.Count;

            for (int i = 0; i < childCount; i++)
            {
                var child = entry.Property.Children[i];

                if (child.ValueEntry != null && child.ValueEntry.ValueState == PropertyValueState.PrimitiveValueConflict)
                {
                    conflict = true;
                    break;
                }
            }

            if (conflict)
            {
                EditorGUI.showMixedValue = true;
                GUI.changed = false;
            }

            this.DrawPropertyField(entry, label);

            if (conflict)
            {
                EditorGUI.showMixedValue = false;

                if (GUI.changed)
                {
                    var value = entry.SmartValue;

                    for (int i = 0; i < entry.ValueCount; i++)
                    {
                        entry.Values[i] = value;
                    }
                }
            }
        }

        /// <summary>
        /// Draws the property field.
        /// </summary>
        protected abstract void DrawPropertyField(IPropertyValueEntry<T> entry, GUIContent label);
    }
}
#endif