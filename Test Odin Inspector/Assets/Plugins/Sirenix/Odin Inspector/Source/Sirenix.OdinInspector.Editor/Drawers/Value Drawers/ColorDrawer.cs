#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ColorDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Utilities;
    using UnityEditor;
    using UnityEngine;
    using System;
    using Sirenix.Utilities.Editor;

    /// <summary>
    /// Color property drawer.
    /// </summary>

    public sealed class ColorDrawer : PrimitiveCompositeDrawer<Color>, IDefinesGenericMenuItems
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyField(IPropertyValueEntry<Color> entry, GUIContent label)
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

        internal static void PopulateGenericMenu<T>(IPropertyValueEntry<T> entry, GenericMenu genericMenu)
        {
            Color color;

            if (entry.TypeOfValue == typeof(Color))
            {
                color = (Color)(object)entry.SmartValue;
            }
            else
            {
                color = (Color32)(object)entry.SmartValue;
            }

            Color colorInClipboard;
            bool hasColorInClipboard = ColorExtensions.TryParseString(EditorGUIUtility.systemCopyBuffer, out colorInClipboard);

            if (genericMenu.GetItemCount() > 0)
            {
                genericMenu.AddSeparator("");
            }

            genericMenu.AddItem(new GUIContent("Copy RGBA"), false, () =>
            {
                EditorGUIUtility.systemCopyBuffer = entry.SmartValue.ToString();
            });
            genericMenu.AddItem(new GUIContent("Copy HEX"), false, () =>
            {
                EditorGUIUtility.systemCopyBuffer = "#" + ColorUtility.ToHtmlStringRGBA(color);
            });
            genericMenu.AddItem(new GUIContent("Copy Color Code Declaration"), false, () =>
            {
                EditorGUIUtility.systemCopyBuffer = ColorExtensions.ToCSharpColor(color);
            });

            if (hasColorInClipboard)
            {
                genericMenu.ReplaceOrAdd("Paste", false, () =>
                {
                    entry.Property.Tree.DelayActionUntilRepaint(() =>
                    {
                        SetEntryValue(entry, colorInClipboard);
                    });

                    GUIHelper.RequestRepaint();
                });
            }
            else if (Clipboard.CanPaste(typeof(Color)) || Clipboard.CanPaste(typeof(Color32)))
            {
                genericMenu.ReplaceOrAdd("Paste", false, () =>
                {
                    entry.Property.Tree.DelayActionUntilRepaint(() =>
                    {
                        SetEntryValue(entry, Clipboard.Paste());
                    });

                    GUIHelper.RequestRepaint();
                });
            }
            else
            {
                genericMenu.AddDisabledItem(new GUIContent("Paste"));
            }
        }

        private static void SetEntryValue<T>(IPropertyValueEntry<T> entry, object value)
        {
            var type = value.GetType();
            T tValue;

            if (typeof(T) == typeof(Color))
            {
                if (type == typeof(Color)) tValue = (T)value;
                else tValue = (T)(object)(Color)(Color32)value;
            }
            else
            {
                if (type == typeof(Color)) tValue = (T)(object)(Color32)(Color)value;
                else tValue = (T)value;
            }

            for (int i = 0; i < entry.ValueCount; i++)
            {
                entry.Values[i] = tValue;
            }
        }

        void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            PopulateGenericMenu((IPropertyValueEntry<Color>)property.ValueEntry, genericMenu);
        }
    }
}
#endif