#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="Color32Drawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.Utilities.Editor;
    using System;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Color32 property drawer.
    /// </summary>
    public sealed class Color32Drawer : PrimitiveCompositeDrawer<Color32>, IDefinesGenericMenuItems
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyField(IPropertyValueEntry<Color32> entry, GUIContent label)
        {
            var rect = EditorGUILayout.GetControlRect(label != null);

            if (label != null)
            {
                rect = EditorGUI.PrefixLabel(rect, label);
            }

            bool disableContext = false;

            if (Event.current.OnMouseDown(rect, 1, false))
            {
                // Disable Unity's color field's own context menu
                GUIHelper.PushEventType(EventType.Used);
                disableContext = true;
            }

            entry.SmartValue = EditorGUI.ColorField(rect, entry.SmartValue);

            if (disableContext)
            {
                GUIHelper.PopEventType();
            }
        }

        void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            ColorDrawer.PopulateGenericMenu((IPropertyValueEntry<Color32>)property.ValueEntry, genericMenu);
        }
    }
}
#endif