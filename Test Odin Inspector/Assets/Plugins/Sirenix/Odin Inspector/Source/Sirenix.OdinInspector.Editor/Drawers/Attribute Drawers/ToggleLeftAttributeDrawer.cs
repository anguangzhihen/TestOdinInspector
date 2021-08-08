#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ToggleLeftAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws properties marked with <see cref="ToggleLeftAttribute"/>.
    /// </summary>
    /// <seealso cref="ToggleLeftAttribute"/>
    public sealed class ToggleLeftAttributeDrawer : OdinAttributeDrawer<ToggleLeftAttribute, bool>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;

            entry.SmartValue = label == null ?
               EditorGUILayout.ToggleLeft(GUIContent.none, entry.SmartValue) :
               EditorGUILayout.ToggleLeft(label, entry.SmartValue);
        }
    }
}
#endif