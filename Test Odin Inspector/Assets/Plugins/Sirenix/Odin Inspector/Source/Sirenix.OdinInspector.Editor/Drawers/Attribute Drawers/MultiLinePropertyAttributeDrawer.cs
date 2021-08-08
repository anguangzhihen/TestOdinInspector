#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="MultiLinePropertyAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws string properties marked with <see cref="MultiLinePropertyAttribute"/>.
    /// This drawer works for both string field and properties, unlike <see cref="MultiLineAttributeDrawer"/>.
    /// </summary>
    /// <seealso cref="MultiLinePropertyAttribute"/>
    /// <seealso cref="MultilineAttribute"/>
    /// <seealso cref="DisplayAsStringAttribute"/>
    /// <seealso cref="InfoBoxAttribute"/>
    /// <seealso cref="DetailedInfoBoxAttribute"/>
    public sealed class MultiLinePropertyAttributeDrawer : OdinAttributeDrawer<MultiLinePropertyAttribute, string>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            var attribute = this.Attribute;

            var position = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight * attribute.Lines);
            position.height -= 2;

            if (label == null)
            {
                entry.SmartValue = EditorGUI.TextArea(position, entry.SmartValue, EditorStyles.textArea);
            }
            else
            {
                var controlID = GUIUtility.GetControlID(label, FocusType.Keyboard, position);
                var areaPosition = EditorGUI.PrefixLabel(position, controlID, label, EditorStyles.label);

                entry.SmartValue = EditorGUI.TextArea(areaPosition, entry.SmartValue, EditorStyles.textArea);
            }
        }
    }
}
#endif