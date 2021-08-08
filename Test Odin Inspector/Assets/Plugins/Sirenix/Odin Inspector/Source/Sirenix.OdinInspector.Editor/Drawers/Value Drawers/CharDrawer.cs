#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="CharDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Char property drawer.
    /// </summary>
    public sealed class CharDrawer : OdinValueDrawer<char>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            EditorGUI.BeginChangeCheck();
            string s = new string(entry.SmartValue, 1);
            s = SirenixEditorFields.TextField(label, s);

            if (EditorGUI.EndChangeCheck() && s.Length > 0)
            {
                entry.SmartValue = s[0];
            }
        }
    }
}
#endif