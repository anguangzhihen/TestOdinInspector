#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="BooleanDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Bool property drawer.
    /// </summary>
    public sealed class BooleanDrawer : OdinValueDrawer<bool>
    {
        private GUILayoutOption[] options;

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            bool value = this.ValueEntry.SmartValue;
            EditorGUI.BeginChangeCheck();
            if (label == null)
            {
                this.options = this.options ?? new GUILayoutOption[] { GUILayout.ExpandWidth(false) };

                var w = GUIHelper.CurrentIndentAmount;
                var rect = GUILayoutUtility.GetRect(15 + w, EditorGUIUtility.singleLineHeight, this.options).AddXMin(w);

                // EditorGUI.Toggle does stuff with EditorGUI.indentLevel that causes the checkbox to move out of the rect that we've given it here.
                // I'm pushing indent level 0 and handling the indentation manually myself to fix this issue.
                GUIHelper.PushIndentLevel(0);
                value = EditorGUI.Toggle(rect, value);
                GUIHelper.PopIndentLevel();
            }
            else
            {
                value = EditorGUILayout.Toggle(label, value);
            }

            if (EditorGUI.EndChangeCheck())
            {
                this.ValueEntry.SmartValue = value;
            }
        }
    }
}
#endif