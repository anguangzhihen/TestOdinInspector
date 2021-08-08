#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TooltipAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.ValueResolvers;
    using Sirenix.Utilities.Editor;
    using UnityEngine;

    /// <summary>
    /// Draws properties marked with <see cref="TooltipAttribute"/>.
    /// </summary>
    /// <seealso cref="TooltipAttribute"/>
    [DrawerPriority(DrawerPriorityLevel.SuperPriority)]
    public sealed class TooltipAttributeDrawer : OdinAttributeDrawer<TooltipAttribute>
    {
        private ValueResolver<string> tooltipResolver;

        protected override void Initialize()
        {
            this.tooltipResolver = ValueResolver.GetForString(this.Property, this.Attribute.tooltip);
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (label != null)
            {
                if (this.tooltipResolver.HasError)
                {
                    SirenixEditorGUI.ErrorMessageBox(this.tooltipResolver.ErrorMessage);
                }

                label.tooltip = this.tooltipResolver.GetValue();
            }

            this.CallNextDrawer(label);
        }
    }

    /// <summary>
    /// Draws properties marked with <see cref="PropertyTooltipAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyTooltipAttribute"/>
    [DrawerPriority(DrawerPriorityLevel.SuperPriority)]
    public sealed class PropertyTooltipAttributeDrawer : OdinAttributeDrawer<PropertyTooltipAttribute>
    {
        private ValueResolver<string> tooltipResolver;

        protected override void Initialize()
        {
            this.tooltipResolver = ValueResolver.GetForString(this.Property, this.Attribute.Tooltip);
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (label != null)
            {
                if (this.tooltipResolver.HasError)
                {
                    SirenixEditorGUI.ErrorMessageBox(this.tooltipResolver.ErrorMessage);
                }

                label.tooltip = this.tooltipResolver.GetValue();
            }

            this.CallNextDrawer(label);
        }
    }
}
#endif