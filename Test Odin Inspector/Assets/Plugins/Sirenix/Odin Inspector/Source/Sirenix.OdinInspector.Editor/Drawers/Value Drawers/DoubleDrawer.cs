#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DoubleDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Double property drawer.
    /// </summary>
    public sealed class DoubleDrawer : OdinValueDrawer<double>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            entry.SmartValue = SirenixEditorFields.DoubleField(label, entry.SmartValue);
        }
    }
}
#endif