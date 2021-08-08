#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="EnumPagingAttributeDrawer.cs" company="Sirenix IVS">
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
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Odin drawer for the <see cref="EnumPagingAttribute"/>.
    /// </summary>

    public class EnumPagingAttributeDrawer<T> : OdinAttributeDrawer<EnumPagingAttribute, T>
    {
        /// <summary>
        /// Returns <c>true</c> if the drawer can draw the type.
        /// </summary>
        public override bool CanDrawTypeFilter(Type type)
        {
            return type.IsEnum;
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            var rect = EditorGUILayout.GetControlRect(label != null);

            if (label != null)
            {
                rect = EditorGUI.PrefixLabel(rect, label);
            }

            const float arrowWidth = 20;

            var btnRightRect = rect.AlignRight(arrowWidth);
            var btnLeftRect = btnRightRect;
            btnLeftRect.x -= btnLeftRect.width;
            btnLeftRect.height -= 1;
            btnRightRect.height -= 1;

            if (GUI.Button(btnLeftRect, GUIContent.none))
            {
                var names = Enum.GetNames(typeof(T));
                var name = Enum.GetName(typeof(T), entry.SmartValue);

                var currNameIndex = (names as IList<string>).IndexOf(name);
                currNameIndex = MathUtilities.Wrap(currNameIndex - 1, 0, names.Length);
                entry.SmartValue = (T)Enum.Parse(typeof(T), names[currNameIndex]);
            }

            if (GUI.Button(btnRightRect, GUIContent.none))
            {
                var names = Enum.GetNames(typeof(T));
                var name = Enum.GetName(typeof(T), entry.SmartValue);

                var currNameIndex = (names as IList<string>).IndexOf(name);
                currNameIndex = MathUtilities.Wrap(currNameIndex + 1, 0, names.Length);
                entry.SmartValue = (T)Enum.Parse(typeof(T), names[currNameIndex]);
            }

            EditorIcons.TriangleLeft.Draw(btnLeftRect.AlignCenter(16, 16));
            EditorIcons.TriangleRight.Draw(btnRightRect.AlignCenter(16, 16));

            rect.xMax -= btnRightRect.width * 2;

            entry.WeakSmartValue = SirenixEditorFields.EnumDropdown(rect, (Enum)entry.WeakSmartValue);
        }
    }
}
#endif