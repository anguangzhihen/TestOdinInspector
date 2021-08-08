#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TypeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.Serialization;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using Sirenix.Utilities.Editor.Expressions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Type property drawer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DrawerPriority(0, 0, 2001)]
    public class TypeDrawer<T> : OdinValueDrawer<T> where T : Type
    {
        private static readonly TwoWaySerializationBinder Binder = new DefaultSerializationBinder();

            public string TypeNameTemp;
            public bool IsValid = true;
            public string UniqueControlName;
            public bool WasFocusedControl;

        protected override void Initialize()
        {
            this.UniqueControlName = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            
            if (!this.IsValid)
            {
                GUIHelper.PushColor(Color.red);
            }

            GUI.SetNextControlName(this.UniqueControlName);
            
            Rect rect = EditorGUILayout.GetControlRect();

            if (label != null)
            {
                rect = EditorGUI.PrefixLabel(rect, label);
            }
            
            Rect fieldRect = rect;
            Rect dropdownRect = rect.AlignRight(18);

            // Dropdown button.
            EditorGUIUtility.AddCursorRect(dropdownRect, MouseCursor.Arrow);
            if (GUI.Button(dropdownRect, GUIContent.none, GUIStyle.none))
            {
                TypeSelector selector = new TypeSelector(AssemblyTypeFlags.All, false);

                selector.SelectionConfirmed += t =>
                {
                    var type = t.FirstOrDefault();

                    entry.Property.Tree.DelayAction(() =>
                    {
                        entry.WeakSmartValue = type;
                        this.IsValid = true;
                        entry.ApplyChanges();
                    });
                };

                selector.SetSelection(entry.SmartValue);
                selector.ShowInPopup(rect, 350);
            }

            // Reset type name.
            if (Event.current.type == EventType.Layout)
            {
                this.TypeNameTemp = entry.SmartValue != null ? Binder.BindToName(entry.SmartValue) : null;
            }

            EditorGUI.BeginChangeCheck();
            this.TypeNameTemp = SirenixEditorFields.DelayedTextField(fieldRect, this.TypeNameTemp);

            // Draw dropdown button.
            EditorIcons.TriangleDown.Draw(dropdownRect);

            if (!this.IsValid)
            {
                GUIHelper.PopColor();
            }

            bool isFocused = GUI.GetNameOfFocusedControl() == this.UniqueControlName;
            bool defocused = false;

            if (isFocused != this.WasFocusedControl)
            {
                defocused = !isFocused;
                this.WasFocusedControl = isFocused;
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (this.TypeNameTemp == null || string.IsNullOrEmpty(this.TypeNameTemp.Trim()))
                {
                    // String is empty
                    entry.SmartValue = null;
                    this.IsValid = true;
                }
                else
                {
                    Type type = Binder.BindToType(this.TypeNameTemp);

                    if (type == null)
                    {
                        type = AssemblyUtilities.GetTypeByCachedFullName(this.TypeNameTemp);
                    }

                    if (type == null)
                    {
                        ExpressionUtility.TryParseTypeNameAsCSharpIdentifier(this.TypeNameTemp, out type);
                    }

                    if (type == null)
                    {
                        this.IsValid = false;
                    }
                    else
                    {
                        // Use WeakSmartValue in case of a different Type-derived instance showing up somehow, so we don't get cast errors
                        entry.WeakSmartValue = type;
                        this.IsValid = true;
                    }
                }
            }

            if (defocused)
            {
                // Ensure we show the full type name when the control is defocused
                this.TypeNameTemp = entry.SmartValue == null ? "" : Binder.BindToName(entry.SmartValue);
                this.IsValid = true;
            }
        }
    }
}
#endif