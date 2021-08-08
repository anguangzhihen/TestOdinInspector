#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="LabelWidthAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Utilities.Editor;
    using UnityEngine;

    /// <summary>
    /// Draws properties marked with the <see cref="LabelWidthAttribute"/>.
    /// </summary>
    /// <seealso cref="LabelTextAttribute"/>
    /// <seealso cref="HideLabelAttribute"/>
    /// <seealso cref="LabelWidthAttribute"/>
    /// <seealso cref="TooltipAttribute"/>
    /// <seealso cref="TitleAttribute"/>
    /// <seealso cref="HeaderAttribute"/>
    /// <seealso cref="GUIColorAttribute"/>

    [DrawerPriority(DrawerPriorityLevel.SuperPriority)]
    public sealed class LabelWidthAttributeDrawer : OdinAttributeDrawer<LabelWidthAttribute>
    {
        /// <summary>
        /// Draws the attribute.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var attribute = this.Attribute;

            if (attribute.Width < 0)
            {
                GUIHelper.PushLabelWidth(GUIHelper.BetterLabelWidth + attribute.Width);
            }
            else
            {
                GUIHelper.PushLabelWidth(attribute.Width);
            }

            this.CallNextDrawer(label);
            GUIHelper.PopLabelWidth();
        }
    }
}
#endif