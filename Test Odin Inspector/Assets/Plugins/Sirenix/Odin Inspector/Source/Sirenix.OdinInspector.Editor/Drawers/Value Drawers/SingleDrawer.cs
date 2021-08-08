#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="SingleDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEngine;

    /// <summary>
    /// Float property drawer.
    /// </summary>
    public sealed class SingleDrawer : OdinValueDrawer<float>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            entry.SmartValue = SirenixEditorFields.FloatField(label, entry.SmartValue, GUILayoutOptions.MinWidth(0));
        }
    }
}
#endif