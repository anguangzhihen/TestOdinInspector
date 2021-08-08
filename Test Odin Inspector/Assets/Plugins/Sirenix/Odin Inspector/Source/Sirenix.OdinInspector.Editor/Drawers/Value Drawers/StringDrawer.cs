#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="StringDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// String property drawer.
    /// </summary>
    public sealed class StringDrawer : OdinValueDrawer<string>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            entry.SmartValue = label == null ?
                EditorGUILayout.TextField(entry.SmartValue, EditorStyles.textField) :
                EditorGUILayout.TextField(label, entry.SmartValue, EditorStyles.textField);
        }
    }
}
#endif