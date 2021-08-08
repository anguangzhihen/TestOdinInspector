#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ResponsiveButtonGroupAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Utilities.Editor;
    using UnityEngine;
    using System.Reflection;
    using System.Collections.Generic;
    using System;
    using System.Linq;

    /// <summary>
    /// Drawer for the ResponsiveButtonGroupAttribute.
    /// </summary>
    public class ResponsiveButtonGroupAttributeDrawer : OdinGroupDrawer<ResponsiveButtonGroupAttribute>
    {
        private Vector2[] btnSizes;
        private int[] colCounts;
        private int prevWidth = 400;
        private bool isFirstFrame = true;
        private int innerWidth;

        /// <summary>
        /// Draws the property with GUILayout support.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var property = this.Property;
            var attribute = this.Attribute;

            if (this.btnSizes == null || this.btnSizes.Length != property.Children.Count)
            {
                // reset
                this.colCounts = null;
                this.prevWidth = 400;

                // sizes
                this.btnSizes = new Vector2[property.Children.Count];
                for (int i = 0; i < this.btnSizes.Length; i++)
                {
                    var prop = property.Children[i].FindChild(x => x.Info.GetMemberInfo() is MethodInfo, true) ?? property.Children[i];
                    var btnAttr = prop.GetAttribute<ButtonAttribute>();
                    var size = btnAttr == null ? (int)attribute.DefaultButtonSize : btnAttr.ButtonHeight;

                    if (btnAttr == null)
                    {
                        prop.Context.GetGlobal("ButtonHeight", 0f).Value = (int)attribute.DefaultButtonSize;
                    }

                    size = (int)(SirenixGUIStyles.Button.CalcSize(prop.Label).x);
                    this.btnSizes[i] = new Vector2(size, btnAttr == null ? (int)attribute.DefaultButtonSize : btnAttr.ButtonHeight);
                }

                if (attribute.UniformLayout)
                {
                    var max = this.btnSizes.Max(x => x.x);
                    for (int i = 0; i < this.btnSizes.Length; i++)
                    {
                        this.btnSizes[i] = new Vector2(max, this.btnSizes[i].y);
                    }
                }
            }

            if (Event.current.type == EventType.Layout)
            {
                var recalc = false;
                var width = this.innerWidth;
                if (this.isFirstFrame)
                {
                    width = 999999;
                    this.isFirstFrame = false;
                    GUIHelper.RequestRepaint();
                }
                if (this.prevWidth != width || this.colCounts == null)
                {
                    if (width > 0) this.prevWidth = width;
                    recalc = true;
                    width = this.prevWidth;
                }

                this.colCounts = this.colCounts ?? new int[property.Children.Count];

                if (recalc)
                {
                    // clear
                    for (int i = 0; i < this.colCounts.Length; i++) this.colCounts[i] = 0;

                    var currentCol = 0;
                    var prevBtnSize = this.btnSizes[0];
                    var btnWidth = 0;
                    var jumpRow = false;

                    for (int i = 0; i < this.btnSizes.Length; i++)
                    {
                        btnWidth = Mathf.Max((int)this.btnSizes[i].x, btnWidth);
                        var colWidth = btnWidth * (this.colCounts[currentCol] + 1);
                        jumpRow = colWidth > width || (int)prevBtnSize.y != (int)this.btnSizes[i].y;
                        prevBtnSize = this.btnSizes[i];

                        if (jumpRow)
                        {
                            btnWidth = (int)this.btnSizes[i].x;
                            if (this.colCounts[currentCol] != 0)
                            {
                                currentCol++;
                            }
                        }

                        this.colCounts[currentCol]++;
                    }
                }
            }

            DefaultMethodDrawer.DontDrawMethodParameters = true;
            int j = 0;
            for (int y = 0; y < this.colCounts.Length && j < property.Children.Count; y++)
            {
                GUILayout.BeginHorizontal();
                for (int x = 0; x < this.colCounts[y] && j < property.Children.Count; x++)
                {
                    var child = property.Children[j];
                    child.Draw(child.Label);
                    j++;
                }
                GUILayout.EndHorizontal();
            }
            DefaultMethodDrawer.DontDrawMethodParameters = false;

            if (Event.current.type == EventType.Repaint)
            {
                this.innerWidth = (int)GUIHelper.GetCurrentLayoutRect().width;
            }
        }
    }

    //public class ResponsiveButtonGroupAttributeResolver : OdinAttributeResolver
    //{
    //    public override bool CanResolve(MemberInfo member)
    //    {
    //        return true;
    //    }

    //    public override void Resolve(MemberInfo member, List<Attribute> attributes)
    //    {
    //        var group = attributes.OfType<ResponsiveButtonGroupAttribute>().FirstOrDefault();
    //        if (group == null)
    //        {
    //            return;
    //        }

    //        if (!attributes.HasAttribute<ButtonAttribute>())
    //        {
    //            attributes.Add(new ButtonAttribute(group.DefaultButtonSize));
    //        }
    //    }
    //}
}
#endif