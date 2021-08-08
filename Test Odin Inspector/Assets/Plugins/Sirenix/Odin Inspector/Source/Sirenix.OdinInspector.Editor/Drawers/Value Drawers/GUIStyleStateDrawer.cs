#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="GUIStyleStateDrawer.cs" company="Sirenix IVS">
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
    /// The GUIStyleState Drawer
    /// </summary>
    /// <seealso cref="Sirenix.OdinInspector.Editor.OdinValueDrawer{UnityEngine.GUIStyleState}" />
    public class GUIStyleStateDrawer : OdinValueDrawer<GUIStyleState>
    {
        private bool isVisible;

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        protected override void Initialize()
        {
            this.isVisible = SirenixEditorGUI.ExpandFoldoutByDefault;
        }

        /// <summary>
        /// Draws the property with GUILayout support.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            var property = entry.Property;

            if (label != null)
            {
                this.isVisible = SirenixEditorGUI.Foldout(this.isVisible, label);
                if (SirenixEditorGUI.BeginFadeGroup(isVisible, this.isVisible))
                {
                    EditorGUI.indentLevel++;
                    entry.SmartValue.background = (Texture2D)SirenixEditorFields.UnityObjectField(label, entry.SmartValue.background, typeof(Texture2D), true);
                    entry.SmartValue.textColor = EditorGUILayout.ColorField(label ?? GUIContent.none, entry.SmartValue.textColor);
                    EditorGUI.indentLevel--;
                }
                SirenixEditorGUI.EndFadeGroup();
            }
            else
            {
                entry.SmartValue.background = (Texture2D)SirenixEditorFields.UnityObjectField(label, entry.SmartValue.background, typeof(Texture2D), true);
                entry.SmartValue.textColor = EditorGUILayout.ColorField(label ?? GUIContent.none, entry.SmartValue.textColor);
            }
        }
    }
}
#endif