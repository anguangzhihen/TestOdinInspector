#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="HideLabelAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using UnityEngine;

    /// <summary>
    /// Draws properties marked with <see cref="HideLabelAttribute"/>.
    /// </summary>
	/// <seealso cref="HideLabelAttribute"/>
	/// <seealso cref="LabelTextAttribute"/>
	/// <seealso cref="TitleAttribute"/>
	/// <seealso cref="HeaderAttribute"/>
	/// <seealso cref="GUIColorAttribute"/>
    [DrawerPriority(91, 0, 0)]
    public sealed class HideLabelAttributeDrawer : OdinAttributeDrawer<HideLabelAttribute>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            this.CallNextDrawer(null);
        }
    }
}
#endif