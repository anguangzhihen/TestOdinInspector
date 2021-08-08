#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="HorizontalGroupAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Utilities;
    using Utilities.Editor;
    using UnityEngine;
    using System.Linq;
    using System;
    using UnityEditor;
    using Sirenix.OdinInspector.Editor.ValueResolvers;

    /// <summary>
    /// Drawer for the <see cref="HorizontalGroupAttribute"/>
    /// </summary>
    /// <seealso cref="HorizontalGroupAttribute"/>
    public class HorizontalGroupAttributeDrawer : OdinGroupDrawer<HorizontalGroupAttribute>
    {
        private float[] widths;
        private float[] minWidths;
        private float[] maxWidths;
        private float[] labelWidths;
        private float totalWidth;
        private Vector2[] margins;
        private Vector2[] paddings;
        private ValueResolver<string> titleGetter;
        private int containsPercentageWidth = 0;

        protected override void Initialize()
        {
            if (this.Attribute.Title != null)
            {
                this.titleGetter = ValueResolver.GetForString(this.Property, this.Attribute.Title);
            }

            this.widths = new float[this.Property.Children.Count];
            this.minWidths = new float[this.Property.Children.Count];
            this.maxWidths = new float[this.Property.Children.Count];
            this.labelWidths = new float[this.Property.Children.Count];
            this.margins = new Vector2[this.Property.Children.Count];
            this.paddings = new Vector2[this.Property.Children.Count];

            float percentageAllocated = 0;
            for (int i = 0; i < this.Property.Children.Count; i++)
            {
                var child = this.Property.Children[i];
                var attr = child.Children.Recurse()
                    .AppendWith(child)
                    .SelectMany(a => a.GetAttributes<HorizontalGroupAttribute>())
                    .FirstOrDefault(x => x.GroupID == Attribute.GroupID);

                if (attr == null)
                {
                    this.widths[i] = -1;
                }
                else
                {
                    this.widths[i] = attr.Width;
                    this.minWidths[i] = attr.MinWidth;
                    this.maxWidths[i] = attr.MaxWidth;
                    this.labelWidths[i] = attr.LabelWidth;

                    if (attr.Width > 0 && attr.Width < 1)
                    {
                        this.containsPercentageWidth++;
                        percentageAllocated += attr.Width;

                        // If we allocate 100% there is no way to resize the window down.
                        // In those cases, we convert the attribute to adjust itself automatically and Unity will ensure that
                        // that it reaches the 100% for us.
                        if (percentageAllocated >= 0.97)
                        {
                            percentageAllocated -= attr.Width;
                            this.widths[i] = 0;
                            attr.Width = 0;
                        }
                    }

                    if (attr.MinWidth > 0 && attr.MinWidth <= 1)
                    {
                        this.containsPercentageWidth++;

                        percentageAllocated += attr.MinWidth;
                        // Same thing for MinWidth.
                        if (percentageAllocated >= 0.97)
                        {
                            percentageAllocated -= attr.MinWidth;
                            this.minWidths[i] = 0;
                            attr.MinWidth = 0;
                        }
                    }

                    this.margins[i] = new Vector2(attr.MarginLeft, attr.MarginRight);
                    this.paddings[i] = new Vector2(attr.PaddingLeft, attr.PaddingRight);
                }
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (this.titleGetter != null)
            {
                if (this.titleGetter.HasError)
                {
                    SirenixEditorGUI.ErrorMessageBox(this.titleGetter.ErrorMessage);
                }
                else
                {
                    SirenixEditorGUI.Title(this.titleGetter.GetValue(), null, TextAlignment.Left, false);
                }
            }

            SirenixEditorGUI.BeginIndentedHorizontal();
            // GUILayout.BeginHorizontal();
            {
                if (Attribute.LabelWidth > 0) { GUIHelper.PushLabelWidth(Attribute.LabelWidth); }

                var prevFieldWidth = EditorGUIUtility.fieldWidth;
                EditorGUIUtility.fieldWidth = 40;

                for (int i = 0; i < Property.Children.Count; i++)
                {
                    float width, minWidth, maxWidth;
                    Vector2 padding, margin;

                    if (this.containsPercentageWidth > 1 && this.totalWidth == 0)
                    {
                        width = 20; // Start small and expand next frame. Instead of starting to big and slowly getting smaller.
                        minWidth = 0;
                        maxWidth = 0;
                        padding = new Vector2();
                        margin = new Vector2();
                    }
                    else
                    {
                        width = this.widths[i];
                        minWidth = this.minWidths[i];
                        maxWidth = this.maxWidths[i];
                        margin = this.margins[i];
                        padding = this.paddings[i];

                        if (padding.x > 0 && padding.x <= 1) padding.x = padding.x * this.totalWidth;
                        if (padding.y > 0 && padding.y <= 1) padding.y = padding.y * this.totalWidth;
                        if (margin.x > 0 && margin.x <= 1) margin.x = margin.x * this.totalWidth;
                        if (margin.y > 0 && margin.y <= 1) margin.y = margin.y * this.totalWidth;

                        if (width <= 1) width = width * this.totalWidth;

                        width -= padding.x + padding.y;

                        if (minWidth > 0)
                        {
                            if (minWidth <= 1) minWidth = minWidth * this.totalWidth;
                            minWidth -= padding.x + padding.y;
                        }

                        if (maxWidth > 0)
                        {
                            if (maxWidth <= 1) maxWidth = maxWidth * this.totalWidth;
                            maxWidth -= padding.x + padding.y;
                        }
                    }

                    GUILayoutOptions.GUILayoutOptionsInstance options = null;

                    if (minWidth > 0) options = GUILayoutOptions.MinWidth(minWidth);
                    if (maxWidth > 0) options = options == null ? GUILayoutOptions.MaxWidth(maxWidth) : options.MaxWidth(maxWidth);
                    if (options == null) options = GUILayoutOptions.Width(width < 0 ? 0 : width);

                    if ((margin.x + padding.x) != 0)
                    {
                        GUILayout.Space(margin.x + padding.x);
                    }

                    GUILayout.BeginVertical(options);
                    Property.Children[i].Draw(Property.Children[i].Label);
                    GUILayout.Space(-3);
                    GUILayout.EndVertical();

                    if ((margin.y + padding.y) != 0)
                    {
                        GUILayout.Space(margin.y + padding.y);
                    }
                }

                if (Event.current.type == EventType.Repaint)
                {
                    var newWidth = GUIHelper.GetCurrentLayoutRect().width;

                    if (this.totalWidth != newWidth)
                    {
                        GUIHelper.RequestRepaint();
                    }

                    this.totalWidth = newWidth;
                }

                EditorGUIUtility.fieldWidth = prevFieldWidth;
                if (Attribute.LabelWidth > 0) { GUIHelper.PopLabelWidth(); }
            }
            // GUILayout.EndHorizontal();
            SirenixEditorGUI.EndIndentedHorizontal();
        }
    }
}
#endif