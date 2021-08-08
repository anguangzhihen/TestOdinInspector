#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DecimalDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.Utilities.Editor;
    using UnityEngine;

    /// <summary>
    /// Decimal property drawer.
    /// </summary>
    public sealed class DecimalDrawer : OdinValueDrawer<decimal>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            entry.SmartValue = SirenixEditorFields.DecimalField(label, entry.SmartValue);
        }
    }
}
#endif