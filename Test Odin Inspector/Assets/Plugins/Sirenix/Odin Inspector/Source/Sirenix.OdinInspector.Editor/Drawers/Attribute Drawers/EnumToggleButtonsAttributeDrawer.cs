#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="EnumToggleButtonsAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System.Linq;
    using UnityEngine;
    using System;
    using System.Collections.Generic;
    using UnityEditor;

    /// <summary>
    /// Draws an enum in a horizontal button group instead of a dropdown.
    /// </summary>
    public class EnumToggleButtonsAttributeDrawer<T> : OdinAttributeDrawer<EnumToggleButtonsAttribute, T>
    {
        private static bool DoManualColoring = UnityVersion.IsVersionOrGreater(2019, 3);
        private static Color ActiveColor = EditorGUIUtility.isProSkin ? Color.white : new Color(0.802f, 0.802f, 0.802f, 1f);
        private static Color InactiveColor = EditorGUIUtility.isProSkin ? new Color(0.75f, 0.75f, 0.75f, 1f) : Color.white;

        private GUIContent[] Names;
        private ulong[] Values;
        private float[] NameSizes;
        private bool IsFlagsEnum;
        private List<int> ColumnCounts;
        private float PreviousControlRectWidth;
        private Color?[] SelectionColors;

        /// <summary>
        /// Returns <c>true</c> if the drawer can draw the type.
        /// </summary>
        public override bool CanDrawTypeFilter(Type type)
        {
            return type.IsEnum;
        }

        protected override void Initialize()
        {
            var enumType = this.ValueEntry.TypeOfValue;
            var enumNames = Enum.GetNames(enumType);

            this.Names = enumNames.Select(x => new GUIContent(StringExtensions.SplitPascalCase(x))).ToArray();
            this.Values = new ulong[this.Names.Length];
            this.IsFlagsEnum = enumType.IsDefined<FlagsAttribute>();
            this.NameSizes = this.Names.Select(x => SirenixGUIStyles.MiniButtonMid.CalcSize(x).x).ToArray();
            this.ColumnCounts = new List<int>() { this.NameSizes.Length };
            GUIHelper.RequestRepaint();

            for (int i = 0; i < this.Values.Length; i++)
            {
                this.Values[i] = TypeExtensions.GetEnumBitmask(Enum.Parse(enumType, enumNames[i]), enumType);
            }

            if (DoManualColoring)
            {
                this.SelectionColors = new Color?[this.Names.Length];
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;

            var t = entry.WeakValues[0].GetType();
            int i = 1;
            for (; i < entry.WeakValues.Count; i++)
            {
                if (t != entry.WeakValues[i].GetType())
                {
                    SirenixEditorGUI.ErrorMessageBox("ToggleEnum does not support multiple different enum types.");
                    return;
                }
            }


            ulong value = TypeExtensions.GetEnumBitmask(entry.SmartValue, typeof(T));

            Rect controlRect = new Rect();

            i = 0;
            for (int j = 0; j < this.ColumnCounts.Count; j++)
            {
                int id;
                bool hasFocus;
                Rect rect;
                SirenixEditorGUI.GetFeatureRichControlRect(j == 0 ? label : GUIContent.none, out id, out hasFocus, out rect);

                if (j == 0)
                {
                    controlRect = rect;
                }
                else
                {
                    rect.xMin = controlRect.xMin;
                }

                var xMax = rect.xMax;
                rect.width /= this.ColumnCounts[j];
                rect.width = (int)rect.width;
                int from = i;
                int to = i + this.ColumnCounts[j];
                for (; i < to; i++)
                {
                    bool selected;

                    if (this.IsFlagsEnum)
                    {
                        var mask = TypeExtensions.GetEnumBitmask(this.Values[i], typeof(T));

                        if (value == 0)
                        {
                            selected = mask == 0;
                        }
                        else if (mask != 0)
                        {
                            selected = (mask & value) == mask;
                        }
                        else
                        {
                            selected = false;
                        }
                    }
                    else
                    {
                        selected = this.Values[i] == value;
                    }

                    Color? color = default(Color?);

                    if (DoManualColoring)
                    {
                        Color targetColor = selected ? ActiveColor : InactiveColor;

                        color = this.SelectionColors[i];

                        if (!color.HasValue)
                        {
                            color = targetColor;
                        }
                        else if (color.Value != targetColor && Event.current.type == EventType.Layout)
                        {
                            float delta = EditorTimeHelper.Time.DeltaTime * 4;

                            color = new Color(
                                Mathf.MoveTowards(color.Value.r, targetColor.r, delta),
                                Mathf.MoveTowards(color.Value.g, targetColor.g, delta),
                                Mathf.MoveTowards(color.Value.b, targetColor.b, delta),
                                Mathf.MoveTowards(color.Value.a, targetColor.a, delta)
                            );

                            GUIHelper.RequestRepaint();
                        }

                        this.SelectionColors[i] = color;
                    }

                    GUIStyle style;
                    Rect btnRect = rect;
                    if (i == from && i == to - 1)
                    {
                        style = selected ? SirenixGUIStyles.MiniButtonSelected : SirenixGUIStyles.MiniButton;
                        btnRect.x -= 1;
                        btnRect.xMax = xMax + 1;
                    }
                    else if (i == from)
                        style = selected ? SirenixGUIStyles.MiniButtonLeftSelected : SirenixGUIStyles.MiniButtonLeft;
                    else if (i == to - 1)
                    {
                        style = selected ? SirenixGUIStyles.MiniButtonRightSelected : SirenixGUIStyles.MiniButtonRight;
                        btnRect.xMax = xMax;
                    }
                    else
                        style = selected ? SirenixGUIStyles.MiniButtonMidSelected : SirenixGUIStyles.MiniButtonMid;

                    if (DoManualColoring)
                    {
                        GUIHelper.PushColor(color.Value * GUI.color);
                    }

                    if (GUI.Button(btnRect, this.Names[i], style))
                    {
                        GUIHelper.RemoveFocusControl();

                        if (!this.IsFlagsEnum || Event.current.button == 1 || Event.current.modifiers == EventModifiers.Control)
                        {
                            entry.WeakSmartValue = Enum.ToObject(typeof(T), this.Values[i]);
                        }
                        else
                        {
                            if (this.Values[i] == 0)
                            {
                                value = 0;
                            }
                            else if (selected)
                            {
                                value &= ~this.Values[i];
                            }
                            else
                            {
                                value |= this.Values[i];
                            }

                            entry.WeakSmartValue = Enum.ToObject(typeof(T), value);
                        }

                        GUIHelper.RequestRepaint();
                    }

                    if (DoManualColoring)
                    {
                        GUIHelper.PopColor();
                    }

                    rect.x += rect.width;
                }
            }

            if (Event.current.type == EventType.Repaint && this.PreviousControlRectWidth != controlRect.width)
            {
                this.PreviousControlRectWidth = controlRect.width;

                float maxBtnWidth = 0;
                int row = 0;
                this.ColumnCounts.Clear();
                this.ColumnCounts.Add(0);
                i = 0;
                for (; i < this.NameSizes.Length; i++)
                {
                    float btnWidth = this.NameSizes[i] + 3;
                    int columnCount = ++this.ColumnCounts[row];
                    float columnWidth = controlRect.width / columnCount;

                    maxBtnWidth = Mathf.Max(btnWidth, maxBtnWidth);

                    if (maxBtnWidth > columnWidth && columnCount > 1)
                    {
                        this.ColumnCounts[row]--;
                        this.ColumnCounts.Add(1);
                        row++;
                        maxBtnWidth = btnWidth;
                    }
                }
            }
        }
    }
}
#endif