#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PropertyTooltipExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    [AttributeExample(typeof(PropertyTooltipAttribute),
        "PropertyTooltip is used to add tooltips to properties in the inspector.\n\nPropertyTooltip can also be applied to properties and methods, unlike Unity's Tooltip attribute.")]
    internal class PropertyTooltipExamples
    {
        [PropertyTooltip("This is tooltip on an int property.")]
        public int MyInt;

        [InfoBox("Use $ to refer to a member string.")]
        [PropertyTooltip("$Tooltip")]
        public string Tooltip = "Dynamic tooltip.";

        [Button, PropertyTooltip("Button Tooltip")]
        private void ButtonWithTooltip()
        {
            // ...
        }
    }
}
#endif